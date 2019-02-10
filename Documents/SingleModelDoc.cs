using SingularityBase;
using SingularityBase.Events;
using SingularityCore.Events;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SingularityCore
{
    internal abstract class SingleModelDoc : ISingleModelDoc
    {
        public abstract void Dispose();


        internal SingleModelDoc(IModelDoc2 doc)
        {
            ModelDoc = doc;
        }


        public IModelDoc2 ModelDoc { get; }
        public abstract swDocumentTypes_e Type { get; }

        private ISingleCustomPropertyManager docProp;


        public ISingleCustomPropertyManager CustomPropertyManager()
        {

            if (docProp != null) return docProp;
            docProp = new SingleCustomPropertyManager(this);

            return docProp;
        }


        private ISingleFeatureManagers _featureManager;
        public ISingleFeatureManagers FeatureManager => _featureManager ?? (_featureManager = new SingleFeatureManager(this));


        private ISingleSelectionManager _selectionManager;
        public ISingleSelectionManager SelectionManager => _selectionManager ?? (_selectionManager = new SingleSelectionManager((ISelectionMgr)ModelDoc.SelectionManager));


        public ISingleSldWorks SldWorks => SingleSldWorks.GetSolidworks;

        public ISingleModelView ActiveView => new SingleModelView(this);


        public void ClearSelection()
        {
            ModelDoc.ClearSelection2(true);
        }

        public bool EditUnsuppress(IEnumerable<IFeature> features)
        {
            ClearSelection();
            foreach (IFeature fea in features)
            {
                fea.Select2(true, -1);
            }
            return ModelDoc.EditUnsuppress2();
        }
        public bool EditSuppress(IEnumerable<ISingleFeature> features)
        {
            ClearSelection();
            foreach (IFeature fea in features)
            {
                fea.Select2(true, -1);
            }
            return ModelDoc.EditSuppress2();
        }
        public bool EditUnsuppress(IEnumerable<ISingleFeature> features)
        {
            ClearSelection();
            foreach (ISingleFeature fea in features)
                fea.Select(true, -1);
            return ModelDoc.EditUnsuppress2();
        }
        public bool EditSuppress(IEnumerable<IFeature> features)
        {
            ClearSelection();
            foreach (ISingleFeature fea in features)
                fea.Select(true, -1);
            return ModelDoc.EditSuppress2();
        }
        public bool EditUnsuppress() => ModelDoc.EditUnsuppress2();
        public bool EditSuppress() => ModelDoc.EditSuppress2();

        public bool ForceRebuild(bool TopOnly) => ModelDoc.ForceRebuild3(TopOnly);

        public swActivateDocError_e ActivateDoc(bool useUserPreferences = true, swRebuildOnActivation_e option = swRebuildOnActivation_e.swDontRebuildActiveDoc)
        {
            return SldWorks.ActivateDoc(this, useUserPreferences, option);
        }

        public string FileName => System.IO.Path.GetFileName(ModelDoc.GetPathName());
        public string FullFileName => ModelDoc.GetPathName();
        public string FileDirectory => System.IO.Path.GetDirectoryName(ModelDoc.GetPathName());

        private ITableManager _tableMgr;
        public ITableManager Tables => _tableMgr ?? (_tableMgr = new TableManager(this));
        public ISingleMathUtility MathUtility => SldWorks.MathUtility;

        #region Events




        public event AddCustomPropertyEventHandler CustomPropertyAdded;
        public event ChangeCustomPropertyEventHandler CustomPropertyChanged;
        public event DeleteCustomPropertyEventHandler CustomPropertyDeleted;
        public event RegenPeNotifyHandler RegenPreNotify;
        public event RegenPostNotifyHandler RegenPostNotify;
        public event UserSelectionPostNotifyHandler UserSelectionPostNotify;
        public event UserSelectionPreNotifyHandler UserSelectionPreNotify;
        public event ClearSelectionsNotifyHandler ClearSelectionsNotify;
        public event NewSelectionPostNotifyHandler NewSelectionPostNotify;
        public event DeleteSelectionPreNotifyHandler DeleteSelectionPreNotify;

        public event FileSavePreNotify SavePreNotify;
        public event FileSavePostNotify SavePostNotify;
        public event FileSaveCancelledNotify SaveCancelledNotify;
        public event FileSaveAsPreNotify SaveAsPreNotify;
        public event ModifyTableNotifyHandler ModifyTableNotify;

        #region  EventSuprression

        public bool CanRaiseEvent

        {
            get {
                //Checks if any blocks are done at the global level, then checks at the model level
                return Disables.Values.Any(e => !e.IsDisposed);
                //  return EventManager.AllEventsSupressors.Any(item => !item.IsDisposed && (item.Level == EventLevel.AllDocuments || item.Level == EventLevel.All)) ||
                // Disables.Any(item => !item.IsDisposed && item.Level == EventLevel.ThisDocument);
            }

        }

        public IEventSupressor DisableEvents()
        {
            //EventSupressor ev = new EventSupressor();
            //   if (level == EventLevel.ThisDocument)
            //ev.Owner = this;
            //Disables.Add(ev.Id, ev);
            //return ev;
            return null;
        }
        public Dictionary<int, IEventSupressor> Disables { get; } = new Dictionary<int, IEventSupressor>();





        #endregion
        #region CustProperties

        internal int AddCustProp(string propName, string Configuration, string Value, Int32 valueType)
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (CustomPropertyAdded != null && CanRaiseEvent)
                    return (int)CustomPropertyAdded(this, propName, Configuration, Value, valueType);
                return 1;
            }
        }

        internal int ChangeCustProp(string propName, string Configuration, string oldValue, string NewValue, Int32 valueType)
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (CustomPropertyChanged != null && CanRaiseEvent)
                    return (int)CustomPropertyChanged.Invoke(this, propName, Configuration, oldValue, NewValue,
                        valueType);
                return 1;
            }
        }

        internal int DeleteCustProp(string propName, string Configuration, string Value, Int32 valueType)
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (CustomPropertyDeleted != null && CanRaiseEvent)
                    return (int)CustomPropertyDeleted(this, propName, Configuration, Value, valueType);
                return 1;
            }
        }
        #endregion
        #region Saving
        internal int SaveAsPre(string filename)
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (SaveAsPreNotify != null & CanRaiseEvent)
                    return (int)SaveAsPreNotify(this, filename);
                return 1;
            }
        }
        internal int SavePre(string filename)
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (SavePreNotify != null & CanRaiseEvent)
                    return (int)SavePreNotify(this, filename);
                return 1;
            }
        }
        internal int SavePost(int savetype, string filename)
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (SavePostNotify != null & CanRaiseEvent)
                    return (int)SavePostNotify(this, savetype, filename);
                return 1;
            }
        }
        internal int SaveCancelled()
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (SaveCancelledNotify != null & CanRaiseEvent)
                    return (int)SaveCancelledNotify(this);
                return 1;
            }
        }


        #endregion

        internal int RegenPostNotify2(object stopFeature)
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (RegenPostNotify != null) return (int)RegenPostNotify.Invoke(this);
                return 1;
            }
        }
        internal int RegenPostNotify3()
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (RegenPostNotify != null) return (int)RegenPostNotify.Invoke(this);
            }
            return (int)EventResponse.Okay;


        }
        internal int Document_RegenPreNotify()
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (RegenPreNotify != null) return (int)RegenPreNotify.Invoke(this);
            }

            return 1;
        }
        #region Selection



        internal int DocumentOnDeleteSelectionPreNotify()
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (DeleteSelectionPreNotify != null) return (int)DeleteSelectionPreNotify.Invoke(this);
            }

            return 1;
        }

        internal int DocumentOnNewSelectionNotify()
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (NewSelectionPostNotify != null) return (int)NewSelectionPostNotify.Invoke(this);
            }

            return 1;
        }

        internal int DocumentOnClearSelectionsNotify()
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (ClearSelectionsNotify != null) return (int)ClearSelectionsNotify.Invoke(this);
            }

            return 1;
        }

        internal int DocumentOnUserSelectionPreNotify(int seltype)
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (UserSelectionPreNotify != null) return (int)UserSelectionPreNotify.Invoke(this, (swSelectType_e)seltype);
            }

            return 1;
        }

        internal int Document_UserSelectionPostNotify()
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (UserSelectionPostNotify != null) return (int)UserSelectionPostNotify.Invoke(this);
            }

            return 1;
        }



        #endregion

        internal int DocumentOnModifyTableNotify(TableAnnotation tableannotation, int tabletype, int reason, int rowinfo, int columninfo, string datainfo)
        {
            using (UserEvent ue = new UserEvent(null, EventType.AddinEventReaction, null))
            {
                if (ModifyTableNotify != null)
                {
                    ISingleTableAnnotation tbl = null;
                    switch ((swTableAnnotationType_e)tabletype)
                    {
                        case swTableAnnotationType_e.swTableAnnotation_BillOfMaterials:
                            tbl = new SingleBomTableAnnotation(this, (IBomTableAnnotation)tableannotation);
                            break;
                        case swTableAnnotationType_e.swTableAnnotation_General:

                            break;
                        case swTableAnnotationType_e.swTableAnnotation_WeldmentCutList:
                            tbl = new SingleWeldmentCutListAnnotation(this, (IWeldmentCutListAnnotation)tableannotation);
                            break;
                    }

                    if (tbl == null) return 1;

                    return (int)ModifyTableNotify.Invoke(this, tbl, (swTableAnnotationType_e)tabletype, (swModifyTableNotifyReason_e)reason, rowinfo, columninfo, datainfo);

                }
            }
            return 1;
        }

        #endregion
    }
}
