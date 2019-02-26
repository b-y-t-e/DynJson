using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynJson.Helpers.CoreHelpers
{
    public static class StringLock
    {
        private static Dictionary<String, Object> locks;

        private static Object innerLock = new Object();

        //////////////////////////////////////////////

        public static Object Get(String Name)
        {
            if (locks == null)
                lock (innerLock)
                    if (locks == null)
                        locks = new Dictionary<String, Object>();

            lock (innerLock)
            {
                Object lck = null;
                if (!locks.TryGetValue(Name, out lck))
                    lck = locks[Name] = new Object();
                return lck;
            }
        }
    }
}
