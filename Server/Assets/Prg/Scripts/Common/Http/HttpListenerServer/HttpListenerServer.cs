using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using ThreadState = System.Threading.ThreadState;

namespace Prg.Scripts.Common.Http.HttpListenerServer
{
    /// <summary>
    /// Simple 'localhost' HTTP Server based on <c>HttpListener</c>.<br />
    /// You can send requests to following URLs when using port number 8090:<br /> 
    /// http://localhost:8090/ or
    /// http://127.0.0.1:8090/ and
    /// http://&lt;COMPUTERNAME&gt;:8090/
    /// </summary>
    /// <remarks>
    /// Inspiration from:<br />
    /// https://gist.github.com/SimonCropp/980071
    /// https://github.com/arshad115/HttpListenerServer
    /// https://github.com/sableangle/UnityHTTPServer
    /// </remarks>
    public interface ISimpleListenerServer
    {
        bool IsRunning { get; }
        void Start();
        void AddHandler(string pathPrefix, IListenerServerHandler handler);
        void Stop();
    }

    public class UnauthorizedException : Exception
    {
    }

    /// <summary>
    /// Request handler for <c>ISimpleListenerServer</c>.
    /// </summary>
    public interface IListenerServerHandler
    {
        /// <summary>
        /// Handles a request that is routed to it based on URI path prefix.
        /// </summary>
        /// <remarks>
        /// Returned <c>object</c> is converted to JSON string and returned <c>string</c> is assumed to be valid JSON already.
        /// </remarks>
        /// <param name="request">the request to handle</param>
        /// <param name="body">body content (as string)</param>
        /// <returns><c>object</c> or <c>string</c> on success and null if ignored it (did not handle).</returns>
        object HandleRequest(HttpListenerRequest request, string body);
    }

    public static class SimpleListenerServerFactory
    {
        public static ISimpleListenerServer Create(int port, Action<Tuple<string, string>> onResponse, 
            HttpListenerServer watchDog = null)
        {
            return new SimpleListenerServer(port, false, onResponse, watchDog);
        }

        public static ISimpleListenerServer CreateMultiThreaded(int port, Action<Tuple<string, string>> onResponse,
            HttpListenerServer watchDog = null)
        {
            return new SimpleListenerServer(port, true, onResponse, watchDog);
        }
    }

    internal class SimpleListenerServer : ISimpleListenerServer
    {
        private const string JsonContentType = "application/json";
        private const string FormPostContentType = "application/x-www-form-urlencoded";
        private const int BufferSize = 4 * 1024;

        private readonly int _port;
        private readonly HttpListener _listener;
        private readonly Dictionary<string, IListenerServerHandler> _handlers = new();
        private readonly Thread _serverThread;

        public bool IsRunning => _listener.IsListening;

