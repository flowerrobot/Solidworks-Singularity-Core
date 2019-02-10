using SingularityBase;
using SolidWorks.Interop.sldworks;
using System;

namespace SingularityCore
{
    /// <summary>
    /// A base solidworks object. This wrapper handle disposal of the com object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SingularityObject<T> : ISingleBaseObject<T>
    {
        /// <summary>
        /// You must set the base object your self
        /// </summary>
        internal SingularityObject() { }

        public SingularityObject(object obj)
        {
            BaseObject = (T)obj;



        }

        public SingularityObject(T obj)
        {
            BaseObject = obj;
        }
        /// <summary>
        /// This is the COM Object. Dealing with the raw object
        /// </summary>
        public T BaseObject { get; internal set; }


        public void Dispose()
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(BaseObject);
            BaseObject = default(T);
        }

    }

}
