using SingularityBase;
using SingularityBase.UI;
using SingularityCore.Plugins;
using SingularityCore.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SingularityCore
{
    /// <summary>
    /// This searches and loads any plugins suitable
    /// </summary>
    internal class PluginLoader
    {
        private static NLog.Logger Logger { get; } = NLog.LogManager.GetLogger("PluginLoader");
        /// <summary>
        /// The counter for command Ids
        /// </summary>
         internal static int CmdCounter { get; set; }= 0;

        private static readonly Type BaseFunc = typeof(ISwBaseFunction);
        private static readonly Type FlyOutBtnFunc = typeof(ISwFlyOutButton);


        /// <summary>
        /// This is the location for the local user
        /// </summary>
        internal DirectoryInfo WorkingPath { get; } = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Singularity\"));
        /// <summary>
        /// This is the directory for the plugin files, based on the main files + plugin path
        /// </summary>
        internal DirectoryInfo PluginFolder { get; } = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", @"Plugins\")); //This is the folder this dll is in

      


        /// <summary>
        /// Core files are plugins which are part of the core Singularity
        /// </summary>
#if DEBUG
        private readonly FileInfo[] coreFiles = new[] { new FileInfo(@"E:\OneDrive\Documents\Programming\GitHub\Solidworks Addin\Singularity Addin\Singularity Tests\bin\Debug\Singularity Tests.dll") };
#else
        private readonly FileInfo[] coreFiles = new[] { new new FileInfo(System.IO.Path.Combine(DirectoryPath, "OswCoreCommands.dll"))};
#endif

       

        /// <summary>
        /// List of all plugins to be implemented
        /// </summary>
        public List<DefinedPlugin> Plugins { get; } = new List<DefinedPlugin>();

        /// <summary>
        /// List of plugins to be loaded at users request
        /// </summary>
        public List<DefinedPlugin> UserPlugins { get; } = new List<DefinedPlugin>();

        /// <summary>
        /// The data of the last load, files to avoid and user specifed files
        /// </summary>
        private PluginsLoadData PluginsData { get; }

        /// <summary>
        /// Informs if Solidworks needs to reload menus, this is due to a module having changed.
        /// </summary>
        public bool NeedsReload { get; internal set; }

        private SingleSldWorks SolidWorks { get; }
        internal PluginLoader(SingleSldWorks solidWorks, SingleCommandMgr mgr)
        {
            SolidWorks = solidWorks;
            PluginsData = PluginsLoadData.LoadSettings(WorkingPath);

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
            if (plugFiles.Any()) LoadPluginFromFiles(plugFiles.Values).ForEach(t => Plugins.Add(t));//- and add it too the list of plugins


            //Load any files to user has specifically request to load.
            List<FileInfo> userFiles = new List<FileInfo>();
            foreach (PluginsData pluginsData in PluginsData.UserLoaded)
            {
                if (File.Exists(pluginsData.FullFileName))
                    userFiles.Add(new FileInfo(pluginsData.FullFileName));
            }
            if (userFiles.Any()) LoadPluginFromFiles(userFiles).ForEach(t => Plugins.Add(t));//- and add it too the list of plugins


            //*** Must check versions of files loaded last time vs last time if different 

            //First create a list of files loaded to be stored.
            Dictionary<PluginsData, Boolean> pd = new Dictionary<PluginsData, bool>();
            foreach (DefinedPlugin plugIn in Plugins)
            {
                pd.Add(new PluginsData(plugIn), false);
            }

            KeyValuePair<PluginsData, bool> defaultStrut= new KeyValuePair<PluginsData, bool>();

            //check if all values match, based on the file last loaded
            foreach (PluginsData lL in PluginsData.LastLoadedPlugins)
            {
                //Check last loaded against the list.
                KeyValuePair<PluginsData, bool> fnd = pd.FirstOrDefault(p =>
                    p.Value != true &&
                    p.Key.FullFileName.Equals(lL.FullFileName, StringComparison.CurrentCultureIgnoreCase) &&
                    p.Key.Version == lL.Version &&
                    p.Key.EditDate == lL.EditDate);

                if (fnd.Equals(defaultStrut))
                {
                    pd[fnd.Key] = true;
                }
                else
                {
                    NeedsReload = true;
                    break;
                }
            }
            //Makes sure there is no new files that had nothing compared
            if (!NeedsReload)
            {
                NeedsReload = pd.Values.All(b => b == true);
            }


            //Save new files and blacklisted files to settings
            PluginsData.LastLoadedPlugins = pd.Keys.ToList();
            PluginsData.SaveSettings();

#if DEBUG
            NeedsReload = true;
#endif

        }

       

        #region Private
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
                            // We only want to create an instance if is a normal command, but is not a flyout button - as they will be created differently from the type definition
                            if (BaseFunc.IsAssignableFrom(type) && !FlyOutBtnFunc.IsAssignableFrom(type) && !(type.IsAbstract || type.IsInterface))
                            {
                                try
                                {
                                    //Create the instance of the class & check if its suitable to load for this user
                                    if (Activator.CreateInstance(type) is ISwBaseFunction instance)
                                    {
                                        SingleBaseCommand cmd;
                                        if (instance is ISwFlyOut flyout)
                                        {
                                            cmd = new SingleBaseFlyoutGroup(SolidWorks, flyout, CmdCounter += 1, newPlugin);
                                            //Through each sub button implement it if suitable
                                            foreach (Type subBtnType in flyout.SubButtons)
                                            {
                                                //Check suitability
                                                if (FlyOutBtnFunc.IsAssignableFrom(subBtnType) && !(type.IsAbstract || type.IsInterface) && Activator.CreateInstance(subBtnType) is ISwFlyOutButton subBtn)
                                                {
                                                    SingleBaseFlyoutButtonCommand btnWrap = new SingleBaseFlyoutButtonCommand(SolidWorks, subBtn, CmdCounter += 1, newPlugin, ((SingleBaseFlyoutGroup)cmd));
                                                    ((SingleBaseFlyoutGroup)cmd).SubCommand.Add(btnWrap);
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
                            }
                        }
                        //Check if this file has any commands
                        if (newPlugin.Functions.Any())
                        {
                            found.Add(newPlugin);
                        }
                        else //Add it to the black list of files that are not plugins
                        {
                            PluginsData.BlackListedFiles.Add(dll.FullName);
                        }
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
        /// Searches the nominated directory for any file that is a .dll and is not part of the existing collection,  it will do recursivly for each sub folder
        /// </summary>
        /// <param name="dlls">The DLLS already found</param>
        /// <param name="dirPath">The directory path.</param>
        private void SearchThisDir(ref Dictionary<string, FileInfo> dlls, DirectoryInfo dirPath)
        {
            if (!dirPath.Exists) return;
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
        #endregion
    }
}
