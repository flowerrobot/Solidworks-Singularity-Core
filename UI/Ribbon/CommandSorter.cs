using System;
using System.Collections.Generic;

namespace SingularityCore.UI
{

    /// <summary>
    /// Comparer for comparing two keys, handling equality as beeing greater
    /// Use this Comparer e.g. with SortedLists or SortedDictionaries, that don't allow duplicate keys
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class CommandSorter<TKey> :IComparer<TKey> where TKey : IComparable
    {
        // -1 The current instance precedes the object specified by the CompareTo method in the sort order.
        // 0 They are equal
        // 1 This current instance follows the object specified by the CompareTo method in the sort order.

        #region IComparer<TKey> Members

        public int Compare(TKey x, TKey y)
        {
            int result = x.CompareTo(y);

            if (result == 0)
                return 1; // Handle equality as beeing greater
            else
                return result;
        }


        //cmds.Sort(
        //                delegate (ISwMenu x, ISwMenu y) //0 is equal, 1 x, -1 y
        //                {
        //                    if (x.MenuOrder == -1 && y.MenuOrder == -1) return 0;
        //                    else if (x.MenuOrder == -1) return -1;
        //                    else if (y.MenuOrder == -1) return 1;
        //                    else return x.MenuOrder.CompareTo(y.MenuOrder);
        //                });

        #endregion
    }
}
