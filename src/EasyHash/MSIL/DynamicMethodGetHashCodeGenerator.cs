namespace EasyHash.MSIL
{
    using System;
    using System.Diagnostics;
    using System.Reflection.Emit;

    [DebuggerDisplay(nameof(DebugView))]
    internal class DynamicMethodGetHashCodeGenerator<T>
    {
        private readonly GetHashCodeConfiguration<T> _configuration;
        internal string DebugView = nameof(DynamicMethodGetHashCodeGenerator<T>);

        public DynamicMethodGetHashCodeGenerator(GetHashCodeConfiguration<T> configuration = null)
        {
            _configuration = configuration ?? GetHashCodeConfiguration<T>.Default;
        }

        internal Func<T, int> Build()
        {
            string methodName = $"{typeof(T).Name}{Guid.NewGuid()}".Replace("-", "");
            DynamicMethod method = new DynamicMethod(methodName, typeof(int), new[] { typeof(T) }, GetType(), true);
            ILGenerator ilGen = method.GetILGenerator();
            new GetHashCodeIlGenerator<T>(_configuration).Generate(ilGen);

#if DEBUG
            DebugView = new DynamicAssemblyGetHashCodeGenerator<T>().BuildMsil(_configuration);
#endif

            return (Func<T, int>)method.CreateDelegate(typeof(Func<T, int>));
        }
    }
}
