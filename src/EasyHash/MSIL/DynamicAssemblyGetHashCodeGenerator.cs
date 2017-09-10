namespace EasyHash.MSIL
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class DynamicAssemblyGetHashCodeGenerator<T>
    {
        internal Func<T, int> Build(GetHashCodeConfiguration<T> configuration = null) =>
            DynamicAssemblyMethodGenerator<Func<T, int>>
            .Build((ilGen) => new GetHashCodeIlGenerator<T>(configuration).Generate(ilGen))
            .Method;

        internal string BuildMsil(GetHashCodeConfiguration<T> configuration = null) =>
            DynamicAssemblyMethodGenerator<Func<T, int>>
            .Build((ilGen) => new GetHashCodeIlGenerator<T>(configuration).Generate(ilGen))
            .IlCode;
    }

    internal class DynamicAssemblyMethodGenerator<TSignatureDelegate> where TSignatureDelegate : class
    {
        private const string Type = "DynamicType";
        private const string Method = "DynamicGetHashCode";

        public static MethodBuildResult<TSignatureDelegate> Build(Action<ILGenerator> generateBody)
        {
            Type[] genericParams = typeof(TSignatureDelegate).GetGenericArguments();
            bool hasReturn = typeof(TSignatureDelegate).GetGenericTypeDefinition().Name.StartsWith("Func`");
            Type returnType = hasReturn ? genericParams.Last() : null;
            Type[] parameterTypes = hasReturn && genericParams.Length > 1 ? genericParams.Take(genericParams.Length - 1).ToArray()
                : genericParams;

            string directory = Directory.GetCurrentDirectory();
            AppDomain domain = AppDomain.CurrentDomain;
            AssemblyName aname = new AssemblyName(Guid.NewGuid().ToString());
            AssemblyBuilder assemBuilder = domain.DefineDynamicAssembly(aname, AssemblyBuilderAccess.RunAndSave, directory);
            string moduleFileName = $"{assemBuilder.GetName().Name}.mod";
            ModuleBuilder modBuilder = assemBuilder.DefineDynamicModule(assemBuilder.GetName().Name, moduleFileName);
            TypeBuilder tb = modBuilder.DefineType(Type, TypeAttributes.Public | TypeAttributes.Class);

            MethodBuilder mb = tb.DefineMethod(Method,
                MethodAttributes.Static | MethodAttributes.Public,
                CallingConventions.Standard,
                returnType,
                parameterTypes);

            generateBody(mb.GetILGenerator());

            Type realType = tb.CreateType();
            var meth = realType.GetMethod(Method);
            string ilCode = GetIlCodeOrError(assemBuilder, moduleFileName, directory);
            var method = Delegate.CreateDelegate(typeof(TSignatureDelegate), meth) as TSignatureDelegate;
            return new MethodBuildResult<TSignatureDelegate>(method, ilCode);
        }

        private static string GetIlCodeOrError(AssemblyBuilder builder, string moduleFileName, string directory)
        {
            try
            {
                string dllName = $"{Guid.NewGuid()}.dll";
                string assemblyPath = Path.Combine(directory, dllName);
                string modulePath = Path.Combine(directory, moduleFileName);

                builder.Save(dllName);

                try
                {
                    return new MsilDisassembler(modulePath).GetMethod(Method, Type);
                }
                finally
                {
                    File.Delete(assemblyPath);
                    File.Delete(modulePath);
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public class MethodBuildResult<TDelegate>
        {
            public readonly TDelegate Method;
            public readonly string IlCode;

            public MethodBuildResult(TDelegate method, string ilCode)
            {
                Method = method;
                IlCode = ilCode;
            }
        }
    }
}
