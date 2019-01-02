using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SingularityBase.UI;
using SingularityCore.UI;

namespace SingularityCore
{
    /// <summary>
    /// This surmises the assembly of the plugin with metadata and commands and functions extracted.
    /// </summary>
    class DefinedPlugin
    {
        /// <summary>
        /// Location of the file
        /// </summary>
        public FileInfo File { get; }

        /// <summary>
        /// Loaded assembly
        /// </summary>
        public Assembly Assembly { get; }

        /// <summary>
        /// Command function
        /// </summary>
        public List<ISingleCommandDef> Functions { get; } = new List<ISingleCommandDef>();

        /// <summary>
        /// Version of the assembly
        /// </summary>
        public string AssemblyVersion => _fvi.FileVersion;

        /// <summary>
        /// Description of the assembly
        /// </summary>
        public string AssemblyDescription => _fvi.FileDescription;

        /// <summary>
        /// Indicates if an enabled command, takes into account must load
        /// </summary>

        internal DefinedPlugin(Assembly assembly) //, FileInfo loadedFrom, string version = "")
        {
            Assembly = assembly;
            _fvi = FileVersionInfo.GetVersionInfo(Assembly.Location);
            File = new FileInfo(assembly.Location);
        }

        private readonly FileVersionInfo _fvi;
    }
}
