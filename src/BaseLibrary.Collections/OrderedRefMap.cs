using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace BaseLibrary.Collections;

/// <summary>
/// Mapa <c>int → TValue</c> com ITERAÇÃO DETERMINÍSTICA por chave ORDENADA, cacheada e reconstruída SOMENTE quando a
/// MEMBRESIA muda (add/remove). Pensado para listas de reflexões (chave = índice inteiro da reflexão):
/// <list type="bullet">
///   <item>lookup/atualização/add thread-safe (<see cref="ConcurrentDictionary{TKey,TValue}"/> interno) — suporta a
///   sincronização paralela e a enumeração concorrente que o motor já usa;</item>
///   <item>iteração ESTÁVEL via <see cref="OrderedValues"/> (array contíguo, ordem por chave) para que somas em
///   ponto flutuante (não-associativas) sejam REPRODUZÍVEIS — sem ordenar no consumidor;</item>
///   <item>preenchimento em LOTE já ordenado via <see cref="SetFromOrdered"/> (para a geração particionada do passo 2).</item>
/// </list>
/// Atualizações de VALOR in-place na MESMA chave (ex.: <c>UpdateFromCell</c>) NÃO invalidam a ordem; só add/remove o
/// fazem (raros, só em mudança de cela). A reconstrução do cache é single-thread (roda na fase de consumo, após a
/// sincronização paralela), protegida por lock para nunca devolver um cache pela metade.
/// </summary>
public sealed class OrderedRefMap<TValue> : IEnumerable<KeyValuePair<int, TValue>>
{
    private readonly ConcurrentDictionary<int, TValue> _map;
    private readonly object _rebuildLock = new();
    private int[] _orderedKeys = [];
    private TValue[] _orderedValues = [];
    private volatile bool _dirty = true;

    public OrderedRefMap() => _map = new();
    public OrderedRefMap(int concurrency, int capacity) => _map = new(concurrency, capacity);

    public int Count => _map.Count;

    /// <summary>Chaves (snapshot, ordem NÃO determinística). Para iteração estável use <see cref="OrderedKeys"/>.</summary>
    public ICollection<int> Keys => _map.Keys;

    /// <summary>Valores (snapshot, ordem NÃO determinística). Para iteração estável use <see cref="OrderedValues"/>.</summary>
    public ICollection<TValue> Values => _map.Values;

    public bool ContainsKey(int key) => _map.ContainsKey(key);

    public bool TryGetValue(int key, [MaybeNullWhen(false)] out TValue value) => _map.TryGetValue(key, out value);

    public TValue this[int key]
    {
        get => _map[key];
        // Troca a referência do valor: chave nova ou ref diferente → o cache (que guarda referências) reconstrói.
        set { _map[key] = value; _dirty = true; }
    }

    /// <summary>Adiciona se a chave não existe (thread-safe). Marca a ordem p/ reconstrução (mudança de membresia).</summary>
    public bool TryAdd(int key, TValue value)
    {
        if (_map.TryAdd(key, value)) { _dirty = true; return true; }
        return false;
    }

    /// <summary>Remove a chave (mudança de membresia → invalida a ordem). Assinatura compatível com o uso atual.</summary>
    public bool Remove(int key, [MaybeNullWhen(false)] out TValue value)
    {
        if (_map.TryRemove(key, out value)) { _dirty = true; return true; }
        return false;
    }

    // --- Compatibilidade drop-in com os call-sites que vinham do DictionarySlim (Cell._reflections) ---
    // O backing é um ConcurrentDictionary (thread-safe; leitura/remoção lock-free), então as variantes
    // "Unsafe" não exigem lock externo; o sufixo é mantido só para os call-sites continuarem idênticos.

    /// <summary>Snapshot da lista de chaves (ordem NÃO determinística). Equivale a DictionarySlim.GetKeysList.</summary>
    public List<int> GetKeysList() => new(_map.Keys);

    /// <summary>ContainsKey direto (o ConcurrentDictionary já é thread-safe); nome mantido por compatibilidade.</summary>
    public bool ContainsKeyUnsafe(int key) => _map.ContainsKey(key);

    /// <summary>Remove sem lock externo; invalida a ordem cacheada (a membresia mudou).</summary>
    public bool RemoveUnsafe(int key) => Remove(key, out _);

    public void Clear()
    {
        _map.Clear();
        lock (_rebuildLock) { _orderedKeys = []; _orderedValues = []; _dirty = false; }
    }

    /// <summary>
    /// Valores em ORDEM DETERMINÍSTICA (chave crescente), em array CONTÍGUO. O cache é reconstruído só quando a
    /// membresia mudou desde a última leitura. Use para QUALQUER acúmulo sensível à ordem (ex.: soma do Yc).
    /// Não chamar concorrente com add/remove (a leitura acontece na fase de consumo single-thread).
    /// </summary>
    public ReadOnlySpan<TValue> OrderedValues
    {
        get
        {
            if (_dirty)
                lock (_rebuildLock) { if (_dirty) Rebuild(); }
            return _orderedValues;
        }
    }

    /// <summary>Chaves ordenadas (paralelo a <see cref="OrderedValues"/>).</summary>
    public ReadOnlySpan<int> OrderedKeys
    {
        get
        {
            if (_dirty)
                lock (_rebuildLock) { if (_dirty) Rebuild(); }
            return _orderedKeys;
        }
    }

    // Reconstrói o array ordenado por chave a partir do dicionário (chamado sob _rebuildLock, single-thread).
    private void Rebuild()
    {
        int n = _map.Count;
        var keys = new int[n];
        _map.Keys.CopyTo(keys, 0);
        Array.Sort(keys);
        var vals = new TValue[n];
        for (int i = 0; i < n; i++)
            vals[i] = _map[keys[i]];
        _orderedKeys = keys;
        _orderedValues = vals;
        _dirty = false;
    }

    /// <summary>
    /// PASSO 2 (geração particionada): preenche o mapa com chaves JÁ ORDENADAS crescentes e seus valores na mesma
    /// ordem, SEM ordenar (a geração lock-free por partição de h entrega a sequência já ordenada). Substitui todo o
    /// conteúdo; os arrays passados viram o cache de iteração diretamente.
    /// </summary>
    public void SetFromOrdered(int[] ascendingKeys, TValue[] valuesInKeyOrder)
    {
        ArgumentNullException.ThrowIfNull(ascendingKeys);
        ArgumentNullException.ThrowIfNull(valuesInKeyOrder);
        lock (_rebuildLock)
        {
            _map.Clear();
            int n = Math.Min(ascendingKeys.Length, valuesInKeyOrder.Length);
            for (int i = 0; i < n; i++)
                _map[ascendingKeys[i]] = valuesInKeyOrder[i];
            _orderedKeys = ascendingKeys;
            _orderedValues = valuesInKeyOrder;
            _dirty = false;
        }
    }

    /// <summary>Enumeração (KeyValuePair) thread-safe do <see cref="ConcurrentDictionary{TKey,TValue}"/> interno ,
    /// compatível com <c>foreach</c>/LINQ/Parallel.ForEach. A ORDEM aqui NÃO é determinística (use
    /// <see cref="OrderedValues"/> quando a ordem importar); consumidores que reordenam na saída usam esta.</summary>
    public IEnumerator<KeyValuePair<int, TValue>> GetEnumerator() => _map.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _map.GetEnumerator();
}
