using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Collections.Generic;

namespace SingularityCore
{
    internal sealed class SingleDrawingDoc : SingleModelDoc, ISingleDrawingDoc
    {



        public DrawingDoc Document { get; }
        public ISingleView GetFirstView => new SingleView(this, (IView)Document.GetFirstView());
        public ISingleView GetNextView(ISingleView view) => view.GetNextView;

        public IList<ISingleView> Views
        {
            get {
                IList<ISingleView> views = new List<ISingleView>();

                ISingleView view = GetFirstView;
                while (view != null)
                {
                    views.Add(view);
                    view = view.GetNextView;
                }

                return views;
            }
        }






        public override swDocumentTypes_e Type => swDocumentTypes_e.swDocDRAWING;

        internal SingleDrawingDoc(DrawingDoc doc) : base((ModelDoc2)doc)
        {
            Document = doc;

            Document.RegenPostNotify += RegenPostNotify3;
            Document.RegenNotify += Document_RegenPreNotify;

            Document.AddCustomPropertyNotify += AddCustProp;
            Document.ChangeCustomPropertyNotify += ChangeCustProp;
            Document.DeleteCustomPropertyNotify += DeleteCustProp;

            Document.FileSavePostNotify += SavePost;
            Document.FileSaveNotify += SavePre;
            Document.FileSavePostCancelNotify += SaveCancelled;
            Document.FileSaveAsNotify2 += SaveAsPre;


            Document.UserSelectionPostNotify += Document_UserSelectionPostNotify;
            Document.UserSelectionPreNotify += DocumentOnUserSelectionPreNotify;
            Document.ClearSelectionsNotify += DocumentOnClearSelectionsNotify;
            Document.NewSelectionNotify += DocumentOnNewSelectionNotify;
            Document.DeleteSelectionPreNotify += DocumentOnDeleteSelectionPreNotify;

            Document.ModifyTableNotify += DocumentOnModifyTableNotify;
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


            Document.UserSelectionPostNotify -= Document_UserSelectionPostNotify;
            Document.UserSelectionPreNotify -= DocumentOnUserSelectionPreNotify;
            Document.ClearSelectionsNotify -= DocumentOnClearSelectionsNotify;
            Document.NewSelectionNotify -= DocumentOnNewSelectionNotify;
            Document.DeleteSelectionPreNotify -= DocumentOnDeleteSelectionPreNotify;

            Document.ModifyTableNotify -= DocumentOnModifyTableNotify;
        }


        public ISingleView CreateDrawViewFromModelView(ISinglePoint lowerLeft, string modelName, string viewName)
        {
           IView view =(IView)Document.CreateDrawViewFromModelView3(modelName, viewName, lowerLeft.X, lowerLeft.Y, lowerLeft.Z);
            return view != null ? new SingleView(this, view) : null;
        }

        public ISingleView CreateFlatPatternViewFromModelView(ISinglePoint lowerLeft, string modelName, string configName, bool hideBendLines,
            bool flipView)
        {
            IView view = Document.CreateFlatPatternViewFromModelView3(modelName, configName, lowerLeft.X, lowerLeft.Y, lowerLeft.Z,hideBendLines, flipView);
            return view != null ? new SingleView(this, view) : null;
        }

        public ISingleView CreateViewport(ISinglePoint lowerLeft, int size, double scale)
        {
            IView view = Document.CreateViewport3(lowerLeft.X, lowerLeft.Y, (short)size, scale);
            return view != null ? new SingleView(this, view) : null;
        }

        public ISingleView CreateAuxiliaryView(ISinglePoint lowerLeft, bool aligned, string label, bool showArrow, bool flipView)
        {
             IView view =(IView)Document.CreateAuxiliaryViewAt2(lowerLeft.X, lowerLeft.Y, lowerLeft.Z, !aligned, label, showArrow, flipView);
            return view != null ? new SingleView(this, view) : null;
        }

        public ISingleView CreateUnfoldedView(ISinglePoint lowerLeft, bool aligned)
        {
             IView view =Document.CreateUnfoldedViewAt3(lowerLeft.X, lowerLeft.Y, lowerLeft.Z, !aligned);
            return view != null ? new SingleView(this, view) : null;
        }

        public ISingleView CreateRelativeView(ISinglePoint lowerLeft, ISingleModelDoc doc, swRelativeViewCreationDirection_e front, swRelativeViewCreationDirection_e right)
        {
            IView view = Document.CreateRelativeView(doc.FileName, lowerLeft.X, lowerLeft.Y, (int)front, (int)right);
            return view != null ? new SingleView(this, view) : null;
        }

        public string ActiveSheet { get => Document.IGetCurrentSheet().GetName(); set => Document.ActivateSheet(value); }
    }
}
