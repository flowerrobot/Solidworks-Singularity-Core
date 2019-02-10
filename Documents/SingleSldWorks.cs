using SingularityBase;
using SingularityBase.Events;
using SingularityCore.Events;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swcommands;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SingularityCore
{
    internal class SingleSldWorks :SingularityObject<ISldWorks>, ISingleSldWorks, IDisposable
    {
        private Hashtable AllDocuments { get; } = new Hashtable();
        public SingleSldWorks(ISldWorks swapp, int addinId) : base(swapp)
        {
            GetSolidworks = this;
            AddinId = addinId;

            CommandManager = new SingularityObject<CommandManager>(Solidworks.GetCommandManager(AddinId)); 


            #region Define Events

            SldWorks sld = ((SldWorks)Solidworks);
            sld.ActiveDocChangeNotify += Sld_ActiveDocChangeNotify;
            sld.ActiveModelDocChangeNotify += Sld_ActiveModelDocChangeNotify;
            //sld.BackgroundProcessingStartNotify += Sld_BackgroundProcessingStartNotify;
            //sld.BackgroundProcessingEndNotify += Sld_BackgroundProcessingEndNotify;
            //sld.BeginRecordNotify += Sld_BeginRecordNotify;
            //sld.BeginTranslationNotify += Sld_BeginTranslationNotify;

            sld.CommandCloseNotify += Sld_CommandCloseNotify;
            sld.CommandOpenPreNotify += Sld_CommandOpenPreNotify;
            // sld.DestroyNotify += Sld_DestroyNotify;
            // sld.DocumentConversionNotify += Sld_DocumentConversionNotify;
            //sld.BeginTranslationNotify

            sld.DocumentLoadNotify += Sld_DocumentLoadNotify;
            //sld.EndRecordNotify += Sld_EndRecordNotify;
            // sld.EndTranslationNotify += Sld_EndTranslationNotify;
            sld.FileCloseNotify += Sld_FileCloseNotify;
            sld.FileNewNotify2 += Sld_FileNewNotify;

            sld.FileOpenNotify2 += Sld_FileOpenNotify;
            sld.FileOpenPostNotify += Sld_FileOpenPostNotify;
            sld.FileOpenPreNotify += Sld_FileOpenPreNotify;
            sld.OnIdleNotify += Sld_OnIdleNotify;
            sld.ReferencedFilePreNotify += Sld_ReferencedFilePreNotify;
            sld.ReferenceNotFoundNotify += Sld_ReferenceNotFoundNotify;
            #endregion
        }

        


        public static ISingleSldWorks GetSolidworks { get; private set; }

        /// <inheritdoc />
        public ISldWorks Solidworks => BaseObject;

        /// <inheritdoc />
        public int AddinId { get; }

        /// <inheritdoc />
        public ISingleBaseObject<CommandManager> CommandManager { get; private set; }

        private ISingleMathUtility _mathUtility;

        /// <inheritdoc />
        public ISingleMathUtility MathUtility => _mathUtility ?? (_mathUtility = new SingleMathUtility());

        /// <inheritdoc />
        public ISingleModelDoc ActiveDocument
        {
            get {
                if (Solidworks.ActiveDoc == null) return null;
                return ConvertDocument((IModelDoc2)Solidworks.ActiveDoc);
            }
            set => ActivateDoc(value, true, 0);
        }

        public IEnumerable<ISingleModelDoc> OpenDocuments
        {
            get {
                List<ISingleModelDoc> docs = new List<ISingleModelDoc>();
                foreach (IModelDoc2 doc in (IModelDoc2[])Solidworks.GetDocuments())
                {
                    docs.Add(ConvertDocument(doc));
                }

                return docs;
            }
        }

        /// <inheritdoc />
        public swActivateDocError_e ActivateDoc(ISingleModelDoc doc, bool useUserPreferences, swRebuildOnActivation_e option)
        {
            int err = 0;
            Solidworks.ActivateDoc3(doc.ModelDoc.GetPathName(), true, (int)option, ref err);
            return (swActivateDocError_e)err;
        }


        /// <inheritdoc />
        public ISingleModelDoc GetDocumentByName(string fileName)
        {
            if (AllDocuments.Contains(fileName)) return (ISingleModelDoc)AllDocuments[fileName];

            ModelDoc2 doc = Solidworks.IGetOpenDocumentByName2(fileName);

            if (doc == null) return null;

            ISingleModelDoc res = ConvertDocument(doc);

            //AllDocuments.Add(res.FullFileName, res);

            return res;

            //object docs = Solidworks.GetDocuments();
            //foreach (object doc in (object[])docs)
            //{
            //    if (System.IO.Path.GetFileName((doc as ModelDoc2)?.GetPathName() ?? "").Equals(fileName, StringComparison.CurrentCultureIgnoreCase))
            //        return ConvertDocument((ModelDoc2)doc);

            //}
            //return null;
        }

        /// <summary>
        /// Converts a document interface into  Singularity document
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        internal ISingleModelDoc ConvertDocument(IModelDoc2 doc)
        {
            if (doc == null) return null;

            if (AllDocuments.ContainsKey(doc.GetPathName())) return (ISingleModelDoc)AllDocuments[doc.GetPathName()];

            ISingleModelDoc newDoc;
            switch ((swDocumentTypes_e)((ModelDoc2)doc).GetType())
            {
                case swDocumentTypes_e.swDocASSEMBLY:
                    newDoc = new SingleAssemblyDoc((AssemblyDoc)doc);
                    break;
                case swDocumentTypes_e.swDocDRAWING:
                    newDoc = new SingleDrawingDoc((DrawingDoc)doc);
                    break;
                case swDocumentTypes_e.swDocPART:
                    newDoc = new SinglePartDoc((PartDoc)doc);
                    break;
                default:
                    return null;
            }
            AllDocuments.Add(newDoc.FullFileName, newDoc);
            return newDoc;
        }



        public new void Dispose()
        {
            #region Define Events

            SldWorks sld = ((SldWorks)Solidworks);
            sld.ActiveDocChangeNotify -= SldOnActiveDocChangeNotify;
            sld.ActiveModelDocChangeNotify -= Sld_ActiveModelDocChangeNotify;
            //sld.BackgroundProcessingStartNotify -= Sld_BackgroundProcessingStartNotify;
            //sld.BackgroundProcessingEndNotify -= Sld_BackgroundProcessingEndNotify;
            //sld.BeginRecordNotify -= Sld_BeginRecordNotify;
            //sld.BeginTranslationNotify -= Sld_BeginTranslationNotify;

            sld.CommandCloseNotify -= Sld_CommandCloseNotify;
            sld.CommandOpenPreNotify -= Sld_CommandOpenPreNotify;
            //sld.DestroyNotify -= Sld_DestroyNotify;
            //sld.DocumentConversionNotify -= Sld_DocumentConversionNotify;
            //sld.BeginTranslationNotify

            sld.DocumentLoadNotify2 -= Sld_DocumentLoadNotify;
            //sld.EndRecordNotify -= Sld_EndRecordNotify;
            //sld.EndTranslationNotify -= Sld_EndTranslationNotify;
            sld.FileCloseNotify -= Sld_FileCloseNotify;
            sld.FileNewNotify2 -= Sld_FileNewNotify;
            sld.FileOpenNotify2 -= Sld_FileOpenNotify;
            sld.FileOpenPostNotify -= Sld_FileOpenPostNotify;
            sld.FileOpenPreNotify -= Sld_FileOpenPreNotify;
            sld.OnIdleNotify -= Sld_OnIdleNotify;
            sld.ReferencedFilePreNotify -= Sld_ReferencedFilePreNotify;
            sld.ReferenceNotFoundNotify -= Sld_ReferenceNotFoundNotify;
            #endregion

            
           
        }

        #region Events



        public event ActiveDocChangeNotifyEventHandler ActiveDocChangeNotify;
        public event ActiveModelDocChangeNotifyEventHandler ActiveModelDocChange;
        public event CommandCloseNotifyEventHandler CommandClose;
        public event CommandOpenPreNotifyEventHandler CommandOpenPreNotify;
        public event DocumentLoadNotifyEventHandler DocumentLoad;
        public event FileCloseNotifyEventHandler FileClose;
        public event FileNewNotifyEventHandler FileNewNotify;
        public event FileNewPreNotifyEventHandler FileNewPreNotify;
        public event FileOpenNotifyEventHandler FileOpenNotify;
        public event FileOpenPostNotifyEventHandler FileOpenPostNotify;
        public event FileOpenPreNotifyEventHandler FileOpenPreNotify;
        public event ReferencedFilePreNotifyEventHandler ReferencedFilePreNotify;
        public event ReferenceNotFoundNotifyEventHandler ReferenceNotFoundNotify;
        public event OnIdleNotifyEventHandler OnIdleNotify;


        private int Sld_OnIdleNotify()
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (OnIdleNotify != null) return (int)OnIdleNotify.Invoke();
            }
            return (int)EventResponse.Okay;
        }

        private int Sld_ReferenceNotFoundNotify(string fileName)
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                IMissingDocumentEvent res = new MissingDocumentEvent(fileName, this);
                ReferenceNotFoundNotify?.Invoke(res);
                if (res.UseNewPath & !string.IsNullOrWhiteSpace(res.NewFileName))
                {
                    Solidworks.SetMissingReferencePathName(res.NewFileName);
                }
            }
            return (int)EventResponse.Okay;
        }

        private int Sld_ReferencedFilePreNotify(string fileName)
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (ReferencedFilePreNotify != null) return (int)ReferencedFilePreNotify?.Invoke(fileName);
            }
            return (int)EventResponse.Okay;
        }



        private int Sld_FileOpenPreNotify(string fileName)
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (FileOpenPreNotify != null) return (int)FileOpenPreNotify.Invoke(fileName);
            }
            return (int)EventResponse.Okay;
        }

        private int Sld_FileOpenPostNotify(string fileName)
        {

            if (!AllDocuments.Contains(fileName)) return (int)EventResponse.Okay;


            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (FileOpenPostNotify != null) return (int)FileOpenPostNotify.Invoke(new DocumentEvent(fileName, this));
            }

            return (int)EventResponse.Okay;
        }

        private int Sld_FileOpenNotify(string fileName)
        {

            //IModelDoc doc = Solidworks.GetOpenDocumentByName2() ;
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (FileOpenNotify != null) return (int)FileOpenNotify.Invoke(new DocumentEvent(fileName, this));
            }
            return (int)EventResponse.Okay;
        }

        private int Sld_FileNewNotify(object NewDoc, int DocType, string TemplateName)
        {
            ISingleModelDoc doc = NewDoc as ISingleModelDoc;
            if (doc == null) return (int)EventResponse.Okay;
            AllDocuments.Add(doc.FileName, doc);

            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (FileNewNotify != null) return (int)FileNewNotify.Invoke(new DocumentEvent(doc, this), TemplateName);
            }
            return (int)EventResponse.Okay;
        }

        private int Sld_FileCloseNotify(string FileName, int reason)
        {
            if (AllDocuments.ContainsKey(FileName)) AllDocuments.Remove(FileName);
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (FileClose != null) return (int)FileClose.Invoke(FileName, (swFileCloseNotifyReason_e)reason);
            }
            return (int)EventResponse.Okay;
        }

        private int Sld_DocumentLoadNotify(string docTitle, string docPath)
        {
            ISingleModelDoc aa = GetDocumentByName(docPath);
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (DocumentLoad != null) return (int)DocumentLoad.Invoke(new DocumentEvent(aa, this));
            }
            return (int)EventResponse.Okay;
        }

        private int Sld_ActiveDocChangeNotify()
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (ActiveDocChangeNotify != null) return (int)ActiveDocChangeNotify?.Invoke();
            }
            return (int)EventResponse.Okay;
        }

        private int Sld_CommandOpenPreNotify(int Command, int UserCommand)
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (CommandOpenPreNotify != null) return (int)CommandOpenPreNotify?.Invoke((swCommands_e)Command, UserCommand);
            }
            return (int)EventResponse.Okay;
        }

        private int Sld_CommandCloseNotify(int Command, int reason)
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (CommandClose != null) return (int)CommandClose?.Invoke((swCommands_e)Command, (swPropertyManagerPageCloseReasons_e)reason);
            }
            return (int)EventResponse.Okay;
        }

        private int Sld_ActiveModelDocChangeNotify()
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (ActiveModelDocChange != null) return (int)ActiveModelDocChange?.Invoke();
            }
            return (int)EventResponse.Okay;
        }

        private int SldOnActiveDocChangeNotify()
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (ActiveDocChangeNotify != null) return (int)ActiveDocChangeNotify?.Invoke();
            }
            return (int)EventResponse.Okay;
        }


        ///Not implemented

        private int Sld_DocumentConversionNotify(string FileName)
        {
            return 1;
            //using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            //{
            //    //return documentc?.Invoke() ?? 1;
            //}
        }

        private int Sld_DestroyNotify()
        {
            return 1;
            //using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            //{
            //    //return ?.Invoke() ?? 1;
            //}
        }

        private int Sld_EndTranslationNotify(string FileName, int Options)
        {
            return 1;
        }

        private int Sld_EndRecordNotify()
        {
            return 1;
        }

        private int Sld_BeginTranslationNotify(string FileName, int Options)
        {
            return 1;
        }

        private int Sld_BackgroundProcessingStartNotify(string FileName)
        {
            return 1;
        }

        private int Sld_BeginRecordNotify()
        {
            return 1;
        }

        private int Sld_BackgroundProcessingEndNotify(string FileName)
        {
            return 1;
        }
        #endregion

    }
}
