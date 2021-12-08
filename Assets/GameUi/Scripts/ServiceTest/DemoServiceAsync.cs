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

            var request = WebRequest.Create(url);
            var response = await request.GetResponseAsync() as HttpWebResponse;
            if (response == null)
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
                    jsonResponse = await reader.ReadToEndAsync();
                }
            }

            //var map = JsonUtility.FromJson<Dictionary<string, object>>(jsonResponse);
            return jsonResponse;
        }
    }
}