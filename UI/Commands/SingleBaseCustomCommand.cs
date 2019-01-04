using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SingularityBase;
using SingularityBase.UI;

namespace SingularityCore.UI
{
    class SingleBaseCustomCommand : SingleBaseCommand
    {
        public SingleBaseCustomCommand(ISingleSldWorks solidworks, ISwBaseFunction command, int id, DefinedPlugin plugin) : base(solidworks, command, id, plugin)
        {
            CmdType = CommandType.Custom;
            
            //Update variables in interface are updates, now init it
            ((ISwCustomFunction)command).Init(solidworks);
        }

    }
}
