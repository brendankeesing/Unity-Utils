using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

public class SimpleGradientTextureCacheItem
{
    public SimpleGradient gradient;
    public Texture2D texture;
    public int version = -1;
}

static class SimpleGradientTextureCache
{
    static List<SimpleGradientTextureCacheItem> _items = new List<SimpleGradientTextureCacheItem>();
    static List<Color> _colorCache = new List<Color>();

    public static Texture2D Get(SimpleGradient gradient, int width)
    {
        width = Mathf.Clamp(width, 32, 1024);

        SimpleGradientTextureCacheItem item = _items.Find(i => i.gradient == gradient);
        if (item != null)
        {
            if (item.texture.width >= width && item.version == gradient.version)
                return item.texture;
            Object.DestroyImmediate(item.texture);
            _items.Remove(item);
        }

        Texture2D tex = new Texture2D(width, 1, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        _colorCache.Clear();
        for (int i = 0; i < width; ++i)
            _colorCache.Add(gradient.Evaluate((float)i / (width - 1)));
        tex.SetPixels(_colorCache.ToArray());
        tex.Apply(false, false);

        _items.Add(new SimpleGradientTextureCacheItem()
        {
            gradient = gradient,
            texture = tex
        });
        return tex;
    }

    public static void Flush(SimpleGradient gradient)
    {
        _items.RemoveAll(i => i.gradient == gradient);
    }
}

[CustomPropertyDrawer(typeof(SimpleGradient))]
class SimpleGradientPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        SimpleGradient gradient = property.GetTargetObject<SimpleGradient>();
        GUI.DrawTexture(position, SimpleGradientTextureCache.Get(gradient, Mathf.RoundToInt(position.width)));
        if (Event.current.type == EventType.MouseDown && position.Contains(Event.current.mousePosition))
            SimpleGradientEditor.Show(gradient);

        EditorGUI.EndProperty();
    }
}
