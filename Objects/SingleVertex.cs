using SingularityBase;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;

namespace SingularityCore
{
    internal class SingleVertex : SingularityObject<IVertex>, ISingleVertex
    {
        public SingleVertex(ISingleModelDoc doc, IVertex ver) : base(ver)
        {
            Document = doc;
        }

        public ISingleModelDoc Document { get; }


        public bool Equals(IVertex other)
        {
            throw new NotImplementedException();
        }

        public IList<ISingleEdge> Edges
        {
            get {
                IList<ISingleEdge> edges = new List<ISingleEdge>();
                object[] ed = BaseObject.GetEdges();
                if (ed is IEdge[] e)
                {
                    foreach (IEdge edge in e)
                    {
                        edges.Add(new SingleEdge(Document, edge));
                    }
                }
                return edges;
            }
        }

        public IList<ISingleFace> AdjacentFaces
        {
            get {
                IList<ISingleFace> faces = new List<ISingleFace>();
                object[] fa = BaseObject.GetAdjacentFaces();
                if (fa is IFace2[] f)
                {
                    foreach (IFace2 face2 in f)
                    {
                        faces.Add(new SingleFace(Document, face2));
                    }
                }

                return faces;
            }

        }
        public ISinglePoint GetClosestPointOn(ISinglePoint point)
        {
            object res = BaseObject.GetClosestPointOn(point.X, point.Y, point.Z);
            if (res != null)
            {
                return new SinglePoint((double[])res);
            }

            return null;
        }

        public ISinglePoint Point => new SinglePoint((double[])BaseObject.GetPoint());


    }
}
