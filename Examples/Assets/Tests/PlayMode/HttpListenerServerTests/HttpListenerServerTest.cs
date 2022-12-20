using System;
using System.Collections;
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
        private bool _isRequestReady;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Debug.Log("");
            _server = new SimpleListenerServer(Port);
            _server.Start();
        }

        [UnityTest]
        public IEnumerator SendRequestAsyncTest()
        {
            Debug.Log($"test");
            yield return new WaitUntil(() => _server.IsRunning);

            StartRequest(SendRequestAsync);
            yield return new WaitUntil(() => _isRequestReady);
        }

        private void StartRequest(Action request)
        {
            Debug.Log("");
            _isRequestReady = false;
            request();
        }
    
        private async void SendRequestAsync()
        {
            Debug.Log("");
            var url = $"http://localhost:{Port}/test";
            var response = await RestApiServiceAsync.ExecuteRequest("GET", url);
            Debug.Log(response.ToString());
            _isRequestReady = true;
        }
    }
}
