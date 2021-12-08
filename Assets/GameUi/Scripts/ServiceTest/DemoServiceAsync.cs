using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace GameUi.Scripts.ServiceTest
{
    public class DemoServiceAsync
    {
        private const string ServiceUrl = "https://jsonplaceholder.typicode.com/todos/1";

        private string _lastResponse = string.Empty;

        public async Task<string> GetVersionInfo()
        {
            Debug.Log($"GetVersionInfo start");

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var request = WebRequest.Create(ServiceUrl);
            if (!(await request.GetResponseAsync() is HttpWebResponse response))
            {
                return $"Request failed (NULL response)";
            }
            Debug.Log($"response {response.StatusCode}");
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return $"Request failed with code {response.StatusCode}";
            }
            _lastResponse = string.Empty;
            using (var dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.
                if (dataStream != null)
                {
                    var reader = new StreamReader(dataStream);
                    // Read the content.
                    _lastResponse = await reader.ReadToEndAsync() ?? string.Empty;
                }
            }
            Debug.Log($"response len {_lastResponse.Length} in {stopWatch.ElapsedMilliseconds} ms");
            _lastResponse = _lastResponse.Replace("\r", "").Replace("\n", "");
            Debug.Log($"json {_lastResponse}");
            await Task.Run(() =>
            {
                while (stopWatch.ElapsedMilliseconds < 3000)
                {
                    Thread.Yield();
                }
            });
            stopWatch.Stop();
            Debug.Log($"GetVersionInfo done in {stopWatch.ElapsedMilliseconds} ms");

            return _lastResponse;
        }
    }
}