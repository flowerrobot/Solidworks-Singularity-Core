using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SingularityCore
{
    internal class SingleBody : SingularityObject<IBody2>, ISingleBody, IDisposable
    {
        public static bool operator ==(SingleBody obj1, SingleBody obj2)
        {
            return obj1?.Name.Equals(obj2?.Name) ?? false;
        }
        public static bool operator !=(SingleBody obj1, SingleBody obj2)
        {
            return !obj1?.Name.Equals(obj2?.Name) ?? false;
        }
        public override bool Equals(object obj)
        {
            return Name.Equals((obj as ISingleBody)?.Name);
        }
        public override int GetHashCode()
        {
            return (Document != null ? Document.GetHashCode() : 0);
        }
        public bool Equals(SingleBody other)
        {
            return Name.Equals(other.Name);
        }
        public bool Equals(ISingleBody other)
        {
            return Name.Equals(other?.Name);
        }

        /// <summary>
        /// Try to avoid this method, as it has to search for the document.
        /// </summary>
        /// <param name="bod"></param>
        public SingleBody(IBody2 bod) : base(bod)
        {
            object[] fces = (object[])bod.GetFaces();
            if (fces == null) throw new Exception("This body has no faces");
            SingleFace face = new SingleFace((IFace2)fces[0]);
            Document = face.Document;
        }

        public SingleBody(ISingleModelDoc doc, IBody2 bod) : base(bod)
        {
            Document = doc;
        }
        public ISingleModelDoc Document { get; }
        public string Name => BaseObject.Name;





        public bool Visible
        {
            get => BaseObject.Visible; set => BaseObject.HideBody(!value);
        }

        public swBodyType_e Type => (swBodyType_e)BaseObject.GetType();

        public IList<ISingleFeature> Features
        {
            get {
                IList<ISingleFeature> feats = new List<ISingleFeature>();
                object[] objs = BaseObject.GetFeatures() as object[];
                if (objs == null) return feats;
                foreach (IFeature fea in objs)
                {

                    feats.Add(new SingleFeature(Document, fea));
                }

                return feats;
            }
        }


        public bool Select(int mark) => Select(false, mark);

        public bool Select(bool append, int mark)
        {
            ISingleBaseObject<ISelectData> a = Document.SelectionManager.CreateSelectData;
            a.BaseObject.Mark = mark;

            return BaseObject.Select2(append, (SelectData)a.BaseObject);
        }

        public bool DeSelect() => BaseObject.DeSelect();


        public IList<ISingleFace> Faces
        {
            get {
                IList<ISingleFace> faces = new List<ISingleFace>();
                object[] fces = (object[])BaseObject.GetFaces();
                if (fces == null) return faces;
                foreach (IFace2 face in fces)
                {
                    faces.Add(new SingleFace(Document, face));
                }

                return faces;
            }
        }

        public ISingleFace GetLargestFace
        {
            get {
                IList<ISingleFace> faces = Faces;

                return (
                    from
                        f in faces
                    orderby
                        f.GetArea descending,
                        f.GetEdgeCount
                    select
                        f
                ).FirstOrDefault();
            }
        }

        public ISingleFace GetLargestOrthogonalFace(ISingleFace originFace)
        {

            IMathVector originFaceNormal = originFace.Normal;
            IList<ISingleFace> faces = Faces;

            //Find the perpendicular face with the largest area and use this as the top face.
            //- Perpendicular faces are determined by comparing face normals, specifically where their dot product = 0.
            //- In addition, we check that the cross product is greater than 0 in case the vectors are parallel.
            //  (Not sure why, but it was in some sample code so...)
            ISingleFace topFace = (
                from
                    face in faces
                let
                    faceNormal = face.Normal
                let
                    dotProduct = originFaceNormal.Dot(faceNormal)
                where
                    Math.Abs(dotProduct) < 0.00000000001
                let
                    crossProduct = (IMathVector)originFaceNormal.Cross(faceNormal)
                where
                    crossProduct.GetLength() > 0.00000000001
                orderby
                    face.GetArea descending
                select
                    face
            ).FirstOrDefault();

            return topFace;
        }

        public ISingleFace GetFaceAdjacentToLongestEdge(ISingleFace originFace) =>originFace.GetFaceAdjacentToLongestEdge;

    
    }
}
