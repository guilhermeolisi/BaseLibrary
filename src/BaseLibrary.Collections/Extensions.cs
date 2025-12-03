using System.Collections;
using System.Collections.ObjectModel;

namespace BaseLibrary.Collections;

public static class Extensions
{
    public static void OrderObservableToString<T>(this ObservableCollection<T> obs)
    {
        Type[] interfaces = obs.GetType().GetGenericArguments()[0].GetInterfaces();
        bool isIComparable = false;
        foreach (Type inter in interfaces)
            if (inter.Name == "IComparable")
            {
                isIComparable = true;
                break;
            }
        if (isIComparable)
        {
            int i = 0;
            while (i < obs.Count - 1)
            {
                if ((obs[i] as IComparable).CompareTo(obs[i + 1]) > 0)
                {
                    obs.Move(i + 1, i);
                    i = 0;
                }
                else
                    i++;
            }
        }
        else
        {
            int i = 0;
            while (i < obs.Count - 1)
            {
                if (obs[i].ToString()?.ToUpper().CompareTo(obs[i + 1].ToString()?.ToUpper()) > 0)
                {
                    obs.Move(i + 1, i);
                    i = 0;
                }
                else
                    i++;
            }
        }
    }
    public static IList<TSource> GOSOrderBy<TSource, TKey>(this IList<TSource> source, Func<TSource, TKey> keySelector)
        where TKey : IComparable
    {
        int i = 1;
        while (i < source.Count)
        {

            if (keySelector(source[i]).CompareTo(keySelector(source[i - 1])) < 0)
            {
                int j = 0;
                while (j < i)
                {
                    if (keySelector(source[i]).CompareTo(keySelector(source[j])) < 0)
                    {
                        TSource itemi = source[i];
                        source.RemoveAt(i);
                        source.Insert(j, itemi);
                        break;
                    }
                    else
                    {
                        j++;
                    }
                }
            }
            else
            {
                i++;
            }
        }
        return source;
    }
    public static bool RemoveIfContain<T>(this ICollection<T> list, T item)
    {
        bool has = false;
        foreach (T item2 in list)
        {
            if (item2.Equals(item))
            {
                has = true;
                break;
            }
        }
        if (has)
        {
            list.Remove(item);
            return true;
        }
        return false;
    }
    public static bool[] RemoveIfContain<T>(this ICollection<T> list, ICollection items)
    {
        bool[] result = new bool[items.Count];
        List<T> toRemove = new();
        int i = 0;
        foreach (var item in items)
        {
            foreach (T itemL in list)
            {
                if (itemL.Equals(item))
                {
                    result[i] = true;
                    toRemove.Add((T)item);
                    break;
                }
            }
            i++;
        }
        for (i = 0; i < toRemove.Count; i++)
        {
            list.Remove(toRemove[i]);
        }
        return result;
    }
    public static IEnumerable MirrorTo<T>(this IEnumerable original, ICollection<T> destine)
    {
        List<T> toRemove = new();
        foreach (T itemD in destine)
        {
            bool has = false;
            if (itemD is not null)
            {
                foreach (T itemO in original)
                {
                    if (itemO is null)
                        continue;
                    if (itemO.Equals(itemD))
                    {
                        has = true;
                        break;
                    }
                }
            }
            if (!has)
            {
                toRemove.Add(itemD);
            }
        }
        for (int i = 0; i < toRemove.Count; i++)
        {
            destine.Remove(toRemove[i]);
        }
        foreach (T itemO in original)
        {
            if (itemO is null)
                continue;
            bool has = false;
            foreach (T itemD in destine)
            {
                if (itemD is null)
                {
                    continue;
                }
                if (itemO.Equals(itemD))
                {
                    has = true;
                    break;
                }
            }
            if (!has)
            {
                destine.Add(itemO);
            }
        }
        return original;
    }
    public static void CloneTo<T>(this IEnumerable original, ICollection<T> destine)
    {
        destine.Clear();
        foreach (T item in original)
        {
            if (item is null)
                continue;
            destine.Add(item);
        }
    }
    public static int GetIndexOf(this IEnumerable original, object value) //where T : IComparable
    {
        int ind = 0;
        foreach (var item in original)
        {
            if (item.Equals(value))
                return ind;
            ind++;
        }
        return -1;
    }
    public static IEnumerable<T> Foreach<T>(this IEnumerable<T> list, Action<T> action)
    {
        foreach (var item in list)
        {
            action.Invoke(item);
        }
        return list;
    }
    public static IEnumerable Foreach(this IEnumerable list, Action<object> action)
    {
        foreach (var item in list)
        {
            action.Invoke(item);
        }
        return list;
    }
    public static IEnumerable<T> GetOnlyDistinct<T, TComp>(this IEnumerable<T> list, Func<T, TComp> propToComp)
    {
        if (propToComp is null)
            throw new ArgumentNullException(nameof(propToComp));
        List<T> result = new();
        int ind1 = 0;
        int ind2 = 0;
        foreach (var item in list)
        {
            bool hasEqual = false;
            ind2 = 0;
            foreach (var item2 in list)
            {
                if (ind2 >= ind1)
                    break;
                if (propToComp.Invoke(item).Equals(propToComp.Invoke(item2)))
                {
                    hasEqual = true;
                    break;
                }
                ind2++;
            }
            if (!hasEqual)
            {
                result.Add(item);
            }
            ind1++;
        }
        return list;
    }
    public static int Count(this IEnumerable list)
    {
        if (list is ICollection collection)
            return collection.Count;
        int count = 0;
        foreach (var item in list)
        {
            count++;
        }
        return count;
    }
    public static void RemoveLast(this IList list)
    {
        if (list.Count > 0)
            list.RemoveAt(list.Count - 1);
    }
    public static void RemoveExtras(this IList list, int total)
    {
        while (list.Count > total)
        {
            list.RemoveLast();
        }
    }
    public static bool IsNullOrEmpty(this IEnumerable? list)
    {
        if (list is null)
            return true;
        if (list is ICollection collection)
            return collection.Count == 0;
        var enumerator = list.GetEnumerator();
        return !enumerator.MoveNext();
    }
    public static bool IsNotNullOrEmpty(this IEnumerable? list) => !list.IsNullOrEmpty();
    /// <summary>
    /// Determines whether two read-only lists of integers are equal by comparing their elements in order.
    /// </summary>
    /// <remarks>The comparison is performed element by element in sequence. If either list is <see langword="null"/>
    /// or empty, the method returns <see langword="false"/>.</remarks>
    /// <param name="main">The first list to compare. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="second">The second list to compare. Cannot be <see langword="null"/> or empty.</param>
    /// <returns><see langword="true"/> if both lists have the same number of elements and all corresponding elements are equal;
    /// otherwise, <see langword="false"/>.</returns>
    public static bool IsEqual(this IReadOnlyList<int> main, IReadOnlyList<int> second)
    {
        if (main.IsNullOrEmpty() || second.IsNullOrEmpty())
            return false;


        if (main.Count != second.Count)
            return false;
        bool isEqual = true;
        for (int i = 0; i < main.Count; i++)
        {
            if (main[i] != second[i])
            {
                isEqual = false;
                break;
            }
        }
        if (isEqual)
            return true;

        return false;
    }
    public static bool IsAllEqual(this IEnumerable<int> main, IEnumerable<int> second)
    {
        if (main.IsNullOrEmpty() || second.IsNullOrEmpty())
            return false;
        if (main.Count() != second.Count())
            return false;
        bool isEqual = true;
        foreach (var itemMain in main)
        {
            bool has = false;
            foreach (var itemSecond in second)
            {
                if (itemMain == itemSecond)
                {
                    has = true;
                    break;
                }
            }
            if (!has)
            {
                isEqual = false;
                break;
            }
        }
        return isEqual;
    }
    public static bool IsAllEqual(this IReadOnlyList<int> main, IReadOnlyList<int> second)
    {
        if (main.IsNullOrEmpty() || second.IsNullOrEmpty())
            return false;
        if (main.Count != second.Count)
            return false;
        bool isEqual = true;
        for (int i = 0; i < main.Count; i++)
        {
            bool has = false;
            for (int j = 0; j < second.Count; j++)
            {
                if (main[i] == second[j])
                {
                    has = true;
                    break;
                }
            }
            if (!has)
            {
                isEqual = false;
                break;
            }
        }
        return isEqual;
    }
    /// <summary>
    /// Determines whether the specified sequence of lists contains a list that is equal to the given list.
    /// </summary>
    /// <remarks>Two lists are considered equal if they have the same number of elements and each
    /// corresponding element is equal. If either <paramref name="main"/> or <paramref name="second"/> is <see
    /// langword="null"/> or empty, the method returns <see langword="false"/>.</remarks>
    /// <param name="main">The sequence of lists to search. Each list in the sequence is compared to <paramref name="second"/>.</param>
    /// <param name="second">The list to compare against each list in <paramref name="main"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="main"/> contains a list that is equal to <paramref name="second"/>;
    /// otherwise, <see langword="false"/>.</returns>
    public static bool HasEqual(this IEnumerable<IList<int>> main, IList<int> second)
    {
        if (main.IsNullOrEmpty() || second.IsNullOrEmpty())
            return false;

        foreach (var item in main)
        {
            if (item.Count != second.Count)
                continue;
            bool isEqual = true;
            for (int i = 0; i < item.Count; i++)
            {
                if (item[i] != second[i])
                {
                    isEqual = false;
                    break;
                }
            }
            if (isEqual)
                return true;
        }
        return false;
    }
    public static IList<double> InverseSign(this IList<double> enumerable)
    {
        double[] array = new double[enumerable.Count];
        for (int i = 0; i < enumerable.Count; i++)
        {
            array[i] = -1 * enumerable[i];
        }
        return array;
    }
    public static IList<int> InvertSign(this IList<int> enumerable)
    {
        int[] array = new int[enumerable.Count];
        for (int i = 0; i < enumerable.Count; i++)
        {
            array[i] = -1 * enumerable[i];
        }
        return array;
    }
}