using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SingularityBase;
using SolidWorks.Interop.sldworks;

namespace SingularityCore
{
   internal class SingleSelectionManager : ISingleSelectionManager, IDisposable
    {
        public SingleSelectionManager(ISelectionMgr selmgr)
        {
            SelectionMgr = selmgr;
        }
        public ISelectionMgr SelectionMgr { get; private set; }
        public ISelectData CreateSelectData => SelectionMgr.CreateSelectData();

        public void Dispose()
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(SelectionMgr);
            SelectionMgr = null;
        }
    }
}
