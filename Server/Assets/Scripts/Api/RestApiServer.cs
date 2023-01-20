using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Model;
using Prg.Scripts.Common.Http.HttpListenerServer;
using Service;
using SQLite;
using UnityEngine;
using UnityEngine.Assertions;

namespace Api
{
    /// <summary>
    /// Simple REST API to test 'server side' operations with <c>LootLocker</c>.
    /// </summary>
    public class RestApiServer : MonoBehaviour, IListenerServerHandler
    {
        [Header("Authentication"), SerializeField] private bool _isRequireAuthentication;
        [SerializeField] private string _authenticationKey;

        [Header("Ping Test"), SerializeField] private bool _isIncludePingTest;
        
        [Header("Debug Data"), SerializeField] private int _messageCount;
        [SerializeField] private string _request;
        [SerializeField] private string _response;
        [SerializeField] private string _responseTime;

        #region JSON Data Transfer Objects

        [Serializable]
        public class Result
        {
            public bool success;
            public string message;
            public string timestamp;

            public Result(bool success, string message)
            {
                this.success = success;
                this.message = message;
                timestamp = Timestamp();
            }

            public static string Timestamp() => $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}";
        }

        public class ErrorResult : Result
        {
            public ErrorResult(string message) : base(false, message)
            {
            }
        }

        public class SuccessResult : Result
        {
            public SuccessResult(string message) : base(true, message)
            {
            }
        }

        public class DatabaseError : Result
        {
            public DatabaseError(SQLiteException exception) : base(false, $"Database error: {exception.Message}")
            {
            }
        }

        public class ClanResult : Result
        {
            public JsonClanModel clan;

            public ClanResult(ClanModel clan, string message) : base(true, message)
            {
                this.clan = new JsonClanModel(clan);
            }
        }

        public class ClanListResult : Result
        {
            public List<JsonClanModel> clans;

            public ClanListResult(List<ClanModel> clans, string message) : base(true, message)
            {
                this.clans = JsonClanModel.ConvertAll(clans);
            }
        }

        #endregion

        private const int ServerPort = 8090;
        private const string AuthenticationPathPrefix = "login";
        private const string PingPathPrefix = "test";
        private const string ServerPathPrefix = "server";
        private const string TimestampName = "timestamp";

        private static readonly ErrorResult CanNotHandle = new("can not handle");

        private IJwtAuthentication _authentication;
        private IStorageService _storage;

        private readonly ConcurrentQueue<Tuple<string, string>> _messageQueue = new();

        private IEnumerator Start()
        {
            var server = SimpleListenerServerFactory.Create(ServerPort);
            server.Start();
            if (_isRequireAuthentication)
            {
                Assert.IsTrue(_authenticationKey?.Length > 0);
                var authentication = new JwtAuthentication(_authenticationKey);
                _authentication = authentication;
                server.AddHandler(AuthenticationPathPrefix, authentication);
            }
            if (_isIncludePingTest)
            {
                server.AddHandler(PingPathPrefix, new PingHandler());
            }
            server.AddHandler(ServerPathPrefix, this);
            yield return new WaitUntil(() => server.IsRunning);
            Debug.Log("http server running");

            yield return new WaitUntil(() => (_storage ??= FindObjectOfType<StorageService>()) != null);
            Debug.Log("storage running");
        }

        private void Update()
        {
            while (_messageQueue.Count > 0)
            {
                if (!_messageQueue.TryDequeue(out var tuple))
                {
                    return;
                }
                _request = tuple.Item1;
                _response = tuple.Item2;
                _responseTime = $"{DateTime.Now:HH:mm:ss.fff}";
            }
        }

        /// <summary>
        /// Parse and handle <c>SimpleHTTPServer</c> request.
        /// </summary>
        /// <remarks>
        /// Return <c>true</c> on success, <c>false</c> if can not handle and <c>Exception</c> if request handling fails.
        /// </remarks>
        public object HandleRequest(HttpListenerRequest request, string body)
        {
            _messageCount += 1;
            try
            {
                var response = _HandleRequest(request, body);
                _messageQueue.Enqueue(new Tuple<string, string>(
                    $"{request.Url.AbsolutePath}{request.Url.Query}",
                    "OK"));
                return response;
            }
            catch (Exception x)
            {
                _messageQueue.Enqueue(new Tuple<string, string>(
                    $"{request.Url.AbsolutePath}{request.Url.Query}",
                    x.Message));
                throw;
            }
        }

