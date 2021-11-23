using System.Collections.Generic;
using System.Threading.Tasks;
using BrainCloud;
using BrainCloud.JsonFx.Json;
using UnityEngine.Assertions;

namespace Prg.Scripts.Service.BrainCloud
{
    /// <summary>
    /// Helper class to store some important <c>BrainCloud</c> user data for convenience.
    /// </summary>
    public class BrainCloudUser
    {
        public readonly string UserId;
        public readonly string UserName;
        public readonly string ProfileId;
        public readonly int StatusCode;

        public bool IsValid => StatusCode == 0;

        public BrainCloudUser(string userId, string userName, string profileId, int statusCode)
        {
            UserId = userId;
            UserName = userName;
            ProfileId = profileId;
            StatusCode = statusCode;
        }

        public override string ToString()
        {
            return $"UserName: {UserName}, ProfileId: {ProfileId}, UserId: {UserId}, StatusCode: {StatusCode}, IsValid: {IsValid}";
        }
    }

    /// <summary>
    /// Async wrapper to callback based <c>BrainCloud</c> SDK API.
    /// </summary>
    /// <remarks>
    /// We use <c>TaskCompletionSource</c> to bind caller and callee "together".
    /// </remarks>
    public static class BrainCloudAsync
    {
        private static BrainCloudWrapper _brainCloudWrapper;

        public static void SetBrainCloudWrapper(BrainCloudWrapper brainCloudWrapper)
        {
            _brainCloudWrapper = brainCloudWrapper;
        }

        /// <summary>
        /// Authenticates a user using universal authentication.
        /// </summary>
        /// <remarks>
        /// Will create a new user if none exists!
        /// </remarks>
        public static Task<BrainCloudUser> Authenticate(string userId, string password)
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(userId), "!string.IsNullOrWhiteSpace(userId)");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(password), "!string.IsNullOrWhiteSpace(password)");
            var taskCompletionSource = new TaskCompletionSource<BrainCloudUser>();
            _brainCloudWrapper.AuthenticateUniversal(userId, password, true,
                (jsonData, ctx) =>
                {
                    var data = GetJsonData(jsonData);
                    var playerName = data["playerName"].ToString();
                    var profileId = data["profileId"].ToString();
                    var user = new BrainCloudUser(userId, playerName, profileId, 0);
                    taskCompletionSource.SetResult(user);
                },
                (status, code, error, ctx) =>
                {
                    if (code == ReasonCodes.TOKEN_DOES_NOT_MATCH_USER)
                    {
                        Debug.Log($"Authenticate '{userId}' INCORRECT PASSWORD {status} : {code} {error}");
                    }
                    else if (code == ReasonCodes.GAME_VERSION_NOT_SUPPORTED)
                    {
                        Debug.LogWarning($"Authenticate '{userId}' GAME_VERSION_NOT_SUPPORTED {status} : {code} {error}");
                    }
                    else
                    {
                        Debug.LogWarning($"Authenticate '{userId}' FAILED {status} : {code} {error}");
                    }
                    var user = new BrainCloudUser(userId, string.Empty, string.Empty, code);
                    taskCompletionSource.SetResult(user);
                });
            return taskCompletionSource.Task;
        }

        public static Task<int> UpdateUserName(string playerName)
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(playerName), "!string.IsNullOrWhiteSpace(playerName)");
            var taskCompletionSource = new TaskCompletionSource<int>();
            _brainCloudWrapper.PlayerStateService.UpdateName(playerName,
                (jsonData, ctx) =>
                {
                    taskCompletionSource.SetResult(0);
                },
                (status, code, error, ctx) =>
                {
                    Debug.Log($"PlayerStateService.UpdateName '{playerName}' FAILED {status} : {code} {error}");
                    taskCompletionSource.SetResult(code);
                });
            return taskCompletionSource.Task;
        }

        private static Dictionary<string, object> GetJsonData(string jsonText)
        {
            return JsonReader.Deserialize<Dictionary<string, object>>(jsonText)["data"] as Dictionary<string, object> ??
                   new Dictionary<string, object>();
        }
    }
}