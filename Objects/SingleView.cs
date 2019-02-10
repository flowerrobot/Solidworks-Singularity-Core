using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;

namespace SingularityCore
{
    internal class SingleView : SingularityObject<IView>, ISingleView
    {
        public ISingleModelDoc Document { get; }

        internal SingleView(ISingleModelDoc doc, IView view) : base(view)
        {
            Document = doc;

        }

        public string Name => BaseObject.Name;

        public ISingleView GetNextView
        {
            get {
                IView v = (IView)BaseObject.GetNextView();
                return v != null ? new SingleView(Document, v) : null;
            }
        }

        public int GetTableAnnotationCount => BaseObject.GetTableAnnotationCount();
        public ISingleBaseObject<IBomTable> GetBomTable => new SingularityObject<IBomTable>(BaseObject.GetBomTable());

        public IEnumerable<ISingleTableAnnotation> GetTableAnnotations
        {
            get {
                List<ISingleTableAnnotation> tbls = new List<ISingleTableAnnotation>();
                foreach (object annotation in (object[])BaseObject.GetTableAnnotations())
                {
                    switch ((swTableAnnotationType_e)((ITableAnnotation)annotation).Type)
                    {
                        case swTableAnnotationType_e.swTableAnnotation_WeldmentCutList:

                            IWeldmentCutListAnnotation tt = annotation as IWeldmentCutListAnnotation;

                            ITableAnnotation ll = annotation as ITableAnnotation;

                            tbls.Add(new SingleWeldmentCutListAnnotation(Document,
                                (IWeldmentCutListAnnotation)annotation));
                            break;
                        case swTableAnnotationType_e.swTableAnnotation_General:
                        case swTableAnnotationType_e.swTableAnnotation_BillOfMaterials:
                            throw new NotImplementedException("Need to implement these");
                        default:
                            break;
                    }
                }

                return tbls;
            }
        }


        public string ReferencedConfiguration { get=> BaseObject.ReferencedConfiguration; set=>BaseObject.ReferencedConfiguration = value; }

        public swDrawingViewTypes_e Type => (swDrawingViewTypes_e)BaseObject.Type;
        public ISingleDrawingComponent RootDrawingComponent
        {
            get {
                DrawingComponent i = BaseObject.RootDrawingComponent;
                return i != null ? new SingleDrawingComponent(i) : null;
            }
        }
        /// <summary>
        /// Gets the document referenced by this drawing view. 
        /// </summary>
        public ISingleModelDoc ReferencedDocument
        {
            get {
                ModelDoc2 t = BaseObject.ReferencedDocument;
                if (t != null)
                {
                    return ((SingleSldWorks)SingleSldWorks.GetSolidworks).ConvertDocument(t);
                }

                return null;
            }
        }

        public bool IsFlatPatternView => BaseObject.IsFlatPatternView();

        public IList<ISingleBody> GetBodies
        {
            get
            {
                IList<ISingleBody> bods = new List<ISingleBody>();
                object[] objs = BaseObject.Bodies as object[];
                if (objs == null) return bods;
                foreach (IBody2 bod in objs)
                {
                    
                    bods.Add(new SingleBody(bod));
                }

                return bods;
            }
        }
    }
}
