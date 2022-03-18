using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BaseLibrary
{
    public static class Collections
    {
        public static void OrderObservable<T>(in ObservableCollection<T> obs)
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
    }
}
