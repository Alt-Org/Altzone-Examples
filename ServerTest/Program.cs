using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace ServerTest
{
    /// <summary>
    /// <c>ISimpleListenerServer</c> and <c>RestApiServer</c> command line request tester.
    /// </summary>
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var requestCount = 30;
            if (args.Length == 1)
            {
                int.TryParse(args[0], out requestCount);
            }
            Console.WriteLine($"Start {requestCount} requests");
            var stopwatch = Stopwatch.StartNew();
            Parallel.For(0, requestCount, i =>
                {
                    var webRequest = WebRequest.Create($"http://localhost:8090/server/ping/{i}");
                    Console.WriteLine($"  Send {stopwatch.ElapsedMilliseconds} #{i}");
                    try
                    {
                        using (var webResponse = webRequest.GetResponse())
                        {
                            if (webResponse is HttpWebResponse httpWebResponse)
                            {
                                Console.WriteLine($"Status {stopwatch.ElapsedMilliseconds} #{i} {httpWebResponse.StatusCode}");
                            }
                        }
                    }
                    catch (WebException x)
                    {
                        Console.WriteLine($" Error {stopwatch.ElapsedMilliseconds} #{i} {x.Status} {x.Message}");
                    }
                }
            );
            stopwatch.Stop();
            Console.WriteLine($"Done in {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}