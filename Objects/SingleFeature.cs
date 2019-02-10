using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace SingularityCore
{
    internal class SingleFeature : SingularityObject<IFeature>,ISingleFeature
    {
        public SingleFeature(ISingleModelDoc doc, IFeature fea): base(fea)
        {
            Document = doc;
        }

        public string Name{get => BaseObject.Name;set => BaseObject.Name = value;}
        public ISingleModelDoc Document { get; }
        public int Id => BaseObject.GetID();

        private FeatureName _getTypeName = FeatureName.None;
        public FeatureName GetTypeName
        {
            get
            {
                if (_getTypeName != FeatureName.None) return _getTypeName;
                _getTypeName = FeatureNameHelpers.ConvertStringToEnum(BaseObject.GetTypeName());
                return _getTypeName;
            }
        }

        public ISingleFeature GetNextFeature
        {
            get
            {
                var fe = BaseObject.GetNextFeature() as IFeature;
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
                       return new SingularityObject<object>(BaseObject.GetSpecificFeature2());
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
                        
                IFeature fea = BaseObject.GetFirstSubFeature() as IFeature;
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
        public swVisibilityState_e Visible { get=> (swVisibilityState_e)BaseObject.Visible;  }

        public IList<ISingleBody> GetBodies
        {
            get
            {
                IList<ISingleBody> bodies = new List<ISingleBody>();
                foreach (ISingleFace face in GetFaces)
                {
                    var bdy = face.Body;
                    if (!bodies.Contains(bdy))
                    {
                        bodies.Add(bdy);
                    }
                }

                return bodies;
            }
        }

        public IList<ISingleFace> GetFaces
        {
            get
            {
                IList<ISingleFace> faces = new List<ISingleFace>();
                object fc = BaseObject.GetFaces();
                if (fc == null) return faces;
                foreach (IFace2 face in (object[])fc)
                {
                    faces.Add(new SingleFace(this,face));
                }

                return faces;
            }
        }

        public bool Select(int mark) => BaseObject.Select2(true, mark);
        public bool Select(bool append, int mark)=> BaseObject.Select2(true, mark);

        public bool DeSelect() => BaseObject.DeSelect();

        public override string ToString()
        {

            return Name + " " + GetTypeName.ConvertEnumToString();
        }

        public override bool Equals(object obj)
        {
            return Id == ((obj as ISingleFeature)?.Id ?? 0);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
