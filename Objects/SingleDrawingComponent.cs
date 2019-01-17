using SingularityBase;
using SolidWorks.Interop.sldworks;

namespace SingularityCore
{
    internal class SingleDrawingComponent : ISingleDrawingComponent
    {
        private IDrawingComponent DrawingComponent { get; }

        public SingleDrawingComponent(IDrawingComponent comp)
        {
            DrawingComponent = comp;
        }

        public ISingleComponent Component => new SingleComponent(DrawingComponent.Component);
        public bool Select(int mark) => Select(false, mark);

        public bool Select(bool append, int mark)
        {
            ISelectData a = Component.Document.SelectionManager.CreateSelectData;
            a.Mark = mark;

            return DrawingComponent.Select(append, (SelectData)a);
        }

        public bool DeSelect() => DrawingComponent.DeSelect();



    }
}
