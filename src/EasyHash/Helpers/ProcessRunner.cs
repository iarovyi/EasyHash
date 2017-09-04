namespace EasyHash.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    internal class ProcessRunner
    {
        private readonly List<string> output = new List<string>();
        private readonly string path;
        private bool adjustDirectory;

        public ProcessRunner(string exeFilePath)
        {
            path = exeFilePath;
        }

        public ProcessResult Run(string arguments) => RunAsync(arguments).Result;

        public async Task<ProcessResult> RunAsync(string arguments)
        {
            string directory = adjustDirectory ? Path.GetDirectoryName(path) : Directory.GetCurrentDirectory();

            var startInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = path,
                Arguments = arguments
            };

            return await UsingCurrentDir(directory, async () =>
            {
                using (Process process = Process.Start(startInfo))
                {
                    process.OutputDataReceived += (s, e) => output.Add(e.Data);
                    process.ErrorDataReceived += (s, e) => output.Add(e.Data);
                    process.BeginOutputReadLine();
                    int code = await process.WaitForExitAsync();

                    return new ProcessResult((ProcessCode)code, string.Join(Environment.NewLine, output), path, arguments);
                }
            });
        }

        public ProcessRunner AdjustCurrentDirectory()
        {
            adjustDirectory = true;
            return this;
        }

        private T UsingCurrentDir<T>(string currentDir, Func<T> action)
        {
            string oldCurrentDir = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory(currentDir);
                return action();
            }
            finally
            {
                Directory.SetCurrentDirectory(oldCurrentDir);
            }
        }
    }

    internal class ProcessResult
    {
        public ProcessResult(ProcessCode code, string output, string executablePath, string arguments)
        {
            Code = code;
            Output = output;
            ExecutablePath = executablePath;
            Arguments = arguments;
        }

        public string ExecutablePath { get; private set; }
        public string Arguments { get; private set; }
        public string Output { get; private set; }
        public ProcessCode Code { get; private set; }

        public bool IsSuccess => Code == ProcessCode.Success;
    }

    internal enum ProcessCode
    {
        Success = 0
    }

    internal static class ProcessExtensions
    {
        public static Task<int> WaitForExitAsync(this Process process, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<int>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(((Process)sender).ExitCode);

            if (cancellationToken != default(CancellationToken))
            {
                cancellationToken.Register(tcs.SetCanceled);
            }

            return tcs.Task;
        }
    }
}
