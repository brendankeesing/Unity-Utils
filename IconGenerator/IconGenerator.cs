using UnityEngine;

[RequireComponent(typeof(Camera))]
public class IconGenerator : MonoBehaviour
{
    public Vector2Int resolution;

#if UNITY_EDITOR
    [ContextMenu("Generate")]
    public void Generate()
    {
        Camera camera = GetComponent<Camera>();

        // setup
        RenderTexture rendertexture = RenderTexture.GetTemporary(resolution.x, resolution.y);
        camera.targetTexture = rendertexture;
        camera.Render();

        RenderTexture lastactivetexture = RenderTexture.active;
        RenderTexture.active = rendertexture;
        Texture2D texture = new Texture2D(rendertexture.width, rendertexture.height, TextureFormat.RGBA32, false);

        // render
        texture.ReadPixels(new Rect(0, 0, rendertexture.width, rendertexture.height), 0, 0);
        byte[] data = texture.EncodeToPNG();

        // cleanup
        DestroyImmediate(texture);
        RenderTexture.active = lastactivetexture;
        camera.targetTexture = null;
        RenderTexture.ReleaseTemporary(rendertexture);

        // save
        string filename = UnityEditor.EditorUtility.SaveFilePanel("Save Icon", Application.dataPath, null, "png");
        if (!string.IsNullOrEmpty(filename))
        {
            System.IO.File.WriteAllBytes(filename, data);
            UnityEditor.AssetDatabase.Refresh();
        }
    }
#endif
}
