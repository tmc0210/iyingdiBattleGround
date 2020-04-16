using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public partial class CardLongKeywordAchievement
{
    /// <summary>
    /// 坎格尔的学徒
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("召唤本次对战中死亡最先死亡的两个友方机械")]
    [GoldDescription("召唤本次对战中死亡最先死亡的四个友方机械")]
    public static bool SummonFirst2DeathMech(GameEvent gameEvent)
    {
        List<Card> summons = new List<Card>();
        foreach (var id in gameEvent.player.deathMinionCollection)
        {
            if (summons.Count >= (gameEvent.hostCard.isGold ? 4 : 2)) break;
            Card card = CardBuilder.GetCard(id);
            if (card.IsMinionType(MinionType.Mechs))
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
    /// 无私的英雄
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使一个其他随机友方随从获得圣盾")]
    [GoldDescription("使两个其他随机友方随从获得圣盾")]
    public static bool GiveOneOrTwoOtherAllyDivineShield(GameEvent gameEvent)
    {
        var targetCard = gameEvent.player.GetAllAllyMinion()
            .Filter(card => card != gameEvent.hostCard && !card.HasKeyword(Keyword.DivineShield))
            .GetOneRandomly();

        if (targetCard != null)
        {
            targetCard.effectsStay.Add(new KeyWordEffect(Keyword.DivineShield));
        }

        if (gameEvent.hostCard.isGold)
        {
            targetCard = gameEvent.player.GetAllAllyMinion()
                .Filter(card => !card.HasKeyword(Keyword.DivineShield)).GetOneRandomly();

            if (targetCard != null)
            {
                targetCard.effectsStay.Add(new KeyWordEffect(Keyword.DivineShield));
            }
        }

        return true;
    }

    /// <summary>
    /// 阿古斯
    /// </summary>
    [CommonDescription("使相邻的友方随从获得+1/+1和嘲讽")]
    [GoldDescription("使相邻的友方随从获得+2/+2和嘲讽")]
    public static bool AdjacentMinionGain1Attack1HealthTaunt(GameEvent gameEvent)
    {
        int dvalue = gameEvent.hostCard.isGold ? 2 : 1;
        Tuple<Card, Card> tuple = gameEvent.player.board.GetAdjacentMinion(gameEvent.hostCard);
        tuple.Item1?.effectsStay.Add(new BodyPlusEffect(dvalue, dvalue));
        tuple.Item2?.effectsStay.Add(new BodyPlusEffect(dvalue, dvalue));
        tuple.Item1?.effectsStay.Add(new KeyWordEffect(Keyword.Taunt));
        tuple.Item2?.effectsStay.Add(new KeyWordEffect(Keyword.Taunt));
        return false;
    }

    /// <summary>
    /// 灵魂杂耍者
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("如果是友方恶魔,对一个随机敌方随从造成3点伤害")]
    [GoldDescription("如果是友方恶魔,对一个随机敌方随从造成3点伤害,触发两次")]
    public static bool Deal3DamageToRandomEnemyMinionWhenDemonDie(GameEvent gameEvent)
    {
        if (gameEvent.player != gameEvent.player.board.GetPlayer(gameEvent.targetCard)) return false;
        if (!gameEvent.targetCard.IsMinionType(MinionType.Demons)) return false;

        Card targetCard = gameEvent.player.board.GetAnotherPlayer(gameEvent.player).RandomlyGetAliveMinion();
        if (targetCard != null)
        {
            gameEvent.player.board.DealDamageToMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = targetCard,
                player = gameEvent.player,
                number = 3
            });
        }
        if (gameEvent.hostCard.isGold)
        {
            targetCard = gameEvent.player.board.GetAnotherPlayer(gameEvent.player).RandomlyGetAliveMinion();
            if (targetCard != null)
            {
                gameEvent.player.board.DealDamageToMinion(new GameEvent()
                {
                    hostCard = gameEvent.hostCard,
                    targetCard = targetCard,
                    player = gameEvent.player,
                    number = 3
                });
            }
        }
        return true;
    }

    /// <summary>
    /// 食尸鬼
    /// </summary>
    [CommonDescription("对所有随从造成1点伤害")]
    [GoldDescription("对所有随从造成2点伤害")]
    public static bool Deal1Or2DamageToAllMinion(GameEvent gameEvent)
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
        foreach (Card minion in gameEvent.player.GetAllAllyMinion())
        {
            gameEvents.Add(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = minion,
                player = gameEvent.player.board.GetPlayer(gameEvent.hostCard),
                number = gameEvent.hostCard.isGold ? 2 : 1
            });
        }
        gameEvent.player.board.AOE(gameEvents);
        return true;
    }

    /// <summary>
    /// 瑞文
    /// </summary>
    [CommonDescription("你的亡语触发两次")]
    [GoldDescription("你的亡语触发三次")]
    public static bool DouleDeathrattle(GameEvent gameEvent)
    {
        if (gameEvent.hostCard.isGold)
        {
            gameEvent.player.hero.effects.Add(new TripleDeathrattleEffect());
        }
        else
        {
            gameEvent.player.hero.effects.Add(new DouleDeathrattleEffect());
        }
        return true;
    }

    /// <summary>
    /// 铜须
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("你的战吼触发两次")]
    [GoldDescription("你的战吼触发三次")]
    public static bool DouleBattlecry(GameEvent gameEvent)
    {
        if (gameEvent.hostCard.isGold)
        {
            gameEvent.player.hero.effects.Add(new TripleBattlecryEffect());
        }
        else
        {
            gameEvent.player.hero.effects.Add(new DouleBattlecryEffect());
        }
        return true;
    }

    /// <summary>
    /// 安布拉
    /// </summary>
    [CommonDescription("如果在战斗阶段，触发它的亡语")]
    [GoldDescription("如果在战斗阶段，触发它的亡语两次")]
    public static bool TriggerDealthrattle(GameEvent gameEvent)
    {
        if (gameEvent.player.board.isBattleField)
        {
            if ((gameEvent.targetCard != gameEvent.hostCard && gameEvent.player == gameEvent.player.board.GetPlayer(gameEvent.targetCard)))
            {
                if (gameEvent.targetCard.GetProxys(ProxyEnum.Deathrattle) != null)
                {
                    gameEvent.player.board.Dealthrattle(new GameEvent()
                    {
                        hostCard = gameEvent.targetCard,
                        player = gameEvent.player.board.GetPlayer(gameEvent.targetCard)
                    });
                    if (gameEvent.hostCard.isGold)
                    {
                        gameEvent.player.board.Dealthrattle(new GameEvent()
                        {
                            hostCard = gameEvent.targetCard,
                            player = gameEvent.player.board.GetPlayer(gameEvent.targetCard)
                        });
                    }
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 塔隆·血魔
    /// </summary>
    [CommonDescription("移除所有其他友方随从并获得亡语:重新召唤他们并使其获得+1/+1")]
    [GoldDescription("移除所有其他友方随从并获得亡语:重新召唤他们两次并使其获得+2/+2")]
    public static bool EatAllAllyThenReSummon(GameEvent gameEvent)
    {
        var cards = gameEvent.player.GetAllAllyMinion().Filter(card => card != gameEvent.hostCard);
        var ids = cards.Map(card => card.id);
        cards.Map(gameEvent.player.RemoveMinionFromBattlePile);

        gameEvent.hostCard.effectsStay.Add(new ProxyEffect(ProxyEnum.Deathrattle, gameEvent2 =>
        {

            bool isGold = gameEvent2.hostCard.isGold;
            int dValue = isGold ? 2 : 1;
            if (true)
            {
                Card byCard = gameEvent2.hostCard;
                foreach (var id in ids)
                {
                    Card newCard = CardBuilder.NewCard(id);
                    newCard.effectsStay.Add(new BodyPlusEffect(dValue, dValue));
                    gameEvent2.player.board.SummonMinionByMinion(new GameEvent()
                    {
                        hostCard = gameEvent2.hostCard,
                        targetCard = newCard,
                        player = gameEvent2.player,
                    }, byCard);
                    byCard = newCard;
                }
            }
            if (isGold)
            {
                Card byCard = gameEvent2.hostCard;
                foreach (var id in ids)
                {
                    Card newCard = CardBuilder.NewCard(id);
                    newCard.effectsStay.Add(new BodyPlusEffect(dValue, dValue));
                    gameEvent2.player.board.SummonMinionByMinion(new GameEvent()
                    {
                        hostCard = gameEvent2.hostCard,
                        targetCard = newCard,
                        player = gameEvent2.player,
                    }, byCard);
                    byCard = newCard;
                }
            }
            return true;
        }));
        return true;
    }

    /// <summary>
    /// 卡德加
    /// </summary>
    [CommonDescription("如果来源是卡牌效果，使召唤效果翻倍")]
    [GoldDescription("如果来源是卡牌效果，使召唤效果翻三倍")]
    public static bool SummonDoubleMinionOrTribleIt(GameEvent gameEvent)
    {
        if (gameEvent.player != gameEvent.player.board.GetPlayer(gameEvent.hostCard)) return false;
        if (gameEvent.player != gameEvent.player.board.GetPlayer(gameEvent.targetCard)) return false;
        if (gameEvent.targetCard.creator == null) return false;
        if (gameEvent.targetCard.creator.cardType == CardType.Hero) return false;

        if (gameEvent.targetCard.creator == gameEvent.hostCard) return false;
        int thisId = gameEvent.hostCard.id;
        int id = gameEvent.targetCard.creator.id;
        if (id == thisId || id == gameEvent.hostCard.goldVersion || gameEvent.targetCard.creator.goldVersion == thisId)
        {
            int creatorIndex = gameEvent.player.GetMinionIndex(gameEvent.targetCard.creator);
            int selfIndex = gameEvent.player.GetMinionIndex(gameEvent.hostCard);
            if (selfIndex > creatorIndex) return false;
        }


        Card cloneCard = gameEvent.targetCard.Clone() as Card;
        Card newCard = cloneCard.NewCard();
        gameEvent.player.board.SummonMinionByMinion(new GameEvent()
        {
            hostCard = gameEvent.hostCard,
            targetCard = newCard,
            player = gameEvent.player,
        }, gameEvent.targetCard);

        if (gameEvent.hostCard.isGold)
        {
            gameEvent.player.board.SummonMinionByMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = cloneCard.NewCard(),
                player = gameEvent.player,
            }, newCard);
        }

        return true;
    }

    /// <summary>
    /// /归来的勇士
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("为其恢复所有生命值")]
    [GoldDescription("为其恢复所有生命值")]
    public static bool FillHealthWhenReborn(GameEvent gameEvent)
    {
        if (gameEvent.targetCard == gameEvent.hostCard) return false;
        if (gameEvent.player != gameEvent.player.board.GetPlayer(gameEvent.targetCard)) return false;
        int healthMax = CardBuilder.GetCard(gameEvent.targetCard.id).health;
        int healthToRestore = healthMax - gameEvent.targetCard.health;
        if (healthToRestore > 0)
        {
            gameEvent.targetCard.health += healthToRestore;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 归来的勇士
    /// </summary>
    [CommonDescription("如果与具有复生的随从相邻，则获得复生")]
    [GoldDescription("如果与具有复生的随从相邻，则获得复生")]
    public static bool GainRebornIfAdjacentMinionHasReborn(GameEvent gameEvent)
    {
        Tuple<Card, Card> tuple = gameEvent.player.board.GetAdjacentMinion(gameEvent.hostCard);
        if (!gameEvent.hostCard.HasKeyword(Keyword.Reborn))
        {
            if ((tuple.Item1?.HasKeyword(Keyword.Reborn) ?? false) || (tuple.Item2?.HasKeyword(Keyword.Reborn) ?? false))
            {
                gameEvent.hostCard.effects.Add(new KeyWordEffect(Keyword.Reborn));
            }
        }
        return false;
    }

    //[CommonDescription("使其获得复生")]
    //public static bool AddReborn(GameEvent gameEvent)
    //{
    //    if (gameEvent.targetCard == gameEvent.hostCard) return false;
    //}

    /// <summary>
    /// 愤怒编织者
    /// </summary>
    [CommonDescription("如果是恶魔,对友方英雄造成1点伤害并获得+2/+2")]
    [GoldDescription("如果是恶魔,对友方英雄造成1点伤害并获得+4/+4")]
    public static bool Deal1damageToHeroAndGain22Or44(GameEvent gameEvent)
    {
        if (gameEvent.targetCard != gameEvent.hostCard &&
            gameEvent.targetCard.IsMinionType(MinionType.Demons) &&
            gameEvent.player == gameEvent.player.board.GetPlayer(gameEvent.targetCard))
        {
            int value = gameEvent.hostCard.isGold ? 4 : 2;
            gameEvent.hostCard.effectsStay.Add(new BodyPlusEffect(value, value));
            gameEvent.player.board.DealDamageToHero(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = gameEvent.player.hero,
                player = gameEvent.player,
                number = 1
            });
            return true;
        }
        return false;
    }

    /// <summary>
    /// 恩佐斯的子嗣
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使你的所有随从获得+1/+1")]
    [GoldDescription("使你的所有随从获得+2/+2")]
    public static bool OtherAllyGain11Or22(GameEvent gameEvent)
    {
        int value = gameEvent.hostCard.isGold ? 2 : 1;
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            ally.effectsStay.Add(new BodyPlusEffect(value, value));
        }
        return true;
    }

    /// <summary>
    /// 人气选手
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("如果其具有战吼,便获得+1/+1")]
    [GoldDescription("如果其具有战吼,便获得+2/+2")]
    public static bool IfHasBattlecryGain11Or22(GameEvent gameEvent)
    {
        if (gameEvent.targetCard != gameEvent.hostCard &&
            gameEvent.targetCard.GetProxys(ProxyEnum.Battlecry) != null &&
            gameEvent.player == gameEvent.player.board.GetPlayer(gameEvent.targetCard))
        {
            int value = gameEvent.hostCard.isGold ? 2 : 1;
            gameEvent.hostCard.effectsStay.Add(new BodyPlusEffect(value, value));
            return true;
        }
        return false;
    }

    /// <summary>
    /// 族群领袖
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("如果是野兽,使其获得+3攻击力")]
    [GoldDescription("如果是野兽,使其获得+6攻击力")]
    public static bool Give3Or6AttackWhenSummonBeast(GameEvent gameEvent)
    {
        if (gameEvent.targetCard != gameEvent.hostCard &&
            gameEvent.targetCard.IsMinionType(MinionType.Beasts) &&
            gameEvent.player == gameEvent.player.board.GetPlayer(gameEvent.targetCard))
        {
            int value = gameEvent.hostCard.isGold ? 6 : 3;
            gameEvent.targetCard.effectsStay.Add(new BodyPlusEffect(value, 0));
            return true;
        }
        return false;
    }

    /// <summary>
    /// 魔瘾结晶者
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使你的所有恶魔获得+1/+1")]
    [GoldDescription("使你的所有恶魔获得+2/+2")]
    public static bool AllyDemonGain11Or22(GameEvent gameEvent)
    {
        int value = gameEvent.hostCard.isGold ? 2 : 1;
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            if (ally == gameEvent.hostCard) continue;
            if (ally.IsMinionType(MinionType.Demons))
            {

                ally.effectsStay.Add(new BodyPlusEffect(value, value));
            }
        }
        return true;
    }

    /// <summary>
    /// 驯兽师
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使一个友方野兽获得+2/+2和嘲讽")]
    [GoldDescription("使一个友方野兽获得+4/+4和嘲讽")]
    public static bool GiveAnAllyBeast22Or44AndTaunt(GameEvent gameEvent)
    {
        int dvalue = gameEvent.hostCard.isGold ? 4 : 2;
        Card card = gameEvent.player.board.ChooseTarget(gameEvent.player.board.GetMinionTargetLambda(gameEvent.player, MinionType.Beasts, gameEvent.hostCard));
        if (card != null)
        {
            card?.effectsStay.Add(new BodyPlusEffect(dvalue, dvalue));
            card.effectsStay.Add(new KeyWordEffect(Keyword.Taunt));
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 展览馆法师
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("随机使一个友方野兽,龙,鱼人获得+2/+2")]
    [GoldDescription("随机使一个友方野兽,龙,鱼人获得+2/+2")]
    public static bool GiveAnRandomAllyMurlocBeastDragon22(GameEvent gameEvent)
    {
        int value = gameEvent.hostCard.isGold ? 4 : 2;
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
    /// 浴火者伯瓦尔
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("获得+2攻击力")]
    [GoldDescription("获得+4攻击力")]
    public static bool Plus2Or4Attack(GameEvent gameEvent)
    {
        if (gameEvent.player == gameEvent.player.board.GetPlayer(gameEvent.targetCard))
        {

            int dvalue = gameEvent.hostCard.isGold ? 4 : 2;
            gameEvent.hostCard.effectsStay.Add(new BodyPlusEffect(dvalue, 0));
            return true;

        }
        return false;
    }

    /// <summary>
    /// 兔妖教头
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使一个友方野兽获得+2/+2")]
    [GoldDescription("使一个友方野兽获得+4/+4")]
    public static bool AddAnAllyBeast2Attack2Health(GameEvent gameEvent)
    {
        int dvalue = gameEvent.hostCard.isGold ? 4 : 2;
        Card card = gameEvent.player.board.ChooseTarget(gameEvent.player.board.GetMinionTargetLambda(gameEvent.player, MinionType.Beasts, gameEvent.hostCard));
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
    /// 硬壳清道夫
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使你具有嘲讽的随从获得+2/+2")]
    [GoldDescription("使你具有嘲讽的随从获得+4/+4")]
    public static bool OtherAllyWithTauntGain22Or44(GameEvent gameEvent)
    {
        int value = gameEvent.hostCard.isGold ? 4 : 2;
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            if (ally == gameEvent.hostCard || !ally.HasKeyword(Keyword.Taunt)) continue;
            ally.effectsStay.Add(new BodyPlusEffect(value, value));
        }
        return true;
    }

    /// <summary>
    /// 光牙执行者
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("随机使一个友方机械，鱼人，野兽，恶魔和龙获得+2/+1")]
    [GoldDescription("随机使一个友方机械，鱼人，野兽，恶魔和龙获得+4/+2")]
    public static bool GiveAllTypeRandomAlly21(GameEvent gameEvent)
    {
        int attackValue = gameEvent.hostCard.isGold ? 4 : 2;
        int healthValue = gameEvent.hostCard.isGold ? 2 : 1;
        List<Card> cardList = Board.FilterCardByMinionType
            (
                gameEvent.player.GetAllAllyMinion(),
                (MinionType.Beasts, 1),
                (MinionType.Dragons, 1),
                (MinionType.Murlocs, 1),
                (MinionType.Mechs, 1),
                (MinionType.Demons, 1)
            );
        foreach (Card card in cardList)
        {
            if (card != null)
            {
                card.effectsStay.Add(new BodyPlusEffect(attackValue, healthValue));
            }
        }
        return true;
    }

    /// <summary>
    /// 锈誓信徒
    /// </summary>
    [CommonDescription("使你的其他随从获得\"亡语:召唤一个1/1的恶魔\"")]
    [GoldDescription("使你的其他随从获得\"亡语:召唤一个2/2的恶魔\"")]
    public static bool OtherAllyGainDeathrattleSummonDemon(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("小鬼囚徒", gameEvent.hostCard.isGold);
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            if (ally == gameEvent.hostCard) continue;
            //if (ally.IsMinionType(MinionType.Demons))
            {
                ally.effectsStay.Add(new ProxyEffect(ProxyEnum.Deathrattle, targetCard.GetProxys(ProxyEnum.Deathrattle)));
            }
        }
        return true;
    }

    /// <summary>
    /// 百变泽鲁斯
    /// </summary>
    [CommonDescription("变形为一个其他随从")]
    [GoldDescription("变形为一个其他随从")]
    public static bool TransformToRandomMinion(GameEvent gameEvent)
    {
        Card ShifterZerus = CardBuilder.SearchCardByName("百变泽鲁斯", gameEvent.hostCard.isGold);
        Card targetCard = gameEvent.player.board.cardPile.cardPile.FilterKey(card => !card.isToken  && card.cardType == CardType.Minion && !card.name.Equals("百变泽鲁斯")).GetOneRandomly();
        if (gameEvent.hostCard.isGold)
        {
            targetCard = CardBuilder.GetCard(targetCard.goldVersion);
        }
        gameEvent.hostCard.TransformToNewCardWithoutEffects(targetCard);

        gameEvent.hostCard.effectsStay.Add(new ProxyEffect(ProxyEnum.TurnStartInHand, ShifterZerus.GetProxys(ProxyEnum.TurnStartInHand)));
        gameEvent.hostCard.effectsStay.Add(new KeyWordEffect(Keyword.Changing));
        return true;
    }

    /// <summary>
    /// 污手街惩罚者
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使你手牌中的所有随从牌获得+1/+1")]
    [GoldDescription("使你手牌中的所有随从牌获得+2/+2")]
    public static bool GiveHandMinion11Or22(GameEvent gameEvent)
    {
        var cards = gameEvent.player.handPile.Filter(card => card.cardType == CardType.Minion);

        foreach (var card in cards)
        {
            var body = card.GetMinionBody();
            card.effectsStay.Add(new BodyPlusEffect(1, 1));
            if (gameEvent.hostCard.isGold)
            {
                card.effectsStay.Add(new BodyPlusEffect(1, 1));
            }
        }
        return true;
    }

    /// <summary>
    /// 高阶祭司阿门特
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("将其生命值增加至与该随从相同")]
    [GoldDescription("将其生命值增加至与该随从相同")]
    public static bool SetHealthWhenSummonMinion(GameEvent gameEvent)
    {
        if (gameEvent.targetCard != gameEvent.hostCard)
        {
            if (gameEvent.player == gameEvent.player.board.GetPlayer(gameEvent.targetCard))
            {
                int value = gameEvent.hostCard.GetMinionBody().y - gameEvent.targetCard.GetMinionBody().y;
                if (value > 0)
                {
                    gameEvent.targetCard.effectsStay.Add(new BodyPlusEffect(0, value));
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 飞刀杂耍者
    /// </summary>
    [CommonDescription("如果在战斗阶段，对一个随机敌方随从造成1点伤害")]
    [GoldDescription("如果在战斗阶段，对一个随从敌方随从造成1点伤害,重复一次")]
    public static bool Deal1DamageOrRepeat(GameEvent gameEvent)
    {
        if (gameEvent.player.board.isBattleField)
        {
            if ((gameEvent.targetCard != gameEvent.hostCard && gameEvent.player == gameEvent.player.board.GetPlayer(gameEvent.targetCard)))
            {
                Card targetCard = gameEvent.player.board.GetAnotherPlayer(gameEvent.player).RandomlyGetAliveMinion();
                if (targetCard != null)
                {
                    gameEvent.player.board.DealDamageToMinion(new GameEvent()
                    {
                        hostCard = gameEvent.hostCard,
                        targetCard = targetCard,
                        player = gameEvent.player,
                        number = 1
                    });
                }
                if (gameEvent.hostCard.isGold)
                {
                    targetCard = gameEvent.player.board.GetAnotherPlayer(gameEvent.player).RandomlyGetAliveMinion();
                    if (targetCard != null)
                    {
                        gameEvent.player.board.DealDamageToMinion(new GameEvent()
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
        }
        return false;
    }

    /// <summary>
    /// 无面潜伏者
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("将该随从的生命值翻倍")]
    [GoldDescription("将该随从的生命值翻三倍")]
    public static bool DoubleOrTripleHealth(GameEvent gameEvent)
    {
        gameEvent.hostCard.effectsStay.Add(new BodyPlusEffect(0, (gameEvent.hostCard.isGold ? 2 : 1) * gameEvent.hostCard.GetMinionBody().y));
        return true;
    }

    /// <summary>
    /// 分裂腐树
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("召唤两个2/2的分裂树苗")]
    [GoldDescription("召唤两个4/4的分裂树苗")]
    public static bool SummonTwo22Tree(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("分裂树苗", gameEvent.hostCard.isGold);
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
    /// 分裂树苗
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("召唤两个1/1的树枝")]
    [GoldDescription("召唤两个2/2的树枝")]
    public static bool SummonTwo11Tree(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("树枝", gameEvent.hostCard.isGold);
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
    /// 软泥教授弗洛普
    /// </summary>
    [CommonDescription("变成它的3/4复制")]
    [GoldDescription("变成它的3/4复制")]
    public static bool TransformTo34CopyOfMinion(GameEvent gameEvent)
    {
        int value1 = 3;
        int value2 = 4;

        Card tmp = CardBuilder.SearchCardByName("软泥教授弗洛普", gameEvent.hostCard.isGold);
        gameEvent.hostCard.TransformToNewCardWithEffects(gameEvent.targetCard.NewCard());
        gameEvent.hostCard.effectsStay.Add(new BodyPlusEffect(value1 - gameEvent.hostCard.GetMinionBody().x,value2 - gameEvent.hostCard.GetMinionBody().y));
        gameEvent.hostCard.effectsStay.Add(new ProxyEffect(ProxyEnum.AfterMinionPlayedInHand, tmp.GetProxys(ProxyEnum.AfterMinionPlayedInHand)));
        gameEvent.hostCard.effectsStay.Add(new KeyWordEffect(Keyword.Changing));
        return true;
    }

    /// <summary>
    /// 软泥教授弗洛普
    /// </summary>
    [CommonDescription("变成它的6/8复制")]
    [GoldDescription("变成它的6/8复制")]
    public static bool TransformTo68CopyOfMinion(GameEvent gameEvent)
    {
        int value1 = 6;
        int value2 = 8;

        Card tmp = CardBuilder.SearchCardByName("软泥教授弗洛普", gameEvent.hostCard.isGold);
        gameEvent.hostCard.TransformToNewCardWithEffects(gameEvent.targetCard.NewCard());
        gameEvent.hostCard.effectsStay.Add(new BodyPlusEffect(value1 - gameEvent.hostCard.GetMinionBody().x, value2 - gameEvent.hostCard.GetMinionBody().y));
        gameEvent.hostCard.effectsStay.Add(new ProxyEffect(ProxyEnum.AfterMinionPlayedInHand, tmp.GetProxys(ProxyEnum.AfterMinionPlayedInHand)));
        gameEvent.hostCard.effectsStay.Add(new KeyWordEffect(Keyword.Changing));
        return true;
    }

    /// <summary>
    /// 夺心者卡什
    /// </summary>
    [CommonDescription("选择一个酒馆中正在出售的随从并获得\"亡语：召唤一个所选随从的全新复制\"")]
    [GoldDescription("选择一个酒馆中正在出售的随从并获得\"亡语：召唤一个所选随从的全新复制\"")]
    public static bool ChooseEnemyThenReSummon(GameEvent gameEvent)
    {
        Card card = gameEvent.player.board.ChooseTarget
            (
                gameEvent.player.board.GetMinionTargetLambda(gameEvent.player.board.GetAnotherPlayer(gameEvent.player), MinionType.Any, null)
            );
        int id = card.id;

        gameEvent.hostCard.effectsStay.Add(new ProxyEffect(ProxyEnum.Deathrattle, gameEvent2 =>
        {
            bool isGold = gameEvent2.hostCard.isGold;
            if (true)
            {
                Card byCard = gameEvent2.hostCard;
                Card newCard = CardBuilder.NewCard(id);
                gameEvent2.player.board.SummonMinionByMinion(new GameEvent()
                {
                    hostCard = gameEvent2.hostCard,
                    targetCard = newCard,
                    player = gameEvent2.player,
                }, byCard);
                byCard = newCard;
            }
            return true;
        }));
        return true;
    }

    /// <summary>
    /// 菌菇术士
    /// </summary>
    [CommonDescription("使相邻的友方随从获得+2/+2")]
    [GoldDescription("使相邻的友方随从获得+4/+4")]
    public static bool AdjacentMinionGain22Or44(GameEvent gameEvent)
    {
        int dvalue = gameEvent.hostCard.isGold ? 4 : 2;
        var index = gameEvent.player.GetMinionIndex(gameEvent.hostCard);
        Tuple<Card, Card> tuple = gameEvent.player.board.GetAdjacentMinion(gameEvent.hostCard);
        tuple.Item1?.effectsStay.Add(new BodyPlusEffect(dvalue, dvalue));
        tuple.Item2?.effectsStay.Add(new BodyPlusEffect(dvalue, dvalue));
        return false;
    }

    /// <summary>
    /// 憎恶弓箭手
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("随机召唤一个在本场战斗中死亡的友方野兽")]
    [GoldDescription("随机召唤两个在本场战斗中死亡的友方野兽")]
    public static bool SummonDeadBeast(GameEvent gameEvent)
    {
        List<Card> summons = new List<Card>();
        List<Card> tmp = new List<Card>();
        foreach (var id in gameEvent.player.deathMinionCollection)
        {
            Card card = CardBuilder.GetCard(id);
            if (card.IsMinionType(MinionType.Beasts))
            {
                tmp.Add(card);
            }
        }
        tmp.Shuffle();
        if (tmp.Count > 0)
        {
            summons.Add(tmp.GetOneRandomly());
            if (gameEvent.hostCard.isGold)
            {
                summons.Add(tmp.GetOneRandomly());
            }
        }
        var byCard = gameEvent.hostCard;
        foreach (var card in summons)
        {
            gameEvent.player.board.SummonMinionByMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = card.NewCard(),
                player = gameEvent.player
            }, byCard);
            byCard = card;
        }
        return true;
    }

    /// <summary>
    /// 荒野行者
    /// </summary>
    [CommonDescription("使一个友方野兽获得+3生命值")]
    [GoldDescription("使一个友方野兽获得+6生命值")]
    public static bool AddAnAllyBeast3Health(GameEvent gameEvent)
    {
        int dvalue = gameEvent.hostCard.isGold ? 6 : 3;
        Card card = gameEvent.player.board.ChooseTarget(gameEvent.player.board.GetMinionTargetLambda(gameEvent.player, MinionType.Beasts, gameEvent.hostCard));
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
    /// 疯狂的炼金师
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("交换一个随从的攻击力与生命值")]
    [GoldDescription("交换一个随从的攻击力与生命值")]
    public static bool ExchangeAnAllyAttackHealth(GameEvent gameEvent)
    {
        Card card = gameEvent.player.board.ChooseTarget(gameEvent.player.board.GetMinionTargetLambda(gameEvent.player, null, gameEvent.hostCard));
        if (card != null)
        {
            int extraValue1 = card.GetMinionBody().x - card.GetMinionBodyWithoutEffect().x;
            int extraValue2 = card.GetMinionBody().y - card.GetMinionBodyWithoutEffect().y;
            int dvalue1 = card.GetMinionBody().x;
            int dvalue2 = card.GetMinionBody().y;
            card?.effectsStay.Add(new BodyPlusEffect(dvalue2 - dvalue1 + extraValue1, dvalue1 - dvalue2 + extraValue2));
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 方阵指挥官
    /// </summary>
    [CommonDescription("你的嘲讽随从获得+2攻击力")]
    [GoldDescription("你的嘲讽随从获得+4攻击力")]
    public static bool PlusAllyWithTaunt2AttackOr4(GameEvent gameEvent)
    {
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            if (ally.HasKeyword(Keyword.Taunt))
            {
                ally.effects.Add(new BodyPlusEffect(gameEvent.hostCard.isGold ? 4 : 2, 0));
            }
        }
        return true;
    }
    
    /// <summary>
    /// 夜色镇执法官
    /// </summary>
    [CommonDescription("每当你召唤一个生命值为1的随从，便使其获得圣盾")]
    [GoldDescription("每当你召唤一个生命值小于或等于2的随从，便使其获得圣盾")]
    public static bool GiveDivineShieldWhenSummonMinion(GameEvent gameEvent)
    {
        if ((gameEvent.targetCard != gameEvent.hostCard && gameEvent.player == gameEvent.player.board.GetPlayer(gameEvent.targetCard)))
        {
            if (gameEvent.targetCard.GetMinionBody().y <= (gameEvent.hostCard.isGold ? 2 : 1))
            {
                gameEvent.targetCard.effectsStay.Add(new KeyWordEffect(Keyword.DivineShield));
                return true;
            }
        }        
        return false;
    }

    /// <summary>
    /// 召唤青玉魔像
    /// </summary>
    [CommonDescription("召唤一个青玉魔像")]
    [GoldDescription("召唤两个青玉魔像")]
    public static bool Summon1Or2JadeGloem(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("青玉魔像");
        if (targetCard != null)
        {
            gameEvent.player.JadeGloem++;
            Card golem1 = targetCard.NewCard();
            golem1.effectsStay.Add(new BodyPlusEffect(gameEvent.player.JadeGloem, gameEvent.player.JadeGloem));
            gameEvent.player.board.SummonMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = golem1,
                player = gameEvent.player,
            });
            if (gameEvent.hostCard.isGold)
            {
                gameEvent.player.JadeGloem++;
                Card golem2 = targetCard.NewCard();
                golem2.effectsStay.Add(new BodyPlusEffect(gameEvent.player.JadeGloem, gameEvent.player.JadeGloem));
                gameEvent.player.board.SummonMinion(new GameEvent()
                {
                    hostCard = gameEvent.hostCard,
                    targetCard = golem2,
                    player = gameEvent.player,
                });
            }
        }
        return true;
    }
}


public class GoldDescriptionAttribute: Attribute
{
    public readonly string Description;

    public GoldDescriptionAttribute(string description)
    {
        Description = description;
    }
}

public class CommonDescriptionAttribute : Attribute
{
    public readonly string Description;

    public CommonDescriptionAttribute(string description)
    {
        Description = description;
    }
}

public class HidePromptAttribute : Attribute
{
    public readonly string Description;
    public HidePromptAttribute(string description = "")
    {
        Description = description;
    }
}

public class SetCounterAttribute: Attribute
{
    public readonly int Value;

    public SetCounterAttribute(int value)
    {
        Value = value;
    }
}