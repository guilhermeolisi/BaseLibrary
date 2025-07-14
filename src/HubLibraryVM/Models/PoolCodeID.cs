using GosControls.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GosControls
{
    /// <summary>
    /// Esta classe tem que ser threadsafe
    /// </summary>
    public static class PoolCodeID
    {
        private static ReaderWriterLockSlim codeIDLock = new ReaderWriterLockSlim();
        public static List<ObjectCodeID> CodeIDRead() { codeIDLock.EnterReadLock(); try { return codeIDs; } finally { codeIDLock.ExitReadLock(); } }
        public static ObjectCodeID CodeIDGet(int value) { codeIDLock.EnterWriteLock(); try { return codeIDs[value]; } finally { codeIDLock.ExitWriteLock(); } }
        public static void CodeIDClear() { codeIDLock.EnterWriteLock(); try { codeIDs.Clear(); } finally { codeIDLock.ExitWriteLock(); } }
        public static int CodeIDCount() { codeIDLock.EnterWriteLock(); try { return codeIDs.Count; } finally { codeIDLock.ExitWriteLock(); } }
        public static void CodeIDWrite(List<ObjectCodeID> value) { codeIDLock.EnterWriteLock(); try { codeIDs = value; } finally { codeIDLock.ExitWriteLock(); } }
        public static void CodeIDAdd(ObjectCodeID value)
        {
            codeIDLock.EnterWriteLock();
            try
            {
                bool find = codeIDs.Contains(value);
                if (!find)
                    codeIDs.Add(value);
            }
            finally { codeIDLock.ExitWriteLock(); }
        }
        public static void CodeIDRemove(ObjectCodeID value) { codeIDLock.EnterWriteLock(); try { codeIDs.Remove(value); } finally { codeIDLock.ExitWriteLock(); } }
        public static void CodeIDRemove(int value) { codeIDLock.EnterWriteLock(); try { codeIDs.RemoveAt(value); } finally { codeIDLock.ExitWriteLock(); } }
        private static List<ObjectCodeID> codeIDs = new();
    }
}
