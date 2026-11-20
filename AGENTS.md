# Instrucoes nativas do Codex

Fonte de sincronizacao: `BaseLibrary/CLAUDE.md`. Nao altere os originais Claude ao manter este porte.
Leia tambem os AGENTS.md ancestrais e o [AGENTS.md do workspace](../AGENTS.md), mesmo se a sessao iniciou neste repositorio Git filho.

Estas instrucoes sao completas para o Codex. Nao e necessario abrir o CLAUDE.md
original para segui-las. As adaptacoes abaixo prevalecem sobre mecanismos do Claude
descritos no conteudo sincronizado: ferramentas, modelos, cache, limites de contexto,
permissoes, plugins, metricas e hooks precisam de suporte verificado no cliente Codex.
Herde o modelo da sessao, sem aplicar aliases ou guardas Anthropic. Use apply_patch
para editar, rg para buscar e PowerShell para comandos. Leia regras paths explicitamente.
O progresso de prompts usa prompts/em-execucao/E###/ e sua trava comum, conforme
prompts/em-execucao/README.md. Adquira a tarefa com execucao-prompt.ps1 antes de editar.
Autorizacoes, aprovacoes e controle de sessao ficam em .codex/state/, separados do Claude.
Use o id real CODEX_THREAD_ID e -Sessao
quando aceito; nunca reutilize marcadores de autorizacao ou revisao de outra sessao.
Para subagentes, use colaboracao nativa e contexto independente na revisao.
Hooks locais so executam em clientes locais apos revisao e confianca do usuario.
Em sessao orquestrada, cumpra as verificacoes explicitamente. A ausencia de hooks
nao dispensa autorizacao de linguagem, testes, revisao, rede ou horario de push.
Nao presuma LSP ou plugins disponiveis; confira ferramentas, XAML e linker.xml.
Comandos que iniciam claude ou dependem dele sao especificos do cliente Claude;
nao os execute como inicializacao do Codex.
O conteudo sincronizado abaixo preserva as regras de negocio e autorizacao.
Se a injecao destas instrucoes nao terminar no comentario CODEX-INSTRUCTIONS-END,
leia este AGENTS.md completo pelo sistema de arquivos antes de trabalhar. Uma sessao
ja aberta ou orquestrada pode manter um limite menor que o configurado no projeto.

## Instrucoes sincronizadas

# BaseLibrary: biblioteca multiuso

Biblioteca C# .NET 10 usada pelas demais solucoes do workspace (Sindarin, Nimloth,
SindarinAI, AutoUpdater, GOS*). Utilitarios de proposito amplo, sem depender do
dominio de difracao. Regras compartilhadas em `../AGENTS.md`.

E dependencia de 14 projetos do `Nimloth.sln` e 12 do `Sindarin.sln`: mudanca de
API aqui quebra consumidores. Commite BaseLibrary ANTES dos consumidores e rode o
build deles (`dotnet build ../Nimloth/Nimloth.sln`) depois de mudar assinatura.

## Componentes notaveis

- **BaseLibrary.Math.Matrix** (namespace `Sindarin.Math.Matrix`): matrizes proprias
  (Diagonal, Triangular, Sparse, Jagged3D, densas). SUBSTITUIU o MathNet no
  `Sindarin.Objects.Calculation`; inclui o caminho `M^T W M` (equacoes normais) do NLS.
- **BaseLibrary.Math** e **BaseLibrary.Math.SpecialFunctions**: Gamma, Erf/Erfc,
  Bessel, Struve, sem dependencia externa.
- **BaseLibrary.DependencyInjection**: padrao de DI de todas as solucoes, exceto o
  nenhuma: o Nimloth tambem usa este, apesar do que o texto antigo dizia (medido em 08/09/2026).
- **BaseLibrary.Console**: ferramentas de linha de comando reutilizaveis.
- **BaseLibrary.File**: preferir `FileServices` a `FileMethods` quando houver as duas.
- Tambem: General, Collections, Exception, HTTP, Text, Numbers.

## Convencoes

- Classes em PascalCase. Comentarios podem ser em portugues; texto de usuario em ingles.
- Preferir classe instanciavel com interface a API estatica (permite DI e mock).
- **Nenhum estado global mutavel** (regra do usuario, 16/11/2026, para o motor Sindarin e para o que ele consome daqui):
  o motor tem de rodar em varias instancias em paralelo no mesmo processo. Nada de campo `static` escrito depois da
  inicializacao, colecao `static` que recebe `Add`, `static Random` compartilhado, fachada estatica que guarda servico
  com estado, nem dependencia da cultura corrente (cultura invariante explicita). O `Container` e o `Locator` sao lidos
  por varias threads: registro e leitura tem de ser seguros entre threads (desde a E318: `Container` com
  trava e bootstrap idempotente; a catraca `GlobalStateConfigTests` do Sindarin le os projetos daqui que o motor
  referencia). Regra completa:
  `../Sindarin/.codex/rules/arquitetura.md`, secao 5.
- Testes: xUnit + Moq + FluentAssertions, AAA, um projeto de teste por projeto
  (`BaseLibrary.Tests`). Regras completas em `../.codex/rules/testes.md`.

## Comandos

- `dotnet build BaseLibrary.sln --no-restore`
- `dotnet test BaseLibrary.sln --no-restore`

<!-- CODEX-INSTRUCTIONS-END -->
