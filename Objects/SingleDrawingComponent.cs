using SingularityBase;
using SolidWorks.Interop.sldworks;

namespace SingularityCore
{
    internal class SingleDrawingComponent :SingularityObject<IDrawingComponent>, ISingleDrawingComponent
    {

        public SingleDrawingComponent(IDrawingComponent comp) : base(comp){}

        public ISingleComponent Component => new SingleComponent(BaseObject.Component);
        public bool Select(int mark) => Select(false, mark);

        public bool Select(bool append, int mark)
        {
            var a = Component.Document.SelectionManager.CreateSelectData;
            a.BaseObject.Mark = mark;

            return BaseObject.Select(append, (SelectData)a.BaseObject);
        }

        public bool DeSelect() => BaseObject.DeSelect();



    }
}
