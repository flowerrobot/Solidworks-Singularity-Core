using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Collections.Generic;

namespace SingularityCore
{
    internal class SingleBomTable : SingleFeature, ISingleBomTable
    {
        public IBomFeature TableFeature { get; private set; }
        public IBomTable Table { get;  }
        public SingleBomTable(ISingleModelDoc doc, ISingleFeature feat) : base(doc, feat.BaseObject)
        {
            TableFeature = (IBomFeature)feat.BaseObject.GetSpecificFeature2();
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
                                    if (((ISingleBomTableAnnotation)tblAnnotation).Table.Id == Id)
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


         
        IList<ISingleTableAnnotation> ISingleTable.TableAnnotations => (IList<ISingleTableAnnotation>)TableAnnotations;

        public IList<ISingleBomTableAnnotation> TableAnnotations
        {
            get {
                IList<ISingleBomTableAnnotation> anns = new List<ISingleBomTableAnnotation>();
                foreach (var an in (object[])TableFeature.GetTableAnnotations())
                {
                    anns.Add(new SingleBomTableAnnotation(this, (IBomTableAnnotation)an));
                }
                //(from bm in (IBomTableAnnotation[])TableFeature.GetTableAnnotations() select new SingleBomTableAnnotation(this, bm)).Cast<ISingleBomTableAnnotation>().ToList();
                return anns;
            }
        }


        //IList<ISingleBomTableAnnotation> ISingleBomTable.TableAnnotations => (IList<ISingleBomTableAnnotation>)TableAnnotations;
        //public IList<ISingleTableAnnotation> TableAnnotations
        //{
        //    get
        //    {
        //        IList<ISingleBomTableAnnotation> anns = new List<ISingleBomTableAnnotation>();
        //        foreach (var an in (object[])TableFeature.GetTableAnnotations())
        //        {
        //            anns.Add(new SingleBomTableAnnotation(this, (IBomTableAnnotation)an));
        //        }
        //        //(from bm in (IBomTableAnnotation[])TableFeature.GetTableAnnotations() select new SingleBomTableAnnotation(this, bm)).Cast<ISingleBomTableAnnotation>().ToList();
        //        return anns;
        //    }
        //}






        public int GetConfigurationCount(bool onlyVisible) => TableFeature.GetConfigurationCount(onlyVisible);

        public IList<string> GetConfigurations(bool onlyVisible, out IEnumerable<bool> isVisible)
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

    internal class SingleBomTableAnnotation : SingleTableAnnotation<ISingleBomColumn,ISingleBomRow>, ISingleBomTableAnnotation
    {
        public new ISingleBomTable Table => (ISingleBomTable)base.Table;
        public new IBomTableAnnotation TableAnnotation { get; }
        public SingleBomTableAnnotation(SingleBomTable table, IBomTableAnnotation anno) : base(table, (ITableAnnotation)anno)
        {
            TableAnnotation = anno;
        }
        public SingleBomTableAnnotation(ISingleModelDoc doc, IBomTableAnnotation anno) : base(new SingleWeldmentCutListTable(doc,new SingleFeature(doc, (IFeature)anno.BomFeature.GetFeature())),  (ITableAnnotation)anno)
        {
            TableAnnotation = anno;
        }

        IList<ISingleBomColumn> ISingleBomTableAnnotation.Columns => Columns;
        public override IList<ISingleBomColumn> Columns
        {
            get {
                List<ISingleBomColumn> cols = new List<ISingleBomColumn>();
                int colCount = ColumnCount;
                for (int i = 0; i < colCount ; i++)
                {
                    cols.Add(new SingleBomColumn(this, i));
                }
                return cols;
            }
        }

        IList<ISingleBomRow> ISingleBomTableAnnotation.Rows => Rows;
        public override IList<ISingleBomRow> Rows
        {
            get {
                List<ISingleBomRow> row = new List<ISingleBomRow>();
                int count = RowCount;
                int start = ((ITableAnnotation) TableAnnotation).GetHeaderCount();
                for (int i = start; i < count; i++)
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

    internal class SingleBomRow : SingleGenericRow<ISingleBomCell>, ISingleBomRow
    {
        public SingleBomRow(ISingleBomTableAnnotation table, int rowNo) : base(table, rowNo) { }

        public new ISingleBomTableAnnotation Annotation => (ISingleBomTableAnnotation)base.Annotation;
        public new IList<ISingleBomColumn> Columns => Annotation.Columns;

        IList<ISingleBomCell> ISingleBomRow.Cells => Cells;
        public override IList<ISingleBomCell> Cells
        {
            get {
                //Columns.Select(col => new SingleCutListCell(this, (SingleGenericColumn)col)).Cast<ISingleCutListCell>().ToList();
                List<ISingleBomCell> cells = new List<ISingleBomCell>();
                foreach (ISingleBomColumn col in Columns)
                {
                    cells.Add(new SingleBomCell(this, (SingleBomColumn)col));
                }
                return cells;
            }
        }

        public IList<ISingleComponent> GetComponents(string configuration)
        {
            IList<ISingleComponent> comps = new List<ISingleComponent>();
            object cmp = Annotation.TableAnnotation.GetComponents2(RowIndex, Annotation.Table.Configuration);
            if (cmp == null) return comps;

            foreach (var cm in (object[])cmp)
            {
                comps.Add(new SingleComponent((IComponent2)cm));
            }

            return comps;
        }

        public IList<ISingleModelDoc> GetModelPathNames(out string itemNumber, out string partNumber)
        {
            List<ISingleModelDoc> docs = new List<ISingleModelDoc>();
            object obj = Annotation.TableAnnotation.GetModelPathNames(RowIndex, out itemNumber, out partNumber);
            if (obj == null) return docs;
            foreach (string name in (string[])obj )
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

        public string RowItemNumber
        {
            get
            {
                Annotation.TableAnnotation.GetModelPathNames(RowIndex,  out string itemNo, out string prtNumber);
                return itemNo;
            }
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
        public IList<string> GetAllCustomProperties => (string[])Annotation.TableAnnotation.GetAllCustomProperties();
    }

    internal class SingleBomCell : SingleGenericCell, ISingleBomCell
    {
        public SingleBomCell(SingleBomRow row, SingleBomColumn column) : base(row, column) { }

        public new ISingleBomTableAnnotation Annotation => (ISingleBomTableAnnotation)base.Annotation;
        public new ISingleBomColumn Column => (ISingleBomColumn)base.Column;
        public new ISingleBomRow Row => (ISingleBomRow)base.Row;
    }
}

