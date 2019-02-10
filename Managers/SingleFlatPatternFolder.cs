using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SingularityBase;
using SolidWorks.Interop.sldworks;

namespace SingularityCore
{
    internal class SingleFlatPatternFolder :SingularityObject<IFlatPatternFolder>, ISingleFlatPatternFolder
    {
        public SingleFlatPatternFolder(ISinglePartDoc doc,IFlatPatternFolder folder) : base(folder)
        {
            Document = doc;
        }

        private ISingleFeature _feature;

        public ISingleFeature GetFeature
        {
            get
            {
                if (_feature != null) return _feature;
                _feature = new SingleFeature(Document, (IFeature)BaseObject.GetFeature());
                return _feature;
            }
        }

        public int GetFlatPatternCount => BaseObject.GetFlatPatternCount();

        public IList<ISingleFeature> GetFlatPatterns
        {
            get
            {
                IList<ISingleFeature> fea = new List<ISingleFeature>();
                var f = BaseObject.GetFlatPatterns();
                if (f == null) return fea;
                foreach (var bf in (object[])f)
                {
                    fea.Add(new SingleFeature(Document, (IFeature)bf));
                }

                return fea;
            }
        }

        public ISinglePartDoc Document { get; }
    }

   
}
