// See https://aka.ms/new-console-template for more information
using DataModelLoader.Report;
using Microsoft.AnalysisServices.Tabular;
using Newtonsoft.Json.Linq;
using Packer2.Library;
using Packer2.Library.DataModel;
using Packer2.Library.Report.Stores.Folder;
using Packer2.Library.Report.Stores.Folder.Transforms;
using Packer2.Library.Report.Transforms;
using Packer2.Library.Tools;
using System.Text.RegularExpressions;
using System.Xml.Linq;

public static class PbiReportLoader
{
    public static PowerBIReport LoadFromPbiArchive(string path)
        => new PBIArchiveStore(path).Read();

    public static PowerBIReport LoadFromFolder(string path)
        => new ReportFolderStore(path).Read();

    public static Database ReadDataModel(this PowerBIReport report)
        => new BimDataModelStore(new JObjFile(report.DataModelSchemaFile)).Read();

    public static PowerBIReport ReplaceModelReferences(this PowerBIReport report, Mappings renames)
    {
        // todo: handle this stuffinf/unstuffing logic in a centralized place

        UnstuffTransform ut = new UnstuffTransform(new DummyLogger<UnstuffTransform>());
        var clone = JObject.Parse(report.Layout.ToString());
        ut.Transform(clone);
        new ReplaceModelReferenceTransform(renames).Transform(clone);
        ut.Restore(clone);
        report.Layout = clone;

        return report;
    }

    public static PowerBIReport DetectModelExtensions(this PowerBIReport report, out IEnumerable<Table> extensions)
    {
        UnstuffTransform ut = new UnstuffTransform(new DummyLogger<UnstuffTransform>());
        var clone = JObject.Parse(report.Layout.ToString());
        ut.Transform(clone);
        
        var tableExtensions = new Dictionary<string, Table>();

        foreach (var elem in clone.SelectTokens("#config.modelExtensions[*].entities[*]"))
        {
            string tableName = elem["extends"]!.ToString();
            if(tableExtensions.TryGetValue(tableName, out var table) == false)
                table = tableExtensions[tableName] = new Table() { Name = tableName };

            foreach (var measureElem in elem.SelectTokens("measures[*]"))
            {
                table.Measures.Add(new Measure()
                {
                    Name = measureElem["name"]!.ToString(),
                    Expression = measureElem["expression"]!.ToString()
                });
            }
            
        }
        extensions = tableExtensions.Values;
        return report;
    }

    public static PowerBIReport DetectReferences(this PowerBIReport report, Detections detections)
    {
        UnstuffTransform ut = new UnstuffTransform(new DummyLogger<UnstuffTransform>());
        DetectModelReferencesTransform detectRefs = new DetectModelReferencesTransform(detections);

        var clone = JObject.Parse(report.Layout.ToString());
        ut.Transform(clone);
        detectRefs.Transform(clone);
        return report;
    }

    public static PowerBIReport SwitchToSSASDataSource(this PowerBIReport report, string connectionString)
    {
        new SwitchDataSourceToSSASTransform(connectionString).Transform(report);
        return report;
    }

    public static PowerBIReport SwitchToLocalDataModel(this PowerBIReport report)
    {
        // todo:
        // - move code related this code to the PowerBIReport class
        // - consider making strongly typed classes for the Connections and Content_Types classes

        var connToken = report.Connections.SelectTokens(".Connections[?(@.ConnectionType == 'analysisServicesDatabaseLive')]").Single();
        var connString = connToken["ConnectionString"]!.ToString();

        connString = Regex.Replace(connString, "Cube=[^;]*", "");

        var dataModelStore = new SSASDataModelStore(connString);
        var db = dataModelStore.Read();

        db.CompatibilityLevel = 1550;
        var model = db.Model as Microsoft.AnalysisServices.Tabular.Model;
        model.DefaultPowerBIDataSourceVersion = PowerBIDataSourceVersion.PowerBI_V3;

        foreach (var exp in model.Expressions.ToList())
        {
            var tableWithSameName = model.Tables.Find(exp.Name);
            if (tableWithSameName != null)
            {
                (tableWithSameName.Partitions.Single().Source as MPartitionSource).Expression = exp.Expression;
                model.Expressions.Remove(exp);
            }
        }

        foreach (var partitionSource in model.Tables.SelectMany(t => t.Partitions).Select(p => p.Source).OfType<MPartitionSource>())
        {
            partitionSource.Partition.Mode = ModeType.Import;

            string pattern = @"let\s*((?'id'\w[^\s=]*)|(\#\""(?'id'\w[^\""]*)\""))\s*=\s*(?'toReplace'(?'id2'\w[^\s=]*)|(\#\""(?'id2'\w[^\""]*)\""))";
            var match = Regex.Match(partitionSource.Expression, pattern, RegexOptions.IgnorePatternWhitespace);
            if (match.Success)
            {
                var dsId = match.Groups["id2"].Value;
                var ds = model.DataSources.OfType<StructuredDataSource>().SingleOrDefault(ds => ds.Name == dsId);
                if (ds != null)
                {
                    var toReplaceGroup = match.Groups["toReplace"];
                    partitionSource.Expression = partitionSource.Expression.Substring(0, toReplaceGroup.Index) + $"Sql.Database(\"{ds.ConnectionDetails.Address.Server}\", \"{ds.ConnectionDetails.Address.Database}\")" + partitionSource.Expression.Substring(toReplaceGroup.Index + toReplaceGroup.Length);
                }
            }
        }

        foreach (var exp in model.Expressions.Where(e => e.Kind == ExpressionKind.M))
        {
            string pattern = @"let\s*((?'id'\w[^\s=]*)|(\#\""(?'id'\w[^\""]*)\""))\s*=\s*(?'toReplace'(?'id2'\w[^\s=]*)|(\#\""(?'id2'\w[^\""]*)\""))";
            var match = Regex.Match(exp.Expression, pattern, RegexOptions.IgnorePatternWhitespace);
            if (match.Success)
            {
                var dsId = match.Groups["id2"].Value;
                var ds = model.DataSources.OfType<StructuredDataSource>().SingleOrDefault(ds => ds.Name == dsId);
                if (ds != null)
                {
                    var toReplaceGroup = match.Groups["toReplace"];
                    exp.Expression = exp.Expression.Substring(0, toReplaceGroup.Index) + $"Sql.Database(\"{ds.ConnectionDetails.Address.Server}\", \"{ds.ConnectionDetails.Address.Database}\")" + exp.Expression.Substring(toReplaceGroup.Index + toReplaceGroup.Length);
                }
            }
        }

        model.DataSources.Clear();

        model.Cultures.Clear();
        db.Annotations.Clear();

        var file = new MemoryFile();
        new BimDataModelStore(file).Save(db);
        report.DataModelSchemaFile = JObject.Parse(file.Text);
        report.Blobs.Remove("DataMashup");
        connToken.Remove();

        var nodesToRemove = new[] { "/DataModelSchema", "/DataModel", "/DataMashup", "/Connections" }.ToHashSet();

        var ns = @"http://schemas.openxmlformats.org/package/2006/content-types";
        report.Content_Types.Descendants(XName.Get("Override", ns))
                .Where(xe => nodesToRemove.Contains(xe.Attribute("PartName")?.Value))
                ?.Remove();
        report.Content_Types.Root!.Add(new XElement(XName.Get("Override", ns), new XAttribute("PartName", "/DataModelSchema"), new XAttribute("ContentType", "")));

        return report;
    }

    public static PowerBIReport SaveToFolder(this PowerBIReport report, string path, bool enableQueryMinimization = true, bool enableBookmarkOptimization = true, bool enableStripVisualState = true)
    {
        var store = new ReportFolderStore(path)
        {
            EnableBookmarkSimplification = enableBookmarkOptimization,
            EnableQueryMinification = enableBookmarkOptimization,
            EnableStripVisualState = enableStripVisualState
        };
        store.Save(report);
        return report;
    }

    public static PowerBIReport SaveToPbiArchive(this PowerBIReport report, string path)
    {
        var store = new PBIArchiveStore(path);
        store.Save(report);
        return report;
    }
}