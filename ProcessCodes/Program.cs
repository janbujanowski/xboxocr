using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProcessCodes
{
    class Program
    {
        static void Main(string[] args)
        {
            string filepathInput = @"C:\REPOS\XboxOcr\captchainput.txt";// ConfigurationManager.AppSettings["filepathInput"];
            string filepathOutput = @"C:\REPOS\XboxOcr\codesoutput.txt";// ConfigurationManager.AppSettings["filepathOutput"];
            string[] lines = File.ReadAllLines(filepathInput);
            var parts = lines[0].Split('-');
            List<string> codes = new List<string>();
            List<string> codeParts = new List<string>();
            var lastPart = parts.Single(x => x.Contains("Z"));
            var variations = parts.Where(x => !x.Contains("Z")).ToArray();
            var operations = variations.ToList();

            foreach (var indexes in Permutate("0123"))
            {
                codes.Add(string.Join("-", variations[int.Parse(indexes[0].ToString())],
                                           variations[int.Parse(indexes[1].ToString())],
                                           variations[int.Parse(indexes[2].ToString())],
                                           variations[int.Parse(indexes[3].ToString())],
                                           lastPart));
            }
            //

            var starredCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            starredCharacters = "0791582346";
            //starredCharacters = "ZYXWABCDEFGVUIJKLMNOPQRST";
            
            //starredCharacters = "A";
            //starredCharacters = "A";
            File.WriteAllLines(filepathOutput, codes);
            Console.WriteLine("plik wysrany");

            var stoper = new Stopwatch();
            stoper.Start();
            codes = File.ReadAllLines(filepathOutput).ToList();
            foreach (var code in codes)
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Host = "purchase.mp.microsoft.com";
                httpClient.DefaultRequestHeaders.Add("MS-CV", "qE0bCM4HaECHUlpE.6.0.6.6");
                httpClient.DefaultRequestHeaders.Add("Origin", "https://www.microsoft.com");
                httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.microsoft.com/uniblends/?client=AccountMicrosoftCom");

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "WLID1.0=\"GABIAgMAAAAEgAAADAAgmwLIFVo+lOmybhMr1qdz0MOjzFNjgGuqYix+9kX6n4wAAckBrKUNSdla1B6fLWYmptIW+z1M30EO78yZ+bAMOpC/R9HxEos2uUpGq7tB+q174bzDny8ly+VVu4mygWQe7RkDQHMryS6VCIFORYx5K0/Zk+Lm3GvqIAhqTCnn6b11hEN8Kh5z57jStsawze+1treNjpHNBYSlypYRy4ftBwQ2fOgBXQvVwJnuADdT8m+9OMdjnocuiGnWkJ5qh1ImoY/LVPQlvmCFmjJf+Sdd2rQCDC7Tfg+RmxAQ4mQmRRxl8Zxlx1uU2gXiVsO+/RKlkS9PWMQb4eg9pg9U5RGjsovKQF2bIpXCtdvaP1kEWcqQ6oi6xtpkwr23fBdtfzvW87AVAXsAFQH+fwMAsRjRxN90+l3fdPpdXyIBAAoQIIAwGwBqYW4uYnVqYW5vd3NraUBob3RtYWlsLmNvbQBVAAAaamFuLmJ1amFub3dza2lAaG90bWFpbC5jb20AAALHUEwAAACb4K8EFQIAAIRQVUAQBEMAA0phbgAKQnVqYW5vd3NraQAAAAAAAAAAAAAAAAAAAAAAAMzCAPAI0dy1AADfdPpd4BtxXgAAAAAAAAAAAAAAAA8AODQuMTAyLjE3Ni4xODQABQYAAAAAAAAAAAAAAAABBAAAAAAAAAAAAAAAAAAAAJm52/M1tjxqAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA/b8jAMp5HYEAAAAAAwA=\"");
                foreach (var replacement in starredCharacters)
                {

                    string urlCode = code.Replace("*", replacement.ToString());
                    var httpResponse = httpClient.GetAsync($"https://purchase.mp.microsoft.com/v7.0/tokenDescriptions/{urlCode}?market=PL&language=en-US&supportMultiAvailabilities=true");
                    httpResponse.Wait();

                    var lol = httpResponse.Result.StatusCode == System.Net.HttpStatusCode.OK;
                    if (httpResponse.Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Console.WriteLine("UNAUTH");
                    }
                    if (lol)
                    {
                        Console.WriteLine("mamy to");
                        Console.WriteLine();
                        Console.WriteLine(urlCode);
                        stoper.Stop();
                        Console.ReadLine();
                        stoper.Start();
                    }
                }
            }
            Console.WriteLine("koniec strzelania");
            Console.WriteLine($"czas {stoper.Elapsed}");
            Console.ReadLine();
        }
        private static IEnumerable<string> Permutate(string source)
        {
            if (source.Length == 1) return new List<string> { source };

            var permutations = from c in source
                               from p in Permutate(new String(source.Where(x => x != c).ToArray()))
                               select c + p;

            return permutations;
        }
        static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });

            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }
    }
}
