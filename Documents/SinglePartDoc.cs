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
             Document.RegenNotify += Document_RegenPreNotify;

            Document.AddCustomPropertyNotify += AddCustProp;
            Document.ChangeCustomPropertyNotify += ChangeCustProp;
            Document.DeleteCustomPropertyNotify += DeleteCustProp;

            Document.FileSavePostNotify += SavePost;
            Document.FileSaveNotify += SavePre;
            Document.FileSavePostCancelNotify += SaveCancelled;
            Document.FileSaveAsNotify2 += SaveAsPre;


            Document.UserSelectionPostNotify += Document_UserSelectionPostNotify;
            Document.UserSelectionPreNotify += DocumentOnUserSelectionPreNotify;
            Document.ClearSelectionsNotify += DocumentOnClearSelectionsNotify;
            Document.NewSelectionNotify += DocumentOnNewSelectionNotify;
            Document.DeleteSelectionPreNotify += DocumentOnDeleteSelectionPreNotify;

            Document.ModifyTableNotify += DocumentOnModifyTableNotify;
        }

        public ISingleCustomPropertyManager CustomPropertyManager(string configName)
        {
            if (string.IsNullOrWhiteSpace(configName)) return CustomPropertyManager(configName);
            return Configuration(configName)?.CustomPropertyManager ?? null;
        }

        public bool HasCutList => CutList.CutListCount > 0;

        private ISingleCutListManager _cutList;
        public ISingleCutListManager CutList => _cutList ?? (_cutList = new SingleCutListManager(this));
        

        public ISingleConfiguration Configuration(string name)
        {
            string[] names = (string[])ModelDoc.GetConfigurationNames();
            if (!names.Any(t => t.Equals(name, StringComparison.CurrentCultureIgnoreCase))) return null;
            var con = _configs.FirstOrDefault(t => t.ConfigName.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if (con != null) return con;
            con = new SingleConfiguration(this, (IConfiguration)ModelDoc.GetConfigurationByName(name));
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
                        _configs.Add(new SingleConfiguration(this,(IConfiguration)ModelDoc.GetConfigurationByName(name)));
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

            Document.UserSelectionPostNotify -= Document_UserSelectionPostNotify;
            Document.UserSelectionPreNotify -= DocumentOnUserSelectionPreNotify;
            Document.ClearSelectionsNotify -= DocumentOnClearSelectionsNotify;
            Document.NewSelectionNotify -= DocumentOnNewSelectionNotify;
            Document.DeleteSelectionPreNotify -= DocumentOnDeleteSelectionPreNotify;

            Document.ModifyTableNotify -= DocumentOnModifyTableNotify;
        }

    }
}
