using SingularityBase;
using SingularityBase.Managers;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SingularityCore.Managers
{
    internal class SingleCustomPropertyManager : ISingleCustomPropertyManager
    {
        public SingleCustomPropertyManager(ISingleModelDoc doc, ISingleConfiguration configuration)
        {
            Document = doc;
            Configuration = configuration;
        }
        public ISingleModelDoc Document { get; private set; }

        public ISingleConfiguration Configuration { get; private set; }
        ICustomPropertyManager _customPropertyManager;
        public ICustomPropertyManager CustomPropertyManager
        {
            get {
                if (_customPropertyManager != null) return _customPropertyManager;
                if (Configuration == null)
                    _customPropertyManager = Document.ModelDoc.Extension.CustomPropertyManager[""];
                else
                    _customPropertyManager = Document.ModelDoc.Extension.CustomPropertyManager[Configuration.ConfigName];
                return _customPropertyManager;
            }
        }

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
            swCustomInfoDeleteResult_e res;
            foreach (string item in ((string[])CustomPropertyManager.GetNames()))
                res = Delete(item);
            return res;
        }

        public IEnumerable<ISingleCustomProperty> GetAll()
        {
            List<ISingleCustomProperty> props = new List<ISingleCustomProperty>();
            foreach(string name in CustomPropertyManager.GetNames())
            {
                if (Configuration == null)
                    props.Add(new SingleCustomProperty(name, this, CustomPropertyType.CustomProperty));
                else
                    props.Add(new SingleCustomProperty(name, this, CustomPropertyType.ConfigurationCustomProperty));
            }
            return props.AsReadOnly();
        }

        public swCustomInfoSetResult_e Set(string Name, string Value)
        {
            return (swCustomInfoSetResult_e)CustomPropertyManager.Set2(Name, Value);
        }
    }
}
