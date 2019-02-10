using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;

namespace SingularityCore
{
    internal class SingleComponent :SingularityObject<IComponent2>, ISingleComponent, IDisposable
    {

        public SingleComponent(IComponent2 comp) : base(comp){}

        public string Name => BaseObject.Name2;
        public ISingleModelDoc Document => ((SingleSldWorks)SingleSldWorks.GetSolidworks).ConvertDocument((IModelDoc2)BaseObject.GetModelDoc2());
        public bool Select(int mark) => Select(true, mark);


        public bool Select(bool append, int mark)
        {
            var a = Document.SelectionManager.CreateSelectData;
            a.BaseObject.Mark = mark;

            return BaseObject.Select3(append, (SelectData)a.BaseObject);
        }

        public bool DeSelect() => BaseObject.DeSelect();


        public ISingleConfiguration ReferencedConfiguration
        {
            get {
                ISingleModelDoc doc = Document;
                if (doc.Type == swDocumentTypes_e.swDocPART)
                {
                    return ((ISinglePartDoc)doc).Configuration(BaseObject.ReferencedConfiguration);
                }
                else if (doc.Type == swDocumentTypes_e.swDocASSEMBLY)
                {
                    return ((ISingleAssemblyDoc)doc).Configuration(BaseObject.ReferencedConfiguration);
                }

                return null;
            }
        }

        public string ReferencedDisplayState => BaseObject.ReferencedDisplayState2;

        public IEnumerable<ISingleComponent> GetChildren
        {
            get
            {
                List<ISingleComponent> comp = new List<ISingleComponent>();
                foreach (var variable in (object[])BaseObject.GetChildren())
                {
                    comp.Add(new SingleComponent((IComponent2) variable));
                }

                return comp;
            }
        }

        public int GetChildrenCount => BaseObject.IGetChildrenCount();
        public bool ExcludedFromBom
        {
            get => BaseObject.ExcludeFromBOM;
            set => BaseObject.ExcludeFromBOM = value;
        }

        public bool IsRoot => BaseObject.IsRoot();
        public bool IsEnvelope => BaseObject.IsEnvelope();
        public ISingleBody GetBody => new SingleBody(Document, (IBody2)BaseObject.GetBody());


    }
}
