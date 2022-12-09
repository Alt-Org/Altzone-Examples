using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Service.LootLocker;
using LootLocker.Requests;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode.LootLockerTests
{
    /// <summary>
    /// SOme <c>LootLocker</c> tests.
    /// </summary>
    /// <remarks>
    /// Note that <c>ServiceLoader</c> should start LootLocker!
    /// </remarks>
    public class LootLockerTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
#if !USE_LOOTLOCKER
            Assert.IsTrue(false, "USE_LOOTLOCKER is not defined");
#endif
        }

        [UnityTest]
        public IEnumerator ListCharacterTypesTest()
        {
            Debug.Log($"test");
            yield return new WaitUntil(() => LootLockerWrapper.IsRunning);

            var isRequestReady = false;
            Debug.Log($"ListCharacterTypes");
            List<LootLockerCharacter_Types> characterTypes = null;
            LootLockerSDKManager.ListCharacterTypes(r =>
            {
                Debug.Log($"success {r.success}");
                if (r.success)
                {
                    characterTypes = r.character_types.ToList();
                }
                isRequestReady = true;
            });
            yield return new WaitUntil(() => isRequestReady);
            Assert.IsNotNull(characterTypes);
            Assert.IsTrue(characterTypes.Count > 0);

            Debug.Log($"done");
        }

        [UnityTest]
        public IEnumerator SomeLootLockerTest()
        {
            Debug.Log($"test");
            yield return new WaitUntil(() => LootLockerWrapper.IsRunning);

            yield return null;

            Debug.Log($"done");
        }
    }
}