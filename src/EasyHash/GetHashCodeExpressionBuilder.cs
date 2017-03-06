namespace EasyHash
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Helpers;
    using Helpers.Extensions;

    [DebuggerDisplay(nameof(DebugView))]
    public sealed class GetHashCodeExpressionBuilder<T>
    {
        private readonly HashSet<string> _skippedMembers = new HashSet<string>();
        private readonly Dictionary<string, Expression<Func<T, int, int>>> _memberHashers = new Dictionary<string, Expression<Func<T, int, int>>>();
        private readonly int _colPrime1 = 486187739;
        private readonly int _colPrime2 = 486190561;
        private int _prime1 = unchecked((int)2166136261);
        private int _prime2 = RandomProvider.GetPrime(); //Force users to not depend on generated hashcode
        private bool _includeCollectionItems = true;

        internal GetHashCodeExpressionBuilder() { }

        internal string DebugView => Build().GetDebugView();

        public GetHashCodeExpressionBuilder<T> WithPrimes(int prime1, int prime2)
        {
            _prime1 = prime1;
            _prime2 = prime2;
            return this;
        }

        public GetHashCodeExpressionBuilder<T> Skip(Expression<Func<T, object>> member)
        {
            _skippedMembers.Add(member.GetMemberName());
            return this;
        }

        public GetHashCodeExpressionBuilder<T> For(Expression<Func<T, object>> member, Expression<Func<T, int, int>> hashMember)
        {
            _memberHashers.Add(member.GetMemberName(), hashMember);
            return this;
        }

        public GetHashCodeExpressionBuilder<T> ExcludeCollectionItems()
        {
            _includeCollectionItems = false;
            return this;
        }

        internal Expression<Func<T, int>> Build()
        {
            MemberInfo[] members = GetHashableMembers();
            var sourceParam = Expression.Parameter(typeof(T), "x");
            var hashVariable = Expression.Parameter(typeof(int), "hash");
            var variables = new List<ParameterExpression>() { hashVariable };
            var expressions = new List<Expression>(members.Length + 2)
            {
                Expression.Assign(hashVariable, Expression.Constant(_prime1, typeof (int)))
            };

            foreach (MemberInfo member in members)
            {
                MemberExpression memberExpr = Expression.PropertyOrField(sourceParam, member.Name);
                Type enumerableType = typeof(string) == memberExpr.Type ? null : memberExpr.Type.GetEnumerableType();

                if (_memberHashers.ContainsKey(member.Name))
                {
                    expressions.Add(BuildItemCustomHashing(_memberHashers[member.Name], sourceParam, hashVariable));
                }
                else if (enumerableType != null && _includeCollectionItems)
                {
                    BuildCollectionHashing(expressions, variables, memberExpr, hashVariable, enumerableType, _prime2);
                }
                else
                {
                    expressions.Add(BuildItemHashing(memberExpr, memberExpr.Type, hashVariable, _prime2));
                }
            }

            expressions.Add(hashVariable);
            return Expression.Lambda<Func<T, int>>(Expression.Block(variables, expressions), sourceParam);
        }

        private void BuildCollectionHashing(List<Expression> expressions,
                                            List<ParameterExpression> variables,
                                            MemberExpression collMember,
                                            ParameterExpression hashVariable,
                                            Type itemType,
                                            int prime2)
        {
            var colhashVar = Expression.Parameter(typeof(int));
            var currentItem = Expression.Parameter(itemType);
            variables.Add(colhashVar);

            expressions.Add(Expression.Assign(colhashVar, Expression.Constant(_colPrime1, typeof(int))));

            expressions.Add(Expression.IfThen(
                                Expression.NotEqual(
                                        collMember,
                                        Expression.Default(collMember.Type)
                                    ),
                                collMember.ForEach(currentItem,
                                    BuildItemHashing(currentItem, itemType, colhashVar, _colPrime2)
                                )
                            ));

            expressions.Add(Expression.Assign(
                                hashVariable,
                                Expression.ExclusiveOr(
                                        Expression.Multiply(
                                                hashVariable,
                                                Expression.Constant(prime2, typeof(int))
                                            ),
                                        colhashVar
                                    )
                            ));
        }

        private Expression BuildItemCustomHashing(Expression<Func<T, int, int>> customHasher,
                                                  ParameterExpression sourceParam,
                                                  ParameterExpression hashVariable)
        {
            return Expression.Assign(
                    hashVariable,
                    //TODO: inline custom expression and rebind parameters with visitor
                    Expression.Invoke(customHasher, sourceParam, hashVariable)
                );
        }

        private Expression BuildItemHashing(Expression item,
                                            Type type,
                                            ParameterExpression hashVariable,
                                            int prime)
        {
            //hash = (hash * prime) ^ (field == default(object) ? 0 : field.GetHashCode());
            return Expression.Assign(
                    hashVariable,
                    Expression.ExclusiveOr(
                            Expression.Multiply(
                                    hashVariable,
                                    Expression.Constant(prime, typeof(int))
                                ),
                            Expression.Condition(
                                    Expression.Equal(
                                            item,
                                            Expression.Default(type)
                                        ),
                                    Expression.Constant(0, typeof(int)),
                                    Expression.Call(item, typeof(object).GetMethod(nameof(GetHashCode)))
                                )
                        )
                );
        }

        private MemberInfo[] GetHashableMembers()
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            IEnumerable<MemberInfo> fields = typeof(T).GetFields(flags)
                .Where(f => !f.IsBackingField());

            IEnumerable<MemberInfo> properties = typeof(T).GetProperties(flags)
                .Where(p => p.CanRead);

            return fields.Union(properties)
                .Where(m => !_skippedMembers.Contains(m.Name))
                .ToArray();
        }
    }
}
