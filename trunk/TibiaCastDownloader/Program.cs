using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;

namespace TibiaCastDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            var pages = 50;

            WebClient webClient = new WebClient();
            Process process = Process.GetProcessesByName("Tibiacast Client").FirstOrDefault();

            for (int page = 0; page < pages; page++)
            {
                Console.WriteLine("Downloading page " + page + " content.");
                var pageContent = webClient.DownloadString("https://www.tibiacast.com/recordings?search=Yalahar&sort=Sort%20by%20date&page=" + page);

                var matches = Regex.Matches(pageContent, @"tibiacast:recording:([\d]+)");
                Console.WriteLine("Found " + matches.Count + " records on page " + page + ".");
                foreach (Match match in matches)
                {
                    if (process == null)
                    {
                        Console.WriteLine("Opening Tibiacast Client.");
                        process = Process.Start(@"C:\Program Files\Tibiacast\Tibiacast Client.exe");
                        process.WaitForInputIdle();
                    }

                    var recordId = match.Groups[1].Value;

                    if (File.Exists(System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tibiacast", "Recordings", recordId + ".recording")))
                    {
                        Console.WriteLine("Skiping record " + recordId + ".");
                        continue;
                    }

                    Console.WriteLine("Sending record " + recordId + " to Tibiacast Client.");

                    Process.Start(@"C:\Program Files\Tibiacast\Tibiacast Client.exe", "tibiacast:recording:" + recordId);
                    process.WaitForInputIdle(1000);
                    Thread.Sleep(1000);

                    process.Refresh();
                    if (process.Responding == false)
                    {
                        Console.WriteLine("Oh no! Tibiacast Client is not responding and will be killed.");
                        process.Kill();
                        process = null;
                    }
                }

                Console.WriteLine("Waiting a little before moving to next page.");
                Thread.Sleep(20000);

                if (process != null && !process.HasExited)
                {
                    Console.WriteLine("Killing the Tibiacast Client before moving to next page.");
                    process.Kill();
                    process = null;
                }

            }

            Console.WriteLine("All pages were processed. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
