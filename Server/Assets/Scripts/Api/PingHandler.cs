using System;
using System.Net;
using Prg.Scripts.Common.Http.HttpListenerServer;

namespace Api
{
    public class PingHandler : IListenerServerHandler
    {
        private static readonly RestApiServer.ErrorResult CanNotHandle = new("can not handle");

        public object HandleRequest(HttpListenerRequest request, string body)
        {
            var path = request.Url.AbsolutePath;
            var tokens = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2)
            {
                throw new InvalidOperationException($"invalid path '{path}'");
            }
            var query = request.Url.Query;
            Debug.Log($"{DateTime.Now:fff} request {path} {query}");
            var parameters = RestApiServer.ParseParameters(query);
            var verb = tokens[1];
            switch (verb)
            {
                case "ping":
                    return HandlePing(tokens);
                default:
                    return CanNotHandle;
            }
        }

        private static object HandlePing(string[] tokens)
        {
            // http://localhost:8090/test/ping
            
            if (tokens.Length < 2)
            {
                return CanNotHandle;
            }
            if (tokens.Length == 2)
            {
                return new RestApiServer.SuccessResult("ping");
            }
            return new RestApiServer.SuccessResult(string.Join('/', tokens));
        }
    }
}