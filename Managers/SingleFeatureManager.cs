using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;

namespace SingularityCore.Managers
{
    internal class SingleFeatureManager : ISingleFeatureManagers
    {
        public SingleFeatureManager(ISingleModelDoc document)
        {
            Document = document;
            FeatureManager = (IFeatureManager)Document.ModelDoc.FeatureManager;
        }

        public ISingleModelDoc Document { get; }

        public IFeatureManager FeatureManager { get; }


        public ISingleFeature GetFirstFeature => new SingleFeature(Document, Document.ModelDoc.FirstFeature());
        //public ISingleFeature GetNextFeature(ISingleFeature next) => next.GetNextFeature;
        public IEnumerable<ISingleFeature> GetFeatures(bool topLevelOnly)
        {
            List<ISingleFeature> lst = new List<ISingleFeature>();
            foreach (dynamic fea in FeatureManager.GetFeatures(topLevelOnly))
            {
                lst.Add(new SingleFeature(Document, (IFeature)fea));
            }
            return lst;
        }




        public int FeatureCount => FeatureManager.GetFeatureCount(true);
        public IDisposable DisableFeatureTree()
        {
            IDisposable f = new SettingRest<IFeatureManager, bool>(FeatureManager, "EnableFeatureTree", true);
            FeatureManager.EnableFeatureTree = false;
            return f;
        }

        public IDisposable DisableFeatureTreeWindow()
        {
            IDisposable f = new SettingRest<IFeatureManager, bool>(FeatureManager, "EnableFeatureTreeWindow", true);
            FeatureManager.EnableFeatureTreeWindow = false;
            return f;
        }

        public IFlatPatternFolder GetFlatPatternFolder => FeatureManager.GetFlatPatternFolder();
        public ISheetMetalFolder GetSheetMetalFolder => FeatureManager.GetSheetMetalFolder();


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

    internal class SettingRest<TAttribute, TValue> : IDisposable
    {
        public SettingRest(TAttribute att, string propertyName, TValue value)
        {
            Att = att;
            Value = value;
            PropertyName = propertyName;
        }

        private TAttribute Att { get; }
        private TValue Value { get; }
        private string PropertyName { get; }
        public void Dispose()
        {
            Att.GetType().GetProperty(PropertyName)?.SetValue(Att, Value, null);
            //PropertyInfo property = typeof(Att).GetProperty("Reference");

            //property.SetValue(myAccount, "...", null);
        }
    }
}
