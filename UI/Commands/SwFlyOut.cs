using SingularityBase;
using SingularityBase.UI.Commands;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SingleCore.Commands
{

    internal class SwFlyOut : SwCommand, ISwFlyOut
    {
        internal SwFlyOut(ISingleSldWorks application, string cmdName) : base(application)
        {
            CommandName = cmdName;
        }

        #region Flyout        
        /// <inheritdoc />
        public swCommandFlyoutStyle_e FlyoutType { get; set; }

        /// <inheritdoc />
        public ReadOnlyCollection<ISwFlyOutButton> Commands => _commands.AsReadOnly();
        private readonly List<ISwFlyOutButton> _commands = new List<ISwFlyOutButton>();
        /// <inheritdoc />
        public ISwFlyOutButton AddCommand(string commandName)
        {
            var b = new SwFlyOutButton(SwApp, commandName);
            _commands.Add(b);
            return b;
        }
        

        internal FlyoutGroup FlyGroup { get; set; }

        #endregion

        #region Ribbion     

        public string RibbionTabName { get; set; }
        public string RibbionSubTabName { get; set; }

        public int RibbonOrder { get; }

        /// <inheritdoc />
        public swCommandTabButtonTextDisplay_e RibbionDisplayType { get; set; } = swCommandTabButtonTextDisplay_e.swCommandTabButton_TextBelow;

        public DocumentTypes DocumentTypes { get; set; }
        #endregion

        #region Menus

        public string MenuName { get; set; }
        public int MenuOrder { get; set; }

        /// <inheritdoc />
        public CommandLocation MenuType { get; set; }
        #endregion

        public sealed override CommandType CmdType => CommandType.FlyOut;

    }
}
