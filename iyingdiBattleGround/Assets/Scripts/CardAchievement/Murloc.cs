using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public partial class CardLongKeywordAchievement
{
    /// <summary>
    /// 鱼人猎潮者
    /// </summary>
    [CommonDescription("召唤一个1/1的鱼人斥候")]    
    [GoldDescription("召唤一个2/2的鱼人斥候")]
    public static bool Summon11Murloc(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("鱼人斥候", gameEvent.hostCard.isGold);
        if (targetCard != null)
        {
            gameEvent.player.board.SummonMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = targetCard.NewCard(),
                player = gameEvent.player,
            });
        }
        return false;
    }

    /// <summary>
    /// 鱼人招潮者
    /// </summary>
    [CommonDescription("如果召唤的是鱼人，便获得+1攻击力")]
    [GoldDescription("如果召唤的是鱼人,便获得+2攻击力")]
    public static bool Plus1AttackWhenSummonAllyOr2(GameEvent gameEvent)
    {
        if (gameEvent.player == gameEvent.player.board.GetPlayer(gameEvent.targetCard))
        {
            if (gameEvent.targetCard != gameEvent.hostCard && gameEvent.targetCard.IsMinionType(gameEvent.hostCard.type))
            {
                int dvalue = gameEvent.hostCard.isGold ? 2 : 1;
                gameEvent.hostCard.effectsStay.Add(new BodyPlusEffect(dvalue, 0));
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 石塘猎人
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使一个友方鱼人获得+1/+1")]
    [GoldDescription("使一个友方鱼人获得+2/+2")]
    public static bool GiveAnAllyMurloc11Or22(GameEvent gameEvent)
    {
        int dvalue = gameEvent.hostCard.isGold ? 2 : 1;
        Card card = gameEvent.player.board.ChooseTarget(gameEvent.player.board.GetMinionTargetLambda(gameEvent.player, MinionType.Murlocs, gameEvent.hostCard));
        if (card != null)
        {
            card?.effectsStay.Add(new BodyPlusEffect(dvalue, dvalue));
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 老瞎眼
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("在战场上每有一个其他友方鱼人便获得+1攻击力")]
    [GoldDescription("在战场上每有一个其他友方鱼人便获得+2攻击力")]
    public static bool Plus1AttackOr2ForEachMurloc(GameEvent gameEvent)
    {
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            if (ally == gameEvent.hostCard) continue;
            if (ally.IsMinionType(gameEvent.hostCard.type))
            {
                gameEvent.hostCard.effects.Add(new BodyPlusEffect(gameEvent.hostCard.isGold ? 2 : 1, 0));
            }
        }
        return true;
    }

    /// <summary>
    /// 领军
    /// </summary>
    [CommonDescription("你的其他鱼人获得+2攻击力")]
    [GoldDescription("你的其他鱼人获得+4攻击力")]
    public static bool PlusAlly2AttackOr4(GameEvent gameEvent)
    {
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            if (ally == gameEvent.hostCard) continue;
            if (ally.IsMinionType(gameEvent.hostCard.type))
            {
                ally.effects.Add(new BodyPlusEffect(gameEvent.hostCard.isGold ? 4 : 2, 0));
            }
        }
        return true;
    }

    /// <summary>
    /// 寒光先知
    /// </summary>
    [CommonDescription("你的其他鱼人获得+2生命值")]
    [GoldDescription("你的其他鱼人获得+4生命值")]
    public static bool PlusAlly2HealthOr4(GameEvent gameEvent)
    {
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            if (ally == gameEvent.hostCard) continue;
            if (ally.IsMinionType(gameEvent.hostCard.type))
            {
                ally.effectsStay.Add(new BodyPlusEffect(0, gameEvent.hostCard.isGold ? 4 : 2));
            }
        }
        return true;
    }

    /// <summary>
    /// 毒鳍鱼人
    /// </summary>
    [CommonDescription("使一个友方鱼人获得剧毒")]
    [GoldDescription("使一个友方鱼人获得剧毒")]
    public static bool GiveAnAllyMurlocPoisonous(GameEvent gameEvent)
    {
        Card card = gameEvent.player.board.ChooseTarget(gameEvent.player.board.GetMinionTargetLambda(gameEvent.player, MinionType.Murlocs, gameEvent.hostCard));
        if (card != null)
        {
            card?.effectsStay.Add(new KeyWordEffect(Keyword.Poisonous));
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 蛮鱼斥候
    /// </summary>
    [CommonDescription("如果你控制一个鱼人，便发现一个鱼人")]
    [GoldDescription("如果你控制一个鱼人,便发现一个鱼人,触发两次")]
    public static bool DiscoverMinionIfYouControlMinionType(GameEvent gameEvent)
    {
        bool flag = false;
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            if (ally == gameEvent.hostCard) continue;
            if (ally.IsMinionType(gameEvent.hostCard.type))
            {
                flag = true;
                break;
            }
        }
        if (flag)
        {
            gameEvent.player.board.DiscoverToHand(gameEvent.player.board.cardPile.cardPile.FilterKey(card => card.IsMinionType(gameEvent.hostCard.type) && card.name != gameEvent.hostCard.name));
            if (gameEvent.hostCard.isGold)
            {
                gameEvent.player.board.DiscoverToHand(gameEvent.player.board.cardPile.cardPile.FilterKey(card => card.IsMinionType(gameEvent.hostCard.type) && card.name != gameEvent.hostCard.name));
            }
            return true;
        }
        Debug.Log("discover finished");
        return false;
    }

    /// <summary>
    /// 拜格尔格国王
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("你的其他鱼人获得+2/+2")]
    [GoldDescription("你的其他鱼人获得+4/+4")]
    public static bool OtherAllyMurlocGain22Or44(GameEvent gameEvent)
    {
        int value = gameEvent.hostCard.isGold ? 4 : 2;
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            if (ally == gameEvent.hostCard || !ally.IsMinionType(MinionType.Murlocs)) continue;
            ally.effectsStay.Add(new BodyPlusEffect(value, value));
        }
        return true;
    }

    /// <summary>
    /// 鲭鱼圣者
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("获得圣盾")]
    [GoldDescription("获得圣盾")]
    public static bool GainDivineShieldWhenMinionDivineShieldBroken(GameEvent gameEvent)
    {
        if (gameEvent.hostCard == gameEvent.targetCard || gameEvent.player != gameEvent.player.board.GetPlayer(gameEvent.targetCard) || gameEvent.hostCard.HasKeyword(Keyword.DivineShield))
        {
            return false;
        }
        else
        {
            gameEvent.hostCard.effectsStay.Add(new KeyWordEffect(Keyword.DivineShield));
        }
        return true;
    }

    /// <summary>
    /// 邪鳍导航员
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使你的所有其他鱼人获得+1/+1")]
    [GoldDescription("使你的所有其他鱼人获得+2/+2")]
    public static bool AllyMurlocGain11Or22(GameEvent gameEvent)
    {
        int value = gameEvent.hostCard.isGold ? 2 : 1;
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            if (ally == gameEvent.hostCard) continue;
            if (ally.IsMinionType(MinionType.Murlocs))
            {

                ally.effectsStay.Add(new BodyPlusEffect(value, value));
            }
        }
        return true;
    }
}
