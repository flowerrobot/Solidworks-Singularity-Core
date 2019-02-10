using SingularityBase;
using SolidWorks.Interop.sldworks;

namespace SingularityCore
{
    internal class SingleAnnotationView :SingularityObject<IAnnotationView>, ISingleAnnotationView
    {
        internal SingleAnnotationView(IAnnotationView view) : base(view){}

        public bool FlatPatternView => BaseObject.FlatPatternView;

        public bool Show
        {
            get => BaseObject.IsShown();
            set {
                if (value) BaseObject.Show();
            }
        }
    }
}
