using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SingularityCore.Plugins
{

    internal class PluginsLoadData
    {
        public PluginsLoadData() { }
        /// <summary>
        /// A list of plugins loaded last time. Used to determine if the UI needs to be reloaded.
        /// </summary>
        public List<PluginsData> LastLoadedPlugins { get; set; } = new List<PluginsData>();

        /// <summary>
        /// A list of .dlls that the user has explicited asked to load, these should be out side the normal search path
        /// </summary>
        public List<PluginsData> UserLoaded { get; } = new List<PluginsData>();

        /// <summary>
        /// A list of .dlls that have been scanned for plugins but were not found.
        /// They are stored in this list so there are not loaded again
        /// This is limited to a full file name
        /// </summary>
        public List<string> BlackListedFiles { get; } = new List<string>();

        /// <summary>
        /// A list of .dlls that regardless of filepath should never be scanned. Path is not taken into consideration.
        /// </summary>
        public List<string> BlackListed { get; } = new List<string>();

        private const string settingName = "pluginData.json";
        private string location;

        private static PluginsLoadData CreateDefault()
        {
            PluginsLoadData pld = new PluginsLoadData();
            //Singularity
            pld.BlackListed.Add("Singularity Addin.dll");
            pld.BlackListed.Add("Singularity Base.dll");
            pld.BlackListed.Add("Singularity Core.dll");
            pld.BlackListed.Add("NLog.dll");
            pld.BlackListed.Add("Newtonsoft.Json.dll");

            //Solidworks
            pld.BlackListed.Add("SolidWorks.Interop.sldworks.dll");
            pld.BlackListed.Add("SolidWorks.Interop.swcommands.dll");
            pld.BlackListed.Add("SolidWorks.Interop.swconst.dll");
            pld.BlackListed.Add("Singularity Addin.dll");
            pld.BlackListed.Add("solidworkstools.dll");

            return pld;
        }
        public static PluginsLoadData LoadSettings(DirectoryInfo pluginFolder)
        {
            string file = Path.Combine(pluginFolder.FullName, settingName);
            if (File.Exists(file))
            {
               var pl = JsonConvert.DeserializeObject<PluginsLoadData>(File.ReadAllText(file));
               if (pl != null)
               {
                   pl.location = file;
                   return pl;
               }
                return CreateDefault();
            }
            return CreateDefault();
        }

        public bool SaveSettings()
        {
            try
            {
                using (StreamWriter file = File.CreateText(location))
                {
                    JsonSerializer js = new JsonSerializer();
                    js.Serialize(file, this);
                }

                return true;
            }
            catch 
            {
                return false;
            }
        }
    }

    /// <summary>
    /// A Class wrapper for Json to represent a file loaded previously.
    /// </summary>
    public class PluginsData
    {
        public PluginsData() { }

        internal PluginsData(DefinedPlugin df)
        {
            FullFileName = df.File.FullName;
            IsLoaded = true;
            Version = df.AssemblyVersion;
            EditDate = df.File.CreationTime.ToLongDateString();
            foreach (var sb in df.Functions)
            {
                CommandNames.Add(sb.Command.CommandName);
            }
        }


        public string FullFileName { get; set; }
        public bool IsLoaded { get; set; }
        public string Version { get; set; }
        public string EditDate { get; set; }

        public List<string> CommandNames { get; } = new List<string>();
    }
}
