using System;
using SingularityBase;
using SingularityBase.UI;
using System.Collections.Generic;
using SolidWorks.Interop.sldworks;

namespace SingularityCore.UI
{
    internal class SingleBaseFlyoutGroup :SingleBaseCommand, ISingleCommandDef , IDisposable
    {
        public SingleBaseFlyoutGroup(ISingleSldWorks solidworks, ISwFlyOut command, int id, DefinedPlugin plugin) : base(solidworks, command, id,plugin)
        {
            CmdType = CommandType.FlyOut;
            Command = command;
        }
        /// <summary>
        /// The command of which the user has defined
        /// </summary>
        public new ISwFlyOut Command { get; }

        /// <summary>
        /// The command of which the user has defined
        /// </summary>
        public List<SingleBaseFlyoutButtonCommand> SubCommand { get; } = new List<SingleBaseFlyoutButtonCommand>();

         public IFlyoutGroup  FlyGroup { get; set; }

         public void Dispose()
         {
             System.Runtime.InteropServices.Marshal.ReleaseComObject(FlyGroup);
             FlyGroup = null;
             //SubCommand.ForEach(t=> t.Dispose);
         }
    }
}
