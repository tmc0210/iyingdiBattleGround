using OrderedJson.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 定制委托事件
/// </summary>
public class ProxyEventDefiner: IOJMethod
{
    readonly ProxyEnum proxyEnum;

    private ProxyEventDefiner(ProxyEnum proxyEnum)
    {
        this.proxyEnum = proxyEnum;
    }

    public string Name => proxyEnum.ToString();


    public object Invoke(OJContext context, params object[] args)
    {
        if (context is GameEvent gameEvent)
        {
            if (args.Length != 1)
            {
                throw new RuntimeException($"{proxyEnum}需要1个参数，提供了{args.Length}个");
            }
            if (args[0] is IOJMethod method)
            {
                gameEvent.hostCard.AddProxy(proxyEnum, method);
            }
            else
            {
                throw new RuntimeException($"{proxyEnum}需要IOJMethod类型的参数，提供了{args[0].GetType()}个");
            }
        }
        return null;
    }

    public static Dictionary<string, IOJMethod> GetProxyEvents()
    {
        Dictionary<string, IOJMethod> map = new Dictionary<string, IOJMethod>();
        foreach (ProxyEnum proxy in Enum.GetValues(typeof(ProxyEnum)))
        {
            map.Add(proxy.ToString(), new ProxyEventDefiner(proxy));
        }
        return map;
    }
}
