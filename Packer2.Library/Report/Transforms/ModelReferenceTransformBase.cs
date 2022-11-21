using DataModelLoader.Report;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Packer2.Library.Report.Transforms
{
    /// <summary>
    /// Locates all uses of queries, filters and expressions in a report's layout file, instantiates
    /// the query/filter/expression, performs an operation on each and saves each back to its location
    /// in the layout file. Used for checking model references, renaming model references, minification
    /// and unminification of query/filter/expression code in the json file.
    /// </summary>
    public abstract class ModelReferenceTransformBase : IReportTransform
    {
        record Finding(string parentPath, string nestedPath, string text, object deserialized, Exception ex);
        public PowerBIReport Transform(PowerBIReport model)
        {
            var stuffedAreaSelectors = new string[] 
            {
                "filters",
                "sections[*].filters",
                "sections[*].visualContainers[*].config",
                "sections[*].visualContainers[*].filters",
                "sections[*].config",
                "sections[*].visualContainers[*].query",
                "sections[*].visualContainers[*].dataTransforms",
                "config",
                "pods[*].parameters" 
            };

            foreach (var selector in stuffedAreaSelectors)
            {
                var stuffedTokens = model.Layout.SelectTokens(selector);
                foreach (var token in stuffedTokens)
                {
                    List<JObject> jObjects = new List<JObject>();
                    var tok = JsonConvert.DeserializeObject(token.ToString());
                    if (tok is JArray arr)
                        jObjects.AddRange(arr.OfType<JObject>());
                    else if (tok is JObject obj)
                        jObjects.Add(obj);
                    else
                        throw new InvalidOperationException();

                    foreach (var areaJObj in jObjects)
                    {
                        var expressionSelectors = new[] { "..expr", "..expression", "..expressions[*]", "..fieldExpr", "..identityKeys[*]", "..identityValues[*]", "..scopeId" };
                        foreach (var expToken in expressionSelectors.SelectMany(areaJObj.SelectTokens).ToArray())
                        {
                            if (TryReadExpression(expToken, out var expObj))
                            {
                                VisitExpression(expObj!, token.Path, expToken.Path);
                                WriteExpression(expToken, expObj!);
                            }
                        }

                        // todo: support orderby
                        // Microsoft.InfoNav.Data.Contracts.Internal.DataShapeBindingOrderBy

                        // todo: support activeProjections, e.g.
                        /*
                         "activeProjections": {
                "Rows": [
                  {
                    "Column": {
                      "Expression": {
                        "SourceRef": {
                          "Entity": "Care Changes"
                        }
                      },
                      "Property": "Resultant Simplified Service Type Crisis"
                    }
                  }
                ]
              }
                         */

                        var queryDefSelectors = new[] { "..prototypeQuery" };
                        foreach (var expToken in queryDefSelectors.SelectMany(areaJObj.SelectTokens).ToArray())
                        {
                            if (TryReadQuery(expToken, out var queryObj))
                            {
                                VisitQuery(queryObj!, token.Path, expToken.Path);
                                WriteQuery(expToken, queryObj!);
                            }
                        }

                        var filterDefSelectors = new[] { "..filter" };
                        foreach (var expToken in filterDefSelectors.SelectMany(areaJObj.SelectTokens).ToArray())
                        {
                            if (TryReadFilter(expToken, out var filterObj))
                            {
                                VisitFilter(filterObj!, token.Path, expToken.Path);
                                WriteFilter(expToken, filterObj!);
                            }
                        }
                    }

                    // write back the stuffed string
                    if (tok is JArray arr2)
                        token.Replace(new JArray(jObjects).ToString(Formatting.None));
                    else if (tok is JObject obj2)
                        token.Replace(jObjects.Single().ToString(Formatting.None));
                    else
                        throw new InvalidOperationException();
                }
            }

            OnProcessingComplete(model);

            return model;
        }

        protected virtual bool TryReadFilter(JToken expToken, out FilterDefinition? filter)
        {
            if (expToken["From"] == null)
            {
                filter = null;
                return false;
            }

            filter = expToken.ToObject<FilterDefinition>()!;
            return true;
        }

        protected virtual bool TryReadQuery(JToken expToken, out QueryDefinition? queryDefinition)
        {
            queryDefinition = expToken.ToObject<QueryDefinition>()!;
            return true;
        }

        protected virtual bool TryReadExpression(JToken expToken, out QueryExpressionContainer? expressionContainer)
        {
            expressionContainer = expToken.ToObject<QueryExpressionContainer>()!;
            return (expressionContainer.Expression != null);
        }

        protected virtual void WriteFilter(JToken expToken, FilterDefinition filterObj)
        {
            // write back the updated object
            expToken.Replace(JObject.FromObject(filterObj));
        }

        protected virtual void WriteQuery(JToken expToken, QueryDefinition queryObj)
        {
            // write back the updated object
            expToken.Replace(JObject.FromObject(queryObj));
        }

        protected virtual void WriteExpression(JToken expToken, QueryExpressionContainer expObj)
        {
            // write back the updated object
            expToken.Replace(JObject.FromObject(expObj));
        }

        protected virtual void OnProcessingComplete(PowerBIReport model) { }

        // todo: should not force deriving from BaseQueryExpressionVisitor, we could make BaseQueryExpressionVisitor
        // a decorator around the user's visitor object. The properties of BaseQueryExpressionVisitor are only used
        // by this class internally, they are not by client code.
        protected abstract BaseQueryExpressionVisitor Visitor { get; }

        protected void VisitExpression(QueryExpressionContainer expObj, string outerPath, string innerPath)
        {
            // we only have aliases for queries and filters (they have a From clause that specifies them)
            Visitor.SourcesByAliasMap = null;
            Visitor.OuterPath = outerPath;
            Visitor.InnerPath = innerPath;
            expObj.Expression.Accept(Visitor);
        }

        protected void VisitFilter(FilterDefinition filterObj, string outerPath, string innerPath)
        {
            Visitor.OuterPath = outerPath;
            Visitor.InnerPath = innerPath;
            Visitor.SourcesByAliasMap = filterObj.From.ToDictionary(f => f.Name, f => f.Entity);
            filterObj.Where.ForEach(w => w.Condition.Expression.Accept(Visitor));
        }

        protected virtual void VisitQuery(QueryDefinition expObj, string outerPath, string innerPath)
        {
            Visitor.OuterPath = outerPath;
            Visitor.InnerPath = innerPath;
            Visitor.SourcesByAliasMap = expObj.From.ToDictionary(f => f.Name, f => f.Entity);

            expObj.From.ForEach(es => es.Expression?.Expression.Accept(Visitor));
            expObj.GroupBy?.ForEach(w => w.Expression.Accept(Visitor));
            expObj.OrderBy?.ForEach(w => w.Expression.Expression.Accept(Visitor));
            expObj.Let?.ForEach(w => w.Expression.Accept(Visitor));
            expObj.Parameters?.ForEach(w => w.Expression.Accept(Visitor));
            expObj.Select?.ForEach(w => w.Expression.Accept(Visitor));
            expObj.Where?.ForEach(w => w.Condition.Expression.Accept(Visitor));
            expObj.Transform?.ForEach(t =>
            {
                t.Input.Parameters.ForEach(p => p.Expression.Accept(Visitor));
                t.Input.Table.Columns.ForEach(c => c.Expression.Expression.Accept(Visitor));
                t.Output.Table.Columns.ForEach(p => p.Expression.Expression.Accept(Visitor));
            });
        }
    }


    //class LocationAwareInfoNavVisitor : InfoNavVisitor
    //{
    //    public string InnerPath { get; internal set; }
    //    public string OuterPath { get; internal set; }
    //}

    class InfoNavVisitor : QueryExpressionVisitor
    {
        protected Dictionary<string, string>? SourcesByAliasMap { get; private set; }

        public virtual void Visit(QueryExpressionContainer expObj)
        {
            SourcesByAliasMap = null;
            expObj.Expression.Accept(this);
        }

        public virtual void Visit(FilterDefinition filterObj)
        {
            SourcesByAliasMap = filterObj.From.ToDictionary(f => f.Name, f => f.Entity);
            filterObj.Where.ForEach(w => w.Condition.Expression.Accept(this));
        }

        public virtual void Visit(QueryDefinition expObj)
        {
            SourcesByAliasMap = expObj.From.ToDictionary(f => f.Name, f => f.Entity);
            expObj.From.ForEach(es => es.Expression?.Expression.Accept(this));
            expObj.Let?.ForEach(w => w.Expression.Accept(this));
            expObj.Parameters?.ForEach(w => w.Expression.Accept(this));
            expObj.Where?.ForEach(w => w.Condition.Expression.Accept(this));
            expObj.Select?.ForEach(w => w.Expression.Accept(this));
            expObj.Transform?.ForEach(t =>
            {
                t.Input.Parameters.ForEach(p => p.Expression.Accept(this));
                t.Input.Table.Columns.ForEach(c => c.Expression.Expression.Accept(this));
                t.Output.Table.Columns.ForEach(p => p.Expression.Expression.Accept(this));
            });
            expObj.GroupBy?.ForEach(w => w.Expression.Accept(this));
            expObj.OrderBy?.ForEach(w => w.Expression.Expression.Accept(this));
        }

        protected override void Visit(QuerySourceRefExpression expression)
        {

        }

        protected override void Visit(QueryPropertyExpression expression)
        {

        }

        protected override void Visit(QueryColumnExpression expression)
        {

        }

        protected override void Visit(QueryMeasureExpression expression)
        {

        }

        protected override void Visit(QueryHierarchyExpression expression)
        {

        }

        protected override void Visit(QueryHierarchyLevelExpression expression)
        {

        }

        protected override void Visit(QueryPropertyVariationSourceExpression expression)
        {

        }

        protected override void Visit(QueryAggregationExpression expression)
        {

        }

        protected override void Visit(QueryDatePartExpression expression)
        {

        }

        protected override void Visit(QueryPercentileExpression expression)
        {

        }

        protected override void Visit(QueryFloorExpression expression)
        {

        }

        protected override void Visit(QueryDiscretizeExpression expression)
        {

        }

        protected override void Visit(QueryMemberExpression expression)
        {

        }

        protected override void Visit(QueryNativeFormatExpression expression)
        {

        }

        protected override void Visit(QueryNativeMeasureExpression expression)
        {

        }

        protected override void Visit(QueryExistsExpression expression)
        {

        }

        protected override void Visit(QueryNotExpression expression)
        {

        }

        protected override void Visit(QueryAndExpression expression)
        {

        }

        protected override void Visit(QueryOrExpression expression)
        {

        }

        protected override void Visit(QueryComparisonExpression expression)
        {

        }

        protected override void Visit(QueryContainsExpression expression)
        {

        }

        protected override void Visit(QueryStartsWithExpression expression)
        {

        }

        protected override void Visit(QueryArithmeticExpression expression)
        {

        }

        protected override void Visit(QueryEndsWithExpression expression)
        {

        }

        protected override void Visit(QueryBetweenExpression expression)
        {

        }

        protected override void Visit(QueryInExpression expression)
        {

        }

        protected override void Visit(QueryScopedEvalExpression expression)
        {

        }

        protected override void Visit(QueryFilteredEvalExpression expression)
        {

        }

        protected override void Visit(QuerySparklineDataExpression expression)
        {

        }

        protected override void Visit(QueryBooleanConstantExpression expression)
        {

        }

        protected override void Visit(QueryDateConstantExpression expression)
        {

        }

        protected override void Visit(QueryDateTimeConstantExpression expression)
        {

        }

        protected override void Visit(QueryDateTimeSecondConstantExpression expression)
        {

        }

        protected override void Visit(QueryDecadeConstantExpression expression)
        {

        }

        protected override void Visit(QueryDecimalConstantExpression expression)
        {

        }

        protected override void Visit(QueryIntegerConstantExpression expression)
        {

        }

        protected override void Visit(QueryNullConstantExpression expression)
        {

        }

        protected override void Visit(QueryStringConstantExpression expression)
        {

        }

        protected override void Visit(QueryNumberConstantExpression expression)
        {

        }

        protected override void Visit(QueryYearAndMonthConstantExpression expression)
        {

        }

        protected override void Visit(QueryYearAndWeekConstantExpression expression)
        {

        }

        protected override void Visit(QueryYearConstantExpression expression)
        {

        }

        protected override void Visit(QueryLiteralExpression expression)
        {

        }

        protected override void Visit(QueryDefaultValueExpression expression)
        {

        }

        protected override void Visit(QueryAnyValueExpression expression)
        {

        }

        protected override void Visit(QueryNowExpression expression)
        {

        }

        protected override void Visit(QueryDateAddExpression expression)
        {

        }

        protected override void Visit(QueryDateSpanExpression expression)
        {

        }

        protected override void Visit(QueryTransformOutputRoleRefExpression expression)
        {

        }

        protected override void Visit(QueryTransformTableRefExpression expression)
        {

        }

        protected override void Visit(QuerySubqueryExpression expression)
        {

        }

        protected override void Visit(QueryLetRefExpression expression)
        {

        }

        protected override void Visit(QueryRoleRefExpression expression)
        {

        }

        protected override void Visit(QuerySummaryValueRefExpression expression)
        {

        }

        protected override void Visit(QueryParameterRefExpression expression)
        {

        }

        protected override void Visit(QueryTypeOfExpression expression)
        {

        }

        protected override void Visit(QueryPrimitiveTypeExpression expression)
        {

        }

        protected override void Visit(QueryTableTypeExpression expression)
        {

        }
    }
}
