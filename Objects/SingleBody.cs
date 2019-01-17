using System;
using SingularityBase;
using SolidWorks.Interop.sldworks;

namespace SingularityCore
{
    internal class SingleBody : SingularityBase.ISingleBody , IDisposable
    {
        public SingleBody(ISingleModelDoc doc, IBody2 bod)
        {
            Document = doc;
            Body = bod;
        }
        public IBody2 Body { get; private set; }
        public ISingleModelDoc Document { get; }
        public string Name => Body.Name;

        public void Dispose()
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(Body);
            Body = null;
        }
    }
}
