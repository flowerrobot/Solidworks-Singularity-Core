﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SingularityBase;
using SingularityBase.Events;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SingularityCore.Managers;

namespace SingularityCore
{
    internal sealed class SingleAssemblyDoc : SingleModelDoc, ISingleAssemblyDoc
    {
        List<ISingleConfiguration> _configs = new List<ISingleConfiguration>();

        public AssemblyDoc Document { get; }
        public override swDocumentTypes_e Type => swDocumentTypes_e.swDocASSEMBLY;

        internal SingleAssemblyDoc(AssemblyDoc doc) : base((ModelDoc2)doc)
        {
            Document = doc;

            Document.RegenPostNotify2 += RegenPostNotify2;
            Document.AddCustomPropertyNotify += AddCustProp;
            Document.ChangeCustomPropertyNotify += ChangeCustProp;
            Document.DeleteCustomPropertyNotify += DeleteCustProp;

            Document.FileSavePostNotify += SavePost;
            Document.FileSaveNotify += SavePre;
            Document.FileSavePostCancelNotify += SaveCancelled;
            Document.FileSaveAsNotify2 += SaveAsPre;
        }

        public ISingleCustomPropertyManager CustomPropertyManager(string configName)
        {
            if (string.IsNullOrWhiteSpace(configName)) return CustomPropertyManager();
            return Configuration(configName)?.CustomPropertyManager ?? null;
        }

        public ISingleConfiguration Configuration(string name)
        {
            string[] names = (string[])((ModelDoc2)Document).GetConfigurationNames();
            if (!names.Any(t => t.Equals(name, StringComparison.CurrentCultureIgnoreCase))) return null;
            var con = _configs.FirstOrDefault(t => t.ConfigName.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if (con != null) return con;
            con = new SingleConfiguration(this,((ModelDoc2)Document).GetConfigurationByName(name));
            _configs.Add(con);
            return con;
        }

        public bool IsInContextEditState { get; }
        public bool InContextEditDocument { get; }

        public IEnumerable<ISingleConfiguration> Configurations
        {
            get {
                foreach (string name in (string[])((ModelDoc2)Document).GetConfigurationNames())
                {
                    if (null == _configs.FirstOrDefault(t => t.ConfigName.Equals(name, StringComparison.CurrentCultureIgnoreCase)))
                        _configs.Add(new SingleConfiguration(this,((ModelDoc2)Document).GetConfigurationByName(name)));
                }
                return _configs.AsReadOnly();
            }
        }

        public override void Dispose()
        {
            Document.RegenPostNotify2 -= RegenPostNotify2;
            Document.AddCustomPropertyNotify -= AddCustProp;
            Document.ChangeCustomPropertyNotify -= ChangeCustProp;
            Document.DeleteCustomPropertyNotify -= DeleteCustProp;

            Document.FileSavePostNotify -= SavePost;
            Document.FileSaveNotify -= SavePre;
            Document.FileSavePostCancelNotify -= SaveCancelled;
            Document.FileSaveAsNotify2 -= SaveAsPre;
        }
    }
}
