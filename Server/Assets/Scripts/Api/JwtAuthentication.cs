using System;
using System.Collections.Generic;
using System.Net;
using Prg.Scripts.Common.Http.HttpListenerServer;

namespace Api
{
    public interface IJwtAuthentication
    {
        bool ValidateToken(string jwtToken);
    }

    public class JwtAuthentication : IListenerServerHandler, IJwtAuthentication
    {
        private static readonly RestApiServer.ErrorResult CanNotHandle = new("can not handle");
        private const string AuthHeaderPrefix = "Bearer ";

        private readonly string _authenticationKey;

        public JwtAuthentication(string authenticationKey)
        {
            _authenticationKey = authenticationKey;
        }

        object IListenerServerHandler.HandleRequest(HttpListenerRequest request, string body)
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
                case "authenticate":
                    return HandleAppId(parameters, body);
                default:
                    return CanNotHandle;
            }
        }

        bool IJwtAuthentication.ValidateToken(string jwtToken)
        {
            if (jwtToken == null || !jwtToken.StartsWith(AuthHeaderPrefix))
            {
                return false;
            }
            var key = jwtToken.Substring(AuthHeaderPrefix.Length);
            return key == _authenticationKey;
        }

        private object HandleAppId(Dictionary<string, string> parameters, string body)
        {
            // http://localhost:8090/login/authenticate?appid=123456
            
            if (parameters.TryGetValue("appid", out var appId) && appId == "123456")
            {
                return new SuccessResult(_authenticationKey);
            }
            throw new InvalidOperationException($"invalid appid");
        }
        
        public class SuccessResult : RestApiServer.Result
        {
            public string AuthenticationKey;
            
            public SuccessResult(string authenticationKey) : base(true, "appId is valid")
            {
                AuthenticationKey = authenticationKey;
            }
        }
        
    }
}