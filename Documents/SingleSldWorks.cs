using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SingularityCore
{
    internal class SingleSldWorks : ISingleSldWorks, IDisposable
    {

        public SingleSldWorks(ISldWorks swapp, int addinId)
        {
            Solidworks = swapp;
            AddinId = addinId;

            CommandManager = Solidworks.GetCommandManager(AddinId);
        }
        public static ISingleSldWorks GetSolidworks { get; }
        public ISldWorks Solidworks { get; private set; }
        /// <inheritdoc />
        public int AddinId { get; }

        private int idCount = 0;
        /// <inheritdoc />
        public int GetNextID => idCount += 1;
        /// <inheritdoc />
        public CommandManager CommandManager { get; private set; }


        /// <inheritdoc />
        public ISingleModelDoc GetDocumentByName(string fileName)
        {
            object docs = Solidworks.GetDocuments();
            foreach (object doc in (object[])docs)
            {
                if (System.IO.Path.GetFileName((doc as ModelDoc2)?.GetPathName() ?? "").Equals(fileName, StringComparison.CurrentCultureIgnoreCase))
                    return ConvertDocument((ModelDoc2)doc);

            }
            return null;
        }

        /// <inheritdoc />
        private ISingleModelDoc ConvertDocument(ModelDoc2 doc)
        {
            switch ((swDocumentTypes_e)((ModelDoc2)doc).GetType())
            {
                case swDocumentTypes_e.swDocASSEMBLY:
                    return new SingleAssemblyDoc((AssemblyDoc)doc);
                case swDocumentTypes_e.swDocDRAWING:
                    return new SingleDrawingDoc((DrawingDoc)doc);
                case swDocumentTypes_e.swDocPART:
                    return new SinglePartDoc((PartDoc)doc);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

       

        public void Dispose()
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(CommandManager);
            CommandManager = null;
            System.Runtime.InteropServices.Marshal.ReleaseComObject(Solidworks);
            Solidworks = null;
        }
    }
}
