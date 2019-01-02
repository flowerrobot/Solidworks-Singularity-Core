using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SingularityBase.UI.Ribbon;
using SingularityBase;
using SingularityCore;
using SolidWorks.Interop.sldworks;

namespace SingularityCore.UI
{
    /// <summary>
    /// This define a ribbon for the Solidworks UI
    /// </summary>
     internal class RibbonCollection : IRibbonCollection
    {
        private static int idCount = 0;
         /// <inheritdoc />
        public RibbonCollection(string ribbonName, ISingleSldWorks solidWorks)
        {
            Solidworks = solidWorks;
            RibbonName = ribbonName;
            Id = idCount += 1;
        }

         /// <inheritdoc />
        public ReadOnlyCollection<IRibbonGroupCollection> SubRibbons => _subRibbons.AsReadOnly();
        private readonly List<IRibbonGroupCollection> _subRibbons = new List<IRibbonGroupCollection>();

         /// <inheritdoc />
        public string RibbonName { get; internal set; }

         /// <inheritdoc />
        public IRibbonGroupCollection GetTabByName(string name, bool createIfMissing)
         {
             if (string.IsNullOrWhiteSpace(name)) name = SingleCommandMgr.DefaultRibbonTabName;

            IRibbonGroupCollection res = SubRibbons.FirstOrDefault(r => r.TabName.Equals(name));

            if (res == null && createIfMissing)
            {
                res = new RibbonGroupCollection(name, Solidworks) as IRibbonGroupCollection;
                _subRibbons.Add(res);
            }
            return res;
        }

        /// <inheritdoc />
        public IRibbonGroupCollection GetTabByName(string name = "")
        {
            return GetTabByName(name, true);
        }

        /// <inheritdoc />
        public ISingleSldWorks Solidworks { get; }

         /// <inheritdoc />
        public int Id { get; }
         
        internal ICommandTab CommandTab { get; set; }

        public void Dispose()
        {
            foreach (var rg in _subRibbons)
                rg.Dispose();

            System.Runtime.InteropServices.Marshal.ReleaseComObject(CommandTab);
            CommandTab = null;
        }
    }
}
