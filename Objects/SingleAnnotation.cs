using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SingularityBase;
using SolidWorks.Interop.sldworks;

namespace SingularityCore
{
    internal class SingleAnnotation : ISingleAnnotation
    {

        public IAnnotation Annotation { get; internal set; }
        public ISingleAnnotationView View { get; }
        public ISingleModelDoc Document { get; }

        public SingleAnnotation(ISingleModelDoc document, ISingleAnnotationView view,IAnnotation anno)
        {
            Document = document;
            Annotation = anno;
            View = view;
        }
        public void Dispose()
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(Annotation);
            Annotation = null;
        }


        public string Name => Annotation.GetName();
    }
}
