using SingularityBase;
using SolidWorks.Interop.sldworks;

namespace SingularityCore
{
    internal class SingleAnnotationView : ISingleAnnotationView
    {
        internal SingleAnnotationView(IAnnotationView view)
        {
            AnnotationView = view;
        }
        public IAnnotationView AnnotationView { get; }
        public bool FlatPatternView => AnnotationView.FlatPatternView;

        public bool Show
        {
            get => AnnotationView.IsShown();
            set {
                if (value) AnnotationView.Show();
            }
        }
    }
}
