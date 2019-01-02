using System;
using SingularityBase;
using SolidWorks.Interop.sldworks;

namespace SingularityCore
{
    internal class SingleModelView : ISingleModelView
    {

        public SingleModelView(ISingleModelDoc document)
        {
            Document = document;
        }

        public IModelView ModelView { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool EnableGraphicsUpdate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IDisposable DisableGraphicsUpdate => throw new NotImplementedException();

        public ISingleModelDoc Document { get; }

        public void Dispose()
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(ModelView);
            ModelView = null;
        }
    }
}