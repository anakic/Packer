using Microsoft.AnalysisServices.Tabular;

namespace Packer2.Library.DataModel.Transofrmations
{
    public class PullUpExpressionsTranform : IDataModelTransform
    {
        public Database Transform(Database database)
        {
            var mPartitionSources = database.Model
                .Tables
                // currently not supporting moving expressions from multiple partitions because we name the expression based on the tableName
                .Where(t => t.Partitions.Count == 1)
                .SelectMany(t => t.Partitions)
                .Where(p => p.SourceType == PartitionSourceType.M)
                .Select(p => p.Source)
                .OfType<MPartitionSource>()
                .ToList();

            foreach (var source in mPartitionSources)
            {
                // todo: use partition name (or index) in name
                var tableName = source.Partition.Table.Name;
                var expression = source.Expression;

                // do not pull up if expression already pulled up
                if (!database.Model.Expressions.Contains(tableName))
                {
                    database.Model.Expressions.Add(new NamedExpression() { Kind = ExpressionKind.M, Name = tableName, Expression = expression });

                    // replace original query with pointer to shared expression
                    source.Expression = $@"let
	Source = #""{tableName}""
in
  Source";
                }
            }

            return database;
        }
    }
}
