using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace GameUi.Scripts.ServiceTest
{
    public class DemoServiceAsync
    {
        public class Response
        {
            private const string OkMessage = "OK";

            public readonly bool Success;
            public readonly string Message;
            public readonly string Payload;

            public Response(string payload)
            {
                Success = true;
                Message = OkMessage;
                Payload = payload;
            }

            public Response(string message, string payload)
            {
                Success = false;
                Message = message;
                Payload = payload;
            }
        }

        private const string ServiceUrl = "https://jsonplaceholder.typicode.com/todos/1";
        //private const string ServiceUrl = "http://www.fakeresponse.com/";

        private string _lastResponse = string.Empty;

        public async Task<Response> GetVersionInfo()
        {
            Debug.Log($"GetVersionInfo start");

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                var request = WebRequest.Create(ServiceUrl);
                if (!(await request.GetResponseAsync() is HttpWebResponse response))
                {
                    return new Response("Request failed: (NULL response)", string.Empty);
                }
                Debug.Log($"response {response.StatusCode}");
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new Response($"Request failed: {response.StatusCode}", string.Empty);
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
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return new Response($"Request failed: {e.Message}", string.Empty);
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
            return new Response(_lastResponse);
        }
    }
}