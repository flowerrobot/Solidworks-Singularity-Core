using System;
using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Collections.Generic;

namespace SingularityCore
{
    internal class SingleView : ISingleView, IDisposable
    {
        public IView View { get; private set; }
        public ISingleModelDoc Document { get; }
        internal SingleView(ISingleModelDoc doc, IView view)
        {
            Document = doc;
            View = view;
        }

        public string Name => View.Name;
        public ISingleView GetNextView => new SingleView(Document, View.GetNextView());
        public int GetTableAnnotationCount => View.GetTableAnnotationCount();
        public IBomTable GetBomTable => View.GetBomTable();

        public IEnumerable<ISingleTableAnnotation> GetTableAnnotations
        {
            get {
                List<ISingleTableAnnotation> tbls = new List<ISingleTableAnnotation>();
                foreach (object annotation in (object[]) View.GetTableAnnotations())
                {
                    switch ((swTableAnnotationType_e)((ITableAnnotation)annotation).Type)
                    {
                        case swTableAnnotationType_e.swTableAnnotation_WeldmentCutList:
                            tbls.Add(new SingleWeldmentCutListAnnotation(Document, (IWeldmentCutListAnnotation)annotation));
                            break;
                        case swTableAnnotationType_e.swTableAnnotation_General:
                        case swTableAnnotationType_e.swTableAnnotation_BillOfMaterials:
                        default:
                            break;
                    }
                }
                return tbls;
            }
        }


        public string ReferencedConfiguration { get; set; }

        public swDrawingViewTypes_e Type => (swDrawingViewTypes_e) View.Type;
        public ISingleDrawingComponent RootDrawingComponent => new SingleDrawingComponent(View.RootDrawingComponent);

        public void Dispose()
        {
            View = null;
            System.Runtime.InteropServices.Marshal.ReleaseComObject(View);
        }
    }
}