        private object _HandleRequest(HttpListenerRequest request, string body)
        {
            if (_isRequireAuthentication)
            {
                var authorization = request.Headers.Get("Authorization");
                if (!_authentication.ValidateToken(authorization))
                {
                    throw new UnauthorizedException();
                }
            }
            var path = request.Url.AbsolutePath;
            var tokens = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2)
            {
                throw new InvalidOperationException($"invalid path '{path}'");
            }
            var query = request.Url.Query;
            Debug.Log($"{DateTime.Now:fff} request {path} {query}");
            var parameters = ParseParameters(query);
            var verb = tokens[1];
            switch (verb)
            {
                case "clan":
                    return tokens.Length != 3 ? CanNotHandle : HandleClan(tokens[2], parameters);
                default:
                    return CanNotHandle;
            }
        }

        private object HandleClan(string verb, Dictionary<string, string> parameters)
        {
            // http://localhost:8090/server/clan/...

            switch (verb)
            {
                case "get":
                    return GetClan();
                case "list":
                    return GetClans();
                case "create":
                    return CreateClan();
                case "update":
                    return UpdateClan();
                case "delete":
                    return DeleteClan();
                default:
                    return CanNotHandle;
            }

            void VerifyParametersCount(int maxParameterCount)
            {
                if (parameters.Count() > maxParameterCount)
                {
                    throw new InvalidOperationException("invalid parameters");
                }
            }

            int GetClanId()
            {
                if (!parameters.TryGetValue("id", out var clanStr) || !int.TryParse(clanStr, out var clanId))
                {
                    throw new InvalidOperationException("clan id is missing or invalid");
                }
                return clanId;
            }

            object GetClan()
            {
                // http://localhost:8090/server/clan/get?id=1

                VerifyParametersCount(1);
                var clanId = GetClanId();
                var clan = _storage.GetClan(clanId);
                if (clan == null)
                {
                    return new ErrorResult("clan not found");
                }
                return new ClanResult(clan, "found");
            }

            object GetClans()
            {
                // http://localhost:8090/server/clan/list

                VerifyParametersCount(0);
                var clans = _storage.GetClans();
                return new ClanListResult(clans, "list all");
            }

            object CreateClan()
            {
                // http://localhost:8090/server/clan/create?name=ABBA

                VerifyParametersCount(1);
                if (!parameters.TryGetValue("name", out var clanName) || string.IsNullOrWhiteSpace(clanName))
                {
                    throw new InvalidOperationException("clan name is missing or invalid");
                }
                // Unfortunately SQLite unique indexes are not case insensitive (unless you use ImplicitIndex 'hack')
                var clans = _storage.GetClans();
                if (clans.Any(x => string.Compare(x.Name, clanName, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    return new ErrorResult($"clan name is in use '{clanName}'");
                }
                var clan = new ClanModel
                {
                    Name = clanName,
                };
                try
                {
                    if (_storage.CreateClan(clan) != 1)
                    {
                        return new ErrorResult($"unable to insert clan '{clan.Name}'");
                    }
                }
                catch (SQLiteException x)
                {
                    return new DatabaseError(x);
                }
                return new ClanResult(clan, "created");
            }

            object UpdateClan()
            {
                // http://localhost:8090/server/clan/update?id=1&name=ABBA

                VerifyParametersCount(2);
                var clanId = GetClanId();
                if (!parameters.TryGetValue("name", out var clanName) || string.IsNullOrWhiteSpace(clanName))
                {
                    throw new InvalidOperationException("clan name is missing or invalid");
                }
                var clan = _storage.GetClan(clanId);
                if (clan == null)
                {
                    return new ErrorResult("clan not found");
                }
                clan.Name = clanName;
                try
                {
                    if (_storage.UpdateClan(clan) != 1)
                    {
                        return new ErrorResult($"unable to update clan '{clanId}'");
                    }
                }
                catch (SQLiteException x)
                {
                    return new DatabaseError(x);
                }

                return new ClanResult(clan, "updated");
            }

            object DeleteClan()
            {
                // http://localhost:8090/server/clan/delete?id=1

                VerifyParametersCount(1);
                var clanId = GetClanId();
                try
                {
                    if (!_storage.DeleteClan(clanId))
                    {
                        return new ErrorResult($"unable to delete clan '{clanId}'");
                    }
                }
                catch (SQLiteException x)
                {
                    return new DatabaseError(x);
                }
                return new SuccessResult("deleted");
            }
        }

        public static Dictionary<string, string> ParseParameters(string queryString)
        {
            var namedParameters = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(queryString))
            {
                return namedParameters;
            }
            var query = queryString.Replace("?", "").Split('&', StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in query)
            {
                var tokens = item.Split('=', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length != 2)
                {
                    throw new InvalidOperationException("invalid query string (key=value)");
                }
                if (namedParameters.ContainsKey(tokens[0]))
                {
                    throw new InvalidOperationException("duplicate query string key");
                }
                namedParameters.Add(tokens[0], tokens[1]);
            }
            return namedParameters;
        }
    }
}