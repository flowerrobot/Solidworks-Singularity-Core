using System.Collections.Generic;
using System.Collections.ObjectModel;
using SingularityBase;
using SingularityBase.UI.Commands;
using SingularityBase.UI.Icons;
using SingularityBase.UI.Ribbon;
using SingleCore.Commands;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace SingleCore.UI
{
    // This defined a tab within the ribbon to implement onto the Solidworks UI
    internal class RibbonGroupCollection : IRibbonGroupCollection
    {
         static int _idCount = 0;
        public RibbonGroupCollection(string tabName, ISingleSldWorks sldworks)
        {
            SwApp = sldworks;
            TabName = tabName;
            Id = _idCount += 1;
        }
        /// <inheritdoc />
        public string TabName { get; }
        /// <inheritdoc />
        public ISingleSldWorks SwApp { get; }

        public int Id { get; }

        #region AddButton
        /// <inheritdoc />
        public ISwButton AddButton(string commandName)
        {
            var cmd =new SwButton(SwApp, commandName);
            _commands.Add(cmd);
            return cmd;
        }

        public ISwButton AddButton(string commandName, string commandToolTop, DocumentTypes documentTypes)
        {
            SwButton b = (SwButton)AddButton(commandName);
            b.CommandToolTop = commandToolTop;
            b.DocumentTypes = documentTypes;
            return b;
        }

        public ISwButton AddButton(string commandName, string commandToolTop, DocumentTypes documentTypes, int ribbonOrder)
        {
            SwButton b = (SwButton)AddButton(commandName);
            b.CommandToolTop = commandToolTop;
            b.DocumentTypes = documentTypes;
            b.RibbonOrder = ribbonOrder;
            return b;
        }

        public ISwButton AddButton(string commandName, string commandToolTop, DocumentTypes documentTypes, int ribbonOrder,
            swCommandTabButtonTextDisplay_e ribbonDisplayType)
        {
            SwButton b = (SwButton)AddButton(commandName);
            b.CommandToolTop = commandToolTop;
            b.DocumentTypes = documentTypes;
              b.RibbonOrder = ribbonOrder;
            b.RibbionDisplayType = ribbonDisplayType;
            return b;
        }

        public ISwButton AddButton(string commandName, string commandToolTop, DocumentTypes documentTypes, int ribbonOrder,
            swCommandTabButtonTextDisplay_e ribbonDisplayType, string menuName = "", int menuOrder = -1,
            CommandLocation menuType = CommandLocation.All)
        {

            SwButton b = (SwButton)AddButton(commandName);
            b.CommandToolTop = commandToolTop;
            b.DocumentTypes = documentTypes;
            b.RibbonOrder = ribbonOrder;
            b.RibbionDisplayType = ribbonDisplayType;
            b.MenuName = menuName;
            b.MenuOrder = menuOrder;
            b.MenuType = menuType;
            return b;
        }

        public ISwButton AddButton(string commandName, string commandToolTop, DocumentTypes documentTypes, int ribbonOrder,
            swCommandTabButtonTextDisplay_e ribbonDisplayType, string menuName, int menuOrder, CommandLocation menuType,
            IIconDef icons)
        {
            SwButton b = (SwButton)AddButton(commandName);
            b.CommandToolTop = commandToolTop;
            b.DocumentTypes = documentTypes;
            b.RibbonOrder = ribbonOrder;
            b.RibbionDisplayType = ribbonDisplayType;
            b.MenuName = menuName;
            b.MenuOrder = menuOrder;
            b.MenuType = menuType;
            b.Icons = icons;
            return b;
        }
#endregion
        public ISwFlyOut AddFlyOutGroup(string commandName)
        {
            var cmd = new SwFlyOut(SwApp, commandName);
            _commands.Add(cmd);
            return cmd;
        }

        ///// <inheritdoc />
        //public ISwFlyOut AddFlyOut(string commandName)
        //{
        //    return AddCommand(new SwFlyOut(SwApp, commandName)) as ISwFlyOut;
        //}
        ///// <inheritdoc />
        //public ISwFlyOutButton AddFlyoutButton(ISwFlyOut flyout, string commandName)
        //{
        //    var btn = new SwFlyOutButton(flyout.SwApp, commandName);
        //    flyout.Commands.Add(btn);
        //    return AddCommand(btn) as ISwFlyOutButton;
        //}
        /// <inheritdoc />
        public ReadOnlyCollection<ISwCommand> Commands => _commands.AsReadOnly();
        private List<ISwCommand> _commands = new List<ISwCommand>();
       


        internal ICommandTabBox CommandGroup { get; set; }

    }
}
