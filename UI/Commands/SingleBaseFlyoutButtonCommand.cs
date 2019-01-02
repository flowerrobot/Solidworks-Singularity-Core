using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SingularityBase;
using SingularityBase.UI;
using SingularityBase.UI.Commands;

namespace SingularityCore.UI
{
    /// <summary>
    /// The controller class the flyout sub buttons
    /// </summary>
    internal class SingleBaseFlyoutButtonCommand :SingleBaseCommand, ISingleCommandDef , IDisposable
    {
        public SingleBaseFlyoutButtonCommand(ISingleSldWorks solidworks, ISwFlyOutButton command, int id, DefinedPlugin plugin) : base(solidworks, command, id, plugin)
        {
            Command = command;
        }
        /// <summary>
        /// The command of which the user has defined
        /// </summary>
        public new ISwFlyOutButton Command { get; }

        public void Dispose()
        {
        }

        
    }
}
