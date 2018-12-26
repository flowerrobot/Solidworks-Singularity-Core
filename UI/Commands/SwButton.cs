using SingularityBase;
using SingularityBase.UI.Commands;
using SingularityBase.UI.Icons;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.sldworks;

namespace SingleCore.Commands
{
    /// <summary>
    /// Implementing this will cause a button to be created on UI
    /// </summary>
    internal class SwButton : SwCommand, ISwButton
    {

        #region "Menu Options"       

        public string MenuName { get; set; }
        public int MenuOrder { get; set; }
        public CommandLocation MenuType { get; set; } = CommandLocation.All;
        #endregion
        #region "Ribbion"      

        public string RibbionTabName { get; set; }
        public string RibbionSubTabName { get; set; }

        public int RibbonOrder { get; internal set; }

        /// <inheritdoc />
        public swCommandTabButtonTextDisplay_e RibbionDisplayType { get; set; } = swCommandTabButtonTextDisplay_e.swCommandTabButton_TextBelow;
        #endregion
        /// <inheritdoc />
        public sealed override CommandType CmdType => CommandType.Button;
        /// <summary>
        /// Inserted into the ribbon and menu for the types in the list
        /// </summary>
        public DocumentTypes DocumentTypes { get; set; } = DocumentTypes.All;
        internal SwButton(ISingleSldWorks application, string cmdName) : base(application)
        {
            this.CommandName = cmdName;
        }


    }
}
