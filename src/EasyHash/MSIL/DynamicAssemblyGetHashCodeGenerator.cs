namespace EasyHash.MSIL
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class DynamicAssemblyGetHashCodeGenerator<T>
    {
        private const string Type = "DynamicType";
        private const string Method = "DynamicGetHashCode";
        public string IlCode { get; private set; }

        internal Func<T, int> Build(GetHashCodeConfiguration<T> configuration = null)
        {
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
                typeof(int),
                new[] { typeof(T) });

            ILGenerator ilGen = mb.GetILGenerator();
            new GetHashCodeIlGenerator<T>(configuration).Generate(ilGen);

            Type realType = tb.CreateType();
            var meth = realType.GetMethod(Method);
            IlCode = GetILCodeOrError(assemBuilder, moduleFileName, directory);

            return (Func<T, int>)Delegate.CreateDelegate(typeof(Func<T, int>), meth);
        }

        internal string BuildMsil(GetHashCodeConfiguration<T> configuration = null)
        {
            Build(configuration);
            return IlCode;
        }

        private string GetILCodeOrError(AssemblyBuilder builder, string moduleFileName, string directory)
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
    }
}
