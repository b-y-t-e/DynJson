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
        public static bool Equals2(this object Obj1, object Obj2)
        {
            return (Obj1 == null && Obj2 == null) || (Obj1 != null && Obj2 != null && Obj1.Equals(Obj2));
        }

        public static void AddRange<T>(this ObservableCollection<T> Collection, IEnumerable<T> Items)
        {
            foreach (T item in Items)
                Collection.Add(item);
        }

        public static IEnumerable<Object> GetNewItemsIn(
            this IEnumerable Items1,
            IEnumerable Items2,
            String KeyProperty)
        {
            if (Items1 != null && Items2 != null)
            {
                foreach (Object item2 in Items2)
                {
                    Boolean existsInItems1 = false;

                    foreach (Object item1 in Items1)
                    {
                        if (HaveEquealKeys(item1, item2, KeyProperty))
                        {
                            existsInItems1 = true;
                            break;
                        }
                    }

                    if (!existsInItems1)
                        yield return item2;
                }
            }
            else if (Items2 != null)
            {
                foreach (Object item2 in Items2)
                    yield return item2;
            }
        }

        public static bool CollectionContainsItem(
            this IEnumerable Items,
            Object Item,
            String KeyProperty)
        {
            IEnumerable dataSource = Items;

            if (Items != null && Item != null)
            {
                Boolean foundItem = false;
                if (dataSource != null)
                {
                    Object itemKey = GetKeyValue(Item, KeyProperty);
                    foreach (Object item in Items)
                    {
                        if (GetKeyValue(item, KeyProperty).Equals2(itemKey))
                        {
                            foundItem = true;
                            break;
                        }
                    }
                }
                return foundItem;
            }

            return false;
        }

        public static Object CollectionFindByKey(
            this IEnumerable Items,
            Object Key,
            String KeyProperty)
        {
            IEnumerable dataSource = Items;

            if (Items != null && Key != null)
            {
                if (dataSource != null)
                {
                    foreach (Object item in Items)
                    {
                        if (GetKeyValue(item, KeyProperty).Equals2(Key))
                        {
                            return item;
                        }
                    }
                }
            }

            return null;
        }

        public static bool HaveEquealKeys(
            this Object Obj1,
            Object Obj2,
            String KeyProperty)
        {
            return
                GetKeyValue(Obj1, KeyProperty).
                Equals2(GetKeyValue(Obj2, KeyProperty));
        }

        private static Object GetKeyValue(Object Item, String KeyPropertyName)
        {
            return ReflectionHelper.GetValue(Item, KeyPropertyName);
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