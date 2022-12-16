using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using Object = System.Object;

namespace SimpleHTTPServer
{
    /// <summary>
    /// Middleware interface.
    /// </summary>
    public interface ISimpleHttpServerRequestHandler
    {
        /// <summary>
        /// Handles a request (or ignores it).
        /// </summary>
        /// <remarks>
        /// Returned <c>object</c> is converted to JSON string and returned <c>string</c> is assumed to be valid JSON already.
        /// </remarks>
        /// <param name="request">the request to handle</param>
        /// <returns><c>object</c> or <c>string</c> on success and null if ignored it (did not handle).</returns>
        object HandleRequest(HttpListenerRequest request);
    }

    public static class SimpleHttpServerX
    {
        public static void AddRequestHandler(ISimpleHttpServerRequestHandler requestHandler)
        {
            var unityHttpServer = UnityEngine.Object.FindObjectOfType<UnityHttpServer>();
            var simpleHttpServer = unityHttpServer.SimpleHttpServer;
            simpleHttpServer.AddRequestHandler(requestHandler);
        }
    }

    /// <summary>
    /// Adopted form https://github.com/sableangle/UnityHTTPServer
    /// </summary>
    internal class SimpleHttpServer
    {
        #region Error page

        private const string Default404Page = @"
<head>
<style>*{
    transition: all 0.6s;
}

html {
    height: 100%;
}

body{
    font-family: 'Lato', sans-serif;
    color: #888;
    margin: 0;
}

#main{
    display: table;
    width: 100%;
    height: 100vh;
    text-align: center;
}

.fof{
	  display: table-cell;
	  vertical-align: middle;
}

.fof h1{
	  font-size: 50px;
	  display: inline-block;
	  padding-right: 12px;
	  animation: type .5s alternate infinite;
}

@keyframes type{
	  from{box-shadow: inset -3px 0px 0px #888;}
	  to{box-shadow: inset -3px 0px 0px transparent;}
}</style>
</head>
<body>
    <div id='main'>
    <div class='fof'>
        <h1>Error 404</h1>
    </div>
    </div>
</body>
";

        #endregion

        private static readonly IDictionary<string, string> MimeTypeMappings =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
                #region extension to MIME type list

                { ".asf", "video/x-ms-asf" },
                { ".asx", "video/x-ms-asf" },
                { ".avi", "video/x-msvideo" },
                { ".bin", "application/octet-stream" },
                { ".cco", "application/x-cocoa" },
                { ".crt", "application/x-x509-ca-cert" },
                { ".css", "text/css" },
                { ".deb", "application/octet-stream" },
                { ".der", "application/x-x509-ca-cert" },
                { ".dll", "application/octet-stream" },
                { ".dmg", "application/octet-stream" },
                { ".ear", "application/java-archive" },
                { ".eot", "application/octet-stream" },
                { ".exe", "application/octet-stream" },
                { ".flv", "video/x-flv" },
                { ".gif", "image/gif" },
                { ".hqx", "application/mac-binhex40" },
                { ".htc", "text/x-component" },
                { ".htm", "text/html" },
                { ".html", "text/html" },
                { ".ico", "image/x-icon" },
                { ".img", "application/octet-stream" },
                { ".svg", "image/svg+xml" },
                { ".iso", "application/octet-stream" },
                { ".jar", "application/java-archive" },
                { ".jardiff", "application/x-java-archive-diff" },
                { ".jng", "image/x-jng" },
                { ".jnlp", "application/x-java-jnlp-file" },
                { ".jpeg", "image/jpeg" },
                { ".jpg", "image/jpeg" },
                { ".js", "application/x-javascript" },
                { ".mml", "text/mathml" },
                { ".mng", "video/x-mng" },
                { ".mov", "video/quicktime" },
                { ".mp3", "audio/mpeg" },
                { ".mpeg", "video/mpeg" },
                { ".mp4", "video/mp4" },
                { ".mpg", "video/mpeg" },
                { ".msi", "application/octet-stream" },
                { ".msm", "application/octet-stream" },
                { ".msp", "application/octet-stream" },
                { ".pdb", "application/x-pilot" },
                { ".pdf", "application/pdf" },
                { ".pem", "application/x-x509-ca-cert" },
                { ".pl", "application/x-perl" },
                { ".pm", "application/x-perl" },
                { ".png", "image/png" },
                { ".prc", "application/x-pilot" },
                { ".ra", "audio/x-realaudio" },
                { ".rar", "application/x-rar-compressed" },
                { ".rpm", "application/x-redhat-package-manager" },
                { ".rss", "text/xml" },
                { ".run", "application/x-makeself" },
                { ".sea", "application/x-sea" },
                { ".shtml", "text/html" },
                { ".sit", "application/x-stuffit" },
                { ".swf", "application/x-shockwave-flash" },
                { ".tcl", "application/x-tcl" },
                { ".tk", "application/x-tcl" },
                { ".txt", "text/plain" },
                { ".war", "application/java-archive" },
                { ".wbmp", "image/vnd.wap.wbmp" },
                { ".wmv", "video/x-ms-wmv" },
                { ".xml", "text/xml" },
                { ".xpi", "application/x-xpinstall" },
                { ".zip", "application/zip" },

                #endregion
            };

        private const int BufferSize = 16;

        private readonly string[] _indexFiles =
        {
            "index.html",
            "index.htm",
            "default.html",
            "default.htm"
        };

        private readonly string _rootDirectory;
        private readonly int _port;
        private readonly Object _methodController;
        private readonly byte[] _buffer = new byte[1024 * BufferSize];
        private readonly Thread _serverThread;

        private HttpListener _listener;

        private ISimpleHttpServerRequestHandler _requestHandler;

        /// <summary>
        /// Construct server with given port, path, controller.
        /// </summary>
        /// <param name="path">The root folder path in your computer (Absolute path)</param>
        /// <param name="port">The port for your http server</param>
        /// <param name="controller">The controller instance for the WebAPI</param>
        /// <param name="onJsonSerialized">The port for your http server</param>
        public SimpleHttpServer(string path, int port, Object controller)
        {
            _rootDirectory = path;
            _port = port;
            _methodController = controller;
            _serverThread = new Thread(ListenThread);
            Debug.Log($"start listening {_port}");
            _serverThread.Start();
        }

        /// <summary>
        /// Stop Server
        /// </summary>
        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }

        public void AddRequestHandler(ISimpleHttpServerRequestHandler requestHandler)
        {
            _requestHandler = requestHandler;
            Debug.Log($"add request handler {_requestHandler}");
        }

        private void ListenThread()
        {
            _listener = new HttpListener();
            var uriPrefix = "http://*:" + _port + "/";
            _listener.Prefixes.Add("http://*:" + _port + "/");
            _listener.Start();
            UnityEngine.Debug.Log($"server started @ {uriPrefix}");
            for (;;)
            {
                try
                {
                    var context = _listener.GetContext();
                    Process(context);
                }
                catch (ThreadAbortException)
                {
                    UnityEngine.Debug.Log($"server stopped @ {uriPrefix}");
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.Log(ex);
                }
            }
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

        private void Process(HttpListenerContext context)
        {
            if (_requestHandler != null)
            {
                try
                {
                    var result = _requestHandler.HandleRequest(context.Request);
                    if (result != null)
                    {
                        context.Response.ContentType = "application/json";
                        var jsonString = result is string ? result.ToString() : JsonUtilityWrapper(result);
                        var jsonByte = Encoding.UTF8.GetBytes(jsonString);
                        context.Response.ContentLength64 = jsonByte.Length;
                        Stream jsonStream = new MemoryStream(jsonByte);
                        int byteCount;
                        while ((byteCount = jsonStream.Read(_buffer, 0, _buffer.Length)) > 0)
                            context.Response.OutputStream.Write(_buffer, 0, byteCount);
                        jsonStream.Close();
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log($"request handler failed: {e.Message}");
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.StatusDescription = e.Message;
                }
                goto WebResponseDone;
            }
            UnityEngine.Debug.Log($"REQUEST {context.Request.Url.AbsolutePath} {context.Request.Url.Query}");
            var filename = context.Request.Url.AbsolutePath.Substring(1);
            if (string.IsNullOrEmpty(filename))
            {
                foreach (var indexFile in _indexFiles)
                {
                    if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
                    {
                        filename = indexFile;
                        break;
                    }
                }
            }
            filename = Path.Combine(_rootDirectory, filename);

            var namedParameters = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(context.Request.Url.Query))
            {
                var query = context.Request.Url.Query.Replace("?", "").Split('&');
                foreach (var item in query)
                {
                    var t = item.Split('=');

                    namedParameters.Add(t[0], t[1]);
                }
            }

            var method = TryParseToController(context.Request.Url);

            if (File.Exists(filename))
            {
                TryServeFile();
            }
            //A ASP.Net MVC like controller route
            else if (method != null)
            {
                context.Response.ContentType = "application/json";
                object result;
                try
                {
                    result = method.InvokeWithNamedParameters(_methodController, namedParameters);
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    UnityEngine.Debug.LogError(ex);
                    context.Response.StatusDescription = ex.Message;
                    goto WebResponseDone;
                }
                if (result == null)
                {
                    result = new VoidResult { msg = "Success" };
                }
                var jsonString = JsonUtilityWrapper(result);
                var jsonByte = Encoding.UTF8.GetBytes(jsonString);
                context.Response.ContentLength64 = jsonByte.Length;
                Stream jsonStream = new MemoryStream(jsonByte);
                int byteCount;
                while ((byteCount = jsonStream.Read(_buffer, 0, _buffer.Length)) > 0)
                    context.Response.OutputStream.Write(_buffer, 0, byteCount);
                jsonStream.Close();
            }
            else
            {
                var resultByte = Encoding.UTF8.GetBytes(Default404Page);
                Stream resultStream = new MemoryStream(resultByte);
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.ContentType = "text/html";
                context.Response.ContentLength64 = resultByte.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", File.GetLastWriteTime(filename).ToString("r"));

                var buffer = new byte[1024 * BufferSize];
                int nbytes;
                while ((nbytes = resultStream.Read(buffer, 0, buffer.Length)) > 0)
                    context.Response.OutputStream.Write(buffer, 0, nbytes);
                resultStream.Close();
            }
            WebResponseDone:
            context.Response.OutputStream.Flush();
            context.Response.OutputStream.Close();

            void TryServeFile()
            {
                try
                {
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    Stream input = new FileStream(filename, FileMode.Open, FileAccess.Read);

                    //Adding permanent http response headers
                    string mime;
                    context.Response.ContentType =
                        MimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";
                    context.Response.ContentLength64 = input.Length;
                    context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                    context.Response.AddHeader("Last-Modified", File.GetLastWriteTime(filename).ToString("r"));

                    var buffer = new byte[1024 * BufferSize];
                    int nbytes;
                    while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                        context.Response.OutputStream.Write(buffer, 0, nbytes);
                    input.Close();
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    UnityEngine.Debug.LogError(ex);
                    context.Response.StatusDescription = ex.Message;
                }
            }
        }

        private MethodInfo TryParseToController(Uri uri)
        {
            if (uri.Segments.Length <= 1)
            {
                return null;
            }
            var methodName = uri.Segments[1].Replace("/", "");
            MethodInfo method;
            try
            {
                method = _methodController.GetType().GetMethod(methodName);
            }
            catch
            {
                method = null;
            }
            return method;
        }

        [Serializable]
        private class VoidResult
        {
            public string msg;
        }
    }

    internal static class ReflectionExtensions
    {
        public static object InvokeWithNamedParameters(this MethodBase self, object obj, IDictionary<string, object> namedParameters)
        {
            return self.Invoke(obj, MapParameters(self, namedParameters));
        }

        private static object[] MapParameters(MethodBase method, IDictionary<string, object> namedParameters)
        {
            var paramInfos = method.GetParameters().ToArray();
            var parameters = new object[paramInfos.Length];
            var index = 0;
            foreach (var item in paramInfos)
            {
                object parameterName;
                if (!namedParameters.TryGetValue(item.Name, out parameterName))
                {
                    parameters[index] = Type.Missing;
                    index++;
                    continue;
                }
                parameters[index] = ObjectCastTypeByParameterInfo(item, parameterName);
                index++;
            }
            return parameters;
        }

        private static object ObjectCastTypeByParameterInfo(ParameterInfo parameterInfo, object value)
        {
            if (parameterInfo.ParameterType == typeof(int) ||
                parameterInfo.ParameterType == typeof(Int32) ||
                parameterInfo.ParameterType == typeof(Int16) ||
                parameterInfo.ParameterType == typeof(Int64))
            {
                return (int)Convert.ChangeType(value, typeof(int));
            }
            return value;
        }
    }
}