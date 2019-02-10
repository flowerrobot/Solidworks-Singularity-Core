using SingularityBase;
using SolidWorks.Interop.sldworks;
using System;
using SolidWorks.Interop.swconst;

namespace SingularityCore
{
    internal class SingleEdge : SingularityObject<IEdge>, IDisposable, ISingleEdge
    {

        public ISingleModelDoc Document { get; }
        public SingleEdge(ISingleModelDoc doc, IEdge edge) : base(edge)
        {
            Document = doc;
        }
        

        public bool Equals(IEdge other)
        {
            throw new NotImplementedException();
        }

   
        public bool Highlight {  set=>BaseObject.Highlight(value); }
        public ISinglePoint GetClosestPointOn(ISinglePoint point)
        {
            object res = BaseObject.GetClosestPointOn(point.X, point.Y, point.Z);
            if (res != null)
            {
                return new SinglePoint((double[])res);
            }

            return null;
        }

        public ICurve GetCurve => BaseObject.IGetCurve();
        public ISingleVertex GetEndVertex => new SingleVertex(Document, BaseObject.GetEndVertex());
        public ISingleVertex GetStartVertex => new SingleVertex(Document, BaseObject.GetStartVertex());
        public ISingleBody Body => new SingleBody(Document, BaseObject.GetBody());
        /// <summary>
        /// Gets the two faces adjacent to an edge.  
        /// </summary>
        public ISingleFace[] GetTwoAdjacentFaces
        {
            get
            {
                IFace2[] faces = BaseObject.GetTwoAdjacentFaces2();
                return new[] {(ISingleFace)new SingleFace(Document, faces[0]), (ISingleFace)new SingleFace(Document, faces[1])};
            }
        }

        public double Length
        {
            get
            {
                 ICurveParamData curveParameters = BaseObject.GetCurveParams3();

                return GetCurve.GetLength3(curveParameters.UMinValue, curveParameters.UMaxValue);
            }
        }
    }
}
