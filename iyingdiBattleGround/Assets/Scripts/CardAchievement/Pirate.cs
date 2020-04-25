using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public partial class CardLongKeywordAchievement
{
    /// <summary>
    /// 血帆袭击者
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("你的英雄每具有1点攻击力，便获得+1/+1")]
    [GoldDescription("你的英雄每具有1点攻击力，便获得+2/+2")]
    public static bool Gain11Or22ForEachAttackOfYourHero(GameEvent gameEvent)
    {
        int value = (gameEvent.hostCard.isGold ? 2 : 1) * gameEvent.player.hero.GetMinionBody().x;
        gameEvent.hostCard.effectsStay.Add(new BodyPlusEffect(value,value));
        return true;
    }

    /// <summary>
    /// 血帆大副
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使你的英雄获得+2攻击力")]
    [GoldDescription("使你的英雄获得+4攻击力")]
    public static bool YourHeroGain2AttackAura(GameEvent gameEvent)
    {
        gameEvent.player.hero.effects.Add(new BodyPlusEffect((gameEvent.hostCard.isGold ? 2 : 1) * 2, 0));
        return true;
    }

    /// <summary>
    /// 南海船长
    /// </summary>
    [CommonDescription("你的其他海盗获得+1/+1")]
    [GoldDescription("你的其他海盗获得+2/+2")]
    public static bool PlusAllyPirate11(GameEvent gameEvent)
    {
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            if (ally == gameEvent.hostCard) continue;
            if (ally.IsMinionType(gameEvent.hostCard.type))
            {
                ally.effects.Add(new BodyPlusEffect(gameEvent.hostCard.isGold ? 2 : 1, gameEvent.hostCard.isGold ? 2 : 1));
            }
        }
        return true;
    }
}
