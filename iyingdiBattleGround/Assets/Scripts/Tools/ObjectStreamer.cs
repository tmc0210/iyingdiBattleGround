
using BIF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ObjectStreamer
{

    public static void Test()
    {
        IPile<Card> cards = new UnfixedPile<Card>();

        // cards中所有活着的
        cards.Filter(card => !card.isDead);


        Map<Card, int> cardMaps = new Map<Card, int>();

        // 攻击力大于1的随从中有 有圣盾的随从吗？
        var judge = cardMaps
            .FilterKey(card => card.GetMinionBody().x > 1)
            .Any(card=>card.keyWords.Contains(Keyword.DivineShield));
    }


    public static List<T> Filter<T>(this ICollection<T> iterable, Func<T, bool> function)
    {
        List<T> rets = new List<T>();
        if (function != null && iterable != null)
        {
            foreach (var item in iterable)
            {
                if (function.Invoke(item))
                {
                    rets.Add(item);
                }
            }
        }
        return rets;
    }
    public static List<T> Filter<T>(this IPile<T> iterable, Func<T, bool> function)
    {
        List<T> rets = new List<T>();
        if (function != null)
        {
            foreach (var item in iterable)
            {
                if (function.Invoke(item))
                {
                    rets.Add(item);
                }
            }
        }
        return rets;
    }
    public static List<TKey> FilterKey<TKey, TValue>(this Map<TKey, TValue> iterable, Func<TKey, bool> function)
    {
        return iterable.GetKeys().Filter(function);
    }
    public static List<TValue> FilterValue<TKey, TValue>(this Map<TKey, TValue> iterable, Func<TValue, bool> function)
    {
        return iterable.GetValues().Filter(function);
    }


    /// <summary>
    /// 将一个类型的序列转化成另一个类型的序列
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <typeparam name="TTo"></typeparam>
    /// <param name="enumerator"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static List<TTo> Map<TFrom, TTo>(this IEnumerable<TFrom> enumerator, Func<TFrom, TTo> func)
    {
        List<TTo> tos = new List<TTo>();
        if (func != null)
        {
            foreach (var item in enumerator)
            {
                tos.Add(func.Invoke(item));
            }
        }
        return tos;
    }

    public static IEnumerable<(T, T)> DoubleZip<T>(this List<T> ts)
    {
        return ts.Skip(1).Zip(ts.Take(ts.Count - 1), (T f, T s) => {
            return (f, s);
        });
    }
    public static string StringJoin<TFrom>(this IEnumerable<TFrom> enumerator, string sep = ",")
    {
        string ret = "";
        foreach (var str in enumerator)
        {
            if (!string.IsNullOrEmpty(ret)) ret += sep;
            ret += str.ToString();
        }
        return ret;
    }


    /// <summary>
    /// 提取子类型
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <typeparam name="TTo"></typeparam>
    /// <param name="enumerator"></param>
    /// <returns></returns>
    public static List<TTo> Map<TFrom, TTo>(this IEnumerable<TFrom> enumerator)
    {
        List<TTo> tos = new List<TTo>();
        foreach (var item in enumerator)
        {
            if (item is TTo tto)
            {
                tos.Add(tto);
            }
        }
        return tos;
    }

    /// <summary>
    /// 遍历一个序列
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <param name="enumerator"></param>
    /// <param name="func"></param>
    public static void Map<TFrom>(this IEnumerable<TFrom> enumerator, Action<TFrom> func)
    {
        if (func != null)
        {
            foreach (var item in enumerator)
            {
                func.Invoke(item);
            }
        }
    }
    public static void Map<TFrom>(this IEnumerable<TFrom> enumerator, Action<TFrom, int> func)
    {
        if (func != null)
        {
            int i = 0;
            foreach (var item in enumerator)
            {
                func.Invoke(item, i++);
            }
        }
    }


    public static T Reduce<T>(this IEnumerable<T> ts, Func<T,T,T> func, T defaultValue = default)
    {
        if (func == null) return defaultValue;
        foreach (var item in ts)
        {
            defaultValue = func.Invoke(item, defaultValue);
        }
        return defaultValue;
    }


    /// <summary>
    /// 打乱原序列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ts"></param>
    /// <returns></returns>
    public static List<T> Shuffle<T>(this List<T> ts)
    {
        int count = ts.Count;
        for (int i = 0; i < count; i++)
        {
            int rnd = random.Next(count);
            T tmp = ts[rnd];
            ts[rnd] = ts[i];
            ts[i] = tmp;
        }
        return ts;
    }

    public static TValue GetByDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
    {
        if (dict.TryGetValue(key, out TValue value))
        {
            return value;
        }
        return defaultValue;
    }


    /// <summary>
    /// 返回一个序列的复制
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ts"></param>
    /// <returns></returns>
    public static List<T> Stream<T>(this IEnumerable<T> ts)
    {
        List<T> ret = new List<T>();
        ret.AddRange(ts);
        return ret;
    }

    /// <summary>
    /// 取序列中的前n个
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ts"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    //public static List<T> Take<T>(this IEnumerable<T> ts, int n)
    //{
    //    List<T> ret = new List<T>();
    //    int i = 0;
    //    foreach(var item in ts)
    //    {
    //        if (i++<n)
    //        {
    //            ret.Add(item);
    //        }
    //    }
    //    return ret;
    //}

    /// <summary>
    /// 是否全部满足
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="iterable"></param>
    /// <param name="function"></param>
    /// <returns></returns>
    public static bool All<T>(this IEnumerable<T> iterable, Func<T, bool> function)
    {
        if (function == null) return false;
        foreach (var item in iterable)
        {
            if (!function.Invoke(item))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 是否存在
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="iterable"></param>
    /// <param name="function"></param>
    /// <returns></returns>
    public static bool Any<T>(this IEnumerable<T> iterable, Func<T, bool> function)
    {
        if (function == null) return false;
        foreach (var item in iterable)
        {
            if (function.Invoke(item))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 获取其中一个元素（不保证是第一个）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="iterable"></param>
    /// <returns></returns>
    public static T GetOne<T>(this ICollection<T> iterable)
    {
        if (iterable.Count == 0) return default;
        foreach (var item in iterable)
        {
            return item;
        }
        return default;
    }

    private static System.Random random = new System.Random(DateTime.Now.Millisecond);
    /// <summary>
    /// 随机获取其中一个元素
    /// </summary>
    public static T GetOneRandomly<T>(this ICollection<T> iterable)
    {
        if (iterable.Count == 0) return default;
        int rnd = random.Next(iterable.Count);
        int i = 0;
        foreach(var item in iterable)
        {
            if (i++ == rnd)
            {
                return item;
            }
        }
        return default;
    }

    public static List<int> ParseListInt(this string str)
    {
        if (!string.IsNullOrEmpty(str))
        {
            var strs = str.Split(',');
            return strs.Select(num => BIFStaticTool.ParseInt(num)).ToList();
        }
        return new List<int>();
    }

    public static string Serialize(this List<int> nums)
    {
        return nums.StringJoin(",");
    }

}
