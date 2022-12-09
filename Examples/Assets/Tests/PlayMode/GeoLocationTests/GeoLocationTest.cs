using System.Collections;
using Altzone.Scripts;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode.GeoLocationTests
{
    public class GeoLocationTest
    {
        [UnityTest, Description("Deletes and loads GeoLocation.Data")]
        public IEnumerator GeoLocationRunner()
        {
            Debug.Log($"test Data {GeoLocation.Data}");
            yield return null;

            GeoLocation.Delete();
            var startTime = Time.time;
            GeoLocation.Load(data => { Debug.Log($"Load Data {data} in {Time.time - startTime:0.00} s"); });
            
            yield return new WaitUntil(() => GeoLocation.HasData);

            var data = GeoLocation.Data;
            Assert.IsNotNull(data.Country);
            Assert.IsNotNull(data.CountryCode);

            Debug.Log($"done Data {GeoLocation.Data}");
        }
    }
}