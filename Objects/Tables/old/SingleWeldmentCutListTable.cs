using SingularityBase;
using SolidWorks.Interop.sldworks;
using System.Collections.Generic;
using System.Linq;
using SingularityBase.old;
using SolidWorks.Interop.swconst;

#if false
namespace SingularityCore.old
{
    internal class SingleWeldmentCutListTable : ISingleWeldmentCutListTable
    {
        public SingleWeldmentCutListTable(ISingleModelDoc doc, ISingleFeature weldTable)
        {
            Document = doc;
            Feature = weldTable;
            TableFeature = weldTable.Feature.GetSpecificFeature2();
        }

        public ISingleModelDoc Document { get; }
        public ISingleFeature Feature { get; }

        public IWeldmentCutListFeature TableFeature { get; internal set; }

        public ISingleView View
        {
            get
            {
                //Loop through all views looking for the table.
                if (Document.Type == swDocumentTypes_e.swDocDRAWING)
                {
                    ISingleDrawingDoc doc = (ISingleDrawingDoc) Document;
                    var view = doc.GetFirstView;
                    while (view != null)
                    {
                        if (view.GetTableAnnotationCount > 0)
                        {
                            foreach (var tblAnnotation in view.GetTableAnnotations)
                            {
                                
                            }
                        }
                        view = view.GetNextView;
                    }
                }
            }
        }

        object ISingleTable<ISingleWeldmentCutListTable, ISingleWeldmentCutListAnnotation, ISingleCutListRow, ISingleCutListColumn, ISingleCutListCell>.TableFeature => TableFeature;

        public IEnumerable<ISingleWeldmentCutListAnnotation> TableAnnotations
        {
            get {
                List<ISingleWeldmentCutListAnnotation> docs = new List<ISingleWeldmentCutListAnnotation>();
                foreach (IWeldmentCutListAnnotation anno in (IEnumerable<IWeldmentCutListAnnotation>)TableFeature.GetTableAnnotations())
                {
                    docs.Add(new SingleWeldmentCutListAnnotation(this, anno));
                }

                return docs;
            }
        }

        public string Configuration { get => TableFeature.Configuration; set => TableFeature.Configuration = value; }
        public bool KeepCurrentItemNumbers { get => TableFeature.KeepCurrentItemNumbers; set => TableFeature.KeepCurrentItemNumbers = value; }
        public bool KeepMissingItems { get => TableFeature.KeepMissingItems; set => TableFeature.KeepMissingItems = value; }
        public int SequenceStartNumber { get => TableFeature.SequenceStartNumber; set => TableFeature.SequenceStartNumber = value; }
        public bool StrikeoutMissingItems { get => TableFeature.StrikeoutMissingItems; set => TableFeature.StrikeoutMissingItems = value; }




        public void Dispose()
        {
            TableFeature = null;
        }
    }
    internal class SingleWeldmentCutListAnnotation : SingleGenericTableAnnotation<ISingleWeldmentCutListTable, ISingleWeldmentCutListAnnotation, ISingleCutListRow, ISingleCutListColumn, ISingleCutListCell>, ISingleWeldmentCutListAnnotation
    {
        public SingleWeldmentCutListAnnotation(ISingleWeldmentCutListTable table, IWeldmentCutListAnnotation anno) : base(table, (ITableAnnotation)anno)
        {
            Annotation = anno;
            
        }
        public SingleWeldmentCutListAnnotation(ISingleModelDoc doc,IWeldmentCutListAnnotation anno) : base(new SingleWeldmentCutListTable(doc, new SingleFeature(doc, (IFeature)anno.WeldmentCutListFeature)), (ITableAnnotation)anno)
        {
            Annotation = anno;

        }

        public override IEnumerable<ISingleCutListColumn> Columns
        {
            get {
                List<ISingleCutListColumn> cols = new List<ISingleCutListColumn>();
                for (int i = 0; i < ColumnCount; i++)
                {
                    cols.Add(new SingleCutListColumn(this, i));
                }
                return cols;
            }
        }

        public override IEnumerable<ISingleCutListRow> Rows
        {
            get {
                List<ISingleCutListRow> row = new List<ISingleCutListRow>();
                for (int i = 0; i < ColumnCount; i++)
                {
                    row.Add(new SingleCutListRow(this, i));
                }
                return row;
            }
        }

        public new IWeldmentCutListAnnotation Annotation { get; }
        public bool Sort(int columnIndex, bool sortAscending)
        {
            return Annotation.Sort(columnIndex, sortAscending);
        }
    }
    internal class SingleCutListRow : SingleGenericRow<ISingleWeldmentCutListTable, ISingleWeldmentCutListAnnotation, ISingleCutListRow, ISingleCutListColumn, ISingleCutListCell>, ISingleCutListRow
    {
        public SingleCutListRow(ISingleWeldmentCutListAnnotation table, int rowNo) : base(table, rowNo) { }

        public override IEnumerable<ISingleCutListCell> Cells => Columns.Select(col => new SingleCutListCell(this, col)).Cast<ISingleCutListCell>().ToList();

    }
    internal class SingleCutListColumn : SingleGenericColumn<ISingleWeldmentCutListTable, ISingleWeldmentCutListAnnotation, ISingleCutListRow, ISingleCutListColumn, ISingleCutListCell>, ISingleCutListColumn
    {
        public SingleCutListColumn(ISingleWeldmentCutListAnnotation table, int columnNo) : base(table, columnNo)
        {
        }

        public string ColumnCustomProperty
        {
            get => Table.Annotation.GetColumnCustomProperty(ColumnIndex);
            set => Table.Annotation.SetColumnCustomProperty(ColumnIndex, value);
        }
        public int GetAllCustomPropertiesCount => Table.Annotation.GetAllCustomPropertiesCount();
        public IEnumerable<string> GetAllCustomProperties => (string[])Table.Annotation.GetAllCustomProperties();
    }
    internal class SingleCutListCell : SingleGenericCell<ISingleWeldmentCutListTable, ISingleWeldmentCutListAnnotation, ISingleCutListRow, ISingleCutListColumn, ISingleCutListCell>, ISingleCutListCell
    {
        public SingleCutListCell(ISingleCutListRow row, ISingleCutListColumn column) : base(row, column)
        {

        }

    }
}
#endif