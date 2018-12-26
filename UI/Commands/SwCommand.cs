using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SingularityBase;
using SingularityBase.UI.Commands;
using SingularityBase.UI.Icons;
using SingleCore.UI.Icons;

namespace SingleCore.Commands
{
    internal abstract class SwCommand : ISwCommand
    {
        public static NLog.Logger Logger => NLog.LogManager.GetCurrentClassLogger();


        #region ISwBaseFunction
        /// <inheritdoc />
        public abstract CommandType CmdType { get; }
        /// <inheritdoc />
        public int Id { get; }
        /// <inheritdoc />
        public int AddinId { get; }
        /// <inheritdoc />
        public ISingleSldWorks SwApp { get; }
        /// <inheritdoc />
        public string CommandName { get; internal set; }

        public bool SuitableToLoad { get; } = true;

        #endregion


        #region ISwCommand

        public event IconStateQueryEventHandler IconStateQuery;
        public event CommandExecutedEventHandler CommandExecuted;

        /// <inheritdoc />
        public ICommandManager CmdMgr => SwApp.CommandManager;
        /// <inheritdoc />
        public IIconDef Icons { get; protected internal set; }

        private IconEnabled _iconState;
        /// <inheritdoc />
        public IconEnabled IconState
        {
            get {
                if (IconStateQuery != null)
                    _iconState = IconStateQuery(this);
                return _iconState;
            }
            set => _iconState = value;
        }

        /// <inheritdoc />
        public SeperatorLocation Seperator { get; set; } = SeperatorLocation.None;
        
        /// <inheritdoc />
        public int ButtonId { get; internal set; }
        /// <inheritdoc />
        public string CommandToolTop { get; set; }
        /// <summary>
        /// Raises the Command Execute Event
        /// </summary>
        internal void ActionCallback()
        {
            CommandExecuted?.Invoke(this);
        }

        #endregion
        internal SwCommand(ISingleSldWorks application)
        {
            Id = application.GetNextID;
            SwApp = application;
            AddinId = application.AddinId;
            Icons = new IconDef(this); 
        }
    }
}
