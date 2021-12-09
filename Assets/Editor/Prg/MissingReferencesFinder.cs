using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor.Prg
{
    /// <summary>
    /// A helper editor script for finding missing references to objects.
    /// </summary>
    /// <remarks>
    /// See: https://github.com/liortal53/MissingReferencesUnity
    /// See: http://www.li0rtal.com/find-missing-references-unity/
    ///
    /// </remarks>
    public static class MissingReferencesFinder
    {
        private const string MenuRoot = "Window/ALT-Zone/Dependencies/Missing References : ";

        /// <summary>
        /// Finds all missing references to objects in the currently loaded scene.
        /// </summary>
        [MenuItem(MenuRoot + "Search in scene", false, 50)]
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
        [MenuItem(MenuRoot + "Search in all scenes", false, 51)]
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
        [MenuItem(MenuRoot + "Search in assets", false, 52)]
        public static void FindMissingReferencesInAssets()
        {
            Debug.Log("*");
            var allAssets = AssetDatabase.GetAllAssetPaths().Where(path => path.StartsWith("Assets/")).ToArray();
            var gameObjects = allAssets.Select(a =>
                AssetDatabase.LoadAssetAtPath(a, typeof(GameObject)) as GameObject).Where(a => a != null).ToArray();
            FindMissingReferences("Project", gameObjects);
            /*var scriptableObjects = allAssets.Select(a =>
                AssetDatabase.LoadAssetAtPath(a, typeof(ScriptableObject)) as ScriptableObject).Where(a => a != null).ToArray();
            FindMissingReferences("Project", scriptableObjects);*/
        }

        private static void FindMissingReferences(string context, GameObject[] gameObjects)
        {
            Debug.Log($"FindMissingReferences gameObjects {gameObjects.Length}");
            var missingCount = 0;
            foreach (var go in gameObjects)
            {
                var components = go.GetComponents<Component>();

                foreach (var component in components)
                {
                    // Missing components will be null, we can't find their type, etc.
                    if (!component)
                    {
                        Debug.LogWarning($"Missing Component ? in GameObject: {GetFullPath(go)}", go);
                        continue;
                    }
                    var so = new SerializedObject(component);
                    var sp = so.GetIterator();
                    var objRefValueMethod = typeof(SerializedProperty).GetProperty("objectReferenceStringValue",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                    // Iterate over the components' properties.
                    while (sp.NextVisible(true))
                    {
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
                                missingCount += 1;
                                ShowMissing(context, go, component.GetType().Name, ObjectNames.NicifyVariableName(sp.name));
                            }
                        }
                    }
                }
            }
            Debug.Log($"missingCount {missingCount}");
        }

        /*private static void FindMissingReferences(string context, ScriptableObject[] scriptableObjects)
        {
            // THis needs more work to be practical!
            Debug.Log($"FindMissingReferences scriptableObjects {scriptableObjects.Length}");
            foreach (var scriptableObject in scriptableObjects)
            {
                var fields = scriptableObject.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var fieldInfo in fields)
                {
                    var value = fieldInfo.GetValue(scriptableObject);
                    if (value == null)
                    {
                        //Debug.Log($"scriptableObject {scriptableObject} fields {fields.Length}");
                        ShowMissing(context, scriptableObject, fieldInfo.Name, fieldInfo.FieldType);
                    }
                }
            }
        }*/

        private static GameObject[] GetSceneObjects()
        {
            // Use this method since GameObject.FindObjectsOfType will not return disabled objects.
            return Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(go => string.IsNullOrEmpty(AssetDatabase.GetAssetPath(go))
                             && go.hideFlags == HideFlags.None).ToArray();
        }

        private static void ShowMissing(string context, GameObject go, string componentName, string propertyName)
        {
            Debug.LogWarning($"Missing Ref in: [{context}]{GetFullPath(go)}. Component: {componentName}, Property: {propertyName}");
        }

        /*private static void ShowMissing(string context, ScriptableObject so, string propertyName, Type propertyType)
        {
            Debug.LogWarning($"Missing Ref in: [{context}]{so.name}. Property: {propertyName} ({propertyType.Name})");
        }*/

        private static string GetFullPath(GameObject go)
        {
            var parent = go.transform.parent;
            return parent == null
                ? go.name
                : GetFullPath(parent.gameObject) + "/" + go.name;
        }
    }
}