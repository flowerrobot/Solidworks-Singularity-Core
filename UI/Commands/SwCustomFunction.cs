using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SingularityBase;
using SingularityBase.UI.Commands;

namespace SingleCore.Commands
{

    internal abstract class SwCustomFunction : ISwCustomFunction
    {
        /// <inheritdoc />
        public ISingleSldWorks SwApp { get;  }

        /// <inheritdoc />
        public int Id => SwApp.GetNextID;
        /// <inheritdoc />
        public int AddinId => SwApp.AddinId;
        /// <inheritdoc />
        public CommandType CmdType => CommandType.Custom;
        /// <inheritdoc />
        public abstract string CommandName { get; set; }

        public bool SuitableToLoad { get; }

        protected SwCustomFunction(ISingleSldWorks application)
        {
            SwApp = application;
        }
    }
}
