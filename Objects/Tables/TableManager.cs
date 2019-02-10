using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SingularityBase;
using SingularityCore.Objects.Tables;

namespace SingularityCore
{
    internal class TableManager : ITableManager
    {
        public TableManager(ISingleModelDoc document)
        {
            Document = document;
        }

        public ISingleModelDoc Document { get; }

        public IList<ISingleWeldmentCutListTable> GetWeldmentTables
        {
            get
            {
                List<ISingleWeldmentCutListTable> tbls = new List<ISingleWeldmentCutListTable>();
                var feat = Document.FeatureManager.GetFirstFeature;
                while (feat != null)
                {
                    if (feat.GetTypeName == FeatureName.WeldmentTableFeat)
                    {
                        tbls.Add(new SingleWeldmentCutListTable(Document, feat));
                    }

                    feat = feat.GetNextFeature;
                }

                return tbls;

            }
        }

        public IList<ISingleBomTable> GetBomTables
        {
            get
            {
                List<ISingleBomTable> tbls = new List<ISingleBomTable>();
                var feat = Document.FeatureManager.GetFirstFeature;
                while (feat != null)
                {
                    if (feat.GetTypeName == FeatureName.BomFeat)
                    {
                        tbls.Add(new SingleBomTable(Document,feat));
                    }

                    feat = feat.GetNextFeature;
                }

                return tbls;
            }
        }


        //public IEnumerable<ISingleTableAnnotation> GetAllTables()
        //{
        //    List<ISingleTableAnnotation> tbls = new List<ISingleTableAnnotation>();
        //    var feat = Document.GetFirstFeature;
        //    while (feat != null)
        //    {
        //        if (feat.GetType == FeatureName.BomFeat)
        //        {

        //        }
        //        feat = Document.GetNextFeature(feat);
        //    }

        //    return tbls;
        //}
    }
}
