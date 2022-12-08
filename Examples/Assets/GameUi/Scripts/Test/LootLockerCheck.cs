using System.Collections;
using Altzone.Scripts.Service.LootLocker;
using UnityEngine;

namespace GameUi.Scripts.Test
{
    public class LootLockerCheck : MonoBehaviour
    {
        private IEnumerator Start()
        {
            Debug.Log($"1 IsRunning {LootLockerWrapper.IsRunning}");
            yield return new WaitUntil(() => LootLockerWrapper.IsRunning);
            Debug.Log($"2 IsRunning {LootLockerWrapper.IsRunning}");
            var result = LootLockerWrapper.PingAsync();
            Debug.Log($"3 IsCompleted {result.IsCompleted}");
            yield return new WaitUntil(() => result.IsCompleted);
            Debug.Log($"4 Result {result.Result}");
            var playerName = LootLockerWrapper.PlayerName;
            Debug.Log($"5 playerName {playerName}");
        }
    }
}