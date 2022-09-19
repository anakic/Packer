using Microsoft.AnalysisServices.Tabular;
using System.Linq;

namespace Packer2.Library.DataModel.Transofrmations
{
    public class PullUpExpressionsTranform : IDataModelTransform
    {
        public Database Transform(Database database)
        {
            var mPartitionSources = database.Model
                .Tables
                .SelectMany(t => t.Partitions)
                .Where(p => p.SourceType == PartitionSourceType.M)
                .Select(p => p.Source)
                .OfType<MPartitionSource>()
                .ToList();

            foreach (var source in mPartitionSources)
            {
                var tableName = source.Partition.Table.Name;
                var expression = source.Expression;

                database.Model.Expressions.Add(new NamedExpression() { Kind = ExpressionKind.M, Name = tableName, Expression = expression });

                // replace original query with pointer to shared expression
                source.Expression = $@"let
	Source = #""{tableName}""
in
  Source";
            }

            return database;
        }
    }
}
