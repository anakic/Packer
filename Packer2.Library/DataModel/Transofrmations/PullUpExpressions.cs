using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Packer2.Library.Tools;

namespace Packer2.Library.DataModel.Transofrmations
{
    public class PullUpExpressionsTranform : IDataModelTransform
    {
        private readonly ILogger<PullUpExpressionsTranform> logger;

        public PullUpExpressionsTranform(ILogger<PullUpExpressionsTranform> logger = null)
        {
            this.logger = logger ?? new DummyLogger<PullUpExpressionsTranform>();
        }

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
                    logger.LogInformation("Pulled up M expression from table {tableName} into global expressions", tableName);
                }
                else
                {
                    logger.LogInformation("Skipped pulling up M expression from table {tableName} into global expressions because global expressions already contains an expression with this name.", tableName);
                }
            }

            return database;
        }
    }
}
