using OrderedJson.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class CommonCommandDefiner
{
    public static void AfterMinionSummon(GameEvent gameEvent, IOJMethod method)
    {
        gameEvent.hostCard.AddProxy(ProxyEnum.AfterMinionSummon, method);
    }
}
