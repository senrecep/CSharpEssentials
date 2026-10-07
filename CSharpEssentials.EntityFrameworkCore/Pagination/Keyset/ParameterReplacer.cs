using System.Linq.Expressions;

namespace CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;

internal sealed class ParameterReplacer(ParameterExpression from, Expression to) : ExpressionVisitor
{
    public static Expression Replace(Expression expression, ParameterExpression from, Expression to) =>
        new ParameterReplacer(from, to).Visit(expression);

    protected override Expression VisitParameter(ParameterExpression node) =>
        node == from ? to : base.VisitParameter(node);
}
