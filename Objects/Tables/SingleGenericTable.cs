using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;

namespace SingularityCore
{
    internal abstract class SingleTableAnnotation : ISingleTableAnnotation
        {
        public ISingleTable Table { get; private set; }
        public ITableAnnotation TableAnnotation { get; private set; }
        protected SingleTableAnnotation(ISingleTable table, ITableAnnotation anno)
        {
            Table = table;
            TableAnnotation = anno;
        }

        
        public ISingleAnnotation GetAnnotation { get; }//=> new SingleAnnotation(Table.Document, this, TableAnnotation.GetAnnotation());
        public swTableAnnotationType_e TableType => (swTableAnnotationType_e)TableAnnotation.Type;

        public abstract IEnumerable<ISingleTableColumn> Columns { get; }
        public abstract IEnumerable<ISingleTableRow> Rows { get; }

        public bool UseDocTextFormat
        {
            get => TableAnnotation.GetUseDocTextFormat();
            set => TableAnnotation.SetTextFormat(value, GetTextFormat);
        }

        public TextFormat GetTextFormat
        {
            get => TableAnnotation.GetTextFormat(); set => TableAnnotation.SetTextFormat(false, value);
        }
        public int HeaderCount
        {
            get => TableAnnotation.GetHeaderCount();
            set => TableAnnotation.SetHeader((int)HeaderStyle, value);
        }
        public swTableHeaderPosition_e HeaderStyle { get => (swTableHeaderPosition_e)TableAnnotation.GetHeaderStyle(); set => TableAnnotation.SetHeader((int)value, HeaderCount); }
        public swTableSplitDirection_e GetSplitInformation(out int index, out int count, out int rangeStart, out int rangeEnd)
        {
            index = count = rangeStart = rangeEnd = 0;
            return (swTableSplitDirection_e)TableAnnotation.GetSplitInformation(ref index, ref count, ref rangeStart, ref rangeEnd);
        }

        public TableAnnotation Split(swTableSplitLocations_e Where, int Index) => TableAnnotation.Split((int)Where, Index);

        public bool Merge(swTableMergeLocations_e @where) => TableAnnotation.Merge((int)@where);

        public bool InsertColumn(swTableItemInsertPosition_e @where, int index, string name, swInsertTableColumnWidthStyle_e widthStyle) => TableAnnotation.InsertColumn2((int)@where, index, name, (int)widthStyle);

        public bool InsertRow(swTableItemInsertPosition_e Where, int Index) => TableAnnotation.InsertRow((int)Where, Index);

        public bool MoveColumn(ISingleTableColumn Source, swTableItemInsertPosition_e Where, ISingleTableColumn Destination)
        {
            throw new NotImplementedException();
        }

        public bool MoveRow(ISingleTableRow Source, swTableItemInsertPosition_e Where, ISingleTableRow Destination)
        {
            throw new NotImplementedException();
        }

        public bool SaveAsTemplate(string FileName) => TableAnnotation.SaveAsTemplate(FileName);

        public bool SaveAsText(string FileName, string Separator) => TableAnnotation.SaveAsText(FileName, Separator);

        public bool SaveAsPdf(string fileName) => TableAnnotation.SaveAsPDF(fileName);

        public object HorizontalAutoSplit(int MaxNumberOfRows, swHorizontalAutoSplitApply_e Apply, swHorizontalAutoSplitPlacementOfSplitTable_e PlacementOfNewSplitTables) => TableAnnotation.HorizontalAutoSplit((int)MaxNumberOfRows, (int)Apply, (int)PlacementOfNewSplitTables);

        public swTableAnnotationType_e Type => (swTableAnnotationType_e)TableAnnotation.Type;
        public int BorderLineWeight { get => TableAnnotation.BorderLineWeight; set => TableAnnotation.BorderLineWeight = value; }
        public int GridLineWeight { get => TableAnnotation.GridLineWeight; set => TableAnnotation.GridLineWeight = value; }
        public swBOMConfigurationAnchorType_e AnchorType { get => (swBOMConfigurationAnchorType_e)TableAnnotation.AnchorType; set => TableAnnotation.AnchorType = (int)value; }
        public swTextAlignmentVertical_e TextVerticalJustification { get => (swTextAlignmentVertical_e)TableAnnotation.TextVerticalJustification; set => TableAnnotation.TextVerticalJustification = (int)value; }

