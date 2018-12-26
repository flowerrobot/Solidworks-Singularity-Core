using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SingularityCore
{
    internal class SingleSldWorks : ISingleSldWorks
    {

        public SingleSldWorks(ISldWorks swapp, int addinId)
        {
            Solidworks = swapp;
            AddinId = addinId;
            Ribbons = new List<IRibbonCollection>().AsReadOnly();
        }
       public static ISingleSldWorks GetSolidworks { get; }
        public ISldWorks Solidworks { get; }
        /// <inheritdoc />
        public int AddinId { get; }

        private int idCount = 0;
        /// <inheritdoc />
        public int GetNextID => idCount += 1;
        /// <inheritdoc />
        public CommandManager CommandManager => Solidworks.GetCommandManager(AddinId);


        /// <inheritdoc />
        public ISingleModelDoc GetDocumentByName(string fileName)
        {
            object docs = Solidworks.GetDocuments();
            foreach (var doc in (object[])docs)
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

        /// <inheritdoc />
        public ReadOnlyCollection<IRibbonCollection> Ribbons { get; private set; }
        /// <inheritdoc />
        public IRibbonCollection GetRibbonByName(string name = "", bool createIfMissing = true)
        {
            if (string.IsNullOrWhiteSpace(name)) name = CommandMgr.DefaultRibbonName; //Assign default

            IRibbonCollection res = Ribbons.FirstOrDefault(r => r.RibbonName.Equals(name));
            if (res == null && createIfMissing)
            {
                res = new RibbonCollection(name, this);
                List<IRibbonCollection> lst = Ribbons.ToList();
                lst.Add(res);

                Ribbons = lst.AsReadOnly();
            }
            return res;

        }


        public Boolean BeginSetup()
        {

        }
        public bool DisconnectFromSW()
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(iCmdMgr);
            iCmdMgr = null;
            System.Runtime.InteropServices.Marshal.ReleaseComObject(iSwApp);
            iSwApp = null;
        }
    }
}
