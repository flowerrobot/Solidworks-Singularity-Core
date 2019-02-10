using SingularityBase;
using SolidWorks.Interop.sldworks;

namespace SingularityCore
{
    internal class SingleSelectionManager : SingularityObject<ISelectionMgr>, ISingleSelectionManager
    {
        public SingleSelectionManager(ISelectionMgr selmgr) : base(selmgr){}
        public ISingleBaseObject<ISelectData> CreateSelectData => new SingularityObject<ISelectData>(BaseObject.CreateSelectData());
    }
}
