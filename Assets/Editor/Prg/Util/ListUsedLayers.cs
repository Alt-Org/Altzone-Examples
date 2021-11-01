using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg.Util
{
    public class ListUsedLayers : MonoBehaviour
    {
        [MenuItem("Window/ALT-Zone/Util/List Used layers in Scene")]
        private static void _ListUsedLayers()
        {
            UnityEngine.Debug.Log("*");
            ListObjectsInLayer(GetSceneObjects());
        }

        private static void ListObjectsInLayer(GameObject[] gameObjects)
        {
            var layerObjects = new Dictionary<int, List<string>>();
            foreach (var go in gameObjects)
            {
                if (go.layer == 0)
                {
                    continue;
                }
                var name = GetFullName(go);
                if (!layerObjects.TryGetValue(go.layer, out var objectList))
                {
                    objectList = new List<string>();
                    layerObjects.Add(go.layer, objectList);
                }
                objectList.Add(name);
            }
            var usedLayers = layerObjects.Keys.ToList();
            usedLayers.Sort();
            foreach (var usedLayer in usedLayers)
            {
                var layerName = LayerMask.LayerToName(usedLayer);
                var objectList = layerObjects[usedLayer];
                UnityEngine.Debug.Log($"Layer {usedLayer:D2} : {layerName,-16} is used in {objectList.Count} GameObject(s)");
            }
        }

        private static GameObject[] GetSceneObjects()
        {
            return Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(go => go.hideFlags == HideFlags.None).ToArray();
        }

        private static string GetFullName(GameObject go)
        {
            var name = go.name;
            while (go.transform.parent != null)
            {
                go = go.transform.parent.gameObject;
                name = go.name + "/" + name;
            }
            return name;
        }
    }
}