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
    internal sealed class SinglePartDoc : SingleModelDoc, ISinglePartDoc
    {
        private readonly List<ISingleConfiguration> _configs = new List<ISingleConfiguration>();

        public override swDocumentTypes_e Type => swDocumentTypes_e.swDocPART;
        public  PartDoc Document { get; }

        // ReSharper disable once SuspiciousTypeConversion.Global
        internal SinglePartDoc(PartDoc doc) : base((ModelDoc2)doc)
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

        private ISingleCutListManager _CutList;
        public ISingleCutListManager CutList => _CutList ?? (_CutList = new SingleCutListManager(this));
        

        public ISingleConfiguration Configuration(string name)
        {
            string[] names = (string[])ModelDoc.GetConfigurationNames();
            if (!names.Any(t => t.Equals(name, StringComparison.CurrentCultureIgnoreCase))) return null;
            var con = _configs.FirstOrDefault(t => t.ConfigName.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if (con != null) return con;
            con = new SingleConfiguration(this,ModelDoc.GetConfigurationByName(name));
            _configs.Add(con);
            return con;
        }

        public ISingleConfiguration ActiveConfiguration { get => new SingleConfiguration(this,(IConfiguration)ModelDoc.GetActiveConfiguration()); }

        public IEnumerable<ISingleConfiguration> Configurations
        {
            get {
                foreach (string name in (string[])ModelDoc.GetConfigurationNames())
                {
                    if (null == _configs.FirstOrDefault(t => t.ConfigName.Equals(name, StringComparison.CurrentCultureIgnoreCase)))
                        _configs.Add(new SingleConfiguration(this,ModelDoc.GetConfigurationByName(name)));
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
