using System;
using System.Collections;
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

        private SimpleListenerServer _server;
        private ServerUrl _serverUrl;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _serverUrl = new ServerUrl($"http://localhost:{Port}/");
            Debug.Log($"{_serverUrl}");
            _server = new SimpleListenerServer(Port);
            _server.Start();
        }

        [UnityTest]
        public IEnumerator SendRequestAsyncTest()
        {
            var url = _serverUrl.GetUrlFor("test");
            Debug.Log($"test {url}");

            yield return new WaitUntil(() => _server.IsRunning);

            var response = StartRequest(SendRequestAsync);
            yield return WaitTaskToComplete(response);
            Debug.Log($"response {response.Result}");

            async Task<RestApiServiceAsync.Response> SendRequestAsync()
            {
                return await RestApiServiceAsync.ExecuteRequest("GET", url);
            }
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
}