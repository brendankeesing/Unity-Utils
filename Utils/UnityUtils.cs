using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public static class UnityUtils
{
    public const float metersToFeet = 3.28084f;

    public static void TODO(string details, Object source = null)
    {
        Debug.Log("TODO: " + details, source);
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            --n;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static Rect Encapsulate(this Rect rect, Vector2 point)
    {
        if (point.x < rect.xMin)
            rect.xMin = point.x;
        else if (point.x > rect.xMax)
            rect.xMax = point.x;
        if (point.y < rect.yMin)
            rect.yMin = point.y;
        else if (point.y > rect.yMax)
            rect.yMax = point.y;
        return rect;
    }

    public static Color MoveTowards(Color a, Color b, float t)
    {
        return new Color(
            Mathf.MoveTowards(a.r, b.r, t),
            Mathf.MoveTowards(a.g, b.g, t),
            Mathf.MoveTowards(a.b, b.b, t),
            Mathf.MoveTowards(a.a, b.a, t)
            );
    }

    public static Color MultiplyRGB(this Color c, float v)
    {
        c.r *= v;
        c.g *= v;
        c.b *= v;
        return c;
    }

    public static Vector2 Rotate(this Vector2 v, float radians)
    {
        float s = -Mathf.Sin(radians);
        float c = -Mathf.Cos(radians);
        return new Vector2(v.x * c - v.y * s, v.y * c + v.x * s);
    }

    public static Quaternion LookAtForceUp(Vector3 forward, Vector3 up)
    {
        Vector3 right = Vector3.Cross(forward, up);
        forward = Vector3.Cross(up, right);
        return Quaternion.LookRotation(forward, up);
    }

    public static Vector2 Multiply(this Vector2 vec1, Vector2 vec2)
    {
        vec1.x *= vec2.x;
        vec1.y *= vec2.y;
        return vec1;
    }

    public static Vector3 Multiply(this Vector3 vec1, Vector3 vec2)
    {
        vec1.x *= vec2.x;
        vec1.y *= vec2.y;
        vec1.z *= vec2.z;
        return vec1;
    }

    public static Vector4 Multiply(this Vector4 vec1, Vector4 vec2)
    {
        vec1.x *= vec2.x;
        vec1.y *= vec2.y;
        vec1.z *= vec2.z;
        vec1.w *= vec2.w;
        return vec1;
    }

    public static float ClampAngle(float angle, float from, float to)
    {
        if (angle < 0f)
            angle = 360 + angle;
        if (angle > 180f)
            return Mathf.Max(angle, 360 + from);
        return Mathf.Min(angle, to);
    }

    public static Color ColorLerp(Color a, Color b, float t)
    {
        return new Color(
            Mathf.Sqrt(Mathf.LerpUnclamped(a.r * a.r, b.r * b.r, t)),
            Mathf.Sqrt(Mathf.LerpUnclamped(a.g * a.g, b.g * b.g, t)),
            Mathf.Sqrt(Mathf.LerpUnclamped(a.b * a.b, b.b * b.b, t)),
            Mathf.LerpUnclamped(a.a, b.a, t));
    }

    public static float ColorDistanceSqr(Color a, Color b)
    {
        float rdiff = a.r - b.r;
        float gdiff = a.g - b.g;
        float bdiff = a.b - b.b;
        float adiff = a.a - b.a;
        return rdiff * rdiff + gdiff * gdiff + bdiff * bdiff + adiff * adiff;
    }

    public static int TriangleIndexToSubmesh(Mesh mesh, int triangleindex)
    {
        for (int i = 1; i < mesh.subMeshCount; ++i)
        {
            if (triangleindex * 3 < mesh.GetIndexStart(i))
                return i - 1;
        }
        return mesh.subMeshCount - 1;
    }

    public static float GetRayToLineSegmentIntersection(Vector2 origin, Vector2 direction, Vector2 a, Vector2 b)
    {
        Vector2 ortho = new Vector2(-direction.y, direction.x);
        Vector2 a2origin = origin - a;
        Vector2 a2b = b - a;

        float denom = Vector2.Dot(a2b, ortho);
        if (Mathf.Abs(denom) < 0.000001f)
            return float.NaN; // parallel

        float cross = a2b.x * a2origin.y - a2b.y * a2origin.x;
        float t1 = cross / denom;
        //float t2 = Vector2.Dot(a2origin, ortho) / denom;

        return t1;
        //return t2 >= 0 && t2 <= 1 && t1 >= 0;
    }

    public static float GetLineSegmentIntersection(Vector2 line0p0, Vector2 line0p1, Vector2 line1p0, Vector2 line1p1)
    {
        return GetRayToLineSegmentIntersection(line0p0, (line0p1 - line0p0).normalized, line1p0, line1p1) / Vector2.Distance(line0p0, line0p1);
    }

    public static Vector2 ClosesPointOnLine(Vector2 point, Vector2 line0, Vector2 line1)
    {
        Vector2 p = point - line0;
        Vector2 dir = line1 - line0;
        return line0 + Vector2.Dot(p, dir) / Vector2.Dot(dir, dir) * dir;
    }

    public static bool IsPointToLeftOfLine(Vector2 line0, Vector2 line1, Vector2 point)
    {
        return ((line1.x - line0.x) * (point.y - line0.y) - (line1.y - line0.y) * (point.x - line0.x)) > 0;
    }

    public static float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    public static float GetFrustumDistanceWithHeight(float height, float fov)
    {
        return height * 0.5f / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
    }

    public static int CompareRaycastHitByDistance(RaycastHit a, RaycastHit b)
    {
        return a.distance.CompareTo(b.distance);
    }

    public static void SetAllChildLayers(Transform transform, int layer)
    {
        transform.gameObject.layer = layer;
        foreach (Transform child in transform)
            SetAllChildLayers(child, layer);
    }

    public static bool HasLayer(LayerMask mask, int layer)
    {
        return (mask.value & 1 << layer) != 0;
    }

    public static List<GameObject> FindChildrenWithTag(this GameObject gameobject, string tag, List<GameObject> objs = null)
    {
        if (objs == null)
            objs = new List<GameObject>();
        FindChildrenWithTag(tag, gameobject.transform, objs);
        return objs;
    }

    static void FindChildrenWithTag(string tag, Transform parent, List<GameObject> objs)
    {
        if (parent.CompareTag(tag))
            objs.Add(parent.gameObject);

        foreach (Transform child in parent)
            FindChildrenWithTag(tag, child, objs);
    }

    public static Vector2 SetX(this Vector2 vec, float x)
    {
        vec.x = x;
        return vec;
    }

    public static Vector2 SetY(this Vector2 vec, float y)
    {
        vec.y = y;
        return vec;
    }

    public static Vector3 SetX(this Vector3 vec, float x)
    {
        vec.x = x;
        return vec;
    }

    public static Vector3 SetY(this Vector3 vec, float y)
    {
        vec.y = y;
        return vec;
    }

    public static Vector3 SetZ(this Vector3 vec, float z)
    {
        vec.z = z;
        return vec;
    }

    public static Color SetR(this Color color, float r)
    {
        color.r = r;
        return color;
    }

    public static Color SetG(this Color color, float g)
    {
        color.g = g;
        return color;
    }

    public static Color SetB(this Color color, float b)
    {
        color.b = b;
        return color;
    }

    public static Color SetA(this Color color, float a)
    {
        color.a = a;
        return color;
    }

    public static float Smooth(float t)
    {
        float tt = Mathf.Abs(t);
        tt = tt * tt * tt * (tt * (6f * tt - 15f) + 10f);
        return tt * Mathf.Sign(t);
    }

    public static int Repeat(int v, int count)
    {
        v %= count;
        if (v < 0)
            v += count;
        return v;
    }

    public static int Repeat(int v, int min, int max)
    {
        v -= min;
        int count = max - min;
        v %= count;
        if (v < 0)
            v += count;
        return v + min;
    }

    // Loops angle so that it is between -180 and +180
    public static float LoopAngle(float angle)
    {
        angle %= 360;
        if (angle < -180)
            angle += 180;
        else if (angle > 180)
            angle -= 180;
        return angle;
    }

    public static Transform FindChildWithName(this Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.gameObject.name == name)
                return child;
            Transform found = FindChildWithName(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

    public static Transform FindChildWithTag(this Transform parent, string tag, bool includeinactive = false)
    {
        foreach (Transform child in parent)
        {
            if (!includeinactive && !child.gameObject.activeSelf)
                continue;
            if (child.CompareTag(tag))
                return child;
            Transform found = FindChildWithTag(child, tag, includeinactive);
            if (found != null)
                return found;
        }
        return null;
    }

    public static Transform FindObjectAtPath(string scenename, string[] splitpath)
    {
        if (!string.IsNullOrEmpty(scenename))
            return FindObjectAtPath(SceneManager.GetSceneByName(scenename), splitpath);

        for (int i = 0; i < SceneManager.sceneCount; ++i)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            Transform obj = FindObjectAtPath(scene, splitpath);
            if (obj != null)
                return obj;
        }
        return null;
    }

    public static Transform FindObjectAtPath(Scene scene, string[] splitpath)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return null;

        foreach (GameObject obj in scene.GetRootGameObjects())
        {
            Transform result = FindObjectAtPath(splitpath, obj.transform);
            if (result != null)
                return result;
        }
        return null;
    }

    public static Transform FindObjectAtPath(string[] splitpath, Transform parent, int pathindex = 0)
    {
        if (parent.gameObject.name != splitpath[pathindex])
            return null;

        ++pathindex;
        if (pathindex >= splitpath.Length)
            return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindObjectAtPath(splitpath, child, pathindex);
            if (result != null)
                return result;
        }
        return null;
    }

    public static List<string> GetObjectPath(Transform obj, out string scenename)
    {
        if (obj == null)
        {
            scenename = null;
            return null;
        }

        scenename = obj.gameObject.scene.name;
        Transform parent = obj;
        List<string> pathsplit = new List<string>();
        while (parent != null)
        {
            pathsplit.Insert(0, parent.gameObject.name);
            parent = parent.parent;
        }
        return pathsplit;
    }

    public static T DeepCopy<T>(T obj)
    {
        return (T)JsonUtility.FromJson(JsonUtility.ToJson(obj), obj.GetType());
    }

    public static void SetCanEmit(this ParticleSystem partices, bool canemit)
    {
        ParticleSystem.EmissionModule emission = partices.emission;
        emission.enabled = canemit;
    }

    public static LayerMask LayerToLayerMask(int layer)
    {
        return 1 << layer;
    }

    public static LayerMask AddLayerMasks(LayerMask a, LayerMask b)
    {
        return a | b;
    }

    public static LayerMask SubtractLayerMasks(LayerMask a, LayerMask b)
    {
        return a & ~b;
    }

    public static LayerMask SetLayerInLayerMask(LayerMask layermask, int layer, bool ison)
    {
        return ison ? AddLayerMasks(layermask, LayerToLayerMask(layer)) : SubtractLayerMasks(layermask, LayerToLayerMask(layer));
    }
    
    public static float LinearToDecibel(float linear)
    {
        if (linear != 0)
            return 20.0f * Mathf.Log10(linear);
        else
            return -144.0f;
    }

    public static float DecibelToLinear(float dB)
    {
        return Mathf.Pow(10.0f, dB / 20.0f);
    }

    public static Texture2D RenderTextureToTexture2D(RenderTexture rt, bool recalculatemipmaps)
    {
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        return RenderTextureToTexture2D(tex, rt, recalculatemipmaps);
    }

    public static Texture2D RenderTextureToTexture2D(Texture2D tex, RenderTexture rt, bool recalculatemipmaps)
    {
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, recalculatemipmaps);
        tex.Apply();
        return tex;
    }

    public static void ResizeTexture(Texture2D tex, int width, int height, bool updatemipmaps, bool makenolongerreadable)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        RenderTexture.active = rt;
        Graphics.Blit(tex, rt);
        tex.Resize(width, height);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply(updatemipmaps, makenolongerreadable);
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
    }

    public static void BakeMeshesToShadowMesh(Transform parent, MeshFilter meshfilter)
    {
        List<CombineInstance> instances = new List<CombineInstance>();
        Matrix4x4 parentmatrix = parent.worldToLocalMatrix;
        foreach (MeshFilter thismeshfilter in parent.GetComponentsInChildren<MeshFilter>())
        {
            MeshRenderer thisrenderer = thismeshfilter.GetComponent<MeshRenderer>();
            if (thisrenderer == null || thisrenderer.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.Off)
                continue;

            thisrenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Matrix4x4 thismatrix = parentmatrix * thismeshfilter.transform.localToWorldMatrix;
            Mesh thismesh = thismeshfilter.sharedMesh;
            for (int i = 0; i < thismesh.subMeshCount; ++i)
            {
                instances.Add(new CombineInstance()
                {
                    mesh = thismesh,
                    subMeshIndex = i,
                    transform = thismatrix
                });
            }
        }

        Mesh mesh = meshfilter.sharedMesh;
        mesh.Clear();
        mesh.CombineMeshes(instances.ToArray(), true, true, false);
        mesh.UploadMeshData(false);
    }

    public static void LerpRectTransform(this RectTransform rt, RectTransform start, RectTransform end, float t)
    {
        rt.pivot = Vector2.LerpUnclamped(start.pivot, end.pivot, t);
        rt.anchorMin = Vector2.LerpUnclamped(start.anchorMin, end.anchorMin, t);
        rt.anchorMax = Vector2.LerpUnclamped(start.anchorMax, end.anchorMax, t);
        rt.anchoredPosition = Vector2.LerpUnclamped(start.anchoredPosition, end.anchoredPosition, t);
        rt.offsetMin = Vector2.LerpUnclamped(start.offsetMin, end.offsetMin, t);
        rt.offsetMax = Vector2.LerpUnclamped(start.offsetMax, end.offsetMax, t);
        rt.sizeDelta = Vector2.LerpUnclamped(start.sizeDelta, end.sizeDelta, t);
    }

    public static void CenterScrollRectOnChild(this UnityEngine.UI.ScrollRect scrollrect, RectTransform child, float animationduration = 0)
    {
        Vector3 worldpos = child.position;
        Vector3 relpos = scrollrect.viewport.InverseTransformPoint(worldpos);

        Vector2 displacement = scrollrect.viewport.rect.center - (Vector2)relpos;

        if (!scrollrect.horizontal)
            displacement.x = 0;
        if (!scrollrect.vertical)
            displacement.y = 0;

        Vector2 targetposition = scrollrect.content.anchoredPosition + displacement;
        if (scrollrect.horizontal)
            targetposition.x = Mathf.Clamp(targetposition.x, 0, scrollrect.content.sizeDelta.x - scrollrect.viewport.rect.width);
        if (scrollrect.vertical)
            targetposition.y = Mathf.Clamp(targetposition.y, 0, scrollrect.content.sizeDelta.y - scrollrect.viewport.rect.height);

        if (animationduration > 0.001f)
        {
            scrollrect.StopAllCoroutines();
            scrollrect.StartCoroutine(AnimateScrollRect(scrollrect, targetposition, animationduration));
        }
        else
            scrollrect.content.anchoredPosition = targetposition;
    }

    static IEnumerator AnimateScrollRect(UnityEngine.UI.ScrollRect scrollrect, Vector2 targetpos, float duration)
    {
        Vector2 startpos = scrollrect.content.anchoredPosition;
        for (float t = 0; t < 1; t += Time.unscaledDeltaTime / duration)
        {
            scrollrect.velocity = Vector2.zero;
            scrollrect.content.anchoredPosition = Vector2.LerpUnclamped(startpos, targetpos, Smooth(t));
            yield return null;
        }
        scrollrect.content.anchoredPosition = targetpos;
    }

    public static void DoAfterSeconds(this MonoBehaviour component, float seconds, System.Action oncomplete)
    {
        component.StartCoroutine(DoAfterSeconds(seconds, oncomplete));
    }

    public static IEnumerator DoAfterSeconds(float seconds, System.Action oncomplete)
    {
        yield return new WaitForSeconds(seconds);
        oncomplete?.Invoke();
    }

    public static void DoAfterSecondsRealtime(this MonoBehaviour component, float seconds, System.Action oncomplete)
    {
        component.StartCoroutine(DoAfterSecondsRealtime(seconds, oncomplete));
    }

    public static IEnumerator DoAfterSecondsRealtime(float seconds, System.Action oncomplete)
    {
        yield return new WaitForSecondsRealtime(seconds);
        oncomplete?.Invoke();
    }

    public static void DoAfterFrames(this MonoBehaviour component, int frames, System.Action oncomplete)
    {
        component.StartCoroutine(DoAfterFrames(frames, oncomplete));
    }

    public static IEnumerator DoAfterFrames(int frames, System.Action oncomplete)
    {
        for (int i = 0; i < frames; ++i)
            yield return null;
        oncomplete?.Invoke();
    }

    public static bool IsSceneActive(string s)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name.Equals(s))
                return true;
        }
        return false;
    }

    // Returns 1 if v1 is later than v2, 0 if equal, -1 if v2 later than v1
    public static int CompareAppVersions(string v1, string v2, int errorvalue)
    {
        try
        {
            if (string.IsNullOrEmpty(v1) || string.IsNullOrEmpty(v2))
                return errorvalue;

            string[] vv1 = v1.Split('.');
            string[] vv2 = v2.Split('.');

            int count = Mathf.Max(vv1.Length, vv2.Length);
            for (int i = 0; i < count; ++i)
            {
                int current = vv1.Length <= i ? 0 : int.Parse(vv1[i]);
                int compare = vv2.Length <= i ? 0 : int.Parse(vv2[i]);
                if (current > compare)
                    return 1;
                else if (current < compare)
                    return -1;
            }
            return vv1.Length.CompareTo(vv2.Length);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            throw new System.Exception(string.Format("App version format is invalid: ({0} vs {1}).", v1 ?? "'null'", v2 ?? "'null'"));
        }
    }
}
