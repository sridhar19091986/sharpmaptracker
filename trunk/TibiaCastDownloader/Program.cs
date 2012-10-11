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
            string tibiacastClient = null;
            if (File.Exists(@"C:\Program Files (x86)\Tibiacast\Tibiacast Client.exe"))
                tibiacastClient = @"C:\Program Files (x86)\Tibiacast\Tibiacast Client.exe";
            else if (File.Exists(@"C:\Program Files\Tibiacast\Tibiacast Client.exe"))
                tibiacastClient = @"C:\Program Files\Tibiacast\Tibiacast Client.exe";

            if (tibiacastClient != null)
            {
                WebClient webClient = new WebClient();
                Process process = Process.GetProcessesByName("Tibiacast Client").FirstOrDefault();

                var recordDirectory = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tibiacast", "Recordings");

                for (int page = 550; page <= 600; page++)
                {
                    Console.WriteLine("Downloading page " + page + " content.");
                    var pageContent = webClient.DownloadString("https://www.tibiacast.com/recordings?sort=Sort%20by%20date&page=" + page);

                    var matches = Regex.Matches(pageContent, @"tibiacast:recording:([\d]+)");
                    Console.WriteLine("Found " + matches.Count + " records on page " + page + ".");

                    int count = 0;

                    foreach (Match match in matches)
                    {
                        if (process == null)
                        {
                            Console.WriteLine("Opening Tibiacast Client.");
                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            startInfo.FileName = tibiacastClient;
                            process = Process.Start(startInfo);
                            process.WaitForInputIdle();
                        }

                        var recordId = match.Groups[1].Value;

                        if (File.Exists(Path.Combine(recordDirectory, recordId + ".recording")))
                        {
                            Console.WriteLine("Skiping record " + recordId + ".");
                            continue;
                        }

                        var recordCount = Directory.GetFiles(recordDirectory, "*.recording").Length;
                        var timeoutTime = DateTime.Now.AddSeconds(20);

                        Console.WriteLine("Sending record " + recordId + " to Tibiacast Client.");

                        Process.Start(tibiacastClient, "tibiacast:recording:" + recordId);

                        while (timeoutTime > DateTime.Now)
                        {
                            if (Directory.GetFiles(recordDirectory, "*.recording").Length > recordCount)
                                break;

                            Thread.Sleep(500);
                        }

                        count++;
                    }

                    if (count > 0)
                    {
                        Console.WriteLine("Waiting a little before moving to next page.");
                        Thread.Sleep(2000);

                        if (process != null && !process.HasExited)
                        {
                            Console.WriteLine("Killing the Tibiacast Client before moving to next page.");
                            process.Kill();
                            process = null;
                        }
                    }
                }

                Console.WriteLine("All pages were processed. Press any key to exit...");
            }
            else
            {
                Console.WriteLine("Tibiacast client not found.");
            }

            Console.ReadKey();
        }
    }
}
