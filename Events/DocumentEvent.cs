using SingularityBase;
using SingularityBase.Events;

namespace SingularityCore.Events
{
    /// <summary>
    /// this class exists to get the value from the hash table only when requested to improve preformance
    /// </summary>
    internal class DocumentEvent : IDocumentEvent
    {
        public ISingleSldWorks Solidworks { get; }

        private ISingleModelDoc document;
        public ISingleModelDoc Document => document ?? (document = Solidworks.GetDocumentByName(FileName));

        internal string FileName { get; }
        internal DocumentEvent(string fileName, ISingleSldWorks sw)
        {
            FileName = fileName;
            Solidworks = sw;
        }

        internal DocumentEvent(ISingleModelDoc doc, ISingleSldWorks sw)
        {
            document = doc;
            Solidworks = sw;
        }
    }
}
