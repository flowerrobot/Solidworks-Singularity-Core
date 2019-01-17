using SingularityBase;
using SingularityCore.Managers;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Collections.Generic;

namespace SingularityCore
{
    internal class SingleConfiguration : ISingleConfiguration
    {
        public SingleConfiguration(ISingleModelDoc doc, IConfiguration config)
        {
            Document = doc;
            Configuration = config;
          
        }

      

        public ISingleModelDoc Document { get; private set; }
        public string Name
        {
            get => Configuration.Name;
            set => Configuration.Name = value;
        }


        public IConfiguration Configuration { get; }

        private ISingleCustomPropertyManager _customPropertyManager;
        public ISingleCustomPropertyManager CustomPropertyManager
        {
            get {
                if (_customPropertyManager != null) return _customPropertyManager;
                _customPropertyManager = new SingleCustomPropertyManager(Document, this);
                return _customPropertyManager;
            }
        }

        public swConfigurationType_e Type => (swConfigurationType_e)Configuration.Type;

        public IEnumerable<ISingleConfiguration> ChildConfigurations
        {
            get {
                List<ISingleConfiguration> configs = new List<ISingleConfiguration>();
                foreach (object config in (object[])Configuration.GetChildren())
                {
                    configs.Add(new SingleConfiguration(Document, (IConfiguration)config));
                }

                return configs;
            }
        }

        public string ConfigName { get => Configuration.Name; set { Configuration.Name = value; } }
    }
}
