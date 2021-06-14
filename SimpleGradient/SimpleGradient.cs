using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SimpleGradientKey
{
    public float time;
    public Color color;
}

[System.Serializable]
public class SimpleGradient
{
    public AnimationCurve blendCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public bool hdr = false;
    public bool linear = false;
    public List<SimpleGradientKey> keys = new List<SimpleGradientKey>();
#if UNITY_EDITOR
    public int version { get; set; } = 0;
#endif

    public SimpleGradient() { }

    public void Set(Gradient gradient)
    {
        keys.Clear();
        foreach (var key in gradient.colorKeys)
        {
            keys.Add(new SimpleGradientKey()
            {
                time = key.time,
                color = key.color
            });
        }
    }

    public Color Evaluate(float t)
    {
        if (keys.Count == 0)
            return Color.red;

        if (t <= keys[0].time)
            return keys[0].color;
        if (t >= keys[keys.Count - 1].time)
            return keys[keys.Count - 1].color;

        for (int i = 1; i < keys.Count; ++i)
        {
            if (t > keys[i].time)
                continue;

            int i1 = i - 1;
            int i2 = i;
            float tt = Mathf.InverseLerp(keys[i1].time, keys[i2].time, t);
            tt = blendCurve.Evaluate(tt);
            if (linear)
                return Color.LerpUnclamped(keys[i1].color, keys[i2].color, tt);
            else
                return UnityUtils.ColorLerp(keys[i1].color, keys[i2].color, tt);
        }
        Debug.LogError("Failed to find key for SimpleGradient.");
        return Color.clear;
    }

    public void Sort()
    {
        keys.Sort((a, b) => a.time.CompareTo(b.time));
    }
}
