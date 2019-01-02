using SingularityBase;
using SingularityBase.Managers;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private ICustomPropertyManager _customPropertyManager;
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

        public ISingleCustomProperty GetProperty(string Name)
        {
            CustomPropertyType a;
            if (Configuration == null)
                a = CustomPropertyType.CustomProperty;
            else
                a = CustomPropertyType.ConfigurationCustomProperty;


            if (((string[])CustomPropertyManager.GetNames()).Any(t => t.Equals(Name, StringComparison.CurrentCultureIgnoreCase)))
                return new SingleCustomProperty(Name, this, a);

            return null;
        }

        public swCustomInfoDeleteResult_e Delete(string Name)
        {
            return (swCustomInfoDeleteResult_e)CustomPropertyManager.Delete2(Name);
        }

        public swCustomInfoDeleteResult_e DeleteAll()
        {
            int res = 0;
            foreach (string item in ((string[])CustomPropertyManager.GetNames()))
                res += (int)Delete(item);

            return res == 0 ? swCustomInfoDeleteResult_e.swCustomInfoDeleteResult_OK : swCustomInfoDeleteResult_e.swCustomInfoDeleteResult_NotPresent;
        }

        public IEnumerable<ISingleCustomProperty> GetAll()
        {
            List<ISingleCustomProperty> props = new List<ISingleCustomProperty>();
            foreach (string name in CustomPropertyManager.GetNames())
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
