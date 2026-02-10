using System;
using System.Linq.Expressions;

namespace ClinicManagement.Domain.Common;

/// <summary>
/// Base class for specifications - encapsulates query logic in a reusable, composable way
/// Follows the Specification Pattern from Domain-Driven Design
/// </summary>
/// <typeparam name="T">The entity type this specification applies to</typeparam>
public abstract class Specification<T>
{
    /// <summary>
    /// Converts the specification to a LINQ expression that can be used in queries
    /// </summary>
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Checks if an entity satisfies this specification
    /// </summary>
    public bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }

    /// <summary>
    /// Combines this specification with another using AND logic
    /// </summary>
    public Specification<T> And(Specification<T> other)
    {
        return new AndSpecification<T>(this, other);
    }

    /// <summary>
    /// Combines this specification with another using OR logic
    /// </summary>
    public Specification<T> Or(Specification<T> other)
    {
        return new OrSpecification<T>(this, other);
    }

    /// <summary>
    /// Negates this specification
    /// </summary>
    public Specification<T> Not()
    {
        return new NotSpecification<T>(this);
    }

    /// <summary>
    /// Implicit conversion to Expression for easy use in LINQ queries
    /// </summary>
    public static implicit operator Expression<Func<T, bool>>(Specification<T> specification)
    {
        return specification.ToExpression();
    }
}

/// <summary>
/// Combines two specifications with AND logic
/// </summary>
internal class AndSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public AndSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();

        var parameter = Expression.Parameter(typeof(T));

        var leftVisitor = new ReplaceExpressionVisitor(leftExpression.Parameters[0], parameter);
        var left = leftVisitor.Visit(leftExpression.Body);

        var rightVisitor = new ReplaceExpressionVisitor(rightExpression.Parameters[0], parameter);
        var right = rightVisitor.Visit(rightExpression.Body);

        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left!, right!), parameter);
    }
}

/// <summary>
/// Combines two specifications with OR logic
/// </summary>
internal class OrSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public OrSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();

        var parameter = Expression.Parameter(typeof(T));

        var leftVisitor = new ReplaceExpressionVisitor(leftExpression.Parameters[0], parameter);
        var left = leftVisitor.Visit(leftExpression.Body);

        var rightVisitor = new ReplaceExpressionVisitor(rightExpression.Parameters[0], parameter);
        var right = rightVisitor.Visit(rightExpression.Body);

        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(left!, right!), parameter);
    }
}

/// <summary>
/// Negates a specification
/// </summary>
internal class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _specification;

    public NotSpecification(Specification<T> specification)
    {
        _specification = specification;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var expression = _specification.ToExpression();
        var parameter = expression.Parameters[0];
        var body = Expression.Not(expression.Body);

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}

/// <summary>
/// Helper class to replace expression parameters when combining specifications
/// </summary>
internal class ReplaceExpressionVisitor : ExpressionVisitor
{
    private readonly Expression _oldValue;
    private readonly Expression _newValue;

    public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
    {
        _oldValue = oldValue;
        _newValue = newValue;
    }

    public override Expression? Visit(Expression? node)
    {
        if (node == _oldValue)
            return _newValue;

        return base.Visit(node);
    }
}
