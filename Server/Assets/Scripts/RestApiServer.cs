using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Model;
using SimpleHTTPServer;
using SQLite;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Simple REST API to test 'server side' operations with <c>LootLocker</c>.
/// </summary>
public class RestApiServer : MonoBehaviour, ISimpleHttpServerRequestHandler
{
    [Serializable]
    public class Result
    {
        public bool success;
        public string message;

        public Result(bool success, string message)
        {
            this.success = success;
            this.message = message;
        }
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

    private const string ServerPathPrefix = "server";

    private static readonly ErrorResult CanNotHandle = new("can not handle");

    private IStorageService _storage;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => GameObject.Find("UnityHttpServer") != null);
        SimpleHttpServerX.AddRequestHandler(this);
        Debug.Log($"AddRequestHandler {name} : {GetType().FullName}");
        yield return new WaitUntil(() => (_storage ??= FindObjectOfType<StorageService>()) != null);
        Debug.Log("storage found");
    }

    /// <summary>
    /// Parse and handle <c>SimpleHTTPServer</c> request.
    /// </summary>
    /// <remarks>
    /// Return <c>true</c> on success, <c>false</c> if can not handle and <c>Exception</c> if request handling fails.
    /// </remarks>
    public object HandleRequest(HttpListenerRequest request)
    {
        var path = request.Url.AbsolutePath;
        var tokens = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 1 || tokens[0] != ServerPathPrefix)
        {
            return null;
        }
        var query = request.Url.Query;
        UnityEngine.Debug.Log($"request {path} {query}");
        var parameters = ParseParameters(query);
        var verb = tokens[1];
        switch (verb)
        {
            case "move":
                return HandleMove(parameters);
            case "clan":
                return tokens.Length != 3 ? CanNotHandle : HandleClan(tokens[2], parameters);
            default:
                return CanNotHandle;
        }
    }

    /// <summary>
    /// Moves one item from player1 to player2.
    /// </summary>
    private object HandleMove(Dictionary<string, string> parameters)
    {
        // http://localhost:8090/server/move?player1=123&player2=456&item=789

        if (!parameters.TryGetValue("player1", out var player1))
        {
            throw new InvalidOperationException("player1 id not found");
        }
        if (!parameters.TryGetValue("player2", out var player2))
        {
            throw new InvalidOperationException("player2 id not found");
        }
        if (!parameters.TryGetValue("item", out var item))
        {
            throw new InvalidOperationException("item id not found");
        }
        var text =
            "{\"players\":[{\"player_id\":@player1@,\"player_public_uid\":\"GJDE7SAD\",\"name\":\"Jaskan\",\"last_active_platform\":\"google_play_store\"}," +
            "{\"player_id\":@player2@,\"player_public_uid\":\"F7D3RPYR\",\"name\":\"Jaskan\",\"last_active_platform\":\"guest\"}]}";
        text = text.Replace("@player1@", player1);
        text = text.Replace("@player2@", player2);
        return text;
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

    private static Dictionary<string, string> ParseParameters(string queryString)
    {
        var namedParameters = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(queryString))
        {
            return namedParameters;
        }
        var query = queryString.Replace("?", "").Split('&');
        Debug.Log($"query {string.Join('|', query)}");
        foreach (var item in query)
        {
            var tokens = item.Split('=');
            if (tokens.Length != 2)
            {
                throw new InvalidOperationException("invalid query string");
            }
            Debug.Log($"{tokens[0]}={tokens[1]}");
            namedParameters.Add(tokens[0], tokens[1]);
        }
        return namedParameters;
    }
}