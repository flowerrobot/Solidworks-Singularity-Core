using SingularityBase;
using SolidWorks.Interop.sldworks;
using System.Collections.Generic;

namespace SingularityCore
{
    internal class SingleFace : SingularityEntity<IFace2>, ISingleFace
    {
        public new ISingleModelDoc Document { get; }
        private ISingleFeature _feature;

        public ISingleFeature Feature
        {
            get {
                if (_feature != null) return _feature;
                _feature = new SingleFeature(Document, (IFeature)BaseObject.GetFeature());
                return _feature;
            }
        }

        internal SingleFace(IFace2 face) : base(face)
        {
            Document = ((SingularityEntity<IFace2>)this).Document;
        }
        internal SingleFace(ISingleModelDoc doc, IFace2 face) : base(face)
        {
            Document = doc;
        }

        internal SingleFace(ISingleFeature feat, IFace2 face) : base(face)
        {
            _feature = feat;
            Document = feat.Document;
        }

        public int Id { get => BaseObject.GetFaceId(); set => BaseObject.SetFaceId(value); }

        public ISingleBody Body
        {
            get {
                dynamic bdy = BaseObject.GetBody();
                if (bdy == null) return null;
                return new SingleBody(Document, (IBody2)bdy);
            }
        }


        public double GetArea { get => BaseObject.GetArea(); }
        public int GetEdgeCount => BaseObject.GetEdgeCount();
        public IMathVector Normal => Document.MathUtility.CreateVector(BaseObject.Normal);

        public IList<ISingleEdge> GetEdges
        {
            get {
                IList<ISingleEdge> edges = new List<ISingleEdge>();
                object[] e = BaseObject.GetEdges();
                if (e is IEdge[] edges2)
                {
                    foreach (IEdge edge in edges2)
                    {
                        edges.Add(new SingleEdge(Document, edge));
                    }
                }

                return edges;
            }
        }


        public ISingleFace GetFaceAdjacentToLongestEdge {
            get
            {
                var edges = this.GetEdges;

                ISingleEdge longestEdge = null;
                double lg = 0;
                foreach (ISingleEdge edge in edges)
                {

                    double len = edge.Length;
                    if (len > lg)
                    {
                        longestEdge = edge;
                        lg = len;
                    }
                }

                if (longestEdge == null) return null;

                var ad = longestEdge.GetTwoAdjacentFaces;

                double a1 = ad[0].GetArea;
                double a2 = ad[1].GetArea;

                if (a1 > a2) return ad[1];
                return ad[2];
            }
        }
    }
}
