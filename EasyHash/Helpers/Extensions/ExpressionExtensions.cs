namespace EasyHash.Helpers.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class ExpressionExtensions
    {
        public static Expression ForEach(this Expression collection, ParameterExpression loopVar, params Expression[] loopContent)
        {
            var elementType = loopVar.Type;
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(elementType);

            var breakLabel = Expression.Label("LoopBreak");
            var enumeratorVar = Expression.Variable(enumeratorType, "enumerator");
            var getEnumeratorCall = Expression.Call(collection, enumerableType.GetMethod(nameof(IEnumerable.GetEnumerator)));
            var moveNextCall = Expression.Call(enumeratorVar, typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext)));
            var assignLoopVar = Expression.Assign(loopVar, Expression.Property(enumeratorVar, nameof(IEnumerator.Current)));
            var expressions = new[] { assignLoopVar }.Union(loopContent);

            var loop = Expression.Block(new[] { enumeratorVar },
                Expression.Assign(enumeratorVar, getEnumeratorCall),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Equal(moveNextCall, Expression.Constant(true)),
                        Expression.Block(new[] { loopVar }, expressions),
                        Expression.Break(breakLabel)
                    ),
                breakLabel)
            );

            return loop;
        }

        public static string GetMemberName<TObj, TMember>(this Expression<Func<TObj, TMember>> memberSelector)
        {
            return GetMemberName(memberSelector.Body);
        }

        internal static string GetDebugView(this Expression exp)
        {
            if (exp == null)
            {
                return null;
            }

            var propertyInfo = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic);
            return propertyInfo.GetValue(exp) as string;
        }

        private static string GetMemberName(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Parameter:
                    return ((ParameterExpression)expression).Name;
                case ExpressionType.MemberAccess:
                    return ((MemberExpression)expression).Member.Name;
                case ExpressionType.Call:
                    return ((MethodCallExpression)expression).Method.Name;
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    return GetMemberName(((UnaryExpression)expression).Operand);
                case ExpressionType.Invoke:
                    return GetMemberName(((InvocationExpression)expression).Expression);
                case ExpressionType.ArrayLength:
                    return nameof(Array.Length);
                default:
                    throw new Exception("not a proper member selector");
            }
        }
    }
}
