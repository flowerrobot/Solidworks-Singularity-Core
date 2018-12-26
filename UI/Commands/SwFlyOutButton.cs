using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SingularityBase;
using SingularityBase.UI.Commands;
using SingularityBase.UI.Icons;
using SolidWorks.Interop.sldworks;

namespace SingleCore.Commands
{
    internal class SwFlyOutButton : SwCommand, ISwFlyOutButton
    {
        internal SwFlyOutButton(ISingleSldWorks application, string cmdName) : base(application)
        {
            CommandName = cmdName;
        }
        
        public sealed override CommandType CmdType => CommandType.FlyoutSubButton;

        public string MenuName { get; set; }
        public int MenuOrder { get; set; }
        public CommandLocation MenuType { get; set; }
    }
}
