using System;
using System.Collections;
using System.Net;
using SimpleHTTPServer;
using UnityEngine;

public class ServerApi : MonoBehaviour, ISimpleHttpServerRequestHandler
{
    private const string ServerPrefix = "server";

    private static readonly Tuple<int, string> NotFound = new(-1, "not found");

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => GameObject.Find("UnityHttpServer") != null);
        SimpleHttpServerX.AddRequestHandler(this);
        Debug.Log($"AddRequestHandler {name} : {GetType().FullName}");
    }

    public Tuple<int, string> HandleRequest(HttpListenerRequest request)
    {
        var path = request.Url.AbsolutePath;
        var tokens = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 1 || tokens[0] != ServerPrefix)
        {
            return NotFound;
        }
        UnityEngine.Debug.Log($"request {path} {request.Url.Query}");
        const int code = 400;
        const string text =
            "{\"players\":[{\"player_id\":3022592,\"player_public_uid\":\"GJDE7SAD\",\"name\":\"Jaskan\",\"last_active_platform\":\"google_play_store\"},{\"player_id\":3027563,\"player_public_uid\":\"F7D3RPYR\",\"name\":\"Jaskan\",\"last_active_platform\":\"guest\"}]}";
        return new Tuple<int, string>(code, text);
    }
}