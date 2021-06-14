using System.Collections;
using UnityEngine;

public enum TweenType
{
    Linear,
    EaseIn,
    EaseOut,
    EaseInOut
}

public static class TweenUtils
{
    public static void Tween(this MonoBehaviour caller, float start, float end, float duration, TweenType type, System.Action<float> onapply, System.Action oncomplete = null)
    {
        Tween(caller, duration, type, t => onapply(Mathf.LerpUnclamped(start, end, t)), oncomplete);
    }

    public static void Tween(this MonoBehaviour caller, Vector2 start, Vector2 end, float duration, TweenType type, System.Action<Vector2> onapply, System.Action oncomplete = null)
    {
        Tween(caller, duration, type, t => onapply(Vector2.LerpUnclamped(start, end, t)), oncomplete);
    }

    public static void Tween(this MonoBehaviour caller, Vector3 start, Vector3 end, float duration, TweenType type, System.Action<Vector3> onapply, System.Action oncomplete = null)
    {
        Tween(caller, duration, type, t => onapply(Vector3.LerpUnclamped(start, end, t)), oncomplete);
    }

    public static void Tween(this MonoBehaviour caller, Vector4 start, Vector4 end, float duration, TweenType type, System.Action<Vector4> onapply, System.Action oncomplete = null)
    {
        Tween(caller, duration, type, t => onapply(Vector4.LerpUnclamped(start, end, t)), oncomplete);
    }

    public static void Tween(this MonoBehaviour caller, Color start, Color end, float duration, TweenType type, System.Action<Color> onapply, System.Action oncomplete = null)
    {
        Tween(caller, duration, type, t => onapply(Color.LerpUnclamped(start, end, t)), oncomplete);
    }

    public static void Tween(this MonoBehaviour caller, float duration, TweenType type, System.Action<float> onapply, System.Action oncomplete = null)
    {
        caller.StartCoroutine(TweenAsync(duration, type, onapply, oncomplete));
    }

    public static IEnumerator TweenAsync(float duration, TweenType type, System.Action<float> onapply, System.Action oncomplete = null)
    {
        for (float t = 0; t < 1; t += Time.unscaledDeltaTime / duration)
        {
            onapply(TimeFromTweenType(t, type));
            yield return null;
        }
        onapply(1);
        oncomplete?.Invoke();
    }

    static float TimeFromTweenType(float t, TweenType type)
    {
        switch (type)
        {
            case TweenType.Linear:
                return t;
            case TweenType.EaseIn:
                return t * t;
            case TweenType.EaseOut:
                return Mathf.Sqrt(t);
            case TweenType.EaseInOut:
                return UnityUtils.Smooth(t);
        }

        Debug.LogError("Unknown TweenType: " + type.ToString());
        return t;
    }
}