        public SimpleListenerServer(int port, bool isMultiThreaded,
            Action<Tuple<string, string>> onResponse = null, HttpListenerServer watchDog = null)
        {
            _port = port;
            _listener = new HttpListener();
            var listener = isMultiThreaded
                ? new MultiThreadedListener(_port, _listener, _handlers, onResponse)
                : new SingleThreadedListener(_port, _listener, _handlers, onResponse);
            _serverThread = new Thread(listener.ListenThread);
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

        public void AddHandler(string pathPrefix, IListenerServerHandler handler)
        {
            Debug.Log($"{_port} {handler}");
            if (_handlers.ContainsKey(pathPrefix))
            {
                throw new InvalidOperationException($"duplicate pathPrefix '{pathPrefix}'");
            }
            _handlers.Add(pathPrefix, handler);
        }

        private class SingleThreadedListener
        {
            protected readonly int Port;
            protected readonly HttpListener Listener;
            private readonly Dictionary<string, IListenerServerHandler> _handlers;
            private readonly Action<Tuple<string, string>> _onResponse;

            public SingleThreadedListener(int port, HttpListener listener,
                Dictionary<string, IListenerServerHandler> handlers, Action<Tuple<string, string>> onResponse)
            {
                Port = port;
                Listener = listener;
                _handlers = handlers;
                _onResponse = onResponse;
            }

            public void ListenThread()
            {
                var uriPrefix = "http://*:" + Port + "/";
                Listener.Prefixes.Add(uriPrefix);
                try
                {
                    Run();
                }
                catch (ThreadAbortException)
                {
                    Debug.Log($"{Port} server stopped");
                }
                catch (Exception x)
                {
                    Debug.Log($"{Port} unhandled exception: {x.GetType().FullName} : {x.Message}");
                    Debug.LogException(x);
                }
                finally
                {
                    Listener.Stop();
                }
            }

            protected virtual void Run()
            {
                Debug.Log($"{Port} start server @ {string.Join(',', Listener.Prefixes)}");
                var buffer = new byte[BufferSize];
                Listener.Start();
                for (; Listener.IsListening;)
                {
                    var context = Listener.GetContext();
                    HandleContext(context, buffer);
                }
            }

            protected void HandleContext(HttpListenerContext context, byte[] buffer)
            {
                try
                {
                    var request = context.Request;
                    if (request.Url.AbsolutePath == "/favicon.ico")
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        return;
                    }
                    string body = null;
                    if (request.HasEntityBody)
                    {
                        var validContentType = request.ContentType is JsonContentType or FormPostContentType;
                        if (!validContentType)
                        {
                            throw new InvalidOperationException($"Invalid content type: {request.ContentType}");
                        }
                        var encoding = request.ContentEncoding;
                        var inputStream = request.InputStream;
                        var reader = new StreamReader(inputStream, encoding);
                        body = reader.ReadToEnd();
                        reader.Close();
                        inputStream.Close();
                    }
                    var response = context.Response;
                    var tokens = request.Url.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length < 2)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        return;
                    }
                    var pathPrefix = tokens[0];
                    if (!_handlers.TryGetValue(pathPrefix, out var handler))
                    {
                        throw new InvalidOperationException("No handlers found");
                    }
                    var responseObject = handler.HandleRequest(context.Request, body);
                    if (responseObject == null)
                    {
                        throw new InvalidOperationException($"Handler failed: {handler.GetType().Name}");
                    }
                    WriteResponse(response, buffer, responseObject);
                    response.OutputStream.Flush();
                    response.OutputStream.Close();
                }
                catch (Exception x)
                {
                    Debug.Log($"{Port} request handler failed: {x.GetType().FullName} : {x.Message}");
                    context.Response.StatusCode = x is UnauthorizedException
                        ? (int)HttpStatusCode.Unauthorized
                        : (int)HttpStatusCode.InternalServerError;
                    context.Response.StatusDescription = $"{x.GetType().FullName} : {x.Message}";
                }
                finally
                {
                    context.Response.Close();
                    _onResponse?.Invoke(new Tuple<string, string>(
                        $"{context.Request.HttpMethod} {context.Request.RawUrl}", 
                        $"{context.Response.StatusCode} {context.Response.StatusDescription}"));
                }
            }

            private static void WriteResponse(HttpListenerResponse response, byte[] buffer, object responseObject)
            {
                response.ContentType = "application/json";
                var jsonString = responseObject is string ? responseObject.ToString() : JsonUtilityWrapper(responseObject);
                var jsonByte = Encoding.UTF8.GetBytes(jsonString);
                response.ContentLength64 = jsonByte.Length;
                var memoryStream = new MemoryStream(jsonByte);
                for (;;)
                {
                    var byteCount = memoryStream.Read(buffer, 0, buffer.Length);
                    if (byteCount <= 0)
                    {
                        break;
                    }
                    response.OutputStream.Write(buffer, 0, byteCount);
                }
                memoryStream.Close();
            }

            private static string JsonUtilityWrapper(object instance)
            {
                // It seems that JsonUtility.ToJson does HTML URL Encoding :-(
                // - we want to revert that behaviour - at least for now as we don't know of anything better!
                var jsonString = JsonUtility.ToJson(instance);
                if (jsonString.IndexOf('%') < 0)
                {
                    return jsonString;
                }
                jsonString = Uri.UnescapeDataString(jsonString);
                Debug.Log($"=>{jsonString}");
                return jsonString;
            }
        }

        private class MultiThreadedListener : SingleThreadedListener
        {
            public MultiThreadedListener(int port, HttpListener listener,
                Dictionary<string, IListenerServerHandler> handlers, Action<Tuple<string, string>> onResponse)
                : base(port, listener, handlers, onResponse)
            {
            }

            protected override void Run()
            {
                Debug.Log($"{Port} start server @ {string.Join(',', Listener.Prefixes)}");
                Listener.Start();
                for (; Listener.IsListening;)
                {
                    var result = Listener.BeginGetContext(ListenerCallback, Listener);
                    result.AsyncWaitHandle.WaitOne();
                }
            }

            private void ListenerCallback(IAsyncResult result)
            {
                var context = Listener.EndGetContext(result);
                var buffer = new byte[BufferSize];
                HandleContext(context, buffer);
            }
        }
    }

    /// <summary>
    /// UNITY Editor helper.
    /// </summary>
    public class HttpListenerServer : MonoBehaviour
    {
        public int _port;

        public ISimpleListenerServer Server;

        private void OnEnable()
        {
            Debug.Log($"port {_port}");
            if (_port > 0 && Server == null)
            {
                Server = SimpleListenerServerFactory.Create(_port, null, this);
                Server.Start();
            }
        }
    }
}