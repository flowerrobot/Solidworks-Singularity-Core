using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Collections.Generic;

namespace SingularityCore
{

    internal class SingleCutListFolder : SingleCustomPropertyManager, ISingleCutListFolder
    {
        public ISingleFeature Feature { get; }
        public new IBodyFolder BaseObject { get; private set; }

        public SingleCutListFolder(ISingleModelDoc doc, ISingleFeature feat) : base(doc, feat.BaseObject.CustomPropertyManager)
        {
            Feature = feat;
            BaseObject = (IBodyFolder)feat.BaseObject.GetSpecificFeature2();
        }
        public SingleCutListFolder(ISingleModelDoc doc, IFeature feat) : this(doc, new SingleFeature(doc, feat)) { }

        public string Name { get => Feature.Name; set => Feature.Name = value; }
        public int Id => Feature.Id;
        public int GetBodyCount => BaseObject.GetBodyCount();




        public IList<ISingleBody> GetBodies
        {
            get
            {
                IList<ISingleBody> bods = new List<ISingleBody>();
                object[] bodies = (object[])BaseObject.GetBodies();

                foreach (IBody2 bod in bodies)
                {
                    bods.Add(new SingleBody(Document, bod));
                }

                return bods;
            }
        }

        public swBodyFolderFeatureType_e Type => (swBodyFolderFeatureType_e)BaseObject.Type;
        public swCutListType_e CutListType => (swCutListType_e)BaseObject.GetCutListType();

        public bool AutomaticCutList { get => BaseObject.GetAutomaticCutList(); set => BaseObject.SetAutomaticCutList(value); }
        public bool AutomaticUpdate { get => BaseObject.GetAutomaticUpdate(); set => BaseObject.SetAutomaticCutList(value); }
        public bool UpdateCutList() => BaseObject.UpdateCutList();




        ISingleModelDoc ISingleCustomPropertyManager.Document => Document;

        public override string ToString() => Name + " " + Type.ToString() + " " + CutListType.ToString();
        public new void Dispose()
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(BaseObject);
            BaseObject = null;
            base.Dispose();
        }

        public override bool Equals(object obj)
        {
            return ((obj as ISingleCutListFolder)?.Id ?? 0) == Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }

}