        public swTextJustification_e TextHorizontalJustification
        {
            get => (swTextJustification_e)TableAnnotation.TextHorizontalJustification;
            set => TableAnnotation.TextHorizontalJustification = (int)value;
        }


        public int ColumnCount => TableAnnotation.TotalColumnCount;
        public int RowCount => TableAnnotation.TotalRowCount;
        public string Title
        {
            get => TableAnnotation.Title;
            set => TableAnnotation.Title = value;
        }
        public bool TitleVisible { get => TableAnnotation.TitleVisible; set => TableAnnotation.TitleVisible = value; }
        public bool Anchored { get => TableAnnotation.Anchored; set => TableAnnotation.Anchored = value; }
        public double BorderLineWeightCustom { get => TableAnnotation.BorderLineWeightCustom; set => TableAnnotation.BorderLineWeightCustom = value; }
        public double GridLineWeightCustom
        {
            get => TableAnnotation.GridLineWeightCustom;
            set => TableAnnotation.GridLineWeightCustom = value;
        }
        public bool StopAutoSplitting { get => TableAnnotation.StopAutoSplitting; set => TableAnnotation.StopAutoSplitting = value; }


        public bool DeleteColumn(int index)
        {
            foreach (SingleGenericRow row in Rows)
            {
                ((List<SingleGenericColumn>) row.Columns).RemoveAt(index);
            }
            ((List<SingleGenericColumn>) Columns).RemoveAt(index);

          return  this.TableAnnotation.DeleteColumn(index);
        }

        public bool DeleteRow(int index)
        {
            foreach (SingleGenericColumn col in Columns)
            {
                ((List<SingleGenericRow>)col.Rows).RemoveAt(index);
            }
            ((List<SingleGenericRow>)Rows).RemoveAt(index);

            return this.TableAnnotation.DeleteRow(index);
        }

        public void Dispose()
        {
           // Table.Dispose();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(TableAnnotation);
            TableAnnotation = null;
         //   Columns = null;
         //   Rows = null;
            GC.SuppressFinalize(this);
        }

    }

    internal abstract class SingleGenericRow : ISingleTableRow, IDisposable
    {
        protected SingleGenericRow(ISingleTableAnnotation table, int rowNo)
        {
            RowIndex = rowNo;
            Annotation = table;
        }

        public int RowIndex { get; }

        public ISingleTableAnnotation Annotation { get; }
        public IEnumerable<ISingleTableColumn> Columns => Annotation.Columns;
        public abstract IEnumerable<ISingleTableCell> Cells { get; }

        public double GetRowHeight() => Annotation.TableAnnotation.GetRowHeight(RowIndex);

        public double SetRowHeight(double height, swTableRowColSizeChangeBehavior_e Options)
        {
            return Annotation.TableAnnotation.SetRowHeight(RowIndex, height, (int)Options);
        }

        public double RowVerticalGap { get => Annotation.TableAnnotation.GetRowVerticalGap(RowIndex); set => Annotation.TableAnnotation.SetRowVerticalGap(RowIndex, value); }
        public bool LockRowHeight { get => Annotation.TableAnnotation.GetLockRowHeight(RowIndex); set => Annotation.TableAnnotation.SetLockRowHeight(RowIndex, value); }
        public bool RowHidden { get => Annotation.TableAnnotation.RowHidden[RowIndex]; set => Annotation.TableAnnotation.RowHidden[RowIndex] = value; }
        public bool DeleteRow() => Annotation.TableAnnotation.DeleteRow(RowIndex);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    internal abstract class SingleGenericColumn : ISingleTableColumn, IDisposable
    {
        protected SingleGenericColumn(SingleTableAnnotation table, int columnNo)
        {
            ColumnIndex = columnNo;
            Annotation = table;
        }

        public int ColumnIndex { get; }
        public ISingleTableAnnotation Annotation { get; }
        public IEnumerable<ISingleTableRow> Rows => Annotation.Rows;

        public double GetColumnWidth => Annotation.TableAnnotation.GetColumnWidth(ColumnIndex);
        public double SetColumnWidth(double width, swTableRowColSizeChangeBehavior_e options) => Annotation.TableAnnotation.SetColumnWidth(ColumnIndex, width, (int) options);
        

        public swTableColumnTypes_e GetColumnType {
            get => (swTableColumnTypes_e)Annotation.TableAnnotation.GetColumnType(ColumnIndex);
            set => Annotation.TableAnnotation.SetColumnType(ColumnIndex, (int) value);
        }
        public string GetColumnTitle {
            get =>Annotation.TableAnnotation.GetColumnTitle(ColumnIndex);
            set => Annotation.TableAnnotation.SetColumnTitle(ColumnIndex, value);
        }
        public bool GetLockColumnWidth
        {
            get => Annotation.TableAnnotation.GetLockColumnWidth(ColumnIndex);
            set => Annotation.TableAnnotation.SetLockColumnWidth(ColumnIndex, value);
        }
        public bool ColumnHidden
        {
            get => Annotation.TableAnnotation.ColumnHidden[ColumnIndex];
            set => Annotation.TableAnnotation.ColumnHidden[ColumnIndex]= value;
        }
        public bool DeleteColumn() => Annotation.DeleteColumn(ColumnIndex);

        public void Dispose()
        {
          GC.SuppressFinalize(this);
        }
    }

