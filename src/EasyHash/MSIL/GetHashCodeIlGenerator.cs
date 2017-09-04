namespace EasyHash.MSIL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using Helpers.Extensions;

    internal class GetHashCodeIlGenerator<T>
    {
        private readonly GetHashCodeConfiguration<T> _configuration;

        public GetHashCodeIlGenerator(GetHashCodeConfiguration<T> configuration = null)
        {
            _configuration = configuration ?? GetHashCodeConfiguration<T>.Default;
        }

        internal void Generate(ILGenerator ilGen)
        {
            MemberInfo[] members = GetHashableMembers();

            if (members.Any())
            {
                EmitMemberHashing(ilGen, members.First(), true);
                foreach (MemberInfo member in members.Skip(1))
                {
                    EmitMemberHashing(ilGen, member, false);
                }
            }
            else
            {
                ilGen.Emit(OpCodes.Ldc_I4_0);
            }
            ilGen.Emit(OpCodes.Ret);
        }

        private void EmitMemberHashing(ILGenerator ilGen, MemberInfo member, bool isFirst)
        {
            Type memberType = (member as PropertyInfo)?.PropertyType ?? ((FieldInfo)member).FieldType;
            Type enumerableType = typeof(string) == memberType ? null : memberType.GetEnumerableType();

            if (_configuration.MemberHashers.ContainsKey(member.Name))
            {
                EmitCustomMemberHashing(ilGen, member);
            }
            else if (enumerableType != null && _configuration.IncludeCollectionItems)
            {
                EmitCollectionMemberHashing(ilGen, member);
            }
            else
            {
                EmitMemberHashingLine(ilGen, member, memberType, isFirst);
            }
        }

        //                   (member != null ? member.GetHashCode() : 0);
        //(hashCode * prime) ^ (member != null ? member.GetHashCode() : 0)
        private void EmitMemberHashingLine(ILGenerator ilGen, MemberInfo member, Type memberType, bool isFirst)
        {
            bool isProp = member is PropertyInfo;
            bool isInt32 = memberType == typeof(int);
            MethodInfo memberGetHashCode = memberType.GetMethod(nameof(GetHashCode));

            if (!isFirst)
            {
                ilGen.Emit(OpCodes.Ldc_I4, _configuration.Prime1);
                ilGen.Emit(OpCodes.Mul);
            }

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.EmitLoadPropertyOfFieldOnStack(member, memberType);

            if (!isInt32 && !memberType.IsEnum)
            {
                if (memberType.IsValueType)
                {
                    if (isProp)
                    {
                        LocalBuilder localVar = ilGen.DeclareLocal(memberType);
                        ilGen.Emit(OpCodes.Stloc, localVar);
                        ilGen.Emit(OpCodes.Ldloca_S, localVar);
                    }

                    ilGen.Emit(OpCodes.Constrained, memberType);
                    ilGen.Emit(OpCodes.Callvirt, memberGetHashCode);
                }
                else
                {
                    Label xorOrNextLabel = ilGen.DefineLabel();
                    Label nonEmptyLabel = ilGen.DefineLabel();

                    ilGen.Emit(OpCodes.Brtrue_S, nonEmptyLabel);
                    ilGen.Emit(OpCodes.Ldc_I4_0);
                    ilGen.Emit(OpCodes.Br_S, xorOrNextLabel);

                    ilGen.MarkLabel(nonEmptyLabel);
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.EmitLoadPropertyOfFieldOnStack(member, memberType);
                    ilGen.Emit(OpCodes.Callvirt, memberGetHashCode);

                    ilGen.MarkLabel(xorOrNextLabel);
                }
            }

            if (!isFirst)
            {
                ilGen.Emit(OpCodes.Xor);
            }
        }

        private void EmitCustomMemberHashing(ILGenerator ilGen, MemberInfo member)
        {
            /*Func<T, int, int> customHashFn = _configuration.MemberHashers[member.Name].Compile();
            ilGen.Emit(OpCodes.Ldnull);
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldc_I4, _configuration.Prime1);
            ilGen.Emit(OpCodes.Call, customHashFn.GetMethodInfo());*/
            throw new NotImplementedException("TODO: implement custom member hashing as IL generation");
        }

        private void EmitCollectionMemberHashing(ILGenerator ilGen, MemberInfo member)
        {
            throw new NotImplementedException("TODO: implement collection member hashing as IL generation");

        }

        private MemberInfo[] GetHashableMembers()
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            IEnumerable<MemberInfo> fields = typeof(T).GetFields(flags)
                .Where(f => !f.IsBackingField());

            IEnumerable<MemberInfo> properties = typeof(T).GetProperties(flags)
                .Where(p => p.CanRead);

            return properties.Union(fields)
                .Where(m => !_configuration.SkippedMembers.Contains(m.Name))
                .ToArray();
        }
    }

    internal static class IlGeneratorExtensions
    {
        public static void EmitLoadPropertyOfFieldOnStack(this ILGenerator ilGen, MemberInfo propertyOfField, Type type)
        {
            if (propertyOfField is PropertyInfo)
            {
                ilGen.Emit(OpCodes.Call, ((PropertyInfo)propertyOfField).GetGetMethod());
            } else
            {
                OpCode loadCode = type.IsValueType && !type.IsEnum && type != typeof(int) ? OpCodes.Ldflda : OpCodes.Ldfld;
                ilGen.Emit(loadCode, (FieldInfo)propertyOfField);
            }
        }
    }
}
