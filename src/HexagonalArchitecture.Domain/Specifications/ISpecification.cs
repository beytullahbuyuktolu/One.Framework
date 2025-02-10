using System.Linq.Expressions;

namespace HexagonalArchitecture.Domain.Specifications;

/// <summary>
/// Encapsulates query logic for domain entities
/// </summary>
/// <typeparam name="T">Type of the entity that check this specification</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Returns a boolean expression tree that can be compiled into a predicate
    /// </summary>
    Expression<Func<T, bool>> Criteria { get; }
    
    /// <summary>
    /// Navigation properties to include
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }
    
    /// <summary>
    /// String-based include statements
    /// </summary>
    List<string> IncludeStrings { get; }
    
    /// <summary>
    /// Ordering expression for the query
    /// </summary>
    Expression<Func<T, object>> OrderBy { get; }
    
    /// <summary>
    /// Ordering descending expression for the query
    /// </summary>
    Expression<Func<T, object>> OrderByDescending { get; }
    
    /// <summary>
    /// Number of entities to skip
    /// </summary>
    int Skip { get; }
    
    /// <summary>
    /// Number of entities to take
    /// </summary>
    int Take { get; }
    
    /// <summary>
    /// True if paging is enabled
    /// </summary>
    bool IsPagingEnabled { get; }
}
