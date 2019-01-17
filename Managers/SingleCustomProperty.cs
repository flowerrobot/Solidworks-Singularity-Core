using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SingularityBase;
using SolidWorks.Interop.swconst;

namespace SingularityCore.Managers
{
    internal class SingleCustomProperty : ISingleCustomProperty
    {
        public SingleCustomProperty(string name, ISingleCustomPropertyManager custPropMgr, CustomPropertyType customPropertyType)
        {
            CustPropMgr = custPropMgr;
            CustomPropertyType = customPropertyType;
            Name = name;

        }
        string _name;
        public string Name
        {
            get => _name; set {
                if (Name == value) return;
                string val = RawValue;
                var type = Type;
                //TODO add event for change
                CustPropMgr.Delete(_name);
                CustPropMgr.Add(value, type, val, 1);
                _name = value;
            }
        }

        public string RawValue
        {
            get {
                CustPropMgr.CustomPropertyManager.Get5(Name, true, out string val, out string res, out bool WasResolved);
                return val;
            }
            set => CustPropMgr.Set(Name, value);

        }

        public string ResolvedValue
        {
            get {
                CustPropMgr.CustomPropertyManager.Get5(Name, true, out string val, out string res, out bool WasResolved);
                return res;
            }
        }

        public swCustomInfoType_e Type
        {
            get => (swCustomInfoType_e)CustPropMgr.CustomPropertyManager.GetType2(Name);
            set {
                if (Type == value) return;
                string val = RawValue;
                //TODO add events to notify for this
                CustPropMgr.Delete(_name);
                CustPropMgr.Add(_name, value, val, 1);
            }
        }

        public string Configuration => CustPropMgr.Configuration?.ConfigName ?? "";

        public ISingleModelDoc Document => CustPropMgr.Document;

        public ISingleCustomPropertyManager CustPropMgr { get; }

        public CustomPropertyType CustomPropertyType { get; }
    }
}
