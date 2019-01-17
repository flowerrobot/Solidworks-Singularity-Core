using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SingularityBase;
using SingularityCore.Managers;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace SingularityCore
{
   
    internal class SingleCutListFolder : ISingleCutListFolder
    {
        public SingleCutListFolder(ISingleModelDoc doc, ISingleFeature feat)
        {
            Document = doc;
            Feature = feat;
        }
        public SingleCutListFolder(ISingleModelDoc doc, IFeature feat)
        {
            Document = doc;
            Feature = new SingleFeature(Document, feat);            
        }
        public string Name => Feature.Name;
        public ISingleFeature Feature { get; }
        public ISingleModelDoc Document { get; }


        public int GetBodyCount => CutFolder.GetBodyCount();

        [Obsolete("By Product on inheritance")]
        public ISingleConfiguration Configuration => throw new NotImplementedException();

        

        public IEnumerable<ISingleBody> GetBodies => (ISingleBody[])CutFolder.GetBodies();

        public swBodyFolderFeatureType_e Type => (swBodyFolderFeatureType_e) CutFolder.Type;
        public swCutListType_e CutListType => (swCutListType_e)CutFolder.GetCutListType();

        public bool AutomaticCutList { get=> CutFolder.GetAutomaticCutList(); set=> CutFolder.SetAutomaticCutList(value); }
        public bool AutomaticUpdate { get => CutFolder.GetAutomaticUpdate(); set=> CutFolder.SetAutomaticCutList(value); }
        public bool UpdateCutList() => CutFolder.UpdateCutList();
        

        public IBodyFolder CutFolder => Feature.Feature.GetSpecificFeature2();

        ISingleModelDoc ISingleCustomPropertyManager.Document => Document;

        public override string ToString() => Name + " " + Type.ToString() + " " + CutListType.ToString();

        


        #region CustomProperties

        public ICustomPropertyManager CustomPropertyManager => Feature.Feature.CustomPropertyManager;

        

        public swCustomInfoAddResult_e Add(string Name, swCustomInfoType_e Type, string Value, int OverwriteExisting)
        {
            return (swCustomInfoAddResult_e)CustomPropertyManager.Add3(Name, (int)Type, Value, OverwriteExisting);
        }

        public swCustomInfoDeleteResult_e Delete(string Name)
        {
            return (swCustomInfoDeleteResult_e)CustomPropertyManager.Delete2(Name);
        }

        public swCustomInfoDeleteResult_e DeleteAll()
        {
            swCustomInfoDeleteResult_e res = swCustomInfoDeleteResult_e.swCustomInfoDeleteResult_NotPresent;
            foreach(string name  in CustomPropertyManager.GetNames())
            {
                res = (swCustomInfoDeleteResult_e)CustomPropertyManager.Delete2(Name);
            }
            return res;            
        }

        public IEnumerable<ISingleCustomProperty> GetAll()
        {
            List<ISingleCustomProperty> res = new List<ISingleCustomProperty>();
            foreach (string name in CustomPropertyManager.GetNames())
            {
                res.Add(new SingleCustomProperty(name, this, CustomPropertyType.Cutlist));
            }
            return res;
        }

        public ISingleCustomProperty GetProperty(string Name)
        {
            if(((string[])CustomPropertyManager.GetNames()).Any(t => t.Equals(Name, StringComparison.CurrentCultureIgnoreCase)))
            {
                return new SingleCustomProperty(Name, this, CustomPropertyType.Cutlist);
            }
            return null;
        }

        public swCustomInfoSetResult_e Set(string Name, string Value)
        {
            return (swCustomInfoSetResult_e)CustomPropertyManager.Set2(Name, Value);
        }

        #endregion
    }
}
