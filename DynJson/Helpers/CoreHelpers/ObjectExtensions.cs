using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

using System.Collections.ObjectModel;


namespace DynJson.Helpers.CoreHelpers
{
    public static class ObjectExtensions
    {
        public static T GetOrDefault<T>(this IList<T> Items, Int32 Index)
        {
            if (Index >= 0 && Index < Items.Count)
                return Items[Index];
            return default(T);
        }

        public static bool Equals2(this object Obj1, object Obj2)
        {
            return (Obj1 == null && Obj2 == null) || (Obj1 != null && Obj2 != null && Obj1.Equals(Obj2));
        }

        public static void AddRange<T>(this ObservableCollection<T> Collection, IEnumerable<T> Items)
        {
            foreach (T item in Items)
                Collection.Add(item);
        }
        
        public static IList<T> RemoveDuplicates<T>(this IList<T> Items, Func<T, Object> KeyGetter)
        {
            for (var i = 0; i < Items.Count; i++)
            {
                Object key1 = KeyGetter(Items[i]);
                for (var j = i + 1; j < Items.Count; j++)
                {
                    Object key2 = KeyGetter(Items[j]);
                    if (key1.Equals2(key2))
                    {
                        Items.RemoveAt(j);
                        j--;
                    }
                }
            }
            return Items;
        }

        public static IEnumerable<Object> ToEnumerable(this IList Items)
        {
            foreach (Object item in Items)
                yield return item;
        }

        public static IEnumerable<Object> ToEnumerable(this IEnumerable Items)
        {
            foreach (Object item in Items)
                yield return item;
        }

        public static bool CollectionEquals2(this object Obj1, object Obj2)
        {
            var lR = false;
            if (Obj1 is IEnumerable && Obj2 is IEnumerable)
            {
                if (Obj1.Equals2(Obj2))
                {
                    lR = true;
                }
                else
                {
                    var lEnumerator1 = (Obj1 as IEnumerable).GetEnumerator();
                    var lEnumerator2 = (Obj2 as IEnumerable).GetEnumerator();
                    while (true)
                    {
                        var lIsNext1 = lEnumerator1.MoveNext();
                        var lIsNext2 = lEnumerator2.MoveNext();

                        if (lIsNext1 && lIsNext2)
                        {
                            if (!lEnumerator1.Current.Equals2(lEnumerator2.Current))
                            {
                                lR = false;
                                break;
                            }
                        }
                        else
                        {
                            if (!lIsNext1 && !lIsNext2)
                            {
                                lR = true;
                            }
                            else
                            {
                                lR = false;
                            }
                            break;
                        }
                    }
                }
            }
            return lR;
        }
    }
}