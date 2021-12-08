using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GameUi.Scripts.ServiceTest
{
    public class DemoServiceAsync
    {
        private readonly string _serviceUrl;

        public DemoServiceAsync(string serviceUrl)
        {
            if (serviceUrl.EndsWith("/"))
            {
                serviceUrl = serviceUrl.Substring(0, serviceUrl.Length - 1);
            }
            Debug.Log($"ctor({serviceUrl})");
            _serviceUrl = serviceUrl;
        }

        public async Task<string> GetVersionInfo(string path)
        {
            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }
            var url = $"{_serviceUrl}/{path}";
            Debug.Log($"GetVersionInfo {url}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var request = WebRequest.Create(url);
            if (!(await request.GetResponseAsync() is HttpWebResponse response))
            {
                return $"Request failed (NULL response)";
            }
            Debug.Log($"response {response.StatusCode}");
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return $"Request failed with code {response.StatusCode}";
            }
            var jsonResponse = string.Empty;
            using (var dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.
                if (dataStream != null)
                {
                    var reader = new StreamReader(dataStream);
                    // Read the content.
                    jsonResponse = await reader.ReadToEndAsync() ?? string.Empty;
                }
            }
            Debug.Log($"response len {jsonResponse.Length} in {stopWatch.ElapsedMilliseconds} ms");
            while (stopWatch.ElapsedMilliseconds < 5000)
            {
                //
            }
            stopWatch.Stop();
            Debug.Log($"GetVersionInfo {url} in {stopWatch.ElapsedMilliseconds} ms");

            //var map = JsonUtility.FromJson<Dictionary<string, object>>(jsonResponse);
            return jsonResponse;
        }
    }
}