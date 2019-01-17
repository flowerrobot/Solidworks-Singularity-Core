using SingularityBase;
using SingularityBase.Events;

namespace SingularityCore.Events
{
    internal class MissingDocumentEvent : IMissingDocumentEvent
    {
     internal   MissingDocumentEvent(string missingFileName, ISingleSldWorks sld)
        {
            MissingFileName = missingFileName;
        }

        public string MissingFileName { get; }
        public string NewFileName { get; set; }
        public bool UseNewPath { get; set; }
    }
}
