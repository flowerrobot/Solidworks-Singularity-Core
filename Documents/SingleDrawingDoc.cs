using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SingularityBase;
using SingularityBase.Interfaces;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace SingleCore.Events
{
    public sealed class SingleDrawingDoc : SingleModelDoc, ISingleDrawingDoc
    {



        public new DrawingDoc Document { get; }
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
