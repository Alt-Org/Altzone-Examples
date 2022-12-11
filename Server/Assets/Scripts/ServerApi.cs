using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using SimpleHTTPServer;
using UnityEngine;

/// <summary>
/// Simple REST API to test 'server side' operations with <c>LootLocker</c>.
/// </summary>
public class ServerApi : MonoBehaviour, ISimpleHttpServerRequestHandler
{
    private const string ServerPrefix = "server";

    private static readonly Tuple<bool, string> CanNotHandle = new(false, "can not handle");

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => GameObject.Find("UnityHttpServer") != null);
        SimpleHttpServerX.AddRequestHandler(this);
        Debug.Log($"AddRequestHandler {name} : {GetType().FullName}");
    }

    /// <summary>
    /// Parse and handle <c>SimpleHTTPServer</c> request.
    /// </summary>
    /// <remarks>
    /// Return <c>true</c> on success, <c>false</c> if can not handle and <c>Exception</c> if request handling fails.
    /// </remarks>
    public Tuple<bool, string> HandleRequest(HttpListenerRequest request)
    {
        var path = request.Url.AbsolutePath;
        var tokens = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 1 || tokens[0] != ServerPrefix)
        {
            return CanNotHandle;
        }
        var query = request.Url.Query;
        UnityEngine.Debug.Log($"request {path} {query}");
        var parameters = ParseParameters(query);
        var verb = tokens[1];
        switch (verb)
        {
            case "move":
                return HandleMove(parameters);
            default:
                return CanNotHandle;
        }
    }

    /// <summary>
    /// Moves one item from player1 to player2.
    /// </summary>
    private Tuple<bool, string> HandleMove(Dictionary<string, string> parameters)
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
        return new Tuple<bool, string>(true, text);
    }

    private static Dictionary<string, string> ParseParameters(string queryString)
    {
        var namedParameters = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(queryString))
        {
            return namedParameters;
        }
        var query = queryString.Replace("?", "").Split('&');
        foreach (var item in query)
        {
            var tokens = item.Split('=');
            if (tokens.Length != 2)
            {
                throw new InvalidOperationException("invalid query string");
            }
            namedParameters.Add(tokens[0], tokens[1]);
        }
        return namedParameters;
    }
}