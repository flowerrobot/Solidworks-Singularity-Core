using SolidWorks.Interop.sldworks;
using System;
using SingularityBase;

namespace SingularityCore
{

    /// <summary>
    /// This represents a x,y,z point.
    /// Is a wrapper for a math point
    /// </summary>
    internal class SinglePoint : ISingleBaseObject<IMathPoint>, ISinglePoint
    {
        public SinglePoint() { }

        public SinglePoint(double x, double y) : this()
        {
            X = x;
            Y = y;
        }
        public SinglePoint(double[] array) : this()
        {
            ToArray3 = array;
        }

        public SinglePoint(double x, double y, double z) : this(x, y)
        {
            Z = z;

        }

        internal SinglePoint(IMathPoint point) 
        {

        }

        private double _x;
        private double _y;
        private double _z;
        private bool _isReadOnly;


        private ISingleMathUtility _mathUtility;

        public ISingleMathUtility MathUtility =>_mathUtility ?? (_mathUtility = SingleSldWorks.GetSolidworks.MathUtility);

        public double X
        {
            get => _x;
            set {
                if (!_isReadOnly)
                {
                    _x = value;
                }
            }
        }
        public double Y
        {
            get => _y;
            set {
                if (!_isReadOnly)
                {
                    _y = value;
                }
            }
        }
        public double Z
        {
            get => _z;
            set {
                if (!_isReadOnly)
                {
                    _z = value;
                }
            }
        }

        /// <summary>
        /// An array of 2 points (X,Y)
        /// </summary>
        public double[] ToArray2
        {
            get => new[] { X, Y };
            set {
                X = value[0];
                Y = value[1];
            }
        }

        /// <summary>
        /// An array of the 3 points
        /// </summary>
        public double[] ToArray3
        {
            get =>new[] {X, Y, Z};
            set
            {
                X = value[0];
                Y = value[1];
                Z = value[2];
            }
        }

        /// <summary>
        /// One set, all fields are locked
        /// </summary>
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set {
                if (!_isReadOnly)
                {
                    _isReadOnly = value;
                }
            }
        }

        public ISinglePoint Clone => new SinglePoint(X, Y, Z);

        /// <summary>
        /// This is the COM Object. Dealing with the raw object
        /// </summary>
        public IMathPoint BaseObject => (IMathPoint) MathUtility.BaseObject.CreatePoint(ToArray3);
        public void Dispose()
        {
          
        }

        /// <summary>
        /// Translates a math point by a math vector to create a new math point.  
        /// </summary>
        public void AddVector(IMathVector vector)
        {
          ToArray3 =  (BaseObject.AddVector(vector) as IMathPoint)?.ArrayData as double[] ?? ToArray3;
        }

        /// <summary>
        /// Converts a math point to a math vector by using the three coordinates of the math point for the components of the math vector. 
        /// </summary>
        public IMathVector ConvertToVector => SingleSldWorks.GetSolidworks.MathUtility.CreateVector(ToArray3);

        /// <summary>
        /// Multiplies a math point with a math transform; the point is rotated, scaled, and then translated. 
        /// </summary>
        /// <param name=""></param>
        public void MultiplyTransform(IMathTransform  transform)
        {
            ToArray3 =  (BaseObject.MultiplyTransform(transform) as IMathPoint)?.ArrayData as double[] ?? ToArray3;
        }
        /// <summary>
        /// Scales a math point's magnitude.  
        /// </summary>
        /// <param name="scale"></param>
        public void Scale(double scale)
        {
            X = X * scale;
            Y = Y * scale;
            Z = Z * scale;
        }

        /// <summary>
        /// Gets a math vector that describes the difference between the math point magnitude from the calling math point.  
        /// </summary>
        /// <param name="point"></param>
        public void Subtract(ISinglePoint point)
        {
            X -= point.X;
            Y -= point.Y;
            Z -= point.Z;
        }

        /// <summary>
        /// Gets a math point that describes the difference between a math vector's magnitude from the calling math point  
        /// </summary>
        /// <param name="vector"></param>
        public void SubtractVector(IMathVector vector)
        {
            ToArray3 =  (BaseObject.SubtractVector(vector) as IMathPoint)?.ArrayData as double[] ?? ToArray3;
        }
    }
}
