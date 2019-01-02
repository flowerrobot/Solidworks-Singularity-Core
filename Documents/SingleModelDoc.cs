using SingularityBase;
using SingularityBase.Events;
using SingularityBase.Managers;
using SingularityCore;
using SingularityCore.Managers;
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
            docProp = new SingleCustomPropertyManager(this, null);

            return docProp;
        }

        public ISingleSldWorks SldWorks => SingleSldWorks.GetSolidworks;

        public IFeature GetFirstFeature => ModelDoc.FirstFeature();

        public IEnumerable<IFeature> GetFeatures
        {
            get {
                List<IFeature> lst = new List<IFeature>();
                var feat = GetFirstFeature;
                while(feat != null)
                {
                    lst.Add(feat);
                    feat = feat.GetNextFeature();
                }
                return lst;
            }
        }

        public ISingleModelView ActiveView => new SingleModelView(this);


        public IFeature GetNextFeature(IFeature next)
        {
            return next.GetNextFeature();
        }

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

        public bool EditSuppress(IEnumerable<IFeature> features)
        {
            ClearSelection();
            foreach (IFeature fea in features)
            {
                fea.Select2(true, -1);
            }
            return ModelDoc.EditSuppress2();
        }

        public bool ForceRebuild(bool TopOnly)
        {
            return ModelDoc.ForceRebuild3(TopOnly);
        }

        #region Events




        public event AddCustomPropertyEventHandler AddCustomProperty;
        public event ChangeCustomPropertyEventHandler ChangeCustomProperty;
        public event DeleteCustomPropertyEventHandler DeleteCustomProperty;
        public event RegenPostNotifyEventHandler RegenPostNotify;
        public event FileSavePreNotify SavePreNotify;
        public event FileSavePostNotify SavePostNotify;
        public event FileSaveCancelledNotify SaveCancelledNotify;
        public event FileSaveAsPreNotify SaveAsPreNotify;

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
            EventSupressor ev = new EventSupressor();
            //   if (level == EventLevel.ThisDocument)
            ev.Owner = this;
            Disables.Add(ev.Id, ev);
            return ev;
        }
        public Dictionary<int, IEventSupressor> Disables { get; } = new Dictionary<int, IEventSupressor>();





        #endregion
        #region CustProperties

        internal int AddCustProp(string propName, string Configuration, string Value, Int32 valueType)
        {

            if (AddCustomProperty != null && CanRaiseEvent)
                return AddCustomProperty(propName, Configuration, Value, valueType);
            return 1;
        }
        internal int ChangeCustProp(string propName, string Configuration, string oldValue, string NewValue, Int32 valueType)
        {
            if (ChangeCustomProperty != null && CanRaiseEvent)
                return ChangeCustomProperty?.Invoke(propName, Configuration, oldValue, NewValue, valueType) ?? 1;
            return 1;
        }
        internal int DeleteCustProp(string propName, string Configuration, string Value, Int32 valueType)
        {
            if (DeleteCustomProperty != null && CanRaiseEvent)
                return DeleteCustomProperty(propName, Configuration, Value, valueType);
            return 1;
        }
        #endregion
        #region Saving
        internal int SaveAsPre(string filename)
        {
            if (SaveAsPreNotify != null & CanRaiseEvent)
                return SaveAsPre(filename);
            return 1;
        }
        internal int SavePre(string filename)
        {
            if (SavePreNotify != null & CanRaiseEvent)
                return SavePreNotify(filename);
            return 1;
        }
        internal int SavePost(int savetype, string filename)
        {
            if (SavePostNotify != null & CanRaiseEvent)
                return SavePostNotify(savetype, filename);
            return 1;
        }
        internal int SaveCancelled()
        {
            if (SaveCancelledNotify != null & CanRaiseEvent)
                return SaveCancelledNotify();
            return 1;
        }


        #endregion

        public int RegenPostNotify2(object stopFeature)
        {
            if (stopFeature == null && RegenPostNotify != null && CanRaiseEvent)
                return RegenPostNotify();
            return 1;
        }
        public int RegenPostNotify3()
        {
            if (RegenPostNotify != null && CanRaiseEvent)
                return RegenPostNotify();
            return 1;
        }




        #endregion
    }
}
