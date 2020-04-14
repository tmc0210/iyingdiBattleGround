using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CardLongKeywordAchievement
{
    /// <summary>
    /// 载人毁灭机
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("从手牌中随机召唤一个星级小于或等于2的随从")]    
    [GoldDescription("从手牌中随机召唤两个星级小于或等于2的随从")]
    public static bool Recall2StarMinionFromHand(GameEvent gameEvent)
    {
        var targetCard = gameEvent.player.handPile.Filter(card => card.cardType == CardType.Minion && card.star <= 2).GetOneRandomly();
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
                targetCard = gameEvent.player.handPile.Filter(card => card.cardType == CardType.Minion && card.star <= 2).GetOneRandomly();
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
    /// 机械袋鼠
    /// </summary>
    [CommonDescription("召唤一个1/1的机械袋鼠宝宝")]    
    [GoldDescription("召唤一个2/2的机械袋鼠宝宝")]
    public static bool Summon11MechOr22(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("机械袋鼠宝宝", gameEvent.hostCard.isGold);
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
    /// 钴制卫士
    /// </summary>
    [CommonDescription("如果召唤的是友方机械,获得圣盾")]    
    [GoldDescription("如果召唤的是友方机械,获得圣盾")]
    public static bool GainDivineShieldWhenSummonMech(GameEvent gameEvent)
    {
        if (gameEvent.hostCard.HasKeyword(Keyword.DivineShield))
        {
            return false;
        }

        if (gameEvent.targetCard != gameEvent.hostCard && gameEvent.targetCard.IsMinionType(MinionType.Mechs))
        {
            if (gameEvent.player == gameEvent.player.board.GetPlayer(gameEvent.targetCard))
            {
                gameEvent.hostCard.effectsStay.Add(new KeyWordEffect(Keyword.DivineShield));
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 爆爆机器人
    /// </summary>
    [CommonDescription("随机对一个敌方随从造成4点伤害")]    
    [GoldDescription("随机对一个敌方随从造成4点伤害,重复一次")]
    public static bool Deal4DamageToRandomEnemyMinionOrDoubleThis(GameEvent gameEvent)
    {
        int cnt = 1;
        if (gameEvent.hostCard.isGold) cnt = 2;
        for (int i = 0; i < cnt; i++)
        {
            Card targetCard = gameEvent.player.board.GetAnotherPlayer(gameEvent.player).RandomlyGetAliveMinion();
            if (targetCard != null)
            {
                gameEvent.player.board.DealDamageToMinion(new GameEvent()
                {
                    hostCard = gameEvent.hostCard,
                    targetCard = targetCard,
                    player = gameEvent.player,
                    number = 4
                });
            }
        }
        return true;
    }

    /// <summary>
    /// 麦田傀儡
    /// </summary>
    [CommonDescription("召唤一个2/1的损坏的傀儡")]    
    [GoldDescription("召唤一个4/2的损坏的傀儡")]
    public static bool Summon21MechOr42(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("损坏的傀儡", gameEvent.hostCard.isGold);
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
    /// 安保
    /// </summary>
    [CommonDescription("召唤一个2/3并具有嘲讽的机械")]    
    [GoldDescription("召唤一个4/6并具有嘲讽的机械")]
    public static bool Summon23TauntMechOr46(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("安保机器人", gameEvent.hostCard.isGold);
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
    /// 回收机器人
    /// </summary>
    [CommonDescription("如果是友方机械，则获得+2/+2")]
    [GoldDescription("如果是友方机械,则获得+4/+4")]
    public static bool Gain22WhenMechDeathOr44(GameEvent gameEvent)
    {
        if (gameEvent.targetCard.IsMinionType(MinionType.Mechs))
        {
            if (gameEvent.player == gameEvent.player.board.GetPlayer(gameEvent.targetCard))
            {
                int dvalue = gameEvent.hostCard.isGold ? 4 : 2;
                gameEvent.hostCard.effectsStay.Add(new BodyPlusEffect(dvalue, dvalue));
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 机械蛋
    /// </summary>
    [CommonDescription("召唤一个8/8的机械暴龙")]
    [GoldDescription("召唤一个16/16的机械暴龙")]
    public static bool Summon88MechOr1616(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("机械暴龙", gameEvent.hostCard.isGold);
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
    /// 废旧螺栓机甲
    /// </summary>
    [CommonDescription("使一个机械获得+2/+2")]
    [GoldDescription("使一个机械获得+4/+4")]
    public static bool AddAnAllyMech2Attack2Health(GameEvent gameEvent)
    {
        int dvalue = gameEvent.hostCard.isGold ? 4 : 2;
        Card card = gameEvent.player.board.ChooseTarget(gameEvent.player.board.GetMinionTargetLambda(gameEvent.player, MinionType.Mechs, gameEvent.hostCard));
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
    /// 普通量产型恐吓机
    /// </summary>
    [CommonDescription("召唤三个1/1的微型机器人")]
    [GoldDescription("召唤三个1/1的微型机器人")]
    public static bool SummonThree11Mech(GameEvent gameEvent)
    {
        for (int i = 0; i < 3; i++)
        {
            Card targetCard = CardBuilder.SearchCardByName("微型机器人", false);
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
    /// 金色量产型恐吓机
    /// </summary>
    [GoldDescription("召唤三个2/2的微型机器人")]
    [CommonDescription("召唤三个2/2的微型机器人")]
    public static bool SummonThree22Mech(GameEvent gameEvent)
    {
        for (int i = 0; i < 3; i++)
        {
            Card targetCard = CardBuilder.SearchCardByName("微型机器人", true);
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
    /// 金刚刃牙兽
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使你的其他机械获得+2攻击力")]
    [GoldDescription("使你的其他机械获得+4攻击力")]
    public static bool PlusAllyMech2AttackOr4(GameEvent gameEvent)
    {
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            if (ally == gameEvent.hostCard) continue;
            if (ally.IsMinionType(gameEvent.hostCard.type))
            {
                ally.effectsStay.Add(new BodyPlusEffect(gameEvent.hostCard.isGold ? 4 : 2, 0));
            }
        }
        return true;
    }

    /// <summary>
    /// 机械动物管理员
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("随机使一个友方野兽,龙,鱼人获得+1/+1")]
    [GoldDescription("随机使一个友方野兽,龙,鱼人获得+2/+2")]
    public static bool GiveAnRandomAllyMurlocBeastDragon11(GameEvent gameEvent)
    {
        int value = gameEvent.hostCard.isGold ? 2 : 1;
        List<Card> cardList = Board.FilterCardByMinionType
            (
                gameEvent.player.GetAllAllyMinion(),
                (MinionType.Beasts, 1),
                (MinionType.Dragons, 1),
                (MinionType.Murlocs, 1)
            );
        foreach (Card card in cardList)
        {
            if (card != null)
            {
                card.effectsStay.Add(new BodyPlusEffect(value, value));
            }
        }
        return true;
    }

    /// <summary>
    /// 钢铁武道家
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使另一个友方机械获得+2/+2")]
    [GoldDescription("使另一个友方机械获得+4/+4")]
    public static bool GiveAnRandomAllyMech22Or44(GameEvent gameEvent)
    {
        int value = gameEvent.hostCard.isGold ? 4 : 2;
        var targetCard = gameEvent.player.GetAllAllyMinion()
                        .Filter(card => card != gameEvent.hostCard && card.IsMinionType(MinionType.Mechs))
                        .GetOneRandomly();
        if (targetCard != null)
        {
            targetCard.effectsStay.Add(new BodyPlusEffect(value, value));
        }

        return true;
    }

    /// <summary>
    /// 湮灭战车
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("从手牌中随机召唤一个机械并触发它的亡语")]
    [GoldDescription("从手牌中随机召唤两个机械并触发它们的亡语两次")]
    public static bool SummonMechFromHandAndTriggerDeathrattle(GameEvent gameEvent)
    {
        var targetCard = gameEvent.player.handPile.Filter(card => card.cardType == CardType.Minion && card.IsMinionType(MinionType.Mechs)).GetOneRandomly();
        if (targetCard != null && gameEvent.player.battlePile.Count < Const.numOfBattlePile)
        {
            gameEvent.player.RemoveMinionFromHandPile(targetCard);
            gameEvent.player.board.SummonMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = targetCard,
                player = gameEvent.player
            });
            if (targetCard.GetProxys(ProxyEnum.Deathrattle) != null)
            {
                gameEvent.player.board.Dealthrattle(new GameEvent()
                {
                    hostCard = targetCard,
                    player = gameEvent.player.board.GetPlayer(targetCard)
                });
                if (gameEvent.hostCard.isGold)
                {
                    gameEvent.player.board.Dealthrattle(new GameEvent()
                    {
                        hostCard = targetCard,
                        player = gameEvent.player.board.GetPlayer(targetCard)
                    });
                }                
            }
            return true;
        }
        if (gameEvent.hostCard.isGold)
        {
            targetCard = gameEvent.player.handPile.Filter(card => card.cardType == CardType.Minion && card.IsMinionType(MinionType.Mechs)).GetOneRandomly();
            if (targetCard != null && gameEvent.player.battlePile.Count < Const.numOfBattlePile)
            {
                gameEvent.player.RemoveMinionFromHandPile(targetCard);
                gameEvent.player.board.SummonMinion(new GameEvent()
                {
                    hostCard = gameEvent.hostCard,
                    targetCard = targetCard,
                    player = gameEvent.player
                });
                if (targetCard.GetProxys(ProxyEnum.Deathrattle) != null)
                {
                    gameEvent.player.board.Dealthrattle(new GameEvent()
                    {
                        hostCard = targetCard,
                        player = gameEvent.player.board.GetPlayer(targetCard)
                    });
                    if (gameEvent.hostCard.isGold)
                    {
                        gameEvent.player.board.Dealthrattle(new GameEvent()
                        {
                            hostCard = targetCard,
                            player = gameEvent.player.board.GetPlayer(targetCard)
                        });
                    }
                }
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 荒疫爬行者
    /// </summary>
    [CommonDescription("召唤一个1/1并具有剧毒的软泥怪")]
    [GoldDescription("召唤一个2/2并具有剧毒的软泥怪")]
    public static bool Summon11PoisonousOr22(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("辐射软泥怪", gameEvent.hostCard.isGold);
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
    /// 陵墓守望者
    /// </summary>
    [CommonDescription("召唤一个该随从的复制")]
    [GoldDescription("召唤两个该随从的复制")]
    public static bool Summon1Or2Copy(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName(gameEvent.hostCard.name, gameEvent.hostCard.isGold).NewCard();
        targetCard.TransformToNewCardWithEffects(gameEvent.hostCard);
        if (targetCard != null)
        {
            gameEvent.player.board.SummonMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = targetCard,
                player = gameEvent.player,
            });
        }
        if (gameEvent.hostCard.isGold)
        {
            targetCard = CardBuilder.SearchCardByName(gameEvent.hostCard.name, gameEvent.hostCard.isGold).NewCard();
            targetCard.TransformToNewCardWithEffects(gameEvent.hostCard);
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
        return true;
    }

    /// <summary>
    /// 机械保险箱
    /// </summary>
    [CommonDescription("召唤一个0/5并具有嘲讽的保险柜")]
    [GoldDescription("召唤一个0/10并具有嘲讽的保险柜")]
    public static bool Summon05TauntOr010(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("保险柜", gameEvent.hostCard.isGold);
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
    /// 高级修理机器人
    /// </summary>
    [CommonDescription("使一个友方机械获得+4生命值")]
    [GoldDescription("使一个友方机械获得+8生命值")]
    public static bool AddAnAllyMech4Health(GameEvent gameEvent)
    {
        int dvalue = gameEvent.hostCard.isGold ? 8 : 4;
        Card card = gameEvent.player.board.ChooseTarget(gameEvent.player.board.GetMinionTargetLambda(gameEvent.player, MinionType.Mechs, gameEvent.hostCard));
        if (card != null)
        {
            card?.effectsStay.Add(new BodyPlusEffect(0, dvalue));
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 微型战斗机甲
    /// </summary>
    [CommonDescription("获得+1攻击力")]
    [GoldDescription("获得+2攻击力")]
    public static bool Plus1AttackOr2(GameEvent gameEvent)
    {
        if (gameEvent.player == gameEvent.player.board.GetPlayer(gameEvent.targetCard))
        {
            int dvalue = gameEvent.hostCard.isGold ? 2 : 1;
            gameEvent.hostCard.effectsStay.Add(new BodyPlusEffect(dvalue, 0));
            return true;
        }
        return false;
    }

    /// <summary>
    /// 载人收割机
    /// </summary>
    [CommonDescription("召唤一个随机的2星随从")]
    [GoldDescription("召唤两个随机的2星随从")]
    public static bool SummonRamdom2StarMinion(GameEvent gameEvent)
    {

        Card targetCard = CardBuilder.GetCardsByStar(2)
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
        if (gameEvent.hostCard.isGold)
        {
            targetCard = CardBuilder.GetCardsByStar(2)
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
    /// 载人飞天魔像
    /// </summary>
    [CommonDescription("召唤一个随机的3星随从")]
    [GoldDescription("召唤两个随机的3星随从")]
    public static bool SummonRamdom3StarMinion(GameEvent gameEvent)
    {

        Card targetCard = CardBuilder.GetCardsByStar(3)
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
        if (gameEvent.hostCard.isGold)
        {
            targetCard = CardBuilder.GetCardsByStar(3)
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
}