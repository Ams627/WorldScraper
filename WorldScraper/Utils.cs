using System.Diagnostics;

namespace WorldScraper
{
    class Utils
    {
        /// <summary>
        /// Run a command using Process.Start - used mainly for running git:
        /// </summary>
        /// <param name="program">program name</param>
        /// <param name="args">arguments - space separated</param>
        /// <returns>The exit code of the command</returns>
        public static int RunCommand(string program, string args)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = program,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            var process = Process.Start(processInfo);

            Debug.WriteLine($"---- stdout: ----");
            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();
                Debug.WriteLine(line);
            }
            Debug.WriteLine($"---- stderr: ----");
            while (!process.StandardError.EndOfStream)
            {
                var line = process.StandardError.ReadLine();
                Debug.WriteLine(line);
            }

            process.WaitForExit();
            var result = process.ExitCode;
            return result;
        }

    }
}