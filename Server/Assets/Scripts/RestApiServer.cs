using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Model;
using Prg.Scripts.Common.HttpListenerServer;
using Prg.Scripts.Common.MiniJson;
using SQLite;
using UnityEngine;

/// <summary>
/// Simple REST API to test 'server side' operations with <c>LootLocker</c>.
/// </summary>
public class RestApiServer : MonoBehaviour, IListenerServerHandler
{
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

    private const int ServerPort = 8090;
    private const string ServerPathPrefix = "server";
    private const string TimestampName = "timestamp";

    private static readonly ErrorResult CanNotHandle = new("can not handle");

    private IStorageService _storage;

    private IEnumerator Start()
    {
        var server = SimpleListenerServerFactory.Create(ServerPort);
        server.Start();
        server.AddHandler(this);
        yield return new WaitUntil(() => server.IsRunning);
        Debug.Log("http server running");

        yield return new WaitUntil(() => (_storage ??= FindObjectOfType<StorageService>()) != null);
        Debug.Log("storage running");
    }

    /// <summary>
    /// Parse and handle <c>SimpleHTTPServer</c> request.
    /// </summary>
    /// <remarks>
    /// Return <c>true</c> on success, <c>false</c> if can not handle and <c>Exception</c> if request handling fails.
    /// </remarks>
    public object HandleRequest(HttpListenerRequest request, string body)
    {
        var path = request.Url.AbsolutePath;
        var tokens = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 1 || tokens[0] != ServerPathPrefix)
        {
            return null;
        }
        var query = request.Url.Query;
        Debug.Log($"{DateTime.Now:fff} request {path} {query}");
        var parameters = ParseParameters(query);
        var verb = tokens[1];
        switch (verb)
        {
            case "move":
                return HandleMove(parameters);
            case "clan":
                return tokens.Length != 3 ? CanNotHandle : HandleClan(tokens[2], parameters);
            case "ping":
                return HandlePing(tokens);
            default:
                return CanNotHandle;
        }
    }

    private static object HandlePing(string[] tokens)
    {
        if (tokens.Length == 2)
        {
            return new SuccessResult("ping");
        }
        if (tokens.Length == 3)
        {
            return new SuccessResult($"ping/{tokens[2]}");
        }
        return CanNotHandle;
    }
    
    /// <summary>
    /// Dummy example: moves one item from player1 to player2.
    /// </summary>
    private static object HandleMove(Dictionary<string, string> parameters)
    {
        // http://localhost:8090/server/move?player1=123&player2=456&item=789

        if (!parameters.TryGetValue("player1", out var playerId1))
        {
            throw new InvalidOperationException("player1 id not found");
        }
        if (!parameters.TryGetValue("player2", out var playerId2))
        {
            throw new InvalidOperationException("player2 id not found");
        }
        if (!parameters.TryGetValue("item", out var itemId))
        {
            throw new InvalidOperationException("item id not found");
        }
        // Just something: list of players involved in operation.
        var player1 = new Dictionary<string, object>
        {
            { "player_id", playerId1 },
            {
                "items_removed", new List<string>
                {
                    itemId,
                }
            },
            { "player_public_uid", "GJDE7SAD" },
            { "name", "Player1" },
            { "last_active_platform", "google_play_store" },
        };
        var player2 = new Dictionary<string, object>
        {
            { "player_id", playerId2 },
            {
                "items_added", new List<string>
                {
                    itemId,
                }
            },
            { "player_public_uid", "F7D3RPYR" },
            { "name", "Player2" },
            { "last_active_platform", "guest" },
        };
        var playerList = new List<Dictionary<string, object>>
        {
            player1, player2
        };
        var players = new Dictionary<string, object>
        {
            { "players", playerList },
            { TimestampName, Result.Timestamp() },
        };
        var json = MiniJson.Serialize(players);
        return json;
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