using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;

namespace SingularityCore
{
    internal class SingleFeatureManager :SingularityObject<IFeatureManager>, ISingleFeatureManagers
    {
        public SingleFeatureManager(ISingleModelDoc document) : base((IFeatureManager)document.ModelDoc.FeatureManager)
        {
            Document = document;
        }

        public ISingleModelDoc Document { get; }

        


        public ISingleFeature GetFirstFeature => new SingleFeature(Document, (IFeature)Document.ModelDoc.FirstFeature());
        //public ISingleFeature GetNextFeature(ISingleFeature next) => next.GetNextFeature;
        public IEnumerable<ISingleFeature> GetFeatures(bool topLevelOnly)
        {
            List<ISingleFeature> lst = new List<ISingleFeature>();
            foreach (object fea in (object[])BaseObject.GetFeatures(topLevelOnly))
            {
                lst.Add(new SingleFeature(Document, (IFeature)fea));
            }
            return lst;
        }




        public int FeatureCount => BaseObject.GetFeatureCount(true);
        public IDisposable DisableFeatureTree()
        {
            IDisposable f = new SettingRest<IFeatureManager, bool>(BaseObject, "EnableFeatureTree", true);
            BaseObject.EnableFeatureTree = false;
            return f;
        }

        public IDisposable DisableFeatureTreeWindow()
        {
            IDisposable f = new SettingRest<IFeatureManager, bool>(BaseObject, "EnableFeatureTreeWindow", true);
            BaseObject.EnableFeatureTreeWindow = false;
            return f;
        }


        public ISingleFlatPatternFolder GetFlatPatternFolder => new SingleFlatPatternFolder((ISinglePartDoc)Document,(IFlatPatternFolder)BaseObject.GetFlatPatternFolder());
        public ISingleBaseObject<ISheetMetalFolder> GetSheetMetalFolder => new SingularityObject<ISheetMetalFolder>( BaseObject.GetSheetMetalFolder());


        //TODO implement these
        public ISingleFeature InsertMacroFeature(string baseName, string ProgId, object MacroMethods, object ParamNames,
            object ParamTypes, object ParamValues, object DimTypes, object DimValues, object EditBodies, object IconFiles,
            swMacroFeatureOptions_e Options)
        {
            throw new NotImplementedException();
        }


        public bool ShowFeatureName { get; set; }
        public bool ShowFeatureDescription { get; set; }
        public bool ShowComponentNames { get; set; }
        public bool ShowComponentDescriptions { get; set; }
        public bool ShowComponentConfigurationNames { get; set; }
        public bool ShowComponentConfigurationDescriptions { get; set; }
        public bool MoveSizeFeatures { get; set; }
        public FeatureStatistics FeatureStatistics { get; }
        public bool ViewDependencies { get; set; }
        public bool ViewFeatures { get; set; }
        public bool ShowHierarchyOnly { get; set; }
        public bool ShowFeatureDetails { get; set; }
        public bool ShowDisplayStateNames { get; set; }
        public bool SolidForTrim { get; set; }
        public bool GroupComponentInstances { get; set; }
    }

    
}
