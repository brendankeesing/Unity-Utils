using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public static class UnityEditorUtils
{
    static string[] _layerNames = null;
    static void SetupLayerNames()
    {
        if (_layerNames != null)
            return;

        _layerNames = new string[32];
        for (int i = 0; i < _layerNames.Length; ++i)
            _layerNames[i] = LayerMask.LayerToName(i);
    }

    public static LayerMask LayerMaskField(LayerMask mask, params GUILayoutOption[] options)
    {
        SetupLayerNames();
        return EditorGUILayout.MaskField(mask.value, _layerNames, options);
    }

    public static LayerMask LayerMaskField(string label, LayerMask mask, params GUILayoutOption[] options)
    {
        SetupLayerNames();
        return EditorGUILayout.MaskField(label, mask.value, _layerNames, options);
    }

    public static string[] monthNames =
    {
        "January",
        "February",
        "March",
        "April",
        "May",
        "June",
        "July",
        "August",
        "September",
        "October",
        "November",
        "December",
    };

    public static System.DateTime DateGUI(string label, System.DateTime value)
    {
        int hour = value.Hour;
        int day = value.Day;
        int month = value.Month;
        int year = value.Year;

        EditorGUILayout.BeginHorizontal();
        hour = Mathf.Clamp(EditorGUILayout.IntField(label, hour), 0, 24);
        day = Mathf.Clamp(EditorGUILayout.IntField(day), 1, System.DateTime.DaysInMonth(year, month));
        month = EditorGUILayout.Popup(month - 1, monthNames) + 1;
        year = Mathf.Clamp(EditorGUILayout.IntField(year), System.DateTime.MinValue.Year, System.DateTime.MaxValue.Year);
        EditorGUILayout.EndHorizontal();

        return new System.DateTime(year, month, day, hour, 0, 0);
    }

    [MenuItem("GameObject/Sort Children", false, 0)]
    public static void SortChildrenInHierarchy()
    {
        Transform parent = Selection.activeTransform;
        if (parent == null)
            return;

        string[] names = new string[parent.childCount];
        for (int i = 0; i < parent.childCount; ++i)
            names[i] = parent.GetChild(i).gameObject.name;

        names.Sort();

        for (int j = 0; j < 100; ++j)
        {
            for (int i = 0; i < parent.childCount; ++i)
                parent.GetChild(i).SetSiblingIndex(names.IndexOf(parent.GetChild(i).gameObject.name));

            // this is horrible, but whatever
            bool isinorder = true;
            for (int i = 0; i < parent.childCount - 1; ++i)
            {
                int c = parent.GetChild(i).gameObject.name.CompareTo(parent.GetChild(i + 1).gameObject.name);
                if (c >= 1)
                {
                    isinorder = false;
                    break;
                }
            }
            if (isinorder)
                break;
        }
    }

    public static object GetTargetObject(this SerializedProperty property)
    {
        object obj = property.serializedObject.targetObject;

        System.Reflection.FieldInfo field = null;
        foreach (var path in property.propertyPath.Split('.'))
        {
            var type = obj.GetType();
            field = type.GetField(path);
            obj = field.GetValue(obj);
        }
        return obj;
    }

    public static T GetTargetObject<T>(this SerializedProperty property)
    {
        return (T)GetTargetObject(property);
    }

    static TMPro.TMP_FontAsset GetTMPFontFromFont(Font font)
    {
        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        foreach (string guid in guids)
        {
            TMPro.TMP_FontAsset tmpfont = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
            if (tmpfont.sourceFontFile == font)
                return tmpfont;
        }
        return null;
    }

    [MenuItem(itemName: "Custom/UI Text To Text Mesh Pro")]
    public static void UITextToTextMeshPro()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            foreach (UnityEngine.UI.Text uitext in obj.GetComponentsInChildren<UnityEngine.UI.Text>())
            {
                GameObject gameobject = uitext.gameObject;

                TMPro.TMP_FontAsset font = GetTMPFontFromFont(uitext.font);
                string text = uitext.text;
                Color color = uitext.color;
                FontStyle style = uitext.fontStyle;
                bool enableAutoSizing = uitext.resizeTextForBestFit;
                bool raycasttarget = uitext.raycastTarget;
                TextAnchor alignment = uitext.alignment;
                Object.DestroyImmediate(uitext);

                TMPro.TMP_Text textmeshpro = gameobject.AddComponent<TMPro.TextMeshProUGUI>();
                textmeshpro.font = font;
                textmeshpro.text = text;
                textmeshpro.color = color;
                textmeshpro.fontStyle = (TMPro.FontStyles)style;
                textmeshpro.enableAutoSizing = enableAutoSizing;
                textmeshpro.raycastTarget = raycasttarget;
                textmeshpro.fontSizeMin = 5;
                textmeshpro.fontSizeMax = 999;
                if (alignment == TextAnchor.UpperLeft) textmeshpro.alignment = TMPro.TextAlignmentOptions.TopLeft;
                if (alignment == TextAnchor.UpperCenter) textmeshpro.alignment = TMPro.TextAlignmentOptions.Top;
                if (alignment == TextAnchor.UpperRight) textmeshpro.alignment = TMPro.TextAlignmentOptions.TopRight;
                if (alignment == TextAnchor.MiddleLeft) textmeshpro.alignment = TMPro.TextAlignmentOptions.Left;
                if (alignment == TextAnchor.MiddleCenter) textmeshpro.alignment = TMPro.TextAlignmentOptions.Center;
                if (alignment == TextAnchor.MiddleRight) textmeshpro.alignment = TMPro.TextAlignmentOptions.Right;
                if (alignment == TextAnchor.LowerLeft) textmeshpro.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                if (alignment == TextAnchor.LowerCenter) textmeshpro.alignment = TMPro.TextAlignmentOptions.Bottom;
                if (alignment == TextAnchor.LowerRight) textmeshpro.alignment = TMPro.TextAlignmentOptions.BottomRight;

                EditorUtility.SetDirty(textmeshpro.gameObject);
            }
        }
    }

    [MenuItem(itemName: "Custom/Run Physics _F8")]
    public static void RunPhysics()
    {
        CharacterController[] characters = Object.FindObjectsOfType<CharacterController>();
        foreach (CharacterController character in characters)
            character.gameObject.SetActive(false);

        List<Rigidbody> rigidbodies = new List<Rigidbody>();
        foreach (GameObject gameobj in Selection.gameObjects)
        {
            if (gameobj.GetComponent<Rigidbody>() == null)
                rigidbodies.Add(gameobj.AddComponent<Rigidbody>());
        }

        Physics.autoSimulation = false;
        Physics.Simulate(Time.fixedDeltaTime);
        Physics.autoSimulation = true;

        foreach (Rigidbody rigidbody in rigidbodies)
            Object.DestroyImmediate(rigidbody);

        foreach (GameObject gameobj in Selection.gameObjects)
            EditorSceneManager.MarkSceneDirty(gameobj.scene);

        foreach (CharacterController character in characters)
            character.gameObject.SetActive(true);
    }

    public static List<T> FindAllAssets<T>() where T : Object
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
        List<T> assets = new List<T>(guids.Length);
        foreach (string guid in guids)
        {
            T item = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
            if (item != null)
                assets.Add(item);
        }
        return assets;
    }

    public static void ForEachChildObject(Transform parent, System.Action<Transform> onfound)
    {
        onfound(parent);
        foreach (Transform child in parent)
            ForEachChildObject(child, onfound);
    }

    public static void ForEachObjectReference<T>(GameObject gameobj, System.Action<T> onfound) where T : Object
    {
        foreach (Component component in gameobj.GetComponents<Component>())
        {
            SerializedObject serializedobject = new SerializedObject(component);
            SerializedProperty prop = serializedobject.GetIterator();
            while (prop.Next(true))
            {
                if (prop.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                Object obj = prop.objectReferenceValue;
                if (obj != null && obj is T objt)
                    onfound(objt);
            }
        }
    }

    public static List<T> GetAllAssetsOfType<T>(string search, System.Func<T, bool> filter = null) where T : Object
    {
        List<T> list = new List<T>();
        string[] allassets = AssetDatabase.FindAssets(search);
        for (int i = 0; i < allassets.Length; ++i)
        {
            string path = AssetDatabase.GUIDToAssetPath(allassets[i]);

            EditorUtility.DisplayProgressBar("Fetching", path, (float)i / (allassets.Length - 1));

            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (filter != null && !filter(asset))
                continue;

            list.Add(asset);
        }
        list.Sort((a, b) => a.name.CompareTo(b.name));

        EditorUtility.ClearProgressBar();

        return list;
    }

    public static void ForEachPrefabInProject(System.Action<Object, GameObject> onfound, System.Action<Object> onpreload = null)
    {
        try
        {
            string[] prefabs = AssetDatabase.FindAssets("t:Prefab");
            for (int i = 0; i < prefabs.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabs[i]);

                Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (obj == null)
                    continue;

                EditorUtility.DisplayProgressBar("Searching Prefabs", path, (float)i / (prefabs.Length - 1));

                onpreload?.Invoke(obj);
                GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(obj);
                if (prefab == null)
                    continue;

                onfound(obj, prefab);
                Object.DestroyImmediate(prefab);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }

        EditorUtility.ClearProgressBar();
    }

    public static void ForEachSceneInProject(System.Action<Scene, SceneAsset> onfound, System.Action<Object> onpreload = null)
    {
        try
        {
            string[] scenes = AssetDatabase.FindAssets("t:Scene");
            for (int i = 0; i < scenes.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(scenes[i]);

                EditorUtility.DisplayProgressBar("Searching Scenes", path, (float)i / (scenes.Length - 1));

                SceneAsset sceneasset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                onpreload?.Invoke(sceneasset);
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                onfound(scene, sceneasset);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }

        EditorUtility.ClearProgressBar();
    }

    public static void ForEachSceneRootObjectInProject(System.Action<Scene, SceneAsset, GameObject> onfound)
    {
        ForEachSceneInProject((scene, sceneasset) =>
        {
            foreach (GameObject rootobject in scene.GetRootGameObjects())
                onfound(scene, sceneasset, rootobject);
        });
    }
}