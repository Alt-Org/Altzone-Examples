using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Model;
using SimpleHTTPServer;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Simple REST API to test 'server side' operations with <c>LootLocker</c>.
/// </summary>
public class RestApiServer : MonoBehaviour, ISimpleHttpServerRequestHandler
{
    [Serializable]
    public class ErrorResult
    {
        public bool success;
        public string message;

        public ErrorResult(string message)
        {
            success = false;
            this.message = message;
        }
    }

    [Serializable]
    public class SuccessResult
    {
        public bool success;
        public int id;
        public string message;

        public SuccessResult(int id, string message)
        {
            success = true;
            this.id = id;
            this.message = message;
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

        if (!parameters.TryGetValue("id", out var clanStr) || !int.TryParse(clanStr, out var clanId))
        {
            throw new InvalidOperationException("clan id is missing or invalid");
        }
        switch (verb)
        {
            case "get":
                return GetClan();
            case "create":
                return CreateClan();
            default:
                return CanNotHandle;
        }

        object GetClan()
        {
            // http://localhost:8090/server/clan/get?id=1
            
            var clan = _storage.GetClan(clanId);
            if (clan == null)
            {
                return new ErrorResult("clan not found");
            }
            return new SuccessResult(clan.Id, clan.Name);
        }

        object CreateClan()
        {
            // http://localhost:8090/server/clan/create?id=1&name=ABBA
            
            if (!parameters.TryGetValue("name", out var clanName) || string.IsNullOrWhiteSpace(clanName))
            {
                throw new InvalidOperationException("clan name is missing or invalid");
            }
            var clan = _storage.GetClan(clanId);
            if (clan != null)
            {
                return new ErrorResult("clan exists already");
            }
            clan = new ClanModel
            {
                Name = clanName,
            };
            if (_storage.CreateClan(clan) != 1)
            {
                throw new InvalidOperationException($"unable to insert clan '{clan.Name}'");
            }
            return $"Clan '{clan.Id}' '{clan.Name}' created";
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