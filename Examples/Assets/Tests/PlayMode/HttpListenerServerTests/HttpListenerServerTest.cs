using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Prg.Scripts.Common.HttpListenerServer;
using Prg.Scripts.Common.RestApi;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode.HttpListenerServerTests
{
    public class HttpListenerServerTest
    {
        private const int Port = 8090;

        private ISimpleListenerServer _server;
        private ServerUrl _serverUrl;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _serverUrl = new ServerUrl($"http://localhost:{Port}/");
            Debug.Log($"{_serverUrl}");
            _server = SimpleListenerServerFactory.Create(Port);
            _server.Start();
            _server.AddHandler(new EchoHandler());
            _server.AddHandler(new MalfunctioningHandler());
            while (!_server.IsRunning)
            {
                // Because our HTTP server is starting in an other thread we can do busy wait!
                Thread.Yield();
            }
        }

        [UnityTest, Description("Start and stop server")]
        public IEnumerator StopListenerTest()
        {
            const int port = Port + 10000;
            Debug.Log($"test {port}");

            var stopServer = SimpleListenerServerFactory.Create(port);
            Assert.IsFalse(stopServer.IsRunning);
            stopServer.Start();
            yield return new WaitUntil(() => stopServer.IsRunning);
            Assert.IsTrue(stopServer.IsRunning);
            stopServer.Stop();
            Assert.IsFalse(stopServer.IsRunning);
        }
        
        [UnityTest, Description("Echo handler for GET and POST")]
        public IEnumerator EchoHandlerTest()
        {
            const string echoPath = "/echo/testing";
            const string echoData = "post_data=testing";
            
            var url = _serverUrl.GetUrlFor(echoPath);
            Debug.Log($"test {url}");

            // GET
            var response = StartRequest(() => ExecuteRequest("GET", url));
            yield return WaitTaskToComplete(response);
            Debug.Log($"response {response.Result}");

            var result = response.Result;
            Assert.AreEqual((int)HttpStatusCode.OK, result.StatusCode);

            var json = JsonUtility.FromJson<EchoMessage>(result.Payload);
            Assert.AreEqual(echoPath, json.message);
            
            // POST
            var bytes = Encoding.UTF8.GetBytes(echoData);
            response = StartRequest(() => ExecuteRequest("POST", url, bytes));
            yield return WaitTaskToComplete(response);
            Debug.Log($"response {response.Result}");

            result = response.Result;
            Assert.AreEqual((int)HttpStatusCode.OK, result.StatusCode);

            json = JsonUtility.FromJson<EchoMessage>(result.Payload);
            Assert.AreEqual(echoData, json.message);
        }
        
        [UnityTest, Description("Should not find a handler")]
        public IEnumerator NoHandlerTest()
        {
            var url = _serverUrl.GetUrlFor("no/handler");
            Debug.Log($"test {url}");

            var response = StartRequest(() => ExecuteRequest("GET", url));
            yield return WaitTaskToComplete(response);
            Debug.Log($"response {response.Result}");
            
            var result = response.Result;
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.IsTrue(result.Message.Contains("InvalidOperationException"));
        }

        [UnityTest, Description("Handler throws an exception")]
        public IEnumerator MalfunctioningHandlerTest()
        {
            var url = _serverUrl.GetUrlFor("malfunction/error");
            Debug.Log($"test {url}");

            var response = StartRequest(() => ExecuteRequest("GET", url));
            yield return WaitTaskToComplete(response);
            Debug.Log($"response {response.Result}");
            
            var result = response.Result;
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.IsTrue(result.Message.Contains("IndexOutOfRangeException"));
        }

        private async Task<RestApiServiceAsync.Response> ExecuteRequest(string verb, string url, object content = null)
        {
            return await RestApiServiceAsync.ExecuteRequest(verb, url, content);
        }

        private static Task<RestApiServiceAsync.Response> StartRequest(Func<Task<RestApiServiceAsync.Response>> request)
        {
            return request();
        }

        private static IEnumerator WaitTaskToComplete(Task task)
        {
            yield return new WaitUntil(() => task.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
        }
    }

    [Serializable]
    public class EchoMessage
    {
        public string message;

        public EchoMessage(string message)
        {
            this.message = message;
        }
    }

    public class EchoHandler : IListenerServerHandler
    {
        public object HandleRequest(HttpListenerRequest request, string body)
        {
            var path = request.Url.AbsolutePath;
            if (!path.StartsWith("/echo/"))
            {
                return null;
            }
            var tokens = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (request.HttpMethod == "GET" && tokens.Length > 1)
            {
                // Echo URL back.
                return new EchoMessage(path);
            }
            if (request.HttpMethod == "POST")
            {
                // Echo body back.
                return new EchoMessage(body);
            }
            throw new InvalidOperationException($"invalid request for {GetType().Name}");
        }
    }

    public class MalfunctioningHandler : IListenerServerHandler
    {
        public object HandleRequest(HttpListenerRequest request, string body)
        {
            var path = request.Url.AbsolutePath;
            if (!path.StartsWith("/malfunction/"))
            {
                return null;
            }
            if (request.HttpMethod == "GET" && path.StartsWith("/malfunction/error"))
            {
                // Will throw IndexOutOfRangeException!
                var array = Array.Empty<byte>();
                return array[99];
            }
            throw new InvalidOperationException($"invalid request for {GetType().Name}");
        }
    }
}