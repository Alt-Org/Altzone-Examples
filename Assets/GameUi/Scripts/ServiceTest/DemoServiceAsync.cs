using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GameUi.Scripts.ServiceTest
{
    public static class DemoServiceAsync
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

        public static Task<Response> GetVersionInfo()
        {
            return ExecuteRequest("https://jsonplaceholder.typicode.com/todos/1");
        }

        public static Task<Response> GetTimeInfo()
        {
            // https://www.programmableweb.com/api/worldtime-rest-api-v011
            return ExecuteRequest("http://worldtimeapi.org/api/timezone/Europe/Helsinki");
        }

        private static async Task<Response> ExecuteRequest(string url)
        {
            Debug.Log($"ExecuteRequest start {url}");
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            string responseString = null;
            try
            {
                var request = WebRequest.Create(url);
                if (!(await request.GetResponseAsync() is HttpWebResponse response))
                {
                    return new Response("Request failed: (NULL response)", string.Empty);
                }
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new Response($"Request failed: {response.StatusCode}", string.Empty);
                }
                using (var dataStream = response.GetResponseStream())
                {
                    if (dataStream != null)
                    {
                        var reader = new StreamReader(dataStream);
                        responseString = await reader.ReadToEndAsync();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return new Response($"Request failed: {e.Message}", string.Empty);
            }
            if (responseString == null)
            {
                responseString = string.Empty;
            }
            Debug.Log($"response len {responseString.Length} in {stopWatch.ElapsedMilliseconds} ms");
            responseString = responseString.Replace("\r", "").Replace("\n", "");
            Debug.Log($"json {responseString}");
            stopWatch.Stop();
            Debug.Log($"ExecuteRequest done in {stopWatch.ElapsedMilliseconds} ms");
            return new Response(responseString);
        }
    }
}