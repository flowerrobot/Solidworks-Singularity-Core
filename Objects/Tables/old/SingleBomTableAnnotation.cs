using System;
using SingularityBase;
using SolidWorks.Interop.sldworks;
using System.Collections.Generic;
using SingularityCore;
using SolidWorks.Interop.swconst;
#if false
namespace SingularityCore.old
{
    internal class SingleBomTable : ISingleTable<ISingleBomTable, ISingleBomTableAnnotation, ISingleBomRow, ISingleBomColumn, ISingleBomCell>, ISingleBomTable
    {

        public SingleBomTable(ISingleModelDoc doc, ISingleFeature feat)
        {
            Document = doc;
            Feature = feat;
            TableFeature = (IBomFeature)feat.Feature.GetSpecificFeature2();
        }

        public ISingleModelDoc Document { get; }
        public ISingleFeature Feature { get; }
        object ISingleTable<ISingleBomTable, ISingleBomTableAnnotation, ISingleBomRow, ISingleBomColumn, ISingleBomCell>.TableFeature => TableFeature;

        IEnumerable<ISingleBomTable> ISingleBomTable.TableAnnotations => (IEnumerable<ISingleBomTable>)TableAnnotations;

        public IBomFeature TableFeature { get; }

        public IEnumerable<ISingleBomTableAnnotation> TableAnnotations
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

        public string Name { get => TableFeature.Name; set => TableFeature.Name = value; }
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



        public void Dispose()
        {
        }
    }

    internal class SingleBomTableAnnotation : SingleGenericTableAnnotation<ISingleBomTable, ISingleBomTableAnnotation, ISingleBomRow, ISingleBomColumn, ISingleBomCell>, ISingleBomTableAnnotation
    {

        public new ISingleBomTable TableFeature { get; }
        public new IBomTableAnnotation Annotation { get; }

        public override IEnumerable<ISingleBomColumn> Columns
        {
            get {
                List<ISingleBomColumn> rows = new List<ISingleBomColumn>();
                for (int i = 0; i < ColumnCount; i++)
                    rows.Add(new SingleBomColumn(this, i));
                return rows;
            }
        }
        public override IEnumerable<ISingleBomRow> Rows
        {
            get {
                List<ISingleBomRow> rows = new List<ISingleBomRow>();
                for (int i = 0; i < RowCount; i++)
                    rows.Add((ISingleBomRow)new SingleBomRow(this, i));
                return rows;
            }
        }
        public BomTableSortData BomTableSortData
        {
            get => Annotation.GetBomTableSortData();
            set {
                bool res = value.SaveCurrentSortParameters; //Set is not actually defined
                Annotation.Sort(value);
                Annotation.ApplySavedSortScheme(value);
            }
        }

        public SingleBomTableAnnotation(ISingleBomTable table, IBomTableAnnotation anno) : base(table, (ITableAnnotation)anno)
        {
            TableFeature = table;
            Annotation = (IBomTableAnnotation)anno;
        }

    }

    internal class SingleBomRow : SingleGenericRow<ISingleBomTable, ISingleBomTableAnnotation, ISingleBomRow, ISingleBomColumn, ISingleBomCell>, ISingleBomRow
    {
        public SingleBomRow(ISingleBomTableAnnotation table, int rowNo) : base(table, rowNo)
        {
            //  Table = table;

        }

        public IEnumerable<Component2> GetComponents(string configuration)
        {
            return (Component2[])Table.Annotation.GetComponents2(RowIndex, configuration);
        }

        public IEnumerable<ISingleModelDoc> GetModelPathNames(out string itemNumber, out string partNumber)
        {
            List<ISingleModelDoc> docs = new List<ISingleModelDoc>();
            foreach (string name in (string[])Table.Annotation.GetModelPathNames(RowIndex, out itemNumber, out partNumber))
            {
                docs.Add(Table.TableFeature.Document.SldWorks.GetDocumentByName(name));
            }

            return docs;
        }

        public int GetComponentsCount(string configuration, out string itemNumber, out string partNumber)
        {
            return Table.Annotation.GetComponentsCount2(RowIndex, configuration, out itemNumber, out partNumber);
        }

        public bool Dissolve()
        {
            return Table.Annotation.Dissolve(RowIndex);
        }

        public bool RestoreRestructuredComponents()
        {
            return Table.Annotation.RestoreRestructuredComponents(RowIndex);
        }

        public override IEnumerable<ISingleBomCell> Cells { get; }
    }

    internal class SingleBomColumn : SingleGenericColumn<ISingleBomTable, ISingleBomTableAnnotation, ISingleBomRow, ISingleBomColumn, ISingleBomCell>, ISingleBomColumn
    {
        public SingleBomColumn(SingleBomTableAnnotation table, int columnNo) : base(table, columnNo)
        {
            Table = table;
        }

