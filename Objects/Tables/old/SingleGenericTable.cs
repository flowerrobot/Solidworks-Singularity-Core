using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using SingularityBase.old;

namespace SingularityCore.old
{
    internal abstract class SingleGenericTableAnnotation<Ttable, Tanno, Trow, Tcol, Tcel> : ISingleTableAnnotation<Ttable, Tanno, Trow, Tcol, Tcel>
        where Trow : ISingleTableRow<Ttable, Tanno, Trow, Tcol, Tcel>
        where Tcol : ISingleTableColumn<Ttable, Tanno, Trow, Tcol, Tcel>
        where Tcel : ISingleTableCell<Ttable, Tanno, Trow, Tcol, Tcel>
        where Tanno : ISingleTableAnnotation<Ttable, Tanno, Trow, Tcol, Tcel>
        where Ttable : ISingleTable<Ttable, Tanno, Trow, Tcol, Tcel>
    {
        protected SingleGenericTableAnnotation(Ttable table, ITableAnnotation anno)
        {
            TableFeature = table;
            TableAnnotation = anno;
        }

        public Ttable TableFeature { get; private set;}
        public ITableAnnotation TableAnnotation { get; private set; }
        public ISingleAnnotation GetAnnotation => null;//=> new SingleAnnotation(TableFeature.Document, this, TableAnnotation.GetAnnotation());
        public swTableAnnotationType_e TableType => (swTableAnnotationType_e)TableAnnotation.Type;

        public abstract IEnumerable<Tcol> Columns { get; }
        public abstract IEnumerable<Trow> Rows { get; }

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

        public bool MoveColumn(Tcol Source, swTableItemInsertPosition_e Where, Tcol Destination)
        {
            throw new NotImplementedException();
        }

        public bool MoveRow(Trow Source, swTableItemInsertPosition_e Where, Trow Destination)
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
            foreach (Trow row in Rows)
            {
                ((List<Tcol>) row.Columns).RemoveAt(index);
            }
            ((List<Tcol>) Columns).RemoveAt(index);

          return  this.TableAnnotation.DeleteColumn(index);
        }

        public bool DeleteRow(int index)
        {
            foreach (Tcol col in Columns)
            {
                ((List<Trow>)col.Rows).RemoveAt(index);
            }
            ((List<Trow>)Rows).RemoveAt(index);

            return this.TableAnnotation.DeleteRow(index);
        }

