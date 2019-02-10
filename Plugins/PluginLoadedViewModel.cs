using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SingularityCore.Loader
{
    internal class PluginLoadedViewModel
    {
        public ObservableCollection<Object> LoadedModules { get; }


        public PluginLoadedViewModel()
        {
            LoadedModules  = new ObservableCollection<object>();
        }
    }
}
