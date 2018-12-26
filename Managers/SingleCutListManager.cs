using SingularityBase;
using SingularityBase.Managers;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;

namespace SingularityCore.Managers
{
    internal class SingleCutListManager : ISingleCutListManager
    {
        public SingleCutListManager(ISinglePartDoc doc, IFeature feat)
        {
            Document = doc;
            if (feat.GetTypeName2().Equals(FeatureNameEnum.FolderFeaturesEnum.TnSolidBodyFolder, StringComparison.CurrentCultureIgnoreCase))
            {
                _cutFeature = feat;
            }
        }
        public SingleCutListManager(ISinglePartDoc doc)
        {
            Document = doc;
        }



        private IFeature _cutFeature;
        public IFeature CutFeature
        {
            get {
                if (_cutFeature != null) return _cutFeature;
                foreach (IFeature fea in Document.GetFeatures)
                {
                    if (fea.GetTypeName2().Equals(FeatureNameEnum.FolderFeaturesEnum.TnSolidBodyFolder, StringComparison.CurrentCultureIgnoreCase))
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

        public ISinglePartDoc Document { get; }

        [Obsolete("Configurations don't work with cutlists")]
        public ISingleConfiguration Configuration => throw new NotImplementedException();

        public ICustomPropertyManager CustomPropertyManager => CutFeature.CustomPropertyManager;

        public IEnumerable<ISingleCutListFolder> CutListFolders
        {
            get {
                List<ISingleCutListFolder> lst = new List<ISingleCutListFolder>();
                IFeature fea = (IFeature)CutFeature?.GetFirstSubFeature() ?? null;
                while (fea != null)
                {
                    if (fea.GetTypeName2().Equals(FeatureNameEnum.FolderFeaturesEnum.TnCutListFolder, StringComparison.CurrentCultureIgnoreCase))
                    {
                        lst.Add(new SingleCutListFolder(Document, fea));
                    }
                    fea = CutFeature.GetNextSubFeature();
                }
                return lst;
            }
        }


        public bool UpdateCutList() => UpdateCutList(false);

        public bool UpdateCutList(bool RebuildFlatPattern)
        {
            if (RebuildFlatPattern)
            {

                using (IDisposable a = Document.ActiveView.DisableGraphicsUpdate)
                {
                    foreach (IFeature fea in Document.GetFeatures)
                    {
                        if (fea.GetTypeName2().Equals(FeatureNameEnum.SheetMetalFeaturesEnum.TnFlatPattern, StringComparison.CurrentCultureIgnoreCase))
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
            return CutFolder?.UpdateCutList() ?? false;
        }
    }
}
