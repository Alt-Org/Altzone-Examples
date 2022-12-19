using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using ThreadState = System.Threading.ThreadState;

namespace Prg.Scripts.Common.HttpListenerServer
{
    /// <summary>
    /// Simple HTTP Server based on <c>HttpListener</c>.
    /// </summary>
    /// <remarks>
    /// Inspiration from:<br />
    /// https://gist.github.com/SimonCropp/980071
    /// https://github.com/arshad115/HttpListenerServer
    /// https://github.com/sableangle/UnityHTTPServer
    /// </remarks>
    public class SimpleListenerServer
    {
        private readonly int _port;
        private readonly Thread _serverThread;
        private readonly HttpListener _listener;

        private bool _isRunning;

        public bool IsRunning => _isRunning;

        public SimpleListenerServer(int port, HttpListenerServer watchDog = null)
        {
            _port = port;
            _serverThread = new Thread(ListenThread);
            _listener = new HttpListener();
            Application.quitting += Stop;
            if (watchDog == null)
            {
                CreateUnityWatchDog();
            }
        }

        [Conditional("UNITY_EDITOR")]
        private void CreateUnityWatchDog()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                return;
            }
#endif
            var parent = new GameObject($"{nameof(SimpleListenerServer)}:{_port}");
            Object.DontDestroyOnLoad(parent);
            var watchdog = parent.AddComponent<HttpListenerServer>();
            watchdog._port = _port;
            watchdog.Server = this;
        }

        public void Start()
        {
            Debug.Log($"{_port} {_serverThread.ThreadState}");
            if (_serverThread.ThreadState != ThreadState.Unstarted)
            {
                return;
            }
            _serverThread.Start();
        }

        public void Stop()
        {
            Debug.Log($"{_port} {_serverThread.ThreadState}");
            _serverThread.Abort();
            _listener.Stop();
        }

        private void ListenThread()
        {
            var uriPrefix = "http://*:" + _port + "/";
            _listener.Prefixes.Add(uriPrefix);
            Debug.Log($"{_port} start server @ {uriPrefix}");
            try
            {
                _listener.Start();
                _isRunning = true;
                for (;;)
                {
                    var context = _listener.GetContext();
                    HandleContext(context);
                }
            }
            catch (ThreadAbortException)
            {
                Debug.Log($"{_port} server stopped");
            }
            catch (Exception x)
            {
                Debug.Log($"{_port} unhandled exception: {x.GetType().FullName} : {x.Message}");
                Debug.LogException(x);
            }
            finally
            {
                _isRunning = false;
                _listener.Stop();
            }
        }

        private void HandleContext(HttpListenerContext context)
        {
            try
            {
                throw new NotImplementedException("HandleContext is not implemented yet");
            }
            catch (Exception x)
            {
                Debug.Log($"{_port} request handler failed: {x.GetType().FullName} : {x.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.StatusDescription = $"{x.GetType().FullName} : {x.Message}";
                context.Response.Close();
            }
        }
    }

    /// <summary>
    /// UNITY Editor helper.
    /// </summary>
    public class HttpListenerServer : MonoBehaviour
    {
        public int _port;

        public SimpleListenerServer Server;

        private void OnEnable()
        {
            Debug.Log($"port {_port}");
            if (_port > 0 && Server == null)
            {
                Server = new SimpleListenerServer(_port, this);
                Server.Start();
            }
        }
    }
}