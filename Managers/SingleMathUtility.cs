using SingularityBase;
using SolidWorks.Interop.sldworks;

namespace SingularityCore
{
    internal class SingleMathUtility : SingularityObject<IMathUtility>, ISingleMathUtility
    {
        public ISinglePoint CreatePoint(double x, double y, double z) => new SinglePoint(x,y,z);


        public ISinglePoint CreatePoint(double[] vals) => new SinglePoint(vals);


        public IMathTransform CreateTransform(double[] array) => BaseObject.CreateTransform (array) as IMathTransform;
        
            
        public IMathTransform CreateTransformRotateAxis(ISinglePoint point, IMathVector vector, double angle)
        {
            return BaseObject.CreateTransformRotateAxis(point.ToArray3, vector, angle) as IMathTransform;
        }

        public IMathVector CreateVector(double[] array) => BaseObject.CreateVector(array) as IMathVector;


        public IMathVector CreateVector(ISinglePoint point) => BaseObject.CreateVector(point.ToArray3) as IMathVector;

    }
}
