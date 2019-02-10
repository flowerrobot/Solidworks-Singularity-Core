using SingularityBase;
using SolidWorks.Interop.sldworks;
using System;

namespace SingularityCore
{
    /// <summary>
    /// A base solidworks object that is derived from the IEntity class. This wrapper handle disposal of the com object
    /// This is used for face, edge or vertex or loop2
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SingularityEntity<T> : SingularityObject<T>, ISingleEntity
    {
        /// <summary>
        /// You must set the base object your self
        /// </summary>
        internal SingularityEntity() { }

        public SingularityEntity(object obj) : base(obj) { }

        public SingularityEntity(T obj) : base(obj) { }


        public bool Select(int mark) => Select(false, mark);

        public bool Select(bool append, int mark)
        {
            ISingleBaseObject<ISelectData> a = Document.SelectionManager.CreateSelectData;
            a.BaseObject.Mark = mark;

            return ((IEntity)BaseObject).Select4(append, (SelectData)a.BaseObject);
        }
        //TODO fix this
        public bool DeSelect() { return true; }





        public bool IsSafe => ((IEntity)BaseObject).IsSafe;
        public ISingleBaseObject<IAttribute> FindAttribute(AttributeDef def, int instance)
        {
            return new SingularityObject<IAttribute>(((IEntity) BaseObject).FindAttribute(def, instance));
        }

        public  ISingleModelDoc Document => Component?.Document ?? DrawingComponent.Component.Document;

        private ISingleComponent _component;
        public ISingleComponent Component
        {
            get {
                if (_component != null) return _component;
                IComponent2 cmp = ((IEntity)BaseObject).GetComponent() as IComponent2;
                if (cmp == null) return null;
                _component = new SingleComponent(cmp);
                return _component;
            }
        }

        private ISingleDrawingComponent _drawingComponent;
        public ISingleDrawingComponent DrawingComponent
        {
            get {
                if (_component != null) return _drawingComponent;
                IDrawingComponent cmp = ((IEntity)BaseObject).GetComponent() as IDrawingComponent;
                if (cmp == null) return null;
                _drawingComponent = new SingleDrawingComponent(cmp);
                return _drawingComponent;
            }
        }

        public bool GetDistance(ISingleEntity ent, bool minDistance, object parameter, out double[] position1, out double[] position2, out double distance)
        {
            int res = ((IEntity)BaseObject).GetDistance(ent, minDistance, parameter, out object obj1, out object obj2, out distance);
            position1 = obj1 as double[];
            position2 = obj2 as double[];

            return res == 0;
        }
    }
}
