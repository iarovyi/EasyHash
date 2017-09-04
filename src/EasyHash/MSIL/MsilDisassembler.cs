namespace EasyHash.MSIL
{
    using System;
    using System.IO;
    using Helpers;
    using System.Text.RegularExpressions;

    internal class MsilDisassembler
    {
        private readonly string Msil;

        public MsilDisassembler(string portableExecutablePath)
        {
            Msil = ParseMsil(portableExecutablePath);
        }

        public string GetMethod(string methodName, string type)
        {
            string pattern = $@"\.method \w+ \w+ \w+  {methodName}((.|\n)*)}}(?= \/\/ end of method {type}::{methodName})";
            return Regex.Match(Msil, pattern).Value;
        }

        private string ParseMsil(string portableExecutablePath)
        {
            var disassembledPath = Path.ChangeExtension(portableExecutablePath, "il");

            try
            {
                ProcessResult result = new ProcessRunner("ildasm.exe").Run($"{portableExecutablePath} /output:{disassembledPath}");
                if (!result.IsSuccess)
                {
                    throw new Exception($"Failed to disassemble dynamic assembly: {result.Output}");
                }

                return File.ReadAllText(disassembledPath);
            }
            finally
            {
                File.Delete(disassembledPath);
            }
        }
    }
}
