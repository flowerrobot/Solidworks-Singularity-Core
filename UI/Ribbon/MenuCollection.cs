using NLog;
using SingularityBase.UI.Commands;
using SingleCore.ModuleLoader;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SingularityBase;
using SingleCore.Commands;

namespace SingleCore.Interfaces
{
    internal class MenuCollection : IMenuCollection
    {
        public static NLog.Logger Logger { get; } = NLog.LogManager.GetLogger("CommandMgr|MenuCollection");


        private static int MenuCounter = 0;


        public MenuCollection(string menuName)
        {
            MenuName = menuName;
            Id = MenuCounter += 1;
        }

        public int Id { get; }
        public string MenuName { get; }

        public ReadOnlyCollection<ISwMenu> Commands => _commands.AsReadOnly();
        public ICommandGroup CmdGroup { get; private set; }
        public ReadOnlyCollection<IMenuCollection> SubMenus => _subMenus.AsReadOnly();


        private readonly List<IMenuCollection> _subMenus = new List<IMenuCollection>();
        internal readonly List<ISwMenu> _commands = new List<ISwMenu>();

        internal MenuCollection AddOrGetMenu(string name)
        {
            MenuCollection i = (MenuCollection)_subMenus.First(t => t.MenuName.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if (i != null) return i;

            MenuCollection men = new MenuCollection(name);
            _subMenus.Add(men);

            return men;
        }

        /// <summary>
        /// Will create the Menu on the Solidworks UI
        /// </summary>
        /// <param name="cmdMgr"></param>
        /// <returns></returns>
        internal bool CreateMenu(CommandMgr cmdMgr)
        {
            return CreateMenu(cmdMgr, "");
        }
        /// <summary>
        /// Create Menu, add commands any flyouts create as sub menu, Then create any sub menus
        /// </summary>
        /// <param name="cmdMgr"></param>
        /// <param name="parentPath"></param>
        /// <returns></returns>
        private bool CreateMenu(CommandMgr cmdMgr,  string parentPath)
        {
            Logger.Trace("Loading Menu {0} ", MenuName);


            if (_commands.Any()) //ensure there are commands
            {
                int err = 0; //swCreateCommandGroupErrors
                CmdGroup = cmdMgr.CmdMgr.CreateCommandGroup2(Id, parentPath + MenuName, "tip", "tip", 5, CommandMgr.ModuleLoader.NeedsReload, ref err);
                if ((swCreateCommandGroupErrors)err == swCreateCommandGroupErrors.swCreateCommandGroup_Success)
                {
                    CmdGroup.LargeIconList = cmdMgr.Icons.ImagePathSize32;
                    CmdGroup.SmallIconList = cmdMgr.Icons.ImagePathSize16;
                    CmdGroup.LargeMainIcon = cmdMgr.Icons.AddinIconSize32;
                    CmdGroup.SmallMainIcon = cmdMgr.Icons.AddinIconSize16;




                    //Add Commands to Menu
                    List<ISwMenu> cmds = _commands.ToList();

                    //Sort menu 
                    cmds.Sort(
                        delegate (ISwMenu x, ISwMenu y) //0 is equal, 1 x, -1 y
                        {
                              if (x.MenuOrder == -1 && y.MenuOrder == -1) return 0;
                              else if (x.MenuOrder == -1) return -1;
                              else if (y.MenuOrder == -1) return 1;
                              else return x.MenuOrder.CompareTo(y.MenuOrder);
                          });


                    int order = 1;
                    List<ISwFlyOut> flyouts = new List<ISwFlyOut>(); //Store any flyouts to create sub menus
                    foreach (ISwMenu menuCmd in cmds)
                    {
                        if (menuCmd is ISwFlyOut @out) //TODO This is handled in cmdmgr, so should remove
                        {
                            var sub = AddOrGetMenu(@out.CommandName);
                            foreach (var swFlyOutButton in @out.Commands)
                            {
                                sub._commands.Add(swFlyOutButton);
                            }
                            continue;
                        }
                        if (!(menuCmd is ISwCommand cmd)) continue; //Check the cast worked, otherwise skip

                        Logger.Trace("Loading command {0} which is a {1}", ((ISwCommand)cmd).CommandName, ((ISwCommand)cmd).CmdType);
                        //This creates the Menu items
                        ISwMenu men = cmd as ISwMenu;
                        int menutype = 0;
                        if (men?.MenuType.HasFlag(CommandLocation.Menu) ?? true) menutype += (int)swCommandItemType_e.swMenuItem; //Always create menu when doesnt have interface
                        if (men?.MenuType.HasFlag(CommandLocation.ToolBar) ?? false) { menutype += (int)swCommandItemType_e.swToolbarItem; }

                        menuCmd.MenuOrder = CmdGroup.AddCommandItem2(((ISwCommand)cmd).CommandName, order,
                                    cmd.CommandToolTop, cmd.CommandName, cmd.Icons.Index, $"CommandCallBack({cmd.Id})", $"DisplayStatus({cmd.Id})", cmd.Id, menutype);

                        order += 1;
                    }
                    CmdGroup.HasToolbar = true;
                    CmdGroup.HasMenu = true;

                    //Activate the group
                    if (!CmdGroup.Activate())
                    {
                        Logger.Error("Could not active cmd group {0}{1}", parentPath ,MenuName);
                    }


                    //map the button ID back
                    //TODO determine if this is even needed
                    int i = 0;
                    foreach (ISwMenu menuCmd in cmds)
                    {
                        if (!(menuCmd is SwCommand cmd)) continue; //Check the cast worked, otherwise skip
                        cmd.ButtonId = CmdGroup.CommandID[i];
                        i += 1;
                    }

                  
                    // Setup any sub menus
                    if (_subMenus.Any())
                    {
                        foreach (IMenuCollection subMenu in _subMenus)
                        {
                            ((MenuCollection)subMenu).CreateMenu(cmdMgr,  parentPath + "\\" + MenuName);
                        }
                    }

                    return true;
                }
            }
            return false;
        }

        
    }
}
