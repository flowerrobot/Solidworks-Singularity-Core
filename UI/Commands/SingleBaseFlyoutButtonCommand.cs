using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SingularityBase;
using SingularityBase.UI;

namespace SingularityCore.UI
{
    /// <summary>
    /// The controller class the flyout sub buttons
    /// </summary>
    internal class SingleBaseFlyoutButtonCommand :SingleBaseCommand, ISingleCommandDef , IDisposable
    {
        public SingleBaseFlyoutButtonCommand(ISingleSldWorks solidworks, ISwFlyOutButton command, int id, DefinedPlugin plugin,SingleBaseFlyoutGroup parent) : base(solidworks, command, id, plugin)
        {
            Command = command;
            Parent = parent;
        }
        /// <summary>
        /// The command of which the user has defined
        /// </summary>
        public new ISwFlyOutButton Command { get; }

        /// <summary>
        /// The parent flyout group
        /// </summary>
        internal SingleBaseFlyoutGroup Parent { get; }

        public void Dispose()
        {
        }

        
    }
}
