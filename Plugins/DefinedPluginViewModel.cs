using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SingularityBase.UI;
using SingularityCore.UI;

namespace SingularityCore.Loader
{
    /// <summary>
    /// View Model for DefinedPlugin
    /// </summary>
    internal class DefinedPluginViewModel
    {
        public DefinedPlugin Model { get; }



        public string FileName { get; }
        public string Version { get; }
        public string Description { get; }

        public ObservableCollection<CommandViewModel> Commands { get; }


        public DefinedPluginViewModel()
        {
            Commands = new ObservableCollection<CommandViewModel>();
        }
        public DefinedPluginViewModel(DefinedPlugin model) : this()
        {
            Model = model;

            FileName = model.File.Name;
            Version = model.AssemblyVersion;

            Description = model.AssemblyDescription;

        }
    }

    /// <summary>
    /// View model for SingleBaseCommand;
    /// </summary>
    public class CommandViewModel
    {
        public ISingleCommandDef Function { get; }
        public CommandViewModel() { }

        public CommandViewModel(ISingleCommandDef function) : this()
        {
            Function = function;
            Id = function.Id;
            CommandType = function.CmdType.ToString();
            CommandName = function.Command.CommandName;
        }
        public int Id { get; }
        public string CommandType { get; }
        public string CommandName { get; }
    }
}
