using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SingularityBase;
using SolidWorks.Interop.sldworks;

namespace SingularityCore
{
    internal class SingleAnnotation : SingularityObject<IAnnotation>,ISingleAnnotation
    {

        public ISingleAnnotationView View { get; }
        public ISingleModelDoc Document { get; }

        public SingleAnnotation(ISingleModelDoc document, ISingleAnnotationView view,IAnnotation anno) : base(anno)
        {
            Document = document;
            View = view;
        }



        public string Name => BaseObject.GetName();
    }
}
