using SingularityBase.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace SingularityCore.Managers
{
    //TODO finish implementation
    internal class SingleCutListFolder : ISingleCutListFolder
    {
        public SingleCutListFolder(ISinglePartDoc doc, IFeature feat)
        {
            Document = doc;
            Feature = feat;            
        }
        public string Name => Feature.Name;

        public IFeature Feature { get; }
        public ISinglePartDoc Document { get; }

        [Obsolete("By Product on inheritance")]
        public ISingleConfiguration Configuration => throw new NotImplementedException();

        public ICustomPropertyManager CustomPropertyManager => Feature.CustomPropertyManager;

        public IBodyFolder[] GetBodies => CutFolder.GetBodies();

        public swCutListType_e Type => (swCutListType_e)CutFolder.GetCutListType();

        public BodyFolder CutFolder => Feature.GetSpecificFeature2();

        ISingleModelDoc ISingleCustomPropertyManager.Document => throw new NotImplementedException();

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
    }
}
