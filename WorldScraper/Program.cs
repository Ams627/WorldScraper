using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WorldScraper
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                //var line = @"a class=""mt_a"" href=""country/saint-pierre-and-miquelon/"">Saint Pierre Miquelon</a>";
                //var pattern = @"a class=""mt_a"" href="".+?"">(.+)</a>";

                //var match = Regex.Match(line, pattern);
                //if (match.Success && match.Groups.Count > 1)
                //{
                //    Console.WriteLine($"{match.Groups[1]}");
                //}

                var wo = new Worldometer();
                var filename = await wo.Scrape();
                Utils.RunCommand("git", $"add {filename}");
                var now = DateTime.Now;
                Utils.RunCommand("git", $"commit -m\"update at {now:HH:mm:ss} on {now:dd-MMM-yyyy}\"");
                Utils.RunCommand("git", "push");
            }
            catch (Exception ex)
            {
                var fullname = System.Reflection.Assembly.GetEntryAssembly().Location;
                var progname = Path.GetFileNameWithoutExtension(fullname);
                Console.Error.WriteLine(progname + ": Error: " + ex.Message);
            }

        }
    }
}
