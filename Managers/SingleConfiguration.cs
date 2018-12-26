using SingularityBase;
using SingularityBase.Managers;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;

namespace SingularityCore.Managers
{
    internal class SingleConfiguration : ISingleConfiguration
    {
        public SingleConfiguration(IConfiguration config)
        {
            Configuration = config;
        }
        public ISingleModelDoc Doc { get; private set; }

        public IConfiguration Configuration { get; }
        ISingleCustomPropertyManager _customPropertyManager;
        public ISingleCustomPropertyManager CustomPropertyManager
        {
            get {
                if (_customPropertyManager != null) return _customPropertyManager;
                _customPropertyManager = new SingleCustomPropertyManager(Doc, this);
                return _customPropertyManager
            }
        }

        public string ConfigName { get => Configuration.Name; set { Configuration.Name = value; } }
    }
}
