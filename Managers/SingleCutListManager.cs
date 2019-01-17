using SingularityBase;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SingularityCore.Managers
{
    internal class SingleCutListManager : ISingleCutListManager
    {
        //public SingleCutListManager(ISinglePartDoc doc, IFeature feat)
        //{
        //    Document = doc;
        //    if (feat.GetTypeName2().Equals(FeatureName.SolidBodyFolder.ToString(), StringComparison.CurrentCultureIgnoreCase))
        //    {
        //        _cutFeature = feat;
        //    }
        //}
        public SingleCutListManager(ISinglePartDoc doc)
        {
            Document = doc;
        }
        public ISinglePartDoc Document { get; }
#if obsolete

        private IFeature _cutFeature;
        public IFeature CutFeature
        {
            get {
                if (_cutFeature != null) return _cutFeature;
                foreach (IFeature fea in Document.GetFeatures)
                {
                    if (fea.GetTypeName2().Equals(FeatureName.SolidBodyFolder.ToString(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        _cutFeature = fea;
                        return _cutFeature;
                    }
                }
                return _cutFeature;
            }
        }
        public IBodyFolder CutFolder => CutFeature?.GetSpecificFeature2() as IBodyFolder;

        public bool AutomaticUpdate { get => CutFolder.GetAutomaticUpdate(); set => CutFolder.SetAutomaticUpdate(value); }
        public bool AutomaticCutList { get => CutFolder.GetAutomaticCutList(); set => CutFolder.SetAutomaticCutList(value); }
      
      

        [Obsolete("Configurations don't work with cutlists")]
        public ISingleConfiguration Configuration => throw new NotImplementedException();

        public ICustomPropertyManager CustomPropertyManager => CutFeature.CustomPropertyManager;
#endif
        public IEnumerable<ISingleCutListFolder> CutListFolders
        {
            get {
                List<ISingleCutListFolder> lst = new List<ISingleCutListFolder>();

                bool found = false;
                ISingleFeature feat = Document.FeatureManager.GetFirstFeature;

                while (feat != null)
                {
                    switch (feat.GetTypeName)
                    {
                        
                        case FeatureName.CutListFolder:
                            lst.Add((ISingleCutListFolder)feat.GetSpecificFeature);
                            found = true;
                            break;
                        default:
                            if (found) return lst;
                            break;
                    }

                    feat = feat.GetNextFeature;
                }
                return lst;
            }
        }

        public int CutListCount => ((List<ISingleCutListFolder>)CutListFolders).Count;


        public bool UpdateCutList() => UpdateCutList(false);
        public bool UpdateCutList(bool RebuildFlatPattern)
        {
            if (RebuildFlatPattern)
            {

                using (IDisposable a = Document.ActiveView.DisableGraphicsUpdate)
                {
                    foreach (IFeature fea in Document.FeatureManager.GetFeatures(false))
                    {
                        if (fea.GetTypeName2().Equals(FeatureName.FlatPattern.ToString(), StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (Document.EditUnsuppress(new[] { fea }))
                            {
                                Document.ForceRebuild(true);
                                Document.EditSuppress(new[] { fea });
                            }
                        }
                    }
                }

            }
            bool res = true;
            foreach (ISingleCutListFolder var in CutListFolders)
            {
                res = res && var.UpdateCutList();
            }

            return res;
        }
    }
}
