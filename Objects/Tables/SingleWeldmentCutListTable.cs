using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SingularityCore
{
    internal class SingleWeldmentCutListTable : SingleFeature, ISingleWeldmentCutListTable
    {
        public IWeldmentCutListFeature TableFeature { get; internal set; }
        public SingleWeldmentCutListTable(ISingleModelDoc doc, ISingleFeature weldTable) : base(doc, weldTable.BaseObject)
        {
            TableFeature = (IWeldmentCutListFeature)weldTable.BaseObject.GetSpecificFeature2();
        }


        public ISingleView View
        {
            get {
                //Loop through all views looking for the table.
                if (Document.Type == swDocumentTypes_e.swDocDRAWING)
                {
                    ISingleDrawingDoc doc = (ISingleDrawingDoc)Document;
                    ISingleView view = doc.GetFirstView;
                    while (view != null)
                    {
                        if (view.GetTableAnnotationCount > 0)
                        {
                            foreach (ISingleTableAnnotation tblAnnotation in view.GetTableAnnotations)
                            {

                                if (tblAnnotation.Type == swTableAnnotationType_e.swTableAnnotation_WeldmentCutList)
                                {
                                    if (((ISingleWeldmentCutListAnnotation)tblAnnotation).Table.Id == Id)
                                        return view;
                                }
                            }
                        }
                        view = view.GetNextView;
                    }
                }
                return null;
            }
        }

        public IList<ISingleWeldmentCutListAnnotation> TableAnnotations
        {
            get {
                List<ISingleWeldmentCutListAnnotation> docs = new List<ISingleWeldmentCutListAnnotation>();
                foreach (object anno in (object[])TableFeature.GetTableAnnotations())
                {
                    docs.Add(new SingleWeldmentCutListAnnotation(this, (IWeldmentCutListAnnotation)anno));
                }

                return docs;
            }
        }
        IList<ISingleTableAnnotation> ISingleTable.TableAnnotations => (IList<ISingleTableAnnotation>)TableAnnotations;


        public string Configuration { get => TableFeature.Configuration; set => TableFeature.Configuration = value; }
        public bool KeepCurrentItemNumbers { get => TableFeature.KeepCurrentItemNumbers; set => TableFeature.KeepCurrentItemNumbers = value; }
        public bool KeepMissingItems { get => TableFeature.KeepMissingItems; set => TableFeature.KeepMissingItems = value; }
        public int SequenceStartNumber { get => TableFeature.SequenceStartNumber; set => TableFeature.SequenceStartNumber = value; }
        public bool StrikeoutMissingItems { get => TableFeature.StrikeoutMissingItems; set => TableFeature.StrikeoutMissingItems = value; }




        public new void Dispose()
        {
            base.Dispose();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(TableFeature);
            TableFeature = null;
        }
    }

    internal class SingleWeldmentCutListAnnotation : SingleTableAnnotation<ISingleCutListColumn,ISingleCutListRow>, ISingleWeldmentCutListAnnotation
    {
        public new IWeldmentCutListAnnotation TableAnnotation { get; }
        public SingleWeldmentCutListAnnotation(ISingleWeldmentCutListTable table, IWeldmentCutListAnnotation anno) : base(table, (ITableAnnotation)anno)
        {
            TableAnnotation = anno;

        }
        public SingleWeldmentCutListAnnotation(ISingleModelDoc doc, IWeldmentCutListAnnotation anno) : base(new SingleWeldmentCutListTable(doc, new SingleFeature(doc, (IFeature)anno.WeldmentCutListFeature.GetFeature())), (ITableAnnotation)anno)
        {
            TableAnnotation = anno;

        }

        IList<ISingleCutListColumn> ISingleWeldmentCutListAnnotation.Columns => (IList<ISingleCutListColumn>)Columns;
        public override IList<ISingleCutListColumn> Columns
        {
            get {
                IList<ISingleCutListColumn> cols = new List<ISingleCutListColumn>();
                for (int i = 0; i < ColumnCount; i++)
                {
                    cols.Add(new SingleCutListColumn(this, i));
                }
                return cols;
            }
        }

        IList<ISingleCutListRow> ISingleWeldmentCutListAnnotation.Rows => (IList<ISingleCutListRow>)Rows;
        public override IList<ISingleCutListRow> Rows
        {
            get {
                IList<ISingleCutListRow> row = new List<ISingleCutListRow>();
                for (int i = 0; i < ColumnCount; i++)
                {
                    row.Add(new SingleCutListRow(this, i));
                }

                return (IList<ISingleCutListRow>)row;
            }
        }




        public bool Sort(int columnIndex, bool sortAscending) => TableAnnotation.Sort(columnIndex, sortAscending);
    }

    internal class SingleCutListRow : SingleGenericRow<ISingleCutListCell>, ISingleCutListRow
    {
        public SingleCutListRow(SingleWeldmentCutListAnnotation table, int rowNo) : base(table, rowNo) { }

        public new ISingleWeldmentCutListAnnotation Annotation => (ISingleWeldmentCutListAnnotation)base.Annotation;


        public new IList<ISingleCutListColumn> Columns => Annotation.Columns;
        IList<ISingleCutListCell> ISingleCutListRow.Cells => (IList<ISingleCutListCell>)Cells;
        public override IList<ISingleCutListCell> Cells
        {
            get {
                //Columns.Select(col => new SingleCutListCell(this, (SingleGenericColumn)col)).Cast<ISingleCutListCell>().ToList();
                List<ISingleCutListCell> cells = new List<ISingleCutListCell>();
                foreach (ISingleCutListColumn col in Columns)
                {
                    cells.Add(new SingleCutListCell(this, (SingleCutListColumn)col));
                }
                return cells;
            }
        }
    }

    internal class SingleCutListColumn : SingleGenericColumn, ISingleCutListColumn
    {
        public SingleCutListColumn(SingleWeldmentCutListAnnotation table, int columnNo) : base(table, columnNo){}

        public new ISingleWeldmentCutListAnnotation Annotation => (ISingleWeldmentCutListAnnotation)base.Annotation;

        public string ColumnCustomProperty
        {
            get => Annotation.TableAnnotation.GetColumnCustomProperty(ColumnIndex);
            set => Annotation.TableAnnotation.SetColumnCustomProperty(ColumnIndex, value);
        }
        public int GetAllCustomPropertiesCount => Annotation.TableAnnotation.GetAllCustomPropertiesCount();
        public IList<string> GetAllCustomProperties => (string[])Annotation.TableAnnotation.GetAllCustomProperties();
    }

    internal class SingleCutListCell : SingleGenericCell, ISingleCutListCell
    {
        public SingleCutListCell(SingleCutListRow row, SingleCutListColumn column) : base(row, column) { }

        public new ISingleWeldmentCutListAnnotation Annotation => (ISingleWeldmentCutListAnnotation)base.Annotation;
        public new ISingleCutListColumn Column => (ISingleCutListColumn)base.Column;
        public new ISingleCutListRow Row => (ISingleCutListRow)base.Row;
    }
}
