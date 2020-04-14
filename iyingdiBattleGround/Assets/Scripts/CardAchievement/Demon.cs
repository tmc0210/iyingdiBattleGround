using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 恶魔

public partial class CardLongKeywordAchievement
{
    /// <summary>
    /// 虚空领主
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("召唤三个1/3并具有嘲讽的虚空行者")]
    [GoldDescription("召唤三个2/6并具有嘲讽的虚空行者")]
    public static bool SummonThree13Demon(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("虚空行者", gameEvent.hostCard.isGold);
        for (int i = 0; i < 3; i++)
        {
            if (targetCard != null)
            {
                gameEvent.player.board.SummonMinion(new GameEvent()
                {
                    hostCard = gameEvent.hostCard,
                    targetCard = (Card)targetCard.NewCard(),
                    player = gameEvent.player,
                });
            }
        }
        return false;
    }

    /// <summary>
    /// 小鬼囚徒
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("召唤一个1/1的小鬼")]
    public static bool Summon11Demon(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("小鬼", false);
        if (targetCard != null)
        {
            gameEvent.player.board.SummonMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = (Card)targetCard.NewCard(),
                player = gameEvent.player,
            });
            return true;
        }
        return false;
    }

    /// <summary>
    /// 小鬼囚徒
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [GoldDescription("召唤一个2/2的小鬼")]
    public static bool Summon22Demon(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("小鬼", true);
        if (targetCard != null)
        {
            gameEvent.player.board.SummonMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = (Card)targetCard.NewCard(),
                player = gameEvent.player,
            });
            return true;
        }
        return false;
    }

    /// <summary>
    /// 邪魔仆从
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使一个随机友方随从获得该随从的攻击力")]
    [GoldDescription("使一个随机友方随从获得该随从的攻击力,触发两次")]
    public static bool GiveAnAllyAttack(GameEvent gameEvent)
    {
        var targetCard = gameEvent.player.GetAllAllyMinion()
            .Filter(card => card != gameEvent.hostCard)
            .GetOneRandomly();

        if (targetCard != null)
        {
            targetCard.effectsStay.Add(new BodyPlusEffect(gameEvent.hostCard.GetMinionBody().x, 0));
        }

        if (gameEvent.hostCard.isGold)
        {
            targetCard = gameEvent.player.GetAllAllyMinion()
                .Filter(card => card != gameEvent.hostCard)
                .GetOneRandomly();

            if (targetCard != null)
            {
                targetCard.effectsStay.Add(new BodyPlusEffect(gameEvent.hostCard.GetMinionBody().x, 0));
            }
        }

        return true;
    }

    /// <summary>
    /// 纳斯雷兹姆监工
    /// </summary>
    [CommonDescription("使一个友方恶魔获得+2/+2")]
    [GoldDescription("使一个友方恶魔获得+4/+4")]
    public static bool AddAnAllyDemon2Attack2Health(GameEvent gameEvent)
    {
        int dvalue = gameEvent.hostCard.isGold ? 4 : 2;
        Card card = gameEvent.player.board.ChooseTarget(gameEvent.player.board.GetMinionTargetLambda(gameEvent.player, MinionType.Demons, gameEvent.hostCard));
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
    /// 二王
    /// </summary>
    [CommonDescription("你的其他恶魔获得+2/+2")]
    [GoldDescription("你的其他恶魔获得+4/+4")]
    public static bool AllyDemonGain2Attack2Health(GameEvent gameEvent)
    {
        int dvalue = gameEvent.hostCard.isGold ? 4 : 2;
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            if (ally == gameEvent.hostCard) continue;
            if (ally.IsMinionType(MinionType.Demons))
            {
                ally.effects.Add(new BodyPlusEffect(dvalue, dvalue));
            }
        }
        return true;
    }

    /// <summary>
    /// 二王
    /// </summary>
    [CommonDescription("你的英雄具有免疫")]
    [GoldDescription("你的英雄具有免疫")]
    public static bool AllyHeroGainImmune(GameEvent gameEvent)
    {
        gameEvent.player.hero.effects.Add(new KeyWordEffect(Keyword.Immune));
        return true;
    }

    /// <summary>
    /// 漂浮观察者
    /// </summary>
    [CommonDescription("便获得+2/+2")]
    [GoldDescription("便获得+4/+4")]
    public static bool Gain22Or44AfterHeroHurt(GameEvent gameEvent)
    {
        if (gameEvent.player == gameEvent.player.board.GetPlayer(gameEvent.targetCard))
        {
            int dvalue = gameEvent.hostCard.isGold ? 4 : 2;
            gameEvent.hostCard.effectsStay.Add(new BodyPlusEffect(dvalue, dvalue));
            return true;
        }
        return false;
    }

    /// <summary>
    /// 黑眼
    /// </summary>
    [CommonDescription("仅在本回合中,获得一枚铸币")]
    [GoldDescription("仅在本回合中,获得两枚铸币")]
    public static bool Gain1Or2Coins(GameEvent gameEvent)
    {
        if (gameEvent.player == gameEvent.player.board.GetPlayer(gameEvent.targetCard))
        {
            int dvalue = gameEvent.hostCard.isGold ? 2 : 1;
            gameEvent.player.GetCoin(dvalue);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 攻城恶魔
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("你的其他恶魔获得+1攻击力")]
    [GoldDescription("你的其他恶魔获得+2攻击力")]
    public static bool PlusAlly1AttackOr2(GameEvent gameEvent)
    {
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            if (ally == gameEvent.hostCard) continue;
            if (ally.IsMinionType(gameEvent.hostCard.type))
            {
                ally.effects.Add(new BodyPlusEffect(gameEvent.hostCard.isGold ? 2 : 1, 0));
            }
        }
        return true;
    }

    /// <summary>
    /// 安尼赫兰战场军官
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("你的英雄每受到一点伤害，便获得+1生命值")]
    [GoldDescription("你的英雄每受到一点伤害，便获得+2生命值")]
    public static bool Gain1Or2HealthForEachHealthYourHEroLost(GameEvent gameEvent)
    {
        int maxHealth = CardBuilder.GetCard(gameEvent.player.hero.id).health + gameEvent.player.hero.GetExtraBody().y;
        gameEvent.hostCard.effectsStay.Add(new BodyPlusEffect(0,
               (gameEvent.hostCard.isGold ? 2 : 1) * (maxHealth - gameEvent.player.hero.GetMinionBody().y)));
        return true;
    }

    /// <summary>
    /// 粗俗的矮劣魔
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("对你的英雄造成2点伤害")]
    [GoldDescription("对你的英雄造成2点伤害")]
    public static bool Deal2DamageToYourHero(GameEvent gameEvent)
    {
        gameEvent.player.board.DealDamageToHero(new GameEvent()
        {
            hostCard = gameEvent.hostCard,
            targetCard = gameEvent.player.hero,
            player = gameEvent.player,
            number = 2
        });
        return true;
    }

    /// <summary>
    /// 鲜血女巫
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("对你的英雄造成1点伤害")]
    [GoldDescription("对你的英雄造成1点伤害")]
    public static bool Deal1DamageToYourHero(GameEvent gameEvent)
    {
        gameEvent.player.board.DealDamageToHero(new GameEvent()
        {
            hostCard = gameEvent.hostCard,
            targetCard = gameEvent.player.hero,
            player = gameEvent.player,
            number = 1
        });
        return true;
    }

    /// <summary>
    /// 小鬼妈妈
    /// </summary>
    [CommonDescription("随机召唤一个恶魔并使其获得嘲讽")]
    [GoldDescription("随机召唤两个恶魔并使其获得嘲讽")]
    public static bool Summon1Or2DemonAndGiveItTaunt(GameEvent gameEvent)
    {
        int times = gameEvent.hostCard.isGold ? 2 : 1;
        for (int i = 0; i < times; i++)
        {
            Card targetCard = CardBuilder.AllCards
                .FilterValue(card => !card.isToken && !card.isGold
                    && card.IsMinionType(MinionType.Demons)
                    && !card.name.Equals(gameEvent.hostCard.name))
                .GetOneRandomly().NewCard();

            targetCard.effectsStay.Add(new KeyWordEffect(Keyword.Taunt));

            if (targetCard != null)
            {
                gameEvent.player.board.SummonMinion(new GameEvent()
                {
                    hostCard = gameEvent.hostCard,
                    targetCard = targetCard,
                    player = gameEvent.player,
                });
            }
        }
        return false;
    }

    /// <summary>
    /// 流放者奥图里斯
    /// </summary>
    [CommonDescription("如果是两排战线的两侧位置且在战斗中，对所有敌方随从造成1点伤害")]
    [GoldDescription("如果是两排战线的两侧位置且在战斗中，对所有敌方随从造成2点伤害")]
    public static bool Deal1Or2DamageToAllEnemyIfOutCast(GameEvent gameEvent)
    {
        if (gameEvent.player.board.isBattleField)
        {
            if (gameEvent.targetCard != gameEvent.hostCard &&
                gameEvent.player.board.IsOutCast(gameEvent.targetCard) &&
                gameEvent.player == gameEvent.player.board.GetPlayer(gameEvent.targetCard)
                )
            {
                List<GameEvent> gameEvents = new List<GameEvent>();
                foreach (Card minion in gameEvent.player.board.GetAnotherPlayer(gameEvent.player).GetAllAllyMinion())
                {
                    gameEvents.Add(new GameEvent()
                    {
                        hostCard = gameEvent.hostCard,
                        targetCard = minion,
                        player = gameEvent.player.board.GetPlayer(gameEvent.hostCard),
                        number = gameEvent.hostCard.isGold ? 2 : 1
                    });
                }
                if (gameEvent.player == gameEvent.player.board.players[0])
                {
                    gameEvent.player.board.AOE(gameEvents, AOEType.Up);
                }
                else
                {
                    gameEvent.player.board.AOE(gameEvents, AOEType.Down);
                }
                return true;
            }
        }
        return false;
    }
}