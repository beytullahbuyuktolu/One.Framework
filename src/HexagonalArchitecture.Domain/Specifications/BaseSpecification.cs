using System.Linq.Expressions;

namespace HexagonalArchitecture.Domain.Specifications;

/// <summary>
/// Base class for specifications
/// </summary>
/// <typeparam name="T">Type of the entity that check this specification</typeparam>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
        OrderBy = x => true;
        OrderByDescending = x => true;
    }
    
    protected BaseSpecification()
    {
        Criteria = _ => true;
        OrderBy = x => true;
        OrderByDescending = x => true;
    }

    public Expression<Func<T, bool>> Criteria { get; protected set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>> OrderBy { get; private set; }
    public Expression<Func<T, object>> OrderByDescending { get; private set; }
    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected virtual void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    protected virtual void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }

    public ISpecification<T> And(Expression<Func<T, bool>> criteria)
    {
        var parameter = Expression.Parameter(typeof(T));
        var body = Expression.AndAlso(
            Expression.Invoke(Criteria, parameter),
            Expression.Invoke(criteria, parameter));
        Criteria = Expression.Lambda<Func<T, bool>>(body, parameter);
        return this;
    }

    public ISpecification<T> Or(Expression<Func<T, bool>> criteria)
    {
        var parameter = Expression.Parameter(typeof(T));
        var body = Expression.OrElse(
            Expression.Invoke(Criteria, parameter),
            Expression.Invoke(criteria, parameter));
        Criteria = Expression.Lambda<Func<T, bool>>(body, parameter);
        return this;
    }
}
