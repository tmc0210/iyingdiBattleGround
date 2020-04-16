
// 野兽

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using UnityEngine;

public partial class CardLongKeywordAchievement
{

    /// <summary>
    /// 魔泉山猫
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("将一张1/1并具有突袭的山猫置入你的手牌")]
    [GoldDescription("将一张2/2并具有突袭的山猫置入你的手牌")]
    public static bool Create11BeastOr22(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("山猫", gameEvent.hostCard.isGold);
        if (targetCard != null)
        {
            gameEvent.player.AddMinionToHandPile(targetCard.NewCard());
        }

        return true;
    }



    /// <summary>
    /// 雄斑虎
    /// </summary>
    [CommonDescription("召唤一个1/1的雌斑虎")]
    [GoldDescription("召唤一个2/2的雌斑虎")]
    public static bool Summon11Beast(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("雌斑虎", gameEvent.hostCard.isGold);
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
    /// 熊妈妈
    /// </summary>
    [CommonDescription("如果是友方野兽,使其获得+5/+5")]
    [GoldDescription("如果是友方野兽,使其1获得+10/+10")]
    public static bool Give55WhenSummonBeast(GameEvent gameEvent)
    {
        if (gameEvent.targetCard != gameEvent.hostCard &&
            gameEvent.targetCard.IsMinionType(MinionType.Beasts) &&
            gameEvent.player == gameEvent.player.board.GetPlayer(gameEvent.targetCard))
        {
            int value = gameEvent.hostCard.isGold?10:5;
            gameEvent.targetCard.effectsStay.Add(new BodyPlusEffect(value, value));
            return true;
        }
        return false;
    }


    /// <summary>
    /// 骑乘迅猛龙
    /// </summary>
    [CommonDescription("召唤一个随机的1星随从")]
    [GoldDescription("召唤两个随机的1星随从")]
    public static bool SummonRamdom1StarMinion(GameEvent gameEvent)
    {
    
        Card targetCard = CardBuilder.GetCardsByStar(1)
            .Filter(card=>!card.isGold && !card.isToken)
            .GetOneRandomly();
        if (targetCard != null)
        {
            gameEvent.player.board.SummonMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = targetCard.NewCard(),
                player = gameEvent.player,
            });
        }
        if (gameEvent.hostCard.isGold)
        {
            targetCard = CardBuilder.GetCardsByStar(1)
                .Filter(card => !card.isGold && !card.isToken)
                .GetOneRandomly();
            if (targetCard != null)
            {
                gameEvent.player.board.SummonMinion(new GameEvent()
                {
                    hostCard = gameEvent.hostCard,
                    targetCard = targetCard.NewCard(),
                    player = gameEvent.player,
                });
            }
        }
        return true;
    }

    /// <summary>
    /// 鼠群
    /// </summary>
    [CommonDescription("召唤等同于攻击力数目的1/1老鼠")]
    [GoldDescription("召唤等同于攻击力数目的2/2的老鼠")]
    public static bool SummonAttacksNumber11Beast(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("老鼠", gameEvent.hostCard.isGold);
        if (targetCard != null)
        {
            int attack = gameEvent.hostCard.GetMinionBody().x;
            for (int i = 0; i < attack; i++)
            {
                gameEvent.player.board.SummonMinion(new GameEvent()
                {
                    hostCard = gameEvent.hostCard,
                    targetCard = targetCard.NewCard(),
                    player = gameEvent.player,
                });
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 食腐土狼
    /// </summary>
    [CommonDescription("如果是友方野兽，则获得+2/+1")]
    [GoldDescription("如果是友方野兽,则获得+4/+2")]
    public static bool Gain21WhenBeastDeathOr42(GameEvent gameEvent)
    {
        if (gameEvent.targetCard.IsMinionType(MinionType.Beasts))
        {
            if (gameEvent.player == gameEvent.player.board.GetPlayer(gameEvent.targetCard))
            {
                int dvalue = gameEvent.hostCard.isGold ? 4 : 2;
                gameEvent.hostCard.effectsStay.Add(new BodyPlusEffect(dvalue, dvalue/2));
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 大蛇
    /// </summary>
    [CommonDescription("召唤两个随机亡语随从")]
    [GoldDescription("召唤四个随机亡语随从")]
    public static bool Summon2DeathrattleMinion(GameEvent gameEvent)
    {
        int times = gameEvent.hostCard.isGold ? 4 : 2;
        for (int i = 0; i < times; i++)
        {
            Card targetCard = CardBuilder.AllCards
                .FilterValue(card => !card.isToken && !card.isGold
                    && card.GetProxys(ProxyEnum.Deathrattle) != null 
                    && !card.name.Equals(gameEvent.hostCard.name))
                .GetOneRandomly();
    
            if (targetCard != null)
            {
                gameEvent.player.board.SummonMinion(new GameEvent()
                {
                    hostCard = gameEvent.hostCard,
                    targetCard = targetCard.NewCard(),
                    player = gameEvent.player,
                });
            }
        }
        return false;
    }

    /// <summary>
    /// 恐狼前锋
    /// </summary>
    [CommonDescription("相邻随从获得+1攻击力")]
    [GoldDescription("相邻随从获得+2攻击力")]
    public static bool AdjacentMinionGein1Attack(GameEvent gameEvent)
    {
        int dvalue = gameEvent.hostCard.isGold ? 2 : 1;
        var index = gameEvent.player.GetMinionIndex(gameEvent.hostCard);
        Tuple<Card, Card> tuple = gameEvent.player.board.GetAdjacentMinion(gameEvent.hostCard);
        tuple.Item1?.effects.Add(new BodyPlusEffect(dvalue, 0));
        tuple.Item2?.effects.Add(new BodyPlusEffect(dvalue, 0));
        return false;
    }

    /// <summary>
    /// 狼王戈德林
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使你的野兽获得+4/+4")]
    [GoldDescription("使你的野兽获得+8/+8")]
    public static bool OtherAllyBeastGain44Or88(GameEvent gameEvent)
    {
        int value = gameEvent.hostCard.isGold ? 8 : 4;
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            if (ally == gameEvent.hostCard || !ally.IsMinionType(MinionType.Beasts)) continue;
            ally.effectsStay.Add(new BodyPlusEffect(value, value));
        }
        return true;
    }

    [CommonDescription("精准选择敌方随从造成7点伤害")]
    [GoldDescription("精准选择敌方随从造成14点伤害")]
    public static bool Deal5DemageToSelectedMinion(GameEvent gameEvent)
    {
        Card card = gameEvent.player.board.ChooseTarget
            (
                gameEvent.player.board.GetMinionTargetLambda(gameEvent.player.board.GetAnotherPlayer(gameEvent.player), MinionType.Any, null)
            );
            
        if (card != null)
        {
            gameEvent.player.board.HurtMinion(new GameEvent() {
                hostCard = gameEvent.hostCard,
                targetCard = card,
                number = gameEvent.hostCard.isGold?14:7,
                player =gameEvent.player,
            });
            return true;
        }

        return false;
    }

    /// <summary>
    /// 长鬃草原狮
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("召唤两个2/2的土狼")]
    [GoldDescription("召唤两个4/4的土狼")]
    public static bool SummonTwo22Beast(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("土狼", gameEvent.hostCard.isGold);
        if (targetCard != null)
        {
            gameEvent.player.board.SummonMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = targetCard.NewCard(),
                player = gameEvent.player,
            });
            gameEvent.player.board.SummonMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = targetCard.NewCard(),
                player = gameEvent.player,
            });
        }
        return true;
    }

    /// <summary>
    /// 温顺的巨壳龙
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("进化你的鱼人")]
    [GoldDescription("进化你的鱼人两次")]
    public static bool EvolveAllAllyMurloc(GameEvent gameEvent)
    {
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            if (ally == gameEvent.hostCard || !ally.IsMinionType(MinionType.Murlocs)) continue;
            gameEvent.player.board.Evolve(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = ally,
            });
        }
        return true;
    }

    /// <summary>
    /// 强能箭猪
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("造成等同于该随从攻击力的伤害,随机分配到所有敌方随从身上")]
    [GoldDescription("造成等同于该随从攻击力两倍的伤害,随机分配到所有敌方随从身上")]
    public static bool DealAttacksNumberDamage(GameEvent gameEvent)
    {
        int num = gameEvent.hostCard.GetMinionBody().x * (gameEvent.hostCard.isGold ? 2 : 1);
        for (int i = 0; i < num; i++)
        {
            Card targetCard = gameEvent.player.board.GetAnotherPlayer(gameEvent.player).RandomlyGetAliveMinion();
            if (targetCard != null)
            {
                gameEvent.player.board.HurtMinion(new GameEvent()
                {
                    hostCard = gameEvent.hostCard,
                    targetCard = targetCard,
                    player = gameEvent.player,
                    number = 1
                });
            }
        }
        return true;
    }

    /// <summary>
    /// 苍绿长颈龙
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("进化一次")]
    [GoldDescription("进化两次")]
    public static bool EvolveSelfOrTwice(GameEvent gameEvent)
    {
        int num = gameEvent.hostCard.isGold ? 2 : 1;
        for (int i = 0; i < num; i++)
        {
            gameEvent.player.board.Evolve(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = gameEvent.hostCard,
            });
        }
        return true;
    }

    /// <summary>
    /// 巨型蟒蛇
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("从手牌中随机召唤一个攻击力大于或等于5的随从")]    
    [GoldDescription("从手牌中随机召唤两个攻击力大于或等于5的随从")]
    public static bool Recall5AttackMinionFromHand(GameEvent gameEvent)
    {
        var targetCard = gameEvent.player.handPile.Filter(card => card.cardType == CardType.Minion && card.GetMinionBody().x >= 5).GetOneRandomly();
        if (targetCard != null && gameEvent.player.battlePile.Count < Const.numOfBattlePile)
        {
            gameEvent.player.RemoveMinionFromHandPile(targetCard);
            gameEvent.player.board.SummonMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = targetCard,
                player = gameEvent.player
            });

            if (gameEvent.hostCard.isGold)
            {
                targetCard = gameEvent.player.handPile.Filter(card => card.cardType == CardType.Minion && card.GetMinionBody().x >= 5).GetOneRandomly();
                if (targetCard != null && gameEvent.player.battlePile.Count < Const.numOfBattlePile)
                {
                    gameEvent.player.RemoveMinionFromHandPile(targetCard);
                    gameEvent.player.board.SummonMinion(new GameEvent()
                    {
                        hostCard = gameEvent.hostCard,
                        targetCard = targetCard,
                        player = gameEvent.player
                    });
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 慈祥的外婆
    /// </summary>
    [CommonDescription("召唤一只3/2的大灰狼")]
    [GoldDescription("召唤一只6/4的大灰狼")]
    public static bool Summon32BeastOr64(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("大灰狼", gameEvent.hostCard.isGold);
        if (targetCard != null)
        {
            gameEvent.player.board.SummonMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = (Card)targetCard.NewCard(),
                player = gameEvent.player,
            });
        }
        return false;
    }

    /// <summary>
    /// 腐化灰熊
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使其获得+1/+1")]
    [GoldDescription("使其获得+2/+2")]
    public static bool Give11Or22WhenSummonMinion(GameEvent gameEvent)
    {
        if (gameEvent.targetCard != gameEvent.hostCard)
        {
            int value = gameEvent.hostCard.isGold ? 2 : 1;
            if (gameEvent.player == gameEvent.player.board.GetPlayer(gameEvent.targetCard))
            {
                gameEvent.targetCard.effectsStay.Add(new BodyPlusEffect(value, value));
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 地穴领主
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("获得+1生命值")]
    [GoldDescription("获得+2生命值")]
    public static bool GainHealthWhenSummonMinion(GameEvent gameEvent)
    {
        if (gameEvent.targetCard != gameEvent.hostCard)
        {
            if (gameEvent.player == gameEvent.player.board.GetPlayer(gameEvent.targetCard))
            {
                gameEvent.hostCard.effectsStay.Add(new BodyPlusEffect(0, gameEvent.hostCard.isGold ? 2 : 1));
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 狂野兽王
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使酒馆中正在出售的所有野兽获得+2/+2")]
    [GoldDescription("使酒馆中正在出售的所有野兽获得+4/+4")]
    public static bool BeastOnSaleGain22Or44(GameEvent gameEvent)
    {
        int value = gameEvent.hostCard.isGold ? 4 : 2;
        bool flag = false;
        foreach (Card ally in gameEvent.player.board.GetAnotherPlayer(gameEvent.player).GetAllAllyMinion())
        {
            if (ally == gameEvent.hostCard) continue;
            if (ally.IsMinionType(MinionType.Beasts))
            {
                ally.effectsStay.Add(new BodyPlusEffect(value, value));
                flag = true;
            }
        }
        return flag;
    }

    /// <summary>
    /// 铁皮恐角龙
    /// </summary>
    [CommonDescription("召唤一只5/5的铁皮小恐龙")]
    [GoldDescription("召唤一只10/10的铁皮小恐龙")]
    public static bool Summon55BeastOr1010(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("铁皮小恐龙", gameEvent.hostCard.isGold);
        if (targetCard != null)
        {
            gameEvent.player.board.SummonMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = (Card)targetCard.NewCard(),
                player = gameEvent.player,
            });
        }
        return false;
    }

    /// <summary>
    /// 哈多诺克斯
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("召唤所有你在本场战斗中死亡的具有嘲讽的随从")]
    [GoldDescription("召唤所有你在本场战斗中死亡的具有嘲讽的随从")]
    public static bool SummonAllDeadTauntMinion(GameEvent gameEvent)
    {
        List<Card> summons = new List<Card>();
        foreach (var id in gameEvent.player.deathMinionCollection)
        {
            if (summons.Count >= 7) break;
            Card card = CardBuilder.GetCard(id);
            if (card.HasKeyword(Keyword.Taunt))
            {
                summons.Add(card.NewCard());
            }
        }
        var byCard = gameEvent.hostCard;
        foreach (var card in summons)
        {
            gameEvent.player.board.SummonMinionByMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = card,
                player = gameEvent.player
            }, byCard);
            byCard = card;
        }
        return true;
    }

    /// <summary>
    /// 寄生恶狼
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("召唤两个1/1的蜘蛛")]
    [GoldDescription("召唤两个2/2的蜘蛛")]
    public static bool SummonTwo11Beast(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("蜘蛛", gameEvent.hostCard.isGold);
        if (targetCard != null)
        {
            gameEvent.player.board.SummonMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = targetCard.NewCard(),
                player = gameEvent.player,
            });
            gameEvent.player.board.SummonMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = targetCard.NewCard(),
                player = gameEvent.player,
            });
        }
        return true;
    }
}