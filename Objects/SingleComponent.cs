using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;

namespace SingularityCore
{
    internal class SingleComponent : ISingleComponent, IDisposable
    {

        public IComponent2 Component { get; private set; }
        public SingleComponent(IComponent2 comp)
        {
            Component = comp;
        }

        public string Name => Component.Name2;
        public ISingleModelDoc Document => ((SingleSldWorks)SingleSldWorks.GetSolidworks).ConvertDocument(Component.GetModelDoc2());
        public bool Select(int mark) => Select(true, mark);


        public bool Select(bool append, int mark)
        {
            ISelectData a = Document.SelectionManager.CreateSelectData;
            a.Mark = mark;

            return Component.Select3(append, (SelectData)a);
        }

        public bool DeSelect() => Component.DeSelect();


        public ISingleConfiguration ReferencedConfiguration
        {
            get {
                ISingleModelDoc doc = Document;
                if (doc.Type == swDocumentTypes_e.swDocPART)
                {
                    return ((ISinglePartDoc)doc).Configuration(Component.ReferencedConfiguration);
                }
                else if (doc.Type == swDocumentTypes_e.swDocASSEMBLY)
                {
                    return ((ISingleAssemblyDoc)doc).Configuration(Component.ReferencedConfiguration);
                }

                return null;
            }
        }

        public string ReferencedDisplayState => Component.ReferencedDisplayState2;

        public IEnumerable<ISingleComponent> GetChildren
        {
            get
            {
                List<ISingleComponent> comp = new List<ISingleComponent>();
                foreach (var variable in Component.GetChildren())
                {
                    comp.Add(new SingleComponent((IComponent2) variable));
                }

                return comp;
            }
        }

        public int GetChildrenCount => Component.IGetChildrenCount();
        public bool ExcludedFromBom
        {
            get => Component.ExcludeFromBOM;
            set => Component.ExcludeFromBOM = value;
        }

        public bool IsRoot => Component.IsRoot();
        public bool IsEnvelope => Component.IsEnvelope();
        public ISingleBody GetBody => new SingleBody(Document, Component.GetBody());

        public void Dispose()
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(Component);
            Component = null;
        }
    }
}
