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

        /// <inheritdoc />
        public bool MenuImplemented { get; internal set; }


        /// <summary>
        /// Link the plugin, the origin of the file
        /// </summary>
        internal DefinedPlugin Plugin { get; }

        /// <summary>
        /// This is the index to the icons
        /// </summary>
        internal int IconIndex { get;  set; }

        /// <summary>
        /// This is the menu order that is given by Solidworks. Compared to the one requested by the user.
        /// </summary>
        internal int MenuOrder { get; set; }


    }
}
