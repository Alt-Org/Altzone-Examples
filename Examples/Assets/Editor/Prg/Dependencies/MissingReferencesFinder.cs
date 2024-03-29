using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor.Prg.Dependencies
{
    /// <summary>
    /// A helper editor script for finding missing references to objects.
    /// </summary>
    /// <remarks>
    /// See: https://github.com/liortal53/MissingReferencesUnity
    /// See: http://www.li0rtal.com/find-missing-references-unity/
    ///
    /// </remarks>
    internal static class MissingReferencesFinder
    {
        /// <summary>
        /// Finds all missing references to objects in the currently loaded scene.
        /// </summary>
        public static void FindMissingReferencesInCurrentScene()
        {
            Debug.Log("*");
            var sceneObjects = GetSceneObjects();
            FindMissingReferences(SceneManager.GetActiveScene().path, sceneObjects);
        }

        /// <summary>
        /// Finds all missing references to objects in all enabled scenes in the project.
        /// This works by loading the scenes one by one and checking for missing object references.
        /// </summary>
        public static void FindMissingReferencesInAllScenes()
        {
            Debug.Log("*");
            foreach (var scene in EditorBuildSettings.scenes.Where(s => s.enabled))
            {
                Debug.Log($"OpenScene {scene.path}");
                EditorSceneManager.OpenScene(scene.path);
                FindMissingReferencesInCurrentScene();
            }
        }

        /// <summary>
        /// Finds all missing references to objects in assets (objects from the project window).
        /// </summary>
        public static void FindMissingReferencesInAssets()
        {
            Debug.Log("*");
            var allAssets = AssetDatabase.GetAllAssetPaths().Where(path => path.StartsWith("Assets/")).ToArray();
            var gameObjects = allAssets.Select(a =>
                AssetDatabase.LoadAssetAtPath(a, typeof(GameObject)) as GameObject).Where(a => a != null).ToArray();
            FindMissingReferences("Project", gameObjects);
        }

        /// <summary>
        /// Finds all missing references to selected objects in the project window.
        /// </summary>
        public static void FindMissingReferencesInSelection()
        {
            Debug.Log("*");
            var selectedGuids = Selection.assetGUIDs;
            Debug.Log($"FindMissingReferences selection {selectedGuids.Length}");
            var missingCount = 0;
            foreach (var guid in selectedGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadMainAssetAtPath(path);
                if (asset is GameObject gameObject)
                {
                    missingCount += FindMissingReferences("Selection", gameObject.transform);
                }
            }
            Debug.Log($"missingCount {missingCount}");
        }

        private static void FindMissingReferences(string context, GameObject[] gameObjects)
        {
            Debug.Log($"FindMissingReferences gameObjects {gameObjects.Length}");
            var missingCount = 0;
            foreach (var gameObject in gameObjects)
            {
                missingCount += FindMissingReferences(context, gameObject.transform);
            }
            Debug.Log($"missingCount {missingCount}");
        }

        private static int FindMissingReferences(string context, Transform transform)
        {
            var gameObject = transform.gameObject;
            var missingCount = 0;
            var components = gameObject.GetComponents<Component>();
            var objRefValueMethod = typeof(SerializedProperty).GetProperty("objectReferenceStringValue",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var component in components)
            {
                // Missing components will be null, we can't find their type, etc.
                if (!component)
                {
                    Debug.LogWarning($"{RichText.Red("NOT FOUND")}: [{context}]{gameObject.GetFullPath()}: Is component deleted?", gameObject);
                    missingCount += 1;
                    continue;
                }
                var so = new SerializedObject(component);
                var sp = so.GetIterator();
                // Iterate over the components' properties.
                while (sp.NextVisible(true))
                {
                    // This should find arrays of object - at least if first item is missing
                    if (sp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        var objectReferenceStringValue = string.Empty;
                        if (objRefValueMethod != null)
                        {
                            objectReferenceStringValue =
                                (string)objRefValueMethod.GetGetMethod(true).Invoke(sp, new object[] { });
                        }

                        if (sp.objectReferenceValue == null
                            && (sp.objectReferenceInstanceIDValue != 0 || objectReferenceStringValue.StartsWith("Missing")))
                        {
                            var propName1 = ObjectNames.NicifyVariableName(sp.name);
                            var propName2 = sp.propertyPath;
                            var propName = string.Equals(propName1, propName2, StringComparison.CurrentCultureIgnoreCase)
                                ? propName1
                                : $"{propName1} ({propName2})";
                            ShowMissing(context, gameObject,
                                component.GetType().Name,
                                propName,
                                objectReferenceStringValue);
                            missingCount += 1;
                        }
                    }
                }
            }
            var childCount = transform.childCount;
            if (childCount > 0)
            {
                for (var i = 0; i < childCount; ++i)
                {
                    var child = transform.GetChild(i);
                    missingCount += FindMissingReferences(context, child);
                }
            }
            return missingCount;
        }

        private static GameObject[] GetSceneObjects()
        {
            // Use this method since GameObject.FindObjectsOfType will not return disabled objects.
            return Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(go => string.IsNullOrEmpty(AssetDatabase.GetAssetPath(go))
                             && go.hideFlags == HideFlags.None).ToArray();
        }

        private static void ShowMissing(string context, GameObject gameObject, string componentName, string propertyName, string propMessage)
        {
            Debug.LogWarning($"{RichText.Yellow("MISSING")}: [{context}]{gameObject.GetFullPath()}: " +
                             $"component: {componentName}, property: {propertyName} : {propMessage}", gameObject);
        }
    }
}