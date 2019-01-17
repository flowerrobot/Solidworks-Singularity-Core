using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace SingularityCore
{
    internal sealed class SingleDrawingDoc : SingleModelDoc, ISingleDrawingDoc
    {



        public  DrawingDoc Document { get; }
        public ISingleView GetFirstView => new SingleView(this, Document.GetFirstView());
        public ISingleView GetNextView(ISingleView view) => view.GetNextView;
        

        public override swDocumentTypes_e Type => swDocumentTypes_e.swDocDRAWING;

        internal SingleDrawingDoc(DrawingDoc doc) : base((ModelDoc2)doc)
        {
            Document = doc;

            Document.RegenPostNotify += RegenPostNotify3;
            Document.AddCustomPropertyNotify += AddCustProp;
            Document.ChangeCustomPropertyNotify += ChangeCustProp;
            Document.DeleteCustomPropertyNotify += DeleteCustProp;

            Document.FileSavePostNotify += SavePost;
            Document.FileSaveNotify += SavePre;
            Document.FileSavePostCancelNotify += SaveCancelled;
            Document.FileSaveAsNotify2 += SaveAsPre;
        }

        public override void Dispose()
        {
            
            Document.AddCustomPropertyNotify -= AddCustProp;
            Document.ChangeCustomPropertyNotify -= ChangeCustProp;
            Document.DeleteCustomPropertyNotify -= DeleteCustProp;

            Document.FileSavePostNotify -= SavePost;
            Document.FileSaveNotify -= SavePre;
            Document.FileSavePostCancelNotify -= SaveCancelled;
            Document.FileSaveAsNotify2 -= SaveAsPre;
        }



    }
}