        public void Dispose()
        {
            TableFeature.Dispose();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(TableAnnotation);
            TableAnnotation = null;
         //   Columns = null;
         //   Rows = null;
            GC.SuppressFinalize(this);
        }

    }

    internal abstract class SingleGenericRow<Ttable, Tanno, Trow, Tcol, Tcel> : ISingleTableRow<Ttable, Tanno, Trow, Tcol, Tcel>, IDisposable
        where Trow : ISingleTableRow<Ttable, Tanno, Trow, Tcol, Tcel>
        where Tcol : ISingleTableColumn<Ttable, Tanno, Trow, Tcol, Tcel>
        where Tcel : ISingleTableCell<Ttable, Tanno, Trow, Tcol, Tcel>
        where Tanno : ISingleTableAnnotation<Ttable, Tanno, Trow, Tcol, Tcel>
        where Ttable : ISingleTable<Ttable, Tanno, Trow, Tcol, Tcel>
    {
        protected SingleGenericRow(Tanno table, int rowNo)
        {
            RowIndex = rowNo;
            Table = table;
        }

        public int RowIndex { get; }

        public Tanno Table { get; }
        public IEnumerable<Tcol> Columns => Table.Columns;
        public abstract IEnumerable<Tcel> Cells { get; }

        public double GetRowHeight() => Table.TableAnnotation.GetRowHeight(RowIndex);

        public double SetRowHeight(double height, swTableRowColSizeChangeBehavior_e Options)
        {
            return Table.TableAnnotation.SetRowHeight(RowIndex, height, (int)Options);
        }

        public double RowVerticalGap { get => Table.TableAnnotation.GetRowVerticalGap(RowIndex); set => Table.TableAnnotation.SetRowVerticalGap(RowIndex, value); }
        public bool LockRowHeight { get => Table.TableAnnotation.GetLockRowHeight(RowIndex); set => Table.TableAnnotation.SetLockRowHeight(RowIndex, value); }
        public bool RowHidden { get => Table.TableAnnotation.RowHidden[RowIndex]; set => Table.TableAnnotation.RowHidden[RowIndex] = value; }
        public bool DeleteRow() => Table.TableAnnotation.DeleteRow(RowIndex);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    internal abstract class SingleGenericColumn<Ttable, Tanno, Trow, Tcol, Tcel> : ISingleTableColumn<Ttable, Tanno, Trow, Tcol, Tcel>, IDisposable
        where Trow : ISingleTableRow<Ttable, Tanno, Trow, Tcol, Tcel>
        where Tcol : ISingleTableColumn<Ttable, Tanno, Trow, Tcol, Tcel>
        where Tcel : ISingleTableCell<Ttable, Tanno, Trow, Tcol, Tcel>
        where Tanno : ISingleTableAnnotation<Ttable, Tanno, Trow, Tcol, Tcel>
        where Ttable : ISingleTable<Ttable, Tanno, Trow, Tcol, Tcel>
    {
        protected SingleGenericColumn(Tanno table, int columnNo)
        {
            ColumnIndex = columnNo;
            Table = table;
        }

        public int ColumnIndex { get; }
        public Tanno Table { get; }
        public IEnumerable<Trow> Rows => Table.Rows;

        public double GetColumnWidth => Table.TableAnnotation.GetColumnWidth(ColumnIndex);

        public double SetColumnWidth(double width, swTableRowColSizeChangeBehavior_e options) => Table.TableAnnotation.SetColumnWidth(ColumnIndex, width, (int) options);
        

        public swTableColumnTypes_e GetColumnType {
            get => (swTableColumnTypes_e)Table.TableAnnotation.GetColumnType(ColumnIndex);
            set => Table.TableAnnotation.SetColumnType(ColumnIndex, (int) value);
        }
        public string GetColumnTitle {
            get =>Table.TableAnnotation.GetColumnTitle(ColumnIndex);
            set => Table.TableAnnotation.SetColumnTitle(ColumnIndex, value);
        }
        public bool GetLockColumnWidth
        {
            get => Table.TableAnnotation.GetLockColumnWidth(ColumnIndex);
            set => Table.TableAnnotation.SetLockColumnWidth(ColumnIndex, value);
        }
        public bool ColumnHidden
        {
            get => Table.TableAnnotation.ColumnHidden[ColumnIndex];
            set => Table.TableAnnotation.ColumnHidden[ColumnIndex]= value;
        }
        public bool DeleteColumn() => Table.DeleteColumn(ColumnIndex);

        public void Dispose()
        {
          GC.SuppressFinalize(this);
        }
    }

    internal abstract class SingleGenericCell<Ttable, Tanno, Trow, Tcol, Tcel> : ISingleTableCell<Ttable, Tanno, Trow, Tcol, Tcel>, IDisposable
        where Trow : ISingleTableRow<Ttable, Tanno, Trow, Tcol, Tcel>
        where Tcol : ISingleTableColumn<Ttable, Tanno, Trow, Tcol, Tcel>
        where Tcel : ISingleTableCell<Ttable, Tanno, Trow, Tcol, Tcel>
        where Tanno : ISingleTableAnnotation<Ttable, Tanno, Trow, Tcol, Tcel>
        where Ttable : ISingleTable<Ttable, Tanno, Trow, Tcol, Tcel>
    {
        internal SingleGenericCell(Trow row, Tcol column)
        {
            Row = row;
            Column = column;
            

            RowIndex = ((Trow)row).RowIndex;
            ColumnIndex = ((Tcol) column).ColumnIndex;
        }
        internal int RowIndex { get; }
        internal int ColumnIndex { get; }
        public Tanno Table => Row.Table;
        public Tcol Column { get; }
        public Trow Row { get; }
        public bool CellUseDocTextFormat
        {
            get => Row.Table.TableAnnotation.GetCellUseDocTextFormat(RowIndex, ColumnIndex);
            set => Row.Table.TableAnnotation.SetCellTextFormat(RowIndex, ColumnIndex, value, CellTextFormat); 
        }
        

        public TextFormat CellTextFormat
        {
            get => (TextFormat)Row.Table.TableAnnotation.GetCellTextFormat(RowIndex, ColumnIndex);
            set => Row.Table.TableAnnotation.SetCellTextFormat(RowIndex, ColumnIndex, CellUseDocTextFormat, value);

        }
        public bool MergeCells(Trow rowEnd, Tcol columnEnd) => Row.Table.TableAnnotation.MergeCells(RowIndex, ColumnIndex, ((Trow) rowEnd).RowIndex,((Tcol) columnEnd).ColumnIndex);
        

        public bool UnMergeCells() => Row.Table.TableAnnotation.UnmergeCells(RowIndex, ColumnIndex);


        public bool IsCellMerged { get; }
        //return Row.Table.Annotation.IsCellMerged(RowIndex, ColumnIndex); }
        

        public bool IsCellTextEditable => Row.Table.TableAnnotation.IsCellTextEditable(RowIndex, ColumnIndex);
        public string Text
        {
            get => Row.Table.TableAnnotation.Text[RowIndex, ColumnIndex];
            set => Row.Table.TableAnnotation.Text[RowIndex, ColumnIndex] = value;
        }

        public string DisplayedText => Row.Table.TableAnnotation.DisplayedText[RowIndex, ColumnIndex];
        public swTextJustification_e TextHorizontalJustification { get=> (swTextJustification_e)Row.Table.TableAnnotation.CellTextHorizontalJustification[RowIndex, ColumnIndex]; set => Row.Table.TableAnnotation.CellTextHorizontalJustification[RowIndex, ColumnIndex] = (int)value; }
        public swTextAlignmentVertical_e TextVerticalJustification { get=> (swTextAlignmentVertical_e)Row.Table.TableAnnotation.CellTextVerticalJustification[RowIndex, ColumnIndex]; set => Row.Table.TableAnnotation.CellTextVerticalJustification[RowIndex, ColumnIndex] = (int)value; }


        public void Dispose()
        {
             GC.SuppressFinalize(this);
        }
    }

}
