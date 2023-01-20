using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace BaseLibrary;

public static class Collections
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
        int i = 0;
        while (i < source.Count - 1)
        {
            if (keySelector(source[i]).CompareTo(keySelector(source[i + 1])) > 0)
            {
                int j = 0;
                while (j < i)
                {
                    if (keySelector(source[j]).CompareTo(keySelector(source[i])) > 0)
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
}
