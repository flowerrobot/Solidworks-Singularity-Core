using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using SingularityBase;
using SolidWorks.Interop.sldworks;

namespace SingularityCore
{
    internal class SingleFeature : ISingleFeature, IDisposable
    {
        public SingleFeature(ISingleModelDoc doc, IFeature fea)
        {
            Document = doc;
            Feature = fea;
        }

        public string Name => Feature.Name;
        public ISingleModelDoc Document { get; }
        public int Id => Feature.GetID();
        public IFeature Feature { get; private set; }


        public FeatureName GetTypeName => FeatureNameHelpers.ConvertStringToEnum(Feature.GetTypeName());

        public ISingleFeature GetNextFeature
        {
            get
            {
                var fe = Feature.GetNextFeature();
                return fe == null ? null : new SingleFeature(Document,fe);
            }
        }

        public object GetSpecificFeature
        {
            get
            {
                switch (GetTypeName)
                {
                    case FeatureName.SolidBodyFolder:
                    case FeatureName.CutListFolder:
                        return new SingleCutListFolder(Document, this);
                    default:
                       return Feature.GetSpecificFeature2();
                }
            }
        }


        public ISingleFeature GetFirstSubFeature
        {
            get
            {
                //For some reason bodies & cutlist return 
                switch (GetTypeName)
                {
                    case FeatureName.SolidBodyFolder:
                    case FeatureName.CutListFolder:
                        return null;
                    default:
                        
                IFeature fea = Feature.GetFirstSubFeature();
                return fea != null ? new SingleFeature(Document, fea) : null;
                }
            }
        }

        public ISingleFeature GetNextSubFeature(ISingleFeature next) => next.GetNextFeature;


        public IEnumerable<ISingleFeature> GetSubFeatures 
        {
            get {
                List<ISingleFeature> lst = new List<ISingleFeature>();
                ISingleFeature feat = GetFirstSubFeature;
                while (feat != null)
                {
                    lst.Add(feat);
                    feat = feat.GetNextFeature;
                }
                return lst;
            }
        }
        public int GetSubFeaturesCount => ((List<ISingleFeature>) GetSubFeatures)?.Count ?? 0;

        public bool Select(int mark) => Feature.Select2(true, mark);
        public bool Select(bool append, int mark)=> Feature.Select2(true, mark);

        public bool DeSelect() => Feature.DeSelect();

        public override string ToString()
        {

            return Name + " " + GetTypeName.ConvertEnumToString();
        }

        public void Dispose()
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(Feature);
            Feature = null;
        }
    }
}
