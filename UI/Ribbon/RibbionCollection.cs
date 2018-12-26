using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SingularityBase.UI.Ribbon;
using SingleCore.Interfaces;
using SingleCore.ModuleLoader;
using SolidWorks.Interop.sldworks;

namespace SingleCore.UI
{
    /// <summary>
    /// This define a ribbon for the Solidworks UI
    /// </summary>
     internal class RibbonCollection : IRibbonCollection
    {
        private static int idCount = 0;
         /// <inheritdoc />
        public RibbonCollection(string ribbonName, ISingleSldWorks sldworks)
        {
            SwApp = sldworks;
            RibbonName = ribbonName;
            Id = idCount += 1;
        }
         /// <inheritdoc />
        public ReadOnlyCollection<IRibbonGroupCollection> SubRibbons => _subRibbons.AsReadOnly();
        private readonly List<IRibbonGroupCollection> _subRibbons = new List<IRibbonGroupCollection>();
         /// <inheritdoc />
        public string RibbonName
        {
            get => RibbonName;
            set => RibbonName =value;
        }
         /// <inheritdoc />
        public IRibbonGroupCollection GetTabByName(string name, bool createIfMissing)
        {
            IRibbonGroupCollection res = SubRibbons.FirstOrDefault(r => r.TabName.Equals(name));

            if (res == null && createIfMissing)
            {
                res = new RibbonGroupCollection(name, SwApp) as IRibbonGroupCollection;
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
        public ISingleSldWorks SwApp { get; }
         /// <inheritdoc />
        public int Id { get; }
         
        internal ICommandTab CommandTab { get; set; }

        /// <summary>
        /// Gets or Adds an item to the read only list
        /// </summary>
        /// <param name="item"></param>
        [Obsolete ("Use GetTabByName", true)]
        internal IRibbonGroupCollection AddorGetSubRibbon(string item) //TODO don't need this, GetTabBYName is the same
        {
            var a = SubRibbons.First(t => t.TabName.Equals(item, StringComparison.CurrentCultureIgnoreCase));
            if (a != null) return a;
        
            
            var i = new RibbonGroupCollection(item, SwApp);
            _subRibbons.Add(i);
            return i;
        }

        
    }
}