    internal abstract class SingleGenericCell: ISingleTableCell
      
    {
        internal SingleGenericCell(SingleGenericRow row, SingleGenericColumn column)
        {
            Row = row;
            Column = column;
            

            RowIndex = ((SingleGenericRow)row).RowIndex;
            ColumnIndex = ((SingleGenericColumn) column).ColumnIndex;
        }
        internal int RowIndex { get; }
        internal int ColumnIndex { get; }
        public ISingleTableAnnotation Annotation => Row.Annotation;
        public ISingleTableColumn Column { get; }
        public ISingleTableRow Row { get; }
        public bool CellUseDocTextFormat
        {
            get => Row.Annotation.TableAnnotation.GetCellUseDocTextFormat(RowIndex, ColumnIndex);
            set => Row.Annotation.TableAnnotation.SetCellTextFormat(RowIndex, ColumnIndex, value, CellTextFormat); 
        }
        

        public TextFormat CellTextFormat
        {
            get => (TextFormat)Row.Annotation.TableAnnotation.GetCellTextFormat(RowIndex, ColumnIndex);
            set => Row.Annotation.TableAnnotation.SetCellTextFormat(RowIndex, ColumnIndex, CellUseDocTextFormat, value);

        }
        public bool MergeCells(ISingleTableRow rowEnd, ISingleTableColumn columnEnd) => Row.Annotation.TableAnnotation.MergeCells(RowIndex, ColumnIndex, ((SingleGenericRow) rowEnd).RowIndex,((SingleGenericColumn) columnEnd).ColumnIndex);
        

        public bool UnMergeCells() => Row.Annotation.TableAnnotation.UnmergeCells(RowIndex, ColumnIndex);


        public bool IsCellMerged { get; }
        //return Row.Table.Annotation.IsCellMerged(RowIndex, ColumnIndex); }
        

        public bool IsCellTextEditable => Row.Annotation.TableAnnotation.IsCellTextEditable(RowIndex, ColumnIndex);
        public string Text
        {
            get => Row.Annotation.TableAnnotation.Text[RowIndex, ColumnIndex];
            set => Row.Annotation.TableAnnotation.Text[RowIndex, ColumnIndex] = value;
        }

        public string DisplayedText => Row.Annotation.TableAnnotation.DisplayedText[RowIndex, ColumnIndex];
        public swTextJustification_e TextHorizontalJustification { get=> (swTextJustification_e)Row.Annotation.TableAnnotation.CellTextHorizontalJustification[RowIndex, ColumnIndex]; set => Row.Annotation.TableAnnotation.CellTextHorizontalJustification[RowIndex, ColumnIndex] = (int)value; }
        public swTextAlignmentVertical_e TextVerticalJustification { get=> (swTextAlignmentVertical_e)Row.Annotation.TableAnnotation.CellTextVerticalJustification[RowIndex, ColumnIndex]; set => Row.Annotation.TableAnnotation.CellTextVerticalJustification[RowIndex, ColumnIndex] = (int)value; }


        public void Dispose()
        {
             GC.SuppressFinalize(this);
        }
    }

}
