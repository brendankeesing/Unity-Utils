using System.Collections.Generic;
using System.Linq;

public static class CsUtils
{
    public static void Swap<T>(ref T a, ref T b)
    {
        T temp = a;
        a = b;
        b = temp;
    }

    public static T Find<T>(this IEnumerable<T> list, System.Func<T, bool> test, T fail = default)
    {
        foreach (T i in list)
        {
            if (test(i))
                return i;
        }
        return fail;
    }

    public static int FindIndex<T>(this IEnumerable<T> list, System.Func<T, bool> test)
    {
        for (int i = 0; i < list.Count(); ++i)
        {
            if (test(list.ElementAt(i)))
                return i;
        }
        return -1;
    }

    public static bool Exists<T>(this IEnumerable<T> list, System.Func<T, bool> test)
    {
        foreach (T i in list)
        {
            if (test(i))
                return true;
        }
        return false;
    }

    public static bool Contains<T>(this IEnumerable<T> list, T item)
    {
        foreach (T i in list)
        {
            if (i.Equals(item))
                return true;
        }
        return false;
    }

    public static void ForEach<T>(this IEnumerable<T> list, System.Action<T> test)
    {
        foreach (T i in list)
            test(i);
    }

    public static void Sort<T>(this T[] list, System.Comparison<T> comparison)
    {
        System.Array.Sort(list, comparison);
    }

    public static void Sort<T>(this T[] list)
    {
        System.Array.Sort(list);
    }

    public static int RemoveDuplicates<T>(this IList<T> list)
    {
        int count = 0;
        for (int i = 0; i < list.Count; ++i)
        {
            for (int j = i + 1; j < list.Count; ++j)
            {
                if (list[j].Equals(list[i]))
                {
                    list.RemoveAt(j);
                    ++count;
                }
            }
        }
        return count;
    }

    public static T[] Copy<T>(this T[] array)
    {
        T[] newarray = new T[array.Length];
        System.Array.Copy(array, newarray, array.Length);
        return newarray;
    }

    public static void Resize<T>(this List<T> list, int count, T defaultvalue = default)
    {
        if (list.Count > count)
            list.RemoveRange(count, list.Count - count);
        else
        {
            while (list.Count < count)
                list.Add(defaultvalue);
        }
    }

    public static int IndexOf<T>(this T[] arr, T t)
    {
        return System.Array.IndexOf(arr, t);
    }

    public static int IndexOf(this System.Text.StringBuilder sb, char character, int startidx = 0, int endidx = -1)
    {
        if (endidx == -1)
            endidx = sb.Length;

        for (int i = startidx; i < endidx; ++i)
        {
            if (sb[i] == character)
                return i;
        }
        return -1;
    }

    public static int IndexOf(this System.Text.StringBuilder sb, string text, int startidx = 0, int endidx = -1)
    {
        if (endidx == -1)
            endidx = sb.Length;

        for (int i = startidx; i <= endidx - text.Length; ++i)
        {
            bool found = true;
            for (int j = 0; j < text.Length; ++j)
            {
                if (sb[i + j] == text[j])
                    continue;

                found = false;
                break;
            }

            if (found)
                return i;
        }
        return -1;
    }
}
