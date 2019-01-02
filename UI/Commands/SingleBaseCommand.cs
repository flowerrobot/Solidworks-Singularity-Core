using SingularityBase;
using SingularityBase.UI;

namespace SingularityCore.UI
{
    internal class SingleBaseCommand : ISingleCommandDef
    {
        public SingleBaseCommand(ISingleSldWorks solidworks, ISwBaseFunction command, int id, DefinedPlugin plugin)
        {
            Solidworks = solidworks;
            Command = command;
            Id = id;
            Plugin = plugin;

            //Update variables in interface
            command.SolidWorks = solidworks;
            command.CommandData = this;

            switch (command)
            {
                case ISwButton _:
                    CmdType = CommandType.Button;
                    break;
                case ISwFlyOut _:
                    CmdType = CommandType.FlyOut;
                    break;
                case ISwFlyOutButton _:
                    CmdType = CommandType.FlyoutSubButton;
                    break;
                case ISwCustomFunction _:
                    CmdType = CommandType.Custom;
                    break;
            }
        }
        /// <inheritdoc />
        public int ButtonId { get; set; }

        /// <inheritdoc />
        public ISingleSldWorks Solidworks { get; }

        /// <inheritdoc />
        public int Id { get; }

        /// <inheritdoc />
        public ISwBaseFunction Command { get; }

        /// <inheritdoc />
        public CommandType CmdType { get; internal set; }

        internal DefinedPlugin Plugin { get; }

        /// <summary>
        /// This is the index to the icons
        /// </summary>
        internal int IconIndex { get;  set; }


    }
}
