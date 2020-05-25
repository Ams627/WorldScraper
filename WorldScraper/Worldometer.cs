using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WorldScraper
{
    internal class Worldometer
    {
        private readonly HashSet<string> euCountries = new HashSet<string>() {
                "UK", "Ireland",
                "Germany", "France", "Austria",
                "Belgium", "Netherlands", "Luxembourg",
                "Portugal", "Italy", "Greece", "Spain",
                "Sweden", "Denmark", "Finland",
                "Estonia", "Latvia", "Lithuania",
                "Poland", "Czechia", "Hungary", "Slovakia", "Slovenia",
                "Romania", "Bulgaria", "Croatia",
                "Cyprus", "Malta"
            };


        List<List<string>> ProcessFile(string filename)
        {
            var countries = new List<List<string>>();
            var startPattern = @"<table id=""main_table_countries_today""";
            var tdStartPattern = @"a class=""mt_a"" href="".+?"">(.+)</a>";

            // <td style="font-weight: bold; text-align:right">889</td>
            var tdPattern = @"<td .+?>(.*)</td>";
            var hrefPattern = @"<a href=.+?>(.*?)</a>";

            var lines = File.ReadLines(filename);
            var e = lines.GetEnumerator();

            // first find <table...
            while (e.MoveNext())
            {
                var line = e.Current;
                if (Regex.Match(line, startPattern).Success)
                {
                    break;
                }
            }

            List<string> oneCountryList;

            for(; ; )
            {
                string country = string.Empty;
                while (e.MoveNext())
                {
                    var line = e.Current;
                    if (line.Contains("</table>"))
                    {
                        goto end;
                    }

                    var match = Regex.Match(line, tdStartPattern);
                    if (match.Success)
                    {
                        if (match.Groups.Count > 1)
                        {
                            country = match.Groups[1].Value;
                        }
                        break;
                    }
                }

                if (string.IsNullOrEmpty(country))
                {
                    continue;
                }

                if (!euCountries.Contains(country))
                {
                    continue;
                }

                oneCountryList = new List<string> { country };

                while (e.MoveNext())
                {
                    var line = e.Current;
                    if (line.Contains("</table>"))
                    {
                        if (oneCountryList.Any())
                        {
                            countries.Add(oneCountryList);
                        }
                        goto end;
                    }

                    if (line.Contains("</tr>"))
                    {
                        countries.Add(oneCountryList);
                        break;
                    }

                    var match = Regex.Match(line, tdPattern);
                    if (match.Success)
                    {
                        if (match.Groups.Count > 1)
                        {
                            var str = match.Groups[1].Value;
                            var match2 = Regex.Match(str, hrefPattern);
                            if (match2.Success)
                            {
                                if (match2.Groups.Count > 1)
                                {
                                    oneCountryList.Add(match2.Groups[1].Value);
                                }
                            }
                            else
                            {
                                oneCountryList.Add(str);
                            }
                        }
                    }
                }
            }

            end:;
            return countries;
        }

        internal async Task<string> Scrape()
        {
            var uri = "https://www.worldometers.info/coronavirus/";
            var filename = "scraped.html";

            using (var client = new HttpClient())
            using (var filestream = new FileStream(filename, FileMode.Create))
            {
                var response = await client.GetAsync(uri);
                var strm = await response.Content.ReadAsStreamAsync();
                strm.CopyTo(filestream);
            }

            var eulist = ProcessFile(filename);
            eulist.Sort(ListComparer);

            var results = eulist.Select(x => new { Country = x[0], TotalCases = x[1], TotalDeaths = x[3], TotalPop = x[7], DeathsPop = x[8] });

            var adocName = "readme.adoc";
            using (var filestream = new StreamWriter(adocName))
            {
                filestream.WriteLine("= Deaths Rates in the EU");
                filestream.WriteLine();
                // [cols="2", options="header"]
                filestream.WriteLine("[options=\"header\"]");
                filestream.WriteLine("|===");
                filestream.WriteLine("| | Country|Total Cases|Total Deaths| Total Cases/(1million pop)| Total Deaths/(1million pop)");
                var count = 1;
                foreach (var entry in results)
                {
                    filestream.WriteLine($"| {count++}|{entry.Country} | {entry.TotalCases} | {entry.TotalDeaths} | {entry.TotalPop} | {entry.DeathsPop}");
                }
                filestream.WriteLine("|===");
            }

            return adocName;
        }

        private int ListComparer(List<string> x, List<string> y)
        {
            var v1 = int.Parse(x[8], NumberStyles.AllowThousands);
            var v2 = int.Parse(y[8], NumberStyles.AllowThousands);
            return v2 - v1;
        }
    }
}