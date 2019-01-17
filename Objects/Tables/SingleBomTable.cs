using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Collections.Generic;

namespace SingularityCore
{
    internal class SingleBomTable : SingleFeature, ISingleBomTable
    {
        public IBomFeature TableFeature { get; private set; }
        public SingleBomTable(ISingleModelDoc doc, ISingleFeature feat) : base(doc, feat.Feature)
        {
            TableFeature = (IBomFeature)feat.Feature.GetSpecificFeature2();
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
                                if (tblAnnotation.Type == swTableAnnotationType_e.swTableAnnotation_BillOfMaterials)
                                {
                                    if (((ISingleBomTableAnnotation) tblAnnotation).Table.Id == Id)
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

        IEnumerable<ISingleBomTable> ISingleBomTable.TableAnnotations => (IEnumerable<ISingleBomTable>)TableAnnotations;
        public IEnumerable<ISingleTableAnnotation> TableAnnotations
        {
            get {
                //TODO finish
                //(from bm in (IBomTableAnnotation[])TableFeature.GetTableAnnotations() select new SingleBomTableAnnotation(this, bm)).Cast<ISingleBomTableAnnotation>().ToList();
                return null;
            }
        }
        

        



        public int GetConfigurationCount(bool onlyVisible) => TableFeature.GetConfigurationCount(onlyVisible);

        public IEnumerable<string> GetConfigurations(bool onlyVisible, out IEnumerable<bool> isVisible)
        {
            object vis = null;
            string[] res = (string[])TableFeature.GetConfigurations(onlyVisible, ref vis);
            isVisible = (bool[])vis;
            return res;
        }

        public bool SetConfigurations(bool onlyVisible, IEnumerable<bool> visible, IEnumerable<string> configurations)
        {
            return TableFeature.SetConfigurations(onlyVisible, (object)visible, (object)configurations);
        }

        public string Configuration => TableFeature.Configuration;
        public bool FollowAssemblyOrder { get => TableFeature.FollowAssemblyOrder2; set => TableFeature.FollowAssemblyOrder2 = value; }
        public ISingleModelDoc ReferencedModelName => Document.SldWorks.GetDocumentByName(TableFeature.GetReferencedModelName());
        public bool DetailedCutList { get => TableFeature.DetailedCutList; set => TableFeature.DetailedCutList = value; }
        public bool DisplayAsOneItem { get => TableFeature.DisplayAsOneItem; set => TableFeature.DisplayAsOneItem = value; }
        public bool KeepCurrentItemNumbers { get => TableFeature.KeepCurrentItemNumbers; set => TableFeature.KeepCurrentItemNumbers = value; }
        public bool KeepMissingItems { get => TableFeature.KeepMissingItems; set => TableFeature.KeepMissingItems = value; }
        public swKeepReplacedCompOption_e KeepReplacedCompOption { get => (swKeepReplacedCompOption_e)TableFeature.KeepReplacedCompOption; set => TableFeature.KeepReplacedCompOption = (int)value; }
        public swNumberingType_e NumberingTypeOnIndentedBom { get => (swNumberingType_e)TableFeature.NumberingTypeOnIndentedBOM; set => TableFeature.NumberingTypeOnIndentedBOM = (int)value; }
        public swPartConfigurationGroupingOption_e PartConfigurationGrouping { get => (swPartConfigurationGroupingOption_e)TableFeature.PartConfigurationGrouping; set => TableFeature.PartConfigurationGrouping = (int)value; }
        public swRoutingComponentGroupingOption_e RoutingComponentGrouping { get => (swRoutingComponentGroupingOption_e)TableFeature.RoutingComponentGrouping; set => TableFeature.RoutingComponentGrouping = (int)value; }
        public int SequenceStartNumber { get => TableFeature.SequenceStartNumber; set => TableFeature.SequenceStartNumber = value; }
        public bool StrikeoutMissingItems { get => TableFeature.StrikeoutMissingItems; set => TableFeature.StrikeoutMissingItems = value; }
        public swBomType_e TableDisplayType => (swBomType_e)TableFeature.TableType;
        public swZeroQuantityDisplay_e ZeroQuantityDisplay { get => (swZeroQuantityDisplay_e)TableFeature.ZeroQuantityDisplay; set => TableFeature.ZeroQuantityDisplay = (int)value; }



        public new void Dispose()
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(TableFeature);
            TableFeature = null;
            base.Dispose();
        }


    }

    internal class SingleBomTableAnnotation : SingleTableAnnotation, ISingleBomTableAnnotation
    {

        public new IBomTableAnnotation TableAnnotation { get; }
        public SingleBomTableAnnotation(SingleBomTable table, IBomTableAnnotation anno) : base(table, (ITableAnnotation)anno)
        {
            TableAnnotation = anno;
        }

        IEnumerable<ISingleBomColumn> ISingleBomTableAnnotation.Columns => (IEnumerable<ISingleBomColumn>)Columns;
        public override IEnumerable<ISingleTableColumn> Columns
        {
            get {
                List<ISingleTableColumn> cols = new List<ISingleTableColumn>();
                for (int i = 0; i < ColumnCount; i++)
                {
                    cols.Add(new SingleBomColumn(this, i));
                }
                return cols;
            }
        }

        IEnumerable<ISingleBomRow> ISingleBomTableAnnotation.Rows => (IEnumerable<ISingleBomRow>)Rows;
        public override IEnumerable<ISingleTableRow> Rows
        {
            get {
                List<ISingleTableRow> row = new List<ISingleTableRow>();
                for (int i = 0; i < ColumnCount; i++)
                {
                    row.Add(new SingleBomRow(this, i));
                }
                return row;
            }
        }

        public BomTableSortData BomTableSortData
        {
            get => TableAnnotation.GetBomTableSortData();
            set {
                bool res = value.SaveCurrentSortParameters; //Set is not actually defined
                TableAnnotation.Sort(value);
                TableAnnotation.ApplySavedSortScheme(value);
            }
        }

    }

    internal class SingleBomRow : SingleGenericRow, ISingleBomRow
    {
        public SingleBomRow(ISingleBomTableAnnotation table, int rowNo) : base(table, rowNo) { }

        public new ISingleBomTableAnnotation Annotation => (ISingleBomTableAnnotation)base.Annotation;
        public new IEnumerable<ISingleBomColumn> Columns => (IEnumerable<ISingleBomColumn>)base.Columns;
        IEnumerable<ISingleBomCell> ISingleBomRow.Cells => (IEnumerable<ISingleBomCell>)Cells;
        public override IEnumerable<ISingleTableCell> Cells
        {
            get {
                //Columns.Select(col => new SingleCutListCell(this, (SingleGenericColumn)col)).Cast<ISingleCutListCell>().ToList();
                List<ISingleTableCell> cells = new List<ISingleTableCell>();
                foreach (ISingleBomColumn col in Columns)
                {
                    cells.Add(new SingleBomCell(this, (SingleBomColumn)col));
                }
                return cells;
            }
        }

        public IEnumerable<Component2> GetComponents(string configuration)
        {
            return (Component2[])Annotation.TableAnnotation.GetComponents2(RowIndex, configuration);
        }

        public IEnumerable<ISingleModelDoc> GetModelPathNames(out string itemNumber, out string partNumber)
        {
            List<ISingleModelDoc> docs = new List<ISingleModelDoc>();
            foreach (string name in (string[])Annotation.TableAnnotation.GetModelPathNames(RowIndex, out itemNumber, out partNumber))
            {
                docs.Add(Annotation.Table.Document.SldWorks.GetDocumentByName(name));
            }

            return docs;
        }

        public int GetComponentsCount(string configuration, out string itemNumber, out string partNumber)
        {
            return Annotation.TableAnnotation.GetComponentsCount2(RowIndex, configuration, out itemNumber, out partNumber);
        }

        public bool Dissolve()
        {
            return Annotation.TableAnnotation.Dissolve(RowIndex);
        }

        public bool RestoreRestructuredComponents()
        {
            return Annotation.TableAnnotation.RestoreRestructuredComponents(RowIndex);
        }


    }

    internal class SingleBomColumn : SingleGenericColumn, ISingleBomColumn
    {
        public SingleBomColumn(SingleBomTableAnnotation table, int columnNo) : base(table, columnNo) { }
        public new ISingleBomTableAnnotation Annotation => (ISingleBomTableAnnotation)base.Annotation;

        public string ColumnCustomProperty
        {
            get => Annotation.TableAnnotation.GetColumnCustomProperty(ColumnIndex);
            set => Annotation.TableAnnotation.SetColumnCustomProperty(ColumnIndex, value);
        }
        public bool ColumnUseTitleAsPartNumber
        {
            get => Annotation.TableAnnotation.GetColumnUseTitleAsPartNumber(ColumnIndex);
            set => Annotation.TableAnnotation.SetColumnUseTitleAsPartNumber(ColumnIndex, value);
        }
        public int GetAllCustomPropertiesCount { get => Annotation.TableAnnotation.GetAllCustomPropertiesCount(); }
        public IEnumerable<string> GetAllCustomProperties => (string[])Annotation.TableAnnotation.GetAllCustomProperties();
    }

    internal class SingleBomCell : SingleGenericCell, ISingleBomCell
    {
        public SingleBomCell(SingleBomRow row, SingleBomColumn column) : base(row, column) { }

        public new ISingleBomTableAnnotation Annotation => (ISingleBomTableAnnotation)base.Annotation;
        public new ISingleBomColumn Column => (ISingleBomColumn)base.Column;
        public new ISingleBomRow Row => (ISingleBomRow)base.Row;
    }
}

