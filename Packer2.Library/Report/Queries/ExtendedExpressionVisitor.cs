using Microsoft.InfoNav.Data.Contracts.Internal;

namespace Packer2.Library.Report.Queries
{
    public class ExtendedExpressionVisitor : DefaultQueryExpressionVisitor
    {
        public virtual void Visit(FilterDefinition filterDefinition)
        {
            if (filterDefinition.From != null)
            {
                foreach (EntitySource item2 in filterDefinition.From)
                {
                    VisitEntitySource(item2);
                }
            }

            if (filterDefinition.Where != null)
            {
                foreach (QueryFilter item3 in filterDefinition.Where)
                {
                    VisitFilter(item3);
                }
            }
        }

        protected virtual void VisitParameterDeclaration(QueryExpressionContainer parameterDeclaration)
        {
            VisitExpression(parameterDeclaration);
        }

        protected virtual void VisitLetBinding(QueryExpressionContainer letBinding)
        {
            VisitExpression(letBinding);
        }

        protected virtual void VisitSelectExpression(QueryExpressionContainer expression)
        {
            VisitExpression(expression);
        }

        protected virtual void VisitGroupByExpression(QueryExpressionContainer expression)
        {
            VisitExpression(expression);
        }

        protected virtual void VisitFilter(QueryFilter filter)
        {
            if (filter.Target != null)
            {
                foreach (QueryExpressionContainer item in filter.Target)
                {
                    VisitExpression(item);
                }
            }

            if (filter.Condition != null)
            {
                VisitExpression(filter.Condition);
            }
        }

        protected virtual void VisitAxis(QueryAxis axis)
        {
            if (axis.Groups == null)
            {
                return;
            }

            foreach (QueryAxisGroup group in axis.Groups)
            {
                VisitAxisGroup(group);
            }
        }

        protected virtual void VisitAxisGroup(QueryAxisGroup axisGroup)
        {
            if (axisGroup.Keys == null)
            {
                return;
            }

            foreach (QueryExpressionContainer key in axisGroup.Keys)
            {
                VisitExpression(key);
            }
        }

        protected virtual void VisitSortClause(QuerySortClause sortClause)
        {
            if (sortClause.Expression != null)
            {
                VisitExpression(sortClause.Expression);
            }
        }

        protected virtual void VisitTransform(QueryTransform transform)
        {
            if (transform.Input != null)
            {
                VisitTransformInput(transform.Input);
            }

            if (transform.Output != null)
            {
                VisitTransformOutput(transform.Output);
            }
        }

        protected virtual void VisitTransformInput(QueryTransformInput transformInput)
        {
            List<QueryExpressionContainer> parameters = transformInput.Parameters;
            if (parameters != null)
            {
                foreach (QueryExpressionContainer item in parameters)
                {
                    VisitExpression(item);
                }
            }

            if (transformInput.Table != null)
            {
                VisitTransformTable(transformInput.Table);
            }
        }

        protected virtual void VisitTransformOutput(QueryTransformOutput transformOutput)
        {
            if (transformOutput.Table != null)
            {
                VisitTransformTable(transformOutput.Table);
            }
        }

        protected virtual void VisitTransformTable(QueryTransformTable transformTable)
        {
            List<QueryTransformTableColumn> columns = transformTable.Columns;
            if (columns != null)
            {
                return;
            }

            foreach (QueryTransformTableColumn item in columns)
            {
                VisitTransformTableColumn(item);
            }
        }

        protected virtual void VisitTransformTableColumn(QueryTransformTableColumn transformTableColumn)
        {
            if (transformTableColumn.Expression != null)
            {
                VisitExpression(transformTableColumn.Expression);
            }
        }

        protected virtual void VisitEntitySource(EntitySource source)
        {
            if (source.Expression != null)
            {
                VisitExpression(source.Expression);
            }
        }

        public virtual void Visit(QueryDefinition queryDefinition)
        {
            if (queryDefinition.Parameters != null)
            {
                foreach (QueryExpressionContainer parameter in queryDefinition.Parameters)
                {
                    VisitParameterDeclaration(parameter);
                }
            }

            if (queryDefinition.Let != null)
            {
                foreach (QueryExpressionContainer item in queryDefinition.Let)
                {
                    VisitLetBinding(item);
                }
            }

            if (queryDefinition.From != null)
            {
                foreach (EntitySource item2 in queryDefinition.From)
                {
                    VisitEntitySource(item2);
                }
            }

            if (queryDefinition.Where != null)
            {
                foreach (QueryFilter item3 in queryDefinition.Where)
                {
                    VisitFilter(item3);
                }
            }

            if (queryDefinition.VisualShape != null)
            {
                foreach (QueryAxis item4 in queryDefinition.VisualShape)
                {
                    VisitAxis(item4);
                }
            }

            if (queryDefinition.OrderBy != null)
            {
                foreach (QuerySortClause item5 in queryDefinition.OrderBy)
                {
                    VisitSortClause(item5);
                }
            }

            if (queryDefinition.Select != null)
            {
                foreach (QueryExpressionContainer item6 in queryDefinition.Select)
                {
                    VisitSelectExpression(item6);
                }
            }

            if (queryDefinition.GroupBy != null)
            {
                foreach (QueryExpressionContainer item7 in queryDefinition.GroupBy)
                {
                    VisitGroupByExpression(item7);
                }
            }

            if (queryDefinition.Transform == null)
            {
                return;
            }

            foreach (QueryTransform item8 in queryDefinition.Transform)
            {
                VisitTransform(item8);
            }
        }
    }
}
