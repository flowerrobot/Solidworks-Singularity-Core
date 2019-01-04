using NLog;
using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SingularityBase.UI;

namespace SingularityCore.UI
{
    internal class MenuCollection : IMenuCollection
    {
        public static NLog.Logger Logger { get; } = NLog.LogManager.GetLogger("MenuCollection");


        private static int _menuCounter = 0;


        public MenuCollection(SingleCommandMgr cmdMgr, string menuName)
        {
            CommandManger = cmdMgr;
            MenuName = menuName;
            Id = _menuCounter += 1;
        }

        public int Id { get; }
        public string MenuName { get; }

        public ReadOnlyCollection<ISwMenu> Commands { get; }//=> _commands.AsReadOnly();
        public ICommandGroup CmdGroup { get; private set; }
        public ReadOnlyCollection<IMenuCollection> SubMenus => _subMenus.AsReadOnly();

        private readonly List<IMenuCollection> _subMenus = new List<IMenuCollection>();
        internal readonly SortedList<int, SingleBaseCommand> _commands = new SortedList<int, SingleBaseCommand>(new CommandSorter<int>());

        internal SingleCommandMgr CommandManger { get; }

        internal MenuCollection AddOrGetMenu(string name)
        {
            MenuCollection i = (MenuCollection)_subMenus.FirstOrDefault(t => t.MenuName.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if (i != null) return i;

            MenuCollection men = new MenuCollection(CommandManger, name);
            _subMenus.Add(men);

            return men;
        }

        /// <summary>
        /// Will create the Menu on the Solidworks UI, Create Menu, add commands, any flyouts added as sub menu, Then create any sub menus
        /// </summary>
        /// <returns></returns>
        internal bool CreateMenu()
        {
            return CreateMenu("");
        }

        /// <summary>
        /// Will create the Menu on the Solidworks UI, Create Menu, add commands, any flyouts added as sub menu, Then create any sub menus
        /// </summary>
        /// <param name="parentPath"></param>
        /// <returns></returns>
        private bool CreateMenu(string parentPath)
        {
            Logger.Trace("Loading Menu {0} ", MenuName);
            

            if (!_commands.Any()) return false;
            try
            {
                int err = 0; //swCreateCommandGroupErrors
                CmdGroup = CommandManger.CmdMgr.CreateCommandGroup2(Id, parentPath + "&" + MenuName, "tip", "tip", 5,CommandManger.PluginLoader.NeedsReload, ref err);
                if ((swCreateCommandGroupErrors) err != swCreateCommandGroupErrors.swCreateCommandGroup_Success)
                    return false;

                CmdGroup.IconList = CommandManger.Icons.CmdImagePaths;
                CmdGroup.MainIconList = CommandManger.Icons.AddinIconPaths;

                int order = 1;

                foreach (SingleBaseCommand menuCmd in _commands.Values)
                {
                    try
                    {
                        if (menuCmd.CmdType == CommandType.FlyOut)
                        {
                            MenuCollection sub = AddOrGetMenu(menuCmd.Command.CommandName);
                            foreach (SingleBaseCommand swFlyOutButton in ((SingleBaseFlyoutGroup) menuCmd).SubCommand)
                                sub._commands.Add(((ISwMenu) swFlyOutButton.Command).MenuOrder, swFlyOutButton);
                            continue; //Now skip stuff below
                        }

                        if (menuCmd.CmdType != CommandType.Button && menuCmd.CmdType != CommandType.FlyoutSubButton)
                            continue; //Check the cast worked, otherwise skip

                        Logger.Trace("Loading command {0} which is a {1}", menuCmd.Command.CommandName, menuCmd.CmdType);

                        //This creates the Menu items
                        ISwCommand cmd = menuCmd.Command as ISwCommand;
                        ISwMenu men = menuCmd.Command as ISwMenu;

                        //Will check flags for the type of command.
                        int menuType = 0;
                        if (men?.MenuType.HasFlag(CommandLocation.Menu) ?? true)
                            menuType += (int) swCommandItemType_e.swMenuItem; //Always create menu when doesnt have interface
                        if (men?.MenuType.HasFlag(CommandLocation.ToolBar) ?? false)
                            menuType += (int) swCommandItemType_e.swToolbarItem;

                        menuCmd.MenuOrder = CmdGroup.AddCommandItem2(menuCmd.Command.CommandName, order, cmd.CommandToolTop, cmd.CommandName, menuCmd.IconIndex, $"CommandCallBack({menuCmd.Id})",$"DisplayStatus({menuCmd.Id})", menuCmd.Id, menuType);
                        menuCmd.MenuImplemented = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Menu cmd error");
                    }

                    order += 1;
                }

                CmdGroup.HasToolbar = true;
                CmdGroup.HasMenu = true;

                //Activate the group
                if (!CmdGroup.Activate())
                {
                    Logger.Error("Could not active cmd group {0}{1}", parentPath, MenuName);
                }


                //map the button ID back
                int i = 0;
                foreach (var menuCmd in _commands)
                {
                    if (menuCmd.Value.MenuImplemented)
                    {
                        menuCmd.Value.ButtonId = CmdGroup.CommandID[i];
                        i += 1;
                    }
                }


                // Setup any sub menus
                if (_subMenus.Any())
                    foreach (IMenuCollection subMenu in _subMenus)
                        ((MenuCollection) subMenu).CreateMenu(parentPath +  MenuName + "\\" );


                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Menu error");
            }

            return false;
        }


        public void Dispose()
        {
            _subMenus.ForEach(a => a.Dispose());
            System.Runtime.InteropServices.Marshal.ReleaseComObject(CmdGroup);
            CmdGroup = null;
        }
    }
}
