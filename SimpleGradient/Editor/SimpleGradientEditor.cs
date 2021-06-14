using UnityEngine;
using UnityEditor;

public class SimpleGradientEditor : EditorWindow
{
    public static SimpleGradientEditor instance { get; private set; }
    public SimpleGradient gradient;

    public static void Show(SimpleGradient gradient)
    {
        SimpleGradientEditor picker = GetWindow<SimpleGradientEditor>(true, "Simple Gradient", true);
        picker.gradient = gradient;
        picker.ShowAuxWindow();
        picker.wantsMouseMove = true;
        picker.minSize = new Vector2(360, 224);
        picker.maxSize = new Vector2(1900, 3000);
        picker.Repaint();
    }

    public void OnEnable()
    {
        hideFlags = HideFlags.DontSave;
    }

    public void OnDisable()
    {
        instance = null;
    }

    public void OnGUI()
    {
        if (gradient == null)
            return;

        EditorGUI.BeginChangeCheck();

        gradient.blendCurve = EditorGUILayout.CurveField("Curve", gradient.blendCurve);
        gradient.linear = EditorGUILayout.Toggle("Linear", gradient.linear);
        gradient.hdr = EditorGUILayout.Toggle("HDR", gradient.hdr);

        int count = Mathf.Max(1, EditorGUILayout.IntField("Count", gradient.keys.Count));
        gradient.keys.Resize(count, new SimpleGradientKey() { time = 0, color = Color.white });

        for (int i = 0; i < gradient.keys.Count; ++i)
        {
            SimpleGradientKey key = gradient.keys[i];

            EditorGUILayout.BeginHorizontal();
            key.time =  EditorGUILayout.Slider(key.time, 0, 1);
            key.color = EditorGUILayout.ColorField(GUIContent.none, key.color, true, true, gradient.hdr);
            EditorGUILayout.EndHorizontal();

            gradient.keys[i] = key;
        }
        
        if (GUILayout.Button("Sort"))
            gradient.Sort();

        if (EditorGUI.EndChangeCheck())
            ++gradient.version;

        Rect rect = EditorGUILayout.GetControlRect(false, 50);
        GUI.DrawTexture(rect, SimpleGradientTextureCache.Get(gradient, Mathf.RoundToInt(rect.width)), ScaleMode.StretchToFill, false);
    }
}
