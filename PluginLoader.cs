using SingularityBase;
using SingularityBase.UI;
using SingularityBase.UI.Commands;
using SingularityCore.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Environment = System.Environment;

namespace SingularityCore
{
    /// <summary>
    /// This searches and loads any plugins suitable
    /// </summary>
    internal class PluginLoader
    {
        private static NLog.Logger Logger { get; } = NLog.LogManager.GetLogger("PluginLoader");
        public static string WorkingPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Singularity\");


        internal DirectoryInfo PluginFolder = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", @"Plugins\")); //This is the folder this dll is in

        private static readonly Type BaseFunc = typeof(ISwBaseFunction);
        private static readonly Type FlyOutBtnFunc = typeof(ISwFlyOutButton);


        /// <summary>
        /// Core files are plugins which are part of the core Singularity
        /// </summary>
#if DEBUG
        private readonly FileInfo[] coreFiles = new[] { new FileInfo(@"C:\Users\setruh\OneDrive\Documents\Programming\GitHub\SW Addin\OswCoreCommands\bin\Debug\OswCoreCommands.dll") };
#else
        private readonly FileInfo[] coreFiles = new[] { new new FileInfo(System.IO.Path.Combine(DirectoryPath, "OswCoreCommands.dll"))};
#endif

        /// <summary>
        /// xmlName - for what?
        /// </summary>
#if DEBUG
        private readonly string xmlName = @"C:\Users\setruh\OneDrive\Documents\Programming\BitBucket\SW Addin\OswCore\ModuleLoader\Modules.xml";
#else
        private readonly string xmlName = System.IO.Path.Combine(DirectoryPath, "Modules.xml");
#endif

        /// <summary>
        /// This path is where the information about any user stored files to load
        /// </summary>
        private readonly string userXml = Path.Combine(WorkingPath, @"UserProfile.xml");

        /// <summary>
        /// List of all plugins to be implemented
        /// </summary>
        public List<DefinedPlugin> Plugins { get; } = new List<DefinedPlugin>();
        /// <summary>
        /// List of plugins to be loaded at users request
        /// </summary>
        // TODO to be implemented later
        public List<DefinedPlugin> UserPlugins { get; } = new List<DefinedPlugin>();

        /// <summary>
        /// Informs if Solidworks needs to reload menus, this is due to a module having changed.
        /// </summary>
        public bool NeedsReload { get; internal set; }

        SingleSldWorks SolidWorks { get; }


        internal PluginLoader(SingleSldWorks solidWorks, SingleCommandMgr mgr)
        {
            SolidWorks = solidWorks;
            //DirectoryPath = System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location);

        }

        /// <summary>
        /// Searches and loads all modules and commands
        /// </summary>
        internal void LoadPlugins()
        {
            //Search and load core plugin files - and add it too the list of plugins
            LoadPluginFromFiles(coreFiles).ForEach(t => Plugins.Add(t));

            //Search and load plugins in the plugins folder - file locate\plugin
            Dictionary<string, FileInfo> plugFiles = new Dictionary<string, FileInfo>();
            SearchThisDir(ref plugFiles, PluginFolder);
            if (plugFiles.Any()) LoadPluginFromFiles(plugFiles.Values);

            //Load any files to user has specifically request to load.
            //This is specified in the  userXml
            //TODO Implement this + UI


            //Must check versions of files loaded last time vs last time if different 
            //TODO implement
            NeedsReload = true;

#if DEBUG
            NeedsReload = true;
#endif

        }

        /// <summary>
        /// Searches and loads all modules and commands
        /// </summary>
        [Obsolete("Implemented again using different logic", true)]
        internal void LoadPlugins_Old()
        {


            #region Load corecommands
            //Load and add to Modules Libary
            LoadPluginFromFiles(coreFiles).ForEach(t => Plugins.Add(t));
            #endregion
            #region Load standard files
            //load settings from XML next to dll.
            // Loads any modules from the XML file, updates existing records for must load
            List<DefinedPlugin> modsInXML = LoadFromXml(xmlName);
            modsInXML.ForEach(t => Plugins.Add(t)); //Add to global list
            #endregion
            #region Load User Files
            //Load from user XML or search
            try
            {
                List<DefinedPlugin> modsInUserXML;
                if (System.IO.File.Exists(userXml))
                    modsInUserXML = LoadFromXml(userXml);
                else
                {
                    //Do a search for avalible modules and write to xml.
                    //modsInUserXML = SearchForModules(Plugins);
                    NeedsReload = true;
                }

              //  modsInUserXML.ForEach(t => Plugins.Add(t));


                //Write to user XML all command that have been loaded
             //   WriteCommandsToXml(userXml, modsInUserXML);
            }
            catch (Exception ex) { Logger.Error(ex); }
            #endregion


            List<DefinedPlugin> lastLoadedFiles = new List<DefinedPlugin>(); //TODO load from storage the modules loaded last time
            //Check all loaded modules to see if they all match filename & version, if not reload
            //Then if any modules loaded last time are left over = reload
            foreach (DefinedPlugin df in Plugins)
            {
                DefinedPlugin found = lastLoadedFiles.FirstOrDefault(llf => llf.File.Name.Equals(df.File.Name) && llf.AssemblyVersion.Equals(df.AssemblyVersion));
                if (found == null)
                {
                    NeedsReload = true;
                    break;
                }
                else
                    lastLoadedFiles.Remove(found);
            }

            NeedsReload = NeedsReload || lastLoadedFiles.Any();
            //TODO save files
#if DEBUG
            NeedsReload = true;
#endif
        }

        #region  Publics        
        /// <summary>
        /// Looks for any modules with the file paths given
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns></returns>
        public bool LoadNewModulesNextStart(List<FileInfo> files)
        {
            List<DefinedPlugin> items = LoadPluginFromFiles(files);

            if (items.Any())
            {
                WriteCommandsToXml(userXml, Plugins);
            }

            return items.Any();
        }

        /// <summary>
        /// Removes the commands from the settings when loading up next restart.
        /// </summary>
        /// <param name="mods">The mods.</param>
        /// <returns></returns>
        private bool RemoveCommands(IEnumerable<DefinedPlugin> mods)
        {
            try
            {
                //TODO implement correctly
                foreach (DefinedPlugin mod in mods)
                {
                    //if (Properties.Settings.Default.ModulesLastLoaded.Contains(mod.File.FullName)) 
                    //Properties.Settings.Default.ModulesLastLoaded.Remove(mod.File.FullName);
                }
                Properties.Settings.Default.Save();
                return true;
            }
            catch (Exception ex) { Logger.Error(ex); }
            return false;
        }
        #endregion
        #region Private
        internal static int CmdCounter = 0;
        private List<DirectoryInfo> _filePaths;
        private List<DirectoryInfo> FilePaths
        {
            get {
                if (_filePaths == null)
                {
                    Logger.Trace("Loading file paths");
                    _filePaths = new List<DirectoryInfo>
                    {
                        new DirectoryInfo(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Remove(0, 6)),
                        new DirectoryInfo(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Remove(0, 6) + @"\Modules\")
                    };
                    foreach (string path in Properties.Settings.Default.FilePaths.Split(';'))
                    {
                        Logger.Trace("User path defined {0}", path);
                        if (!string.IsNullOrWhiteSpace((path)))
                        {
                            DirectoryInfo di = new DirectoryInfo(path);
                            if (di.Exists)
                            {
                                _filePaths.Add(di);
                            }
                            else
                            {
                                Logger.Trace("Path does not exist");
                            }
                        }
                    }
                }
                return _filePaths;
            }
        }

       
        /// <summary>
        /// Will search each file listed for any classes that implement the required modules.
        /// </summary>
        /// <param name="foundFiles">The library files.</param>
        /// <returns></returns>
        private List<DefinedPlugin> LoadPluginFromFiles(ICollection<FileInfo> foundFiles)
        {
            List<DefinedPlugin> found = new List<DefinedPlugin>();
            try
            {
                //Ensure there is files
                if (foundFiles.Count == 0)
                {
                    Logger.Trace("No libarys found to load");
                    return null;
                }

                //Loop through each file
                foreach (FileInfo dll in foundFiles)
                {
                    if (!dll.Exists) continue;
                    try
                    {

                        //load the assembly carefully.
                        Logger.Trace("Loading assembly {0}", dll.FullName);
                        Assembly assembly = Assembly.UnsafeLoadFrom(dll.FullName);

                        DefinedPlugin newPlugin = new DefinedPlugin(assembly);

                        //Loop through each type found
                        foreach (Type type in assembly.GetTypes())
                        {
                            // We only want to create an instance if is a normal command, but is not a flyout button - as they will be created differently
                            if (BaseFunc.IsAssignableFrom(type) && !BaseFunc.IsAssignableFrom(FlyOutBtnFunc) && !(type.IsAbstract || type.IsInterface))
                            {
                                try
                                {
                                    //Create the instance of the class & check if its suitable to load for this user
                                    if (Activator.CreateInstance(type) is ISwBaseFunction instance)
                                    {
                                        ISingleCommandDef cmd;
                                        if (instance is ISwFlyOut Flyout)
                                        {
                                            cmd = new SingleBaseFlyoutCommand(SolidWorks, Flyout, CmdCounter += 1, newPlugin);
                                            //Through each sub button implement it if suitable
                                            foreach (Type subBtnType in Flyout.SubButtons)
                                            {
                                                //Check suitability
                                                if (subBtnType.IsAssignableFrom(FlyOutBtnFunc) && !(type.IsAbstract || type.IsInterface) && Activator.CreateInstance(type) is ISwFlyOutButton subBtn)
                                                {
                                                    SingleBaseCommand btnWrap = new SingleBaseCommand(SolidWorks, subBtn, CmdCounter += 1, newPlugin);
                                                    ((SingleBaseFlyoutCommand)cmd).SubCommand.Add(btnWrap);
                                                }
                                            }
                                        }
                                        else if (instance is ISwCustomFunction) { cmd = new SingleBaseCustomCommand(SolidWorks, instance, CmdCounter += 1, newPlugin); }
                                        // else if (instance is ISwFlyOutButton) { } //This will never hit as its excluded
                                        else { cmd = new SingleBaseCommand(SolidWorks, instance, CmdCounter += 1, newPlugin); }

                                        newPlugin.Functions.Add(cmd);
                                    }
                                }
                                catch (Exception ex) { Logger.Error(ex); }

                                break;
                            }
                        }
                        if (newPlugin.Functions.Any()) found.Add(newPlugin);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Could not read the modul: {0}");
                    }
                }
                return found;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return found;
            }
        }
        /// <summary>
        /// Gets all directories where this DLLS is in search for more modules to load
        /// </summary>
        /// <returns></returns>
        private List<FileInfo> FindAllDlls()
        {

            Dictionary<string, FileInfo> dlls = new Dictionary<string, FileInfo>();

            //Add this file so the core functions can be added
            FileInfo thisfile = new FileInfo(GetType().Assembly.Location);
            dlls.Add(thisfile.Name, thisfile);

            //Search through all sub folders in path looking at dlls
            foreach (DirectoryInfo path in FilePaths)
            {
                SearchThisDir(ref dlls, path);
            }

            return dlls.Values.ToList();
        }
        /// <summary>
        /// Searches the nominated directory, which it will do recursivly for each sub folder
        /// </summary>
        /// <param name="dlls">The DLLS.</param>
        /// <param name="dirPath">The dirpath.</param>
        private void SearchThisDir(ref Dictionary<string, FileInfo> dlls, DirectoryInfo dirPath)
        {
            try
            {
                FileInfo[] files = dirPath.GetFiles("*.dll");
                foreach (FileInfo file in files)
                {
                    if (file.Extension.ToUpper() == ".DLL" && !dlls.ContainsKey(file.Name))
                        dlls.Add(file.Name, file);
                }

                foreach (DirectoryInfo path in dirPath.GetDirectories())
                {
                    SearchThisDir(ref dlls, path);
                }
            }
            catch (Exception ex) { Logger.Error(ex); }

        }

        private List<DefinedPlugin> LoadFromXml(string xmlFile)
        {
            return new List<DefinedPlugin>();
        }
        private bool WriteCommandsToXml(string userXmlPath, IEnumerable<DefinedPlugin> mods)
        {
            return true;
        }
        ///// <summary>
        ///// Loads settings from XML.
        ///// </summary>
        ///// <param name="xmlFile">Name of the XML.</param>
        ///// <returns></returns>
        //private List<DefinedPlugin> LoadFromXml(string xmlFile)
        //{

        //    List<DefinedPlugin> modules = new List<DefinedPlugin>();
        //    try
        //    {
        //        if (File.Exists(xmlFile))
        //        {
        //            XmlSerializer ser = new XmlSerializer(typeof(root));
        //            using (FileStream fs = new FileStream(xmlFile, FileMode.Open))
        //            {
        //                if (ser.Deserialize(fs) is root xml && xml.ModuleDef.Count != 0)
        //                    //Loop for each item in the xml, if the file exists load its modules within
        //                    foreach (root.ModuleDefRow item in xml.ModuleDef)
        //                        if (File.Exists(item.Path))
        //                            //Set the must load flag if mentioned in the xml
        //                            modules.AddRange(LoadPluginFromFiles(new[] { new FileInfo(item.Path) }.ToList()));
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex);
        //    }
        //    return modules;
        //}
        ////
        ///// <summary>
        ///// Writes the commands to XML.
        ///// </summary>
        ///// <param name="userXmlPath">The user XML file</param>
        ///// <param name="mods">The Modules to write</param>
        ///// <returns>Was successful</returns>
        //private bool WriteCommandsToXml(string userXmlPath, IEnumerable<DefinedPlugin> mods)
        //{
        //    try
        //    {
        //        if (!File.Exists(userXmlPath))
        //        {
        //            Directory.CreateDirectory(Path.GetDirectoryName(userXmlPath) ?? "");
        //            File.Create(userXmlPath).Close();
        //        }



        //        root r = new root();

        //        foreach (DefinedPlugin cmd in mods)
        //            r.ModuleDef.AddModuleDefRow(cmd.File.FullName, true);


        //        XmlSerializer ser = new XmlSerializer(typeof(root));
        //        using (StreamWriter stream = new StreamWriter(userXmlPath))
        //            ser.Serialize(stream, r);

        //        return true;
        //    }
        //    catch (Exception ex) { Logger.Error(ex); }
        //    return false;
        //}

        #endregion
    }
}
