namespace EasyHash
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Helpers;
    using Helpers.Extensions;

    public class GetHashCodeConfiguration<T>
    {
        private readonly HashSet<string> _skippedMembers = new HashSet<string>();
        private readonly Dictionary<string, Expression<Func<T, int, int>>> _memberHashers = new Dictionary<string, Expression<Func<T, int, int>>>();

        public readonly int ColPrime1 = 486187739;
        public readonly int ColPrime2 = 486190561;
        public int Prime1 { get; private set; } = unchecked((int)2166136261);
        public int Prime2 { get; private set; } = RandomProvider.GetPrime(); //Force users to not depend on generated hashcode
        public bool IncludeCollectionItems { get; private set; } = true;
        public ISet<string> SkippedMembers => _skippedMembers;
        public IReadOnlyDictionary<string, Expression<Func<T, int, int>>> MemberHashers => _memberHashers;
        public static readonly GetHashCodeConfiguration<T> Default = new GetHashCodeConfiguration<T>();

        internal GetHashCodeConfiguration() { }

        public GetHashCodeConfiguration<T> WithPrimes(int prime1, int prime2)
        {
            Prime1 = prime1;
            Prime2 = prime2;
            return this;
        }

        public GetHashCodeConfiguration<T> Skip(Expression<Func<T, object>> member)
        {
            _skippedMembers.Add(member.GetMemberName());
            return this;
        }

        public GetHashCodeConfiguration<T> For(Expression<Func<T, object>> member, Expression<Func<T, int, int>> hashMember)
        {
            _memberHashers.Add(member.GetMemberName(), hashMember);
            return this;
        }

        public GetHashCodeConfiguration<T> ExcludeCollectionItems()
        {
            IncludeCollectionItems = false;
            return this;
        }
    }
}