        public new ISingleBomTableAnnotation Table { get; }
        //public override IEnumerable<ISingleBomRow> Rows { get; }

        public string ColumnCustomProperty
        {
            get => Table.Annotation.GetColumnCustomProperty(ColumnIndex);
            set => Table.Annotation.SetColumnCustomProperty(ColumnIndex, value);
        }
        public bool ColumnUseTitleAsPartNumber
        {
            get => Table.Annotation.GetColumnUseTitleAsPartNumber(ColumnIndex);
            set => Table.Annotation.SetColumnUseTitleAsPartNumber(ColumnIndex, value);
        }
        public int GetAllCustomPropertiesCount { get => Table.Annotation.GetAllCustomPropertiesCount(); }
        public IEnumerable<string> GetAllCustomProperties => (string[])Table.Annotation.GetAllCustomProperties();
    }

    internal class SingleBomCell : SingleGenericCell<ISingleBomTable, ISingleBomTableAnnotation, ISingleBomRow, ISingleBomColumn, ISingleBomCell>, ISingleTableCell<ISingleBomTable, ISingleBomTableAnnotation, ISingleBomRow, ISingleBomColumn, ISingleBomCell>
    {
        public SingleBomCell(ISingleBomRow row, ISingleBomColumn column) : base(row, column)
        {
        }
    }
}


#if false

namespace SingularityCore
{
    internal class SingleBomTable : ISingleBomTable, ISingleTable<SingleBomTable, SingleBomTableAnnotation, SingleBomRow, SingleBomColumn, SingleBomCell>
    {
        private IEnumerable<ISingleBomTable> _tableAnnotations;

        public SingleBomTable(ISingleModelDoc doc, ISingleFeature feat)
        {
            Document = doc;
            Feature = feat;
            TableFeature = (IBomFeature)feat.Feature.GetSpecificFeature2();
        }

        public ISingleModelDoc Document { get; }
        public ISingleFeature Feature { get; }
        


        public IBomFeature TableFeature { get; }

        public IEnumerable<ISingleBomTableAnnotation> TableAnnotations
        {
            get
            {
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

        public string Name { get => TableFeature.Name; set => TableFeature.Name = value; }
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

        object ISingleTable<ISingleBomTable, ISingleBomTableAnnotation, ISingleBomRow, ISingleBomColumn, ISingleBomCell>.TableFeature => TableFeature;
        object ISingleTable<SingleBomTable, SingleBomTableAnnotation, SingleBomRow, SingleBomColumn, SingleBomCell>.TableFeature => throw new NotImplementedException();
        IEnumerable<SingleBomTableAnnotation> ISingleTable<SingleBomTable, SingleBomTableAnnotation, SingleBomRow, SingleBomColumn, SingleBomCell>.TableAnnotations => throw new NotImplementedException();
        IEnumerable<ISingleBomTable> ISingleBomTable.TableAnnotations => _tableAnnotations;


        public void Dispose()
        {
        }
    }

    internal class SingleBomTableAnnotation : SingleGenericTableAnnotation<SingleBomTable, SingleBomTableAnnotation, SingleBomRow, SingleBomColumn, SingleBomCell>, ISingleBomTableAnnotation
    {
        private IEnumerable<ISingleBomColumn> _columns;
        private IEnumerable<ISingleBomRow> _rows;

        public new ISingleBomTable TableFeature { get; }
        public new IBomTableAnnotation Annotation { get; }

        public override IEnumerable<SingleBomColumn> Columns
        {
            get {
                List<SingleBomColumn> rows = new List<SingleBomColumn>();
                for (int i = 0; i < ColumnCount; i++)
                    rows.Add(new SingleBomColumn(this, i));
                return rows;
            }
        }

        IEnumerable<ISingleBomRow> ISingleTableAnnotation<ISingleBomTable, ISingleBomTableAnnotation, ISingleBomRow, ISingleBomColumn, ISingleBomCell>.Rows => null;
        IEnumerable<ISingleBomColumn> ISingleTableAnnotation<ISingleBomTable, ISingleBomTableAnnotation, ISingleBomRow, ISingleBomColumn, ISingleBomCell>.Columns => null;

        public bool MoveColumn(ISingleBomColumn Source, swTableItemInsertPosition_e Where, ISingleBomColumn Destination)
        {
            throw new NotImplementedException();
        }
        public bool MoveRow(ISingleBomRow Source, swTableItemInsertPosition_e Where, ISingleBomRow Destination)
        {
            throw new NotImplementedException();
        }


        public override IEnumerable<SingleBomRow> Rows
        {
            get
            {
                List<SingleBomRow> rows = new List<SingleBomRow>();
                for (int i = 0; i < RowCount; i++)
                    rows.Add(new SingleBomRow(this, i));
                return rows;
            }
        }

        public BomTableSortData BomTableSortData
        {
            get => Annotation.GetBomTableSortData();
            set {
                bool res = value.SaveCurrentSortParameters; //Set is not actually defined
                Annotation.Sort(value);
                Annotation.ApplySavedSortScheme(value);
            }
        }

        public SingleBomTableAnnotation(SingleBomTable table, ITableAnnotation anno) : base(table, anno)
        {
            TableFeature = table;
            Annotation = (IBomTableAnnotation)anno;
        }

    }

    internal class SingleBomRow : SingleGenericRow<SingleBomTable, SingleBomTableAnnotation, SingleBomRow, SingleBomColumn, SingleBomCell>, ISingleBomRow
    {
        public SingleBomRow(SingleBomTableAnnotation table, int rowNo) : base(table, rowNo)
        {
            Table = table;
            
        }

        public IEnumerable<Component2> GetComponents(string configuration)
        {
            return (Component2[])Table.Annotation.GetComponents2(rowIndex, configuration);
        }

        public IEnumerable<ISingleModelDoc> GetModelPathNames(out string itemNumber, out string partNumber)
        {
            List<ISingleModelDoc> docs = new List<ISingleModelDoc>();
            foreach (string name in (string[])Table.Annotation.GetModelPathNames(RowIndex, out itemNumber, out partNumber))
            {
                docs.Add(Table.TableFeature.Document.SldWorks.GetDocumentByName(name));
            }

            return docs;
        }

        public int GetComponentsCount(string configuration, out string itemNumber, out string partNumber)
        {
            return Table.Annotation.GetComponentsCount2(RowIndex, configuration, out itemNumber, out partNumber);
        }

        public bool Dissolve()
        {
            return Table.Annotation.Dissolve(RowIndex);
        }

        public bool RestoreRestructuredComponents()
        {
            return Table.Annotation.RestoreRestructuredComponents(RowIndex);
        }

        public new ISingleBomTableAnnotation Table { get; }
        public new IEnumerable<ISingleBomColumn> Columns { get; }
        public new IEnumerable<ISingleBomCell> Cells { get; }
    }
    internal class SingleBomColumn : SingleGenericColumn<SingleBomTable, SingleBomTableAnnotation, SingleBomRow, SingleBomColumn, SingleBomCell>//, ISingleBomColumn
    {
        public SingleBomColumn(SingleBomTableAnnotation table, int columnNo) : base(table, columnNo)
        {
            Table = table;
        }

        public new ISingleBomTableAnnotation Table { get; }
        public override IEnumerable<SingleBomRow> Rows { get; }

        public string ColumnCustomProperty
        {
            get => Table.Annotation.GetColumnCustomProperty(ColumnIndex);
            set => Table.Annotation.SetColumnCustomProperty(ColumnIndex, value);
        }
        public bool ColumnUseTitleAsPartNumber
        {
            get => Table.Annotation.GetColumnUseTitleAsPartNumber(ColumnIndex);
            set => Table.Annotation.SetColumnUseTitleAsPartNumber(ColumnIndex, value);
        }
        public int GetAllCustomPropertiesCount { get => Table.Annotation.GetAllCustomPropertiesCount(); }
        public IEnumerable<string> GetAllCustomProperties => (string[])Table.Annotation.GetAllCustomProperties();
    }
    internal class SingleBomCell : SingleGenericCell<SingleBomTable, SingleBomTableAnnotation, SingleBomRow, SingleBomColumn, SingleBomCell>//, ISingleTableCell<SingleBomTable, SingleBomTableAnnotation, SingleBomRow, SingleBomColumn, SingleBomCell>
    {
        public SingleBomCell(SingleBomRow row, SingleBomColumn column) : base(row, column)
        {
        }
    }
}


#endif


#if false

namespace SingularityCore
{
    internal class SingleBomTable : ISingleBomTable
    {

        public SingleBomTable(ISingleModelDoc doc, ISingleFeature feat)
        {
            Document = doc;
            Feature = feat;
            TableFeature = (IBomFeature)feat.Feature.GetSpecificFeature2();
        }

        public ISingleModelDoc Document { get; }
        public ISingleFeature Feature { get; }
        object ISingleTable<ISingleBomTable, ISingleBomTableAnnotation, ISingleBomRow, ISingleBomColumn, ISingleBomCell>.TableFeature => TableFeature;
        object ISingleTable.TableFeature => TableFeature;
        IEnumerable<ISingleTableAnnotation> ISingleTable.TableAnnotations => TableAnnotations;

        public IBomFeature TableFeature { get; }

        public IEnumerable<ISingleBomTableAnnotation> TableAnnotations
        {
            get
            {
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

        public string Name { get => TableFeature.Name; set => TableFeature.Name = value; }
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

        
        public void Dispose()
        {
        }
    }

    internal class SingleBomTableAnnotation : SingleGenericTableAnnotation<SingleBomTable, SingleBomTableAnnotation, SingleBomRow, SingleBomColumn, SingleBomCell>//, ISingleBomTableAnnotation
    {
        private IEnumerable<ISingleBomColumn> _columns;

        public SingleBomTableAnnotation(SingleBomTable table, IBomTableAnnotation anno) : base(table, (ITableAnnotation)anno)
        {
            TableFeature = table;
            Annotation = anno;
        }

        public new IBomTableAnnotation Annotation { get; }
        public new ISingleBomTable TableFeature { get; }

        public override IEnumerable<ISingleBomColumn> Columns
        {
            get {
                List<ISingleBomColumn> rows = new List<ISingleBomColumn>();
                for (int i = 0; i < ColumnCount; i++)
                    rows.Add(new SingleBomColumn(this, i));
                return rows;
            }
        }
        public new IEnumerable<ISingleBomRow> Rows
        {
            get {
                List<ISingleBomRow> rows = new List<ISingleBomRow>();
                for (int i = 0; i < RowCount; i++)
                    rows.Add(new SingleBomRow(this, i));
                return rows;
            }
        }
        public override IEnumerable<ISingleTableRow> Rows => Rows;
        public BomTableSortData BomTableSortData
        {
            get => Annotation.GetBomTableSortData();
            set {
                bool res = value.SaveCurrentSortParameters; //Set is not actually defined
                Annotation.Sort(value);
                Annotation.ApplySavedSortScheme(value);
            }
        }
    }

    internal class SingleBomRow : SingleGenericRow<SingleBomTable, SingleBomTableAnnotation, SingleBomRow, SingleBomColumn, SingleBomCell>//, ISingleBomRow
    {
        public SingleBomRow(SingleBomTableAnnotation table, int rowNo) : base(table, rowNo)
        {
            Table = table;
        }

        ISingleTableAnnotation ISingleTableRow.Table => Table;
        IEnumerable<ISingleTableCell> ISingleTableRow.Cells => Cells;
        public new ISingleBomTableAnnotation Table { get; }

        public IEnumerable<Component2> GetComponents(string configuration)
        {
            return (Component2[])Table.Annotation.GetComponents2(rowIndex, configuration);
        }

        public IEnumerable<ISingleBomCell> Cells { get; }
        public IEnumerable<ISingleModelDoc> GetModelPathNames(out string itemNumber, out string partNumber)
        {
            List<ISingleModelDoc> docs = new List<ISingleModelDoc>();
            foreach (string name in (string[])Table.Annotation.GetModelPathNames(RowIndex, out itemNumber, out partNumber))
            {
                docs.Add(Table.TableFeature.Document.SldWorks.GetDocumentByName(name));
            }

            return docs;
        }

        public int GetComponentsCount(string configuration, out string itemNumber, out string partNumber)
        {
            return Table.Annotation.GetComponentsCount2(RowIndex, configuration, out itemNumber, out partNumber);
        }

        public bool Dissolve()
        {
            return Table.Annotation.Dissolve(RowIndex);
        }

        public bool RestoreRestructuredComponents()
        {
            return Table.Annotation.RestoreRestructuredComponents(RowIndex);
        }

    }
    internal class SingleBomColumn : SingleGenericColumn<SingleBomTable, SingleBomTableAnnotation, SingleBomRow, SingleBomColumn, SingleBomCell>//, ISingleBomColumn
    {
        public SingleBomColumn(SingleBomTableAnnotation table, int columnNo) : base(table, columnNo)
        {
            Table = table;
        }

        public new ISingleBomTableAnnotation Table { get; }
        public override IEnumerable<ISingleTableRow> Rows { get; }

        public string ColumnCustomProperty
        {
            get => Table.Annotation.GetColumnCustomProperty(ColumnIndex);
            set => Table.Annotation.SetColumnCustomProperty(ColumnIndex, value);
        }
        public bool ColumnUseTitleAsPartNumber
        {
            get => Table.Annotation.GetColumnUseTitleAsPartNumber(ColumnIndex);
            set => Table.Annotation.SetColumnUseTitleAsPartNumber(ColumnIndex, value);
        }
        public int GetAllCustomPropertiesCount { get => Table.Annotation.GetAllCustomPropertiesCount(); }
        public IEnumerable<string> GetAllCustomProperties => (string[])Table.Annotation.GetAllCustomProperties();
    }
    internal class SingleBomCell : SingleGenericCell<SingleBomTable, SingleBomTableAnnotation, SingleBomRow, SingleBomColumn, SingleBomCell>//, ISingleTableCell<SingleBomTable, SingleBomTableAnnotation, SingleBomRow, SingleBomColumn, SingleBomCell>
    {

    }
}

#endif
#endif