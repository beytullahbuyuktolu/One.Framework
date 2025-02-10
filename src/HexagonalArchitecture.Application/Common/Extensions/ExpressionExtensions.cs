using System.Linq.Expressions;

namespace HexagonalArchitecture.Application.Common.Extensions;

public static class ExpressionExtensions
{
    public static Expression Replace(this Expression expression, Expression searchEx, Expression replaceEx)
    {
        return new ExpressionReplacer(searchEx, replaceEx).Visit(expression);
    }

    private class ExpressionReplacer : ExpressionVisitor
    {
        private readonly Expression _from;
        private readonly Expression _to;

        public ExpressionReplacer(Expression from, Expression to)
        {
            _from = from;
            _to = to;
        }

        public override Expression Visit(Expression node)
        {
            return node == _from ? _to : base.Visit(node);
        }
    }
}
