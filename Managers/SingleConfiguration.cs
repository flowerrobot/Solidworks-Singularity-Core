using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Collections.Generic;

namespace SingularityCore
{
    internal class SingleConfiguration : SingularityObject<IConfiguration>, ISingleConfiguration
    {
        public SingleConfiguration(ISingleModelDoc doc, IConfiguration config) : base(config)
        {
            Document = doc;

        }

        public SingleConfiguration(ISingleModelDoc doc, IConfiguration config, ISingleConfiguration parent) : this(doc, config)
        {
            _parent = parent;
        }



        public ISingleModelDoc Document { get; private set; }
        public int id => BaseObject.GetID();

        public string Name
        {
            get => BaseObject.Name;
            set => BaseObject.Name = value;
        }




        private ISingleCustomPropertyManager _customPropertyManager;
        public ISingleCustomPropertyManager CustomPropertyManager
        {
            get {
                if (_customPropertyManager != null) return _customPropertyManager;
                _customPropertyManager = new SingleCustomPropertyManager(Document, this);
                return _customPropertyManager;
            }
        }

        public swConfigurationType_e Type => (swConfigurationType_e)BaseObject.Type;

        public IList<ISingleConfiguration> ChildConfigurations
        {
            get {
                List<ISingleConfiguration> configs = new List<ISingleConfiguration>();
                foreach (object config in (object[])BaseObject.GetChildren())
                {
                    configs.Add(new SingleConfiguration(Document, (IConfiguration)config, this));
                }

                return configs;
            }
        }

        private ISingleConfiguration _parent;
        public ISingleConfiguration Parent
        {
            get {
                if (_parent != null) return _parent;
                Configuration a = BaseObject.GetParent();
                _parent = new SingleConfiguration(Document, a);
                return _parent;
            }
        }

        public bool IsDerived => BaseObject.IsDerived();
        public Dictionary<string, string> Parameters
        {
            get {
                Dictionary<string, string> vals = new Dictionary<string, string>();
                BaseObject.GetParameters(out object names, out object values);

                string[] nameArr = (string[])names;
                string[] valArr = (string[])values;
                for (int i = 0; i < nameArr.Length; i++)
                {
                    if (!vals.ContainsKey(nameArr[i]))
                        vals.Add(nameArr[i], valArr[i]);
                }

                return vals;
            }
            set {
                BaseObject.SetParameters((object)value.Keys, (object)value.Values);
            }
        }

        public string ConfigName { get => BaseObject.Name; set { BaseObject.Name = value; } }
    }
}
