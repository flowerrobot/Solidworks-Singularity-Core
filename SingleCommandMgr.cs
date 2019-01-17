using SingularityBase;
using SingularityBase.UI;
using SingularityCore.Events;
using SingularityCore.UI;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SingularityCore
{
    /// <summary>
    /// This manages all the addins and their commands, it finds & initialized all functions, then unloads
    /// </summary>
    public class SingleCommandMgr : IDisposable
    {
        internal SingleSldWorks Solidworks { get; }
        internal PluginLoader PluginLoader { get; }
        internal IconManager Icons { get; } = new IconManager();
        internal CommandManager CmdMgr => Solidworks.CommandManager;

        public static NLog.Logger Logger { get; } = NLog.LogManager.GetLogger("CommandMgr");
        public static string DefaultRibbonName = "Singularity";
        public static string DefaultRibbonTabName = "Tab1";

        private Dictionary<int, SingleBaseCustomCommand> AllCustomFunctions { get; } = new Dictionary<int, SingleBaseCustomCommand>();
        private Dictionary<int, SingleBaseCommand> AllCommands { get; } = new Dictionary<int, SingleBaseCommand>();
        private Dictionary<int, SingleBaseFlyoutGroup> AllFlyOuts { get; } = new Dictionary<int, SingleBaseFlyoutGroup>();

        /// <summary>
        /// List of all high level Ribbons
        /// </summary>
        private List<RibbonCollection> Ribbons { get; } = new List<RibbonCollection>();

        /// <summary>
        /// List of all high level menus
        /// </summary>
        private Dictionary<string, MenuCollection> Menus { get; } = new Dictionary<string, MenuCollection>();

        public SingleCommandMgr(ISldWorks app, int addinId)
        {
            Solidworks = new SingleSldWorks(app, addinId);

            //Setup callbacks
            app.SetAddinCallbackInfo2(0, this, addinId);


            PluginLoader = new PluginLoader(Solidworks, this);
            LoadPlugins();
        }
        public void DisconnectFromSw()
        {
            Solidworks.Dispose();
        }

        #region UI Callbacks
        public void CommandCallBack(int commandIndex)
        {
            UserEvent ev = null;
            try
            {
                //Ensure its out command
                if (!AllCommands.ContainsKey(commandIndex)) return;

                SingleBaseCommand cmd = AllCommands[commandIndex];

                //if flyout create the icons again.
                if (cmd.CmdType == CommandType.FlyOut)
                    //LoadFlyoutSubmenu(cmd.Id);


                    //Raise command
                    Logger.Trace("Command call back {0}", commandIndex);
                ev = new UserEvent(cmd.Plugin, EventType.AddinButton, cmd);

                (cmd.Command as ISwCommand)?.ActionCallback();
            }
            catch (Exception ex) { Logger.Error(ex); }
            finally
            {
                ev?.Dispose();
                ev = null;
            }
        }
        public int DisplayStatus(int commandIndex)
        {
            UserEvent ev = null;
            try
            {
                Logger.Trace("display state {0}", commandIndex);

                //Ensure its out command
                if (!AllCommands.ContainsKey(commandIndex)) return (int)IconEnabled.Deselect_Enable;
                try
                {
                    SingleBaseCommand cmd = AllCommands[commandIndex];
                    ev = new UserEvent(cmd.Plugin, EventType.AddinIconState, cmd);
                    if (cmd.CmdType == CommandType.FlyOut) //if its a flyout check if any of the sub commands are enabled.
                    {
                        if (((SingleBaseFlyoutGroup)cmd).SubCommand.Any(t => t.Command.IconState == IconEnabled.Deselect_Enable))
                            return (int)IconEnabled.Deselect_Enable;
                    }
                    return (int)((cmd.Command as ISwCommand)?.IconState ?? IconEnabled.Deselect_Enable);
                }

                catch (Exception ex) { Logger.Error(ex, "display state {0}", commandIndex); }

                return (int)IconEnabled.Deselect_Disable;
            }
            catch (Exception ex) { Logger.Error(ex); }
            finally
            {
                ev?.Dispose();
                ev = null;
            }
            return (int)IconEnabled.Deselect_Enable;
        }
        #endregion

        /// <summary>
        /// Will load all plugins, create the icons pack & implement commands
        /// </summary>
        /// <returns></returns>
        private bool LoadPlugins()
        {
            try
            {
                Logger.Trace("Loading plugins");

                PluginLoader.LoadPlugins();
                //Sort functions in plugin into groups.
                foreach (DefinedPlugin plugin in PluginLoader.Plugins)
                {
                    foreach (SingleBaseCommand cmd in plugin.Functions)
                    {
                        switch (cmd.CmdType)
                        {
                            case CommandType.Button:
                                AllCommands.Add(cmd.Id, cmd);
                                break;
                            case CommandType.FlyOut:
                                AllCommands.Add(cmd.Id, cmd); //add it to the general population for easy searching
                                AllFlyOuts.Add(cmd.Id, (SingleBaseFlyoutGroup)cmd);
                                foreach (SingleBaseFlyoutButtonCommand swFlyOutButton in ((SingleBaseFlyoutGroup)cmd).SubCommand)
                                {
                                    AllCommands.Add(swFlyOutButton.Id, swFlyOutButton);
                                }
                                break;
                            case CommandType.Custom:
                                AllCustomFunctions.Add(cmd.Id, (SingleBaseCustomCommand)cmd);
                                break;
                        }
                    }
                }

                if (PluginLoader.Plugins.Count <= 0)
                {
                    Logger.Error("No commands found to implement");
                    return false;

                }

                try
                {
                    Logger.Trace("Loading icons");
                    LoadAllIcons(); //Load all icons, removes ones that have invalid icons.
                    Logger.Trace("Loading commands");
                    LoadCommands(); //Load all commands
                }
                catch (Exception ex) { Logger.Error(ex); }
                return true;
            }
            catch (Exception ex) { Logger.Error(ex); return false; }
        }
        /// <summary>
        /// Will add commands to ribbon groups, create menu then create ribbon
        /// </summary>
        private void LoadCommands()
        {
            //Sort commands onto ribbons and into ribbon tabs.
            foreach (SingleBaseCommand cmd in AllCommands.Values)
            {
                //Put commands into Ribbon
                if (cmd.CmdType != CommandType.FlyoutSubButton) //We don't put sub btn's onto menu or ribbon as this is done in sub code.
                {
                    //Check defaults
                    string RibName = ((ISwRibbon)cmd.Command).RibbonTabName;
                    string SubRib = ((ISwRibbon)cmd.Command).RibbonSubTabName;

                    if (string.IsNullOrWhiteSpace(RibName))
                        RibName = DefaultRibbonName;

                    if (string.IsNullOrWhiteSpace(SubRib))
                        SubRib = DefaultRibbonTabName;

                    RibbonCollection rib = GetRibbonByName(RibName);
                    RibbonGroupCollection subRib = (RibbonGroupCollection)rib.GetTabByName(SubRib);

                    subRib.AddButtonOrFlyOut(cmd);

                    //Put commands into menu
                    ValidateForMenu(cmd);
                }
            }

            // *** Create Menu
            //Create menu & tool bar & commands - //Flyouts will be created as sub menus within
            foreach (KeyValuePair<string, MenuCollection> menu in Menus)
                menu.Value.CreateMenu();



            // *** Create flyout commands
            foreach (SingleBaseFlyoutGroup cmd in AllFlyOuts.Values)
            {

                Logger.Trace("Trying to create a flyout but this is not supported");
                cmd.FlyGroup = CmdMgr.CreateFlyoutGroup2(cmd.Id, cmd.Command.CommandName, cmd.Command.CommandName, ((ISwCommand)cmd.Command).CommandToolTop,
                    (object)Icons.AddinIconPaths, (object)Icons.CmdImagePaths, $"CommandCallBack({cmd.Id})", $"DisplayStatus({cmd.Id})");
                cmd.ButtonId = cmd.FlyGroup.CmdID;
                //Call this like this, as call backs will do it too.
                LoadFlyoutSubmenu(cmd.Id);
            }



            Logger.Trace("Start creating the ribbions");
            //*** for each type document start creating ribbons and inserting commands

            foreach (swDocumentTypes_e docType in new[] { swDocumentTypes_e.swDocASSEMBLY, swDocumentTypes_e.swDocDRAWING, swDocumentTypes_e.swDocPART })
            {
                //Go through each ribbon
                foreach (RibbonCollection rib in Ribbons)
                {

                    //Get ribbon if its already created
                    Logger.Trace("Creating a new ribbion {0} for document type {1}", rib.RibbonName, docType);
                    rib.CommandTab = CmdMgr.GetCommandTab((int)docType, rib.RibbonName);



                    //if tab exists, but we have ignored the registry info, re-create the tab.  Otherwise the ids won't match up and the tab will be blank
                    if (rib.CommandTab != null && PluginLoader.NeedsReload)
                    {
                        Logger.Trace("Removing tab");
                        CmdMgr.RemoveCommandTab((CommandTab)rib.CommandTab);
                        rib.CommandTab = null;
                    }

                    //If command tab is null, create & add commands
                    if (rib.CommandTab == null)
                    {
                        Logger.Trace("Creating a new tab for {0}", rib.RibbonName);
                        rib.CommandTab = CmdMgr.AddCommandTab((int)docType, rib.RibbonName);

                        //Add each command on each tab     
                        foreach (IRibbonGroupCollection rtab in rib.SubRibbons)
                        {
                            Dictionary<int, int> cmds = new Dictionary<int, int>(); //id and ribbon type
                                                                                    //
                            Logger.Trace("Adding commands to group");
                            foreach (ISingleCommandDef cmd in ((RibbonGroupCollection)rtab).RibbonCommands.Values)
                            {
                                if (((ISwRibbon)cmd.Command).DocumentTypes.DocumentsEqual(docType)) //Ensure its suitable for the document
                                    if (((ISwMenu)cmd.Command).MenuType.HasFlag(CommandLocation.Ribbon)) //Ensure it was flagged to go onto the ribbon
                                        cmds.Add(cmd.ButtonId, (int)((ISwRibbon)cmd.Command).RibbonDisplayType);
                            }

                            CommandTabBox cmdBox = rib.CommandTab.AddCommandTabBox();

                            if (cmdBox.AddCommands(cmds.Keys.ToArray(), cmds.Values.ToArray()))
                            {
                                int buttonNumber = cmdBox.GetCommands(out object idObject, out object textTypeObject); //this is always 0
                                if (buttonNumber != cmds.Count) { }
                            }
                            else
                                Logger.Error("Could not add ribbion tab {0} to the part {1}", rtab.TabName, docType);

                            Logger.Trace("Adding seporators if they exist");
                            foreach (ISingleCommandDef cmd in ((RibbonGroupCollection)rtab).RibbonCommands.Values)
                                if (((ISwCommand)cmd.Command).Separator == SeperatorLocation.After)
                                {
                                    rib.CommandTab.AddSeparator(cmdBox, cmd.ButtonId);
                                }
                        }
                    }
                    rib.CommandTab.Active = true;
                    rib.CommandTab.Visible = true;
                }
            }

        }


        /// <summary>
        /// Grabs the Icons from each command to compile into a list.
        /// </summary>
        private void LoadAllIcons()
        {
            foreach (SingleBaseCommand cmd in AllCommands.Values)
                Icons.Images.Add(cmd);
            Icons.AppendImages();
        }

        /// <summary>
        /// Will add all the commands required for the flyout
        /// </summary>
        /// <param name="flyOutId">Button allocated by Singularity</param>
        /// <param name="buttonId">Button allocated by Solidworks</param>
        private void LoadFlyoutSubmenu(int flyOutId)
        {
            try
            {

                SingleBaseFlyoutGroup flyParent = AllFlyOuts[flyOutId];
                //if (flyOutId != -1)
                //    flyParent = AllFlyOuts[flyOutId];
                //else if (buttonId != -1)
                //    flyParent = AllFlyOuts.FirstOrDefault(x => x.Value.ButtonId == buttonId).Value;

                if (flyParent == null) return;

                IFlyoutGroup flyGroup = flyParent.FlyGroup;//_cmdMgr.GetFlyoutGroup(flyParent.Id); //user ID,not sw id
                flyGroup.RemoveAllCommandItems();

                foreach (SingleBaseFlyoutButtonCommand subBtn in flyParent.SubCommand)
                    flyGroup.AddCommandItem(subBtn.Command.CommandName, subBtn.Command.CommandToolTop, subBtn.IconIndex, $"CommandCallBack({subBtn.Id})", $"DisplayStatus({subBtn.Id})");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }



        /// <summary>
        /// Will retrieve the command group required for a command
        /// </summary>
        /// <param name="Name">Name of the command group required</param>
        /// <returns></returns>



        /// <summary>
        /// Will retrieve or optionally create is not found a new Ribbon
        /// The name of the ribbon must be unique and can be shared across modules.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="createIfMissing"></param>
        /// <returns></returns>
        private RibbonCollection GetRibbonByName(string name = "", bool createIfMissing = true)
        {
            if (string.IsNullOrWhiteSpace(name)) name = DefaultRibbonName; //Assign default

            RibbonCollection res = Ribbons.FirstOrDefault(r => r.RibbonName.Equals(name));
            if (res != null || !createIfMissing) return res;

            res = new RibbonCollection(name, Solidworks);
            Ribbons.Add(res);
            return res;
        }


        private MenuCollection ValidateForMenu(SingleBaseCommand cmd)
        {
            ISwMenu menu = (ISwMenu)cmd.Command;
            string menuName = menu.MenuName;
            if (string.IsNullOrEmpty(menuName))
                menuName = DefaultRibbonName;


            string[] path = menuName.Split('\\');

            //Get or create the high level menu
            MenuCollection men;
            if (!Menus.ContainsKey(path[0]))
            {
                men = new MenuCollection(this, path[0]);
                Menus.Add(menuName, men);
            }
            else
            {
                men = Menus[path[0]];
            }
            //If the path contained submenus create these now
            for (int i = 1; i < path.Length; i++)
                men = men.AddOrGetMenu(path[i]);

            men._commands.Add(cmd.Id, cmd);

            return men;
        }

        public void Dispose()
        {
            Ribbons.ForEach(t => t.Dispose());
            Ribbons.Clear();

            //Menus.Values.ForEach(t => t.Dispose());
            //DisconnectFromSw();
        }


#if obsolete
        

        private void LoadCommands()
        {
            //This orders all commands in order that should be on the ribbono
            //  Dictionary<string, IRibbonCollection> ribbons = new Dictionary<string, IRibbonCollection>();



            SortedDictionary<string, MenuCollection> menus = new SortedDictionary<string, MenuCollection>();
            List<ISwCommand> flyOuts = new List<ISwCommand>();

            //Sort into logical groups & initilise
        #region Group & Int
            //Loop through all Ribbons, which were created by the modules, then sub groups & add commands to global list.
            foreach (IRibbonCollection ribbon in SwApp.Ribbons)
            {
                foreach (IRibbonGroupCollection subRibbon in ribbon.SubRibbons)
                {
                    foreach (ISwCommand command in subRibbon.Commands)
                    {
                        AllCommands.Add(command.Id, command);

                        // Add to the menu list
                        MenuCollection thismenu = ValidateForMenu(ref menus, (ISwMenu)command);

                        if (command.CmdType == CommandType.FlyOut) //store flyouts in another list for creation.
                        {
                            flyOuts.Add(command);

                            thismenu = thismenu.AddOrGetMenu(command.CommandName); //Add sub flyout commands to sub menu
                            foreach (ISwFlyOutButton swFlyOutButton in ((ISwFlyOut)flyOuts).Commands)
                            {
                                AllCommands.Add(swFlyOutButton.Id, swFlyOutButton);
                                if (swFlyOutButton.MenuType.HasFlag(CommandLocation.Menu))
                                    thismenu._commands.Add(swFlyOutButton);
                            }
                        }
                    }
                }
            }

        #region Old Modules
#if false
            foreach (NewModuleTest mod in ModuleLoader.Modules.Values)
            {
                switch (mod.Function.CmdType)
                {
                    case CommandType.Button:
                        ISwButton thisModule = (SwButton)mod.Function;



                        // Add to the menu list
                        if (ValidateForMenu(ref menus, thisModule))
                            menus[thisModule.MenuName].Commands.Add(thisModule);

                        //This add the command onto the list into the ribbion.
                        if (thisModule.MenuType.HasFlag(CommandLocation.Ribbion))
                            ValidateForRibbon(ref ribbons, thisModule);

                        break;
                    case CommandType.FlyOut:
                        ISwFlyOut flyP = (ISwFlyOut)mod.Function;
                        flyOuts.Add(flyP);

                        // Add to the menu list
                        if (flyP.MenuType.HasFlag(CommandLocation.Menu))
                            ValidateForMenu(ref menus, flyP);



                        //This add the command onto the list into the ribbion.
                        if (flyP.MenuType.HasFlag(CommandLocation.Ribbion))
                            ValidateForRibbon(ref ribbons, flyP);

                        string subMenu = $"{flyP.MenuName}\\{flyP.CommandName}";
                        foreach (Type tsubfly in flyP.Commands)
                        {

                            SwFlyOutButton instance = Activator.CreateInstance(tsubfly, SwApp, AddinId, ModuleLoader.CmdCounter) as SwFlyOutButton;
                            ModuleLoader.CmdCounter += 1;

                            if (instance != null)
                            {
                                if (!menus.ContainsKey(subMenu))
                                    menus.Add(subMenu, new MenuCollection(subMenu));

                                flyP.Commands.Add(instance);
                                menus[subMenu].Commands.Add(instance);
                            }
                        }


                        break;
                    case CommandType.FlyoutSubButton:
                        //Does not matter as the flyout will sort it out.
                        break;
                }
            } 
#endif
        #endregion
        #endregion

        #region Create Menu
            //Create menu & tool bar & commands
            //Flyouts will be created as sub menus.
            foreach (KeyValuePair<string, MenuCollection> menu in menus)
            {
                menu.Value.CreateMenu(this);
            }

#if false
            foreach (KeyValuePair<string, MenuCollection> menu in menus)
            {

                ICommandGroup cmdgrp = GetCmdGroup(menu.Key);
                menu.Value.CmdGroup = cmdgrp;

                List<ISwMenu> cmds = menu.Value.Commands.ToList();

                //Sort menu 
                cmds.Sort(
                    delegate (ISwMenu x, ISwMenu y) //0 is equal, 1 x, -1 y
                      {
                          if (x.MenuOrder == -1 && y.MenuOrder == -1) return 0;
                          else if (x.MenuOrder == -1) return -1;
                          else if (y.MenuOrder == -1) return 1;
                          else return x.MenuOrder.CompareTo(y.MenuOrder);
                      });

                //if (cmdgrp == null) ;
                int order = 1;
                foreach (ISwMenu menuCmd in cmds)
                {
                    if (!(menuCmd is ISwCommand cmd)) continue; //Check the cast worked, otherwise skip

                    Logger.Trace("Loading command {0} which is a {1}", ((ISwCommand)cmd).CommandName, ((ISwCommand)cmd).CmdType);
                    //This creates the Menu items
                    ISwMenu men = cmd as ISwMenu;
                    int menutype = 0;
                    if (men?.MenuType.HasFlag(CommandLocation.Menu) ?? true) menutype += (int)swCommandItemType_e.swMenuItem; //Always create menu when doesnt have interface
                    if (men?.MenuType.HasFlag(CommandLocation.ToolBar) ?? false) { menutype += (int)swCommandItemType_e.swToolbarItem; }

                    menuCmd.MenuOrder = cmdgrp.AddCommandItem2(((ISwCommand)cmd).CommandName, order,
                                cmd.CommandToolTop, cmd.CommandName, cmd.Icons.Index,
                                 $"CommandCallBack({cmd.Id})",
                                $"DisplayStatus({cmd.Id})", cmd.Id, menutype);

                    order += 1;
                }

                cmdgrp.HasToolbar = true;
                cmdgrp.HasMenu = true;

                if (!cmdgrp.Activate())
                {
                    Logger.Error("Could not active cmd group {0}", cmdgrp.Name);
                }

                //map the button ID back
                for (int i = 0; i < cmds.Count(); i++)
                {
                    ((SwCommand)cmds[i]).ButtonId = cmdgrp.CommandID[i];
                }
            }
#endif

            //Create flyout commands
            foreach (ISwCommand cmd in flyOuts)
            {

                SwFlyOut fly = (SwFlyOut)cmd;


                Logger.Trace("Trying to create a flyout but this is not supported");
                fly.FlyGroup = _cmdMgr.CreateFlyoutGroup(fly.Id, fly.CommandName, fly.CommandName, fly.CommandToolTop,
                    Icons.ExtractImage(fly.Icons.ImageSize16),
                    Icons.ExtractImage(fly.Icons.ImageSize32),
                    Icons.ImagePathSize16, Icons.ImagePathSize32, $"CommandCallBack({fly.Id})", $"DisplayStatus({fly.Id})");
                fly.ButtonId = fly.FlyGroup.CmdID;

                AllFlyOuts.Add(fly.Id, fly);//Add to global collection

                LoadFlyoutSubmenu(fly.Id);
            }
        #endregion
#if false
            foreach (swDocumentTypes_e docType in new[]
                {swDocumentTypes_e.swDocASSEMBLY, swDocumentTypes_e.swDocDRAWING, swDocumentTypes_e.swDocPART})
            {
                ICommandTab cmdTab = cmdMgr.GetCommandTab((int)docType, "Testname1");
                if (cmdTab != null )
                {
                    cmdMgr.RemoveCommandTab((CommandTab)cmdTab);
                    cmdTab = null;
                }

                cmdTab = cmdMgr.AddCommandTab((int)docType, "Testname1");

                CommandTabBox cmdBox = cmdTab.AddCommandTabBox();
                int[] cmdIDs = new int[2];
                int[] TextType = new int[2];

                cmdIDs[0] = menus.Values.First().Commands[0].ButtonId;
                TextType[0] = (int)swCommandTabButtonTextDisplay_e.swCommandTabButton_TextHorizontal;
                cmdIDs[1] = menus.Values.First().Commands[1].ButtonId;
                TextType[1] = (int)swCommandTabButtonTextDisplay_e.swCommandTabButton_TextHorizontal;

                //cmdIDs[2] = menus.Values.First().CmdGroup.ToolbarId;
                //TextType[2] = (int)swCommandTabButtonTextDisplay_e.swCommandTabButton_TextHorizontal;

                 cmdBox.AddCommands(cmdIDs, TextType);
                // Call GetCommands to confirm number of commands added

                int buttonNumber = 0;

                object idObject = null;

                object textTypeObject = null;

                buttonNumber = cmdBox.GetCommands(out idObject, out textTypeObject);

                //CommandTabBox cmdBox1 = cmdTab.AddCommandTabBox();
                //cmdIDs = new int[1];
                //TextType = new int[1];
                //cmdIDs[0] = menus.Values.First().CmdGroup.ToolbarId;
                //TextType[0] = (int)swCommandTabButtonTextDisplay_e.swCommandTabButton_TextBelow;
                //cmdBox1.AddCommands(cmdIDs, TextType);
                //cmdTab.AddSeparator(cmdBox1, menus.Values.First().CmdGroup.ToolbarId);
            }



#else
            Logger.Trace("Start creating the ribbions");
            //for each type document start creating ribbions and inserting commands

            foreach (swDocumentTypes_e docType in new[] { swDocumentTypes_e.swDocASSEMBLY, swDocumentTypes_e.swDocDRAWING, swDocumentTypes_e.swDocPART })
            {
                //Got through each ribbon
                foreach (IRibbonCollection rib in SwApp.Ribbons)
                {

                    //Get ribbon if its already created
                    Logger.Trace("Creating a new ribbion {0} for document type {1}", rib.RibbonName, docType);
                    ICommandTab cmdTab = _cmdMgr.GetCommandTab((int)docType, rib.RibbonName);

                    //if tab exists, but we have ignored the registry info, re-create the tab.  Otherwise the ids won't match up and the tab will be blank
                    if (cmdTab != null && SingularityCore.PluginLoader.NeedsReload)
                    {
                        Logger.Trace("Removing tab");
                        _cmdMgr.RemoveCommandTab((CommandTab)cmdTab);
                        cmdTab = null;
                    }

                    //If command tab is null, create & add commands
                    if (cmdTab == null)
                    {
                        Logger.Trace("Creating a new tab for {0}", rib.RibbonName);

                        cmdTab = _cmdMgr.AddCommandTab((int)docType, rib.RibbonName);

                        //Add each command on each tab     
                        foreach (IRibbonGroupCollection rtab in rib.SubRibbons)
                        {
                            //Sort menu  - This no longer works.
                            List<ISwCommand> commands = rtab.Commands.ToList();
                            commands.Sort(
                                delegate (ISwCommand x, ISwCommand y) //0 is equal, 1 x, -1 y
                                {
                                    if (x.Id == -1 && y.Id == -1) return 0;
                                    else if (x.Id == -1) return -1;
                                    else if (y.Id == -1) return 1;
                                    else return x.Id.CompareTo(y.Id);
                                });


                            Dictionary<int, int> cmds = new Dictionary<int, int>(); //id and ribbion type
                            Logger.Trace("Adding commands to group");
                            foreach (ISwCommand cmd in commands)
                            {
                                cmds.Add(cmd.ButtonId, (int)((ISwRibbion)cmd).RibbionDisplayType);
                            }
                            CommandTabBox cmdBox = cmdTab.AddCommandTabBox();

                            if (cmdBox.AddCommands(cmds.Keys.ToArray(), cmds.Values.ToArray()))
                            {
                                int buttonNumber = cmdBox.GetCommands(out object idObject, out object textTypeObject); //this is always 0
                                if (buttonNumber != cmds.Count) { }
                            }
                            else
                                Logger.Error("Could not add ribbion tab {0} to the part {1}", rtab.TabName, docType);

                            Logger.Trace("Adding seporators if they exist");
                            foreach (ISwCommand cmd in commands)
                            {
                                if (cmd.Seperator == SeperatorLocation.After)
                                {
                                    cmdTab.AddSeparator(cmdBox, cmd.Id);
                                }
                            }
                        }
                    }
                    cmdTab.Active = true;
                    cmdTab.Visible = true;
                }
            }
 [Obsolete("Done within Menu collection", true)]
        private ICommandGroup GetCmdGroup(string Name)
        {
            //If the Grp is blank, give it a default value
            if (string.IsNullOrEmpty(Name)) { Name = DefaultRibbonName; }



            if (cmdgrps.Count == 0)
            {
                object ogrp = _cmdMgr.GetGroups();
                if (ogrp != null)
                {
                    foreach (ICommandGroup grp in (ICommandGroup[])ogrp)
                    {
                        cmdgrps.Add(grp);
                    }
                }
            }

            foreach (ICommandGroup grp in cmdgrps)
            {
                if (grp.Name.Equals(Name, StringComparison.OrdinalIgnoreCase))
                    return grp;
            }

            int err = 0;

            ICommandGroup menu = _cmdMgr.CreateCommandGroup2(cmdGrpCount, Name, "tip", "tip", 5, SingularityCore.PluginLoader.NeedsReload, ref err);
            cmdGrpCount += 1;
            if ((swCreateCommandGroupErrors)err != swCreateCommandGroupErrors.swCreateCommandGroup_Success)
                return null;
            else
            {
                cmdgrps.Add(menu);
                menu.LargeIconList = Icons.ImagePathSize32;
                menu.SmallIconList = Icons.ImagePathSize16;
                menu.LargeMainIcon = Icons.AddinIconSize32;
                menu.SmallMainIcon = Icons.AddinIconSize16;

                CreatedCmdGrps.Add(menu);

                return menu;
            }


        }
#endif
        }
#endif
    }


}
