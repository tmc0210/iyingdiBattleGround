using System.Collections.Generic;
using System.ComponentModel;

public partial class CardLongKeywordAchievement
{
    /// <summary>
    /// 艾莫莉斯
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使你手牌中的所有随从牌获得+8/+8")]
    [GoldDescription("使你手牌中的所有随从牌获得+16/+16")]
    public static bool DoubleHandMinionBody(GameEvent gameEvent)
    {
        var cards = gameEvent.player.handPile.Filter(card => card.cardType == CardType.Minion);

        foreach (var card in cards)
        {
            var body = card.GetMinionBody();
            //card.effectsStay.Add(new BodyPlusEffect(body.x, body.y));
            card.effectsStay.Add(new BodyPlusEffect(8, 8));
            if (gameEvent.hostCard.isGold)
            {
                //card.effectsStay.Add(new BodyPlusEffect(body.x / 2, body.y / 2));
                card.effectsStay.Add(new BodyPlusEffect(8, 8));
            }
        }
        return true;
    }


    /// <summary>
    /// 青铜传令官
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("将两张4/4的“青铜龙”置入你的手牌")]
    [GoldDescription("将两张8/8的“青铜龙”置入你的手牌")]
    public static bool Create244Dragons(GameEvent gameEvent)
    {
        var targetCard = CardBuilder.SearchCardByName("青铜龙", gameEvent.hostCard.isGold);
        if (targetCard != null)
        {
            gameEvent.player.AddMinionToHandPile(targetCard.NewCard());
            gameEvent.player.AddMinionToHandPile(targetCard.NewCard());
        }

        return true;
    }

    /// <summary>
    /// 龙人执行者
    /// </summary>
    [CommonDescription("便获得+2/+2")]
    [GoldDescription("便获得+4/+4")]
    public static bool Gain22Or44AfterMinionDivineShieldBroken(GameEvent gameEvent)
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
    /// 扭曲巨龙泽拉库
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("如果在酒馆中，召唤一个6/6的龙")]
    [GoldDescription("如果在酒馆中，召唤一个12/12的龙")]
    public static bool Summon66Or1212Dragon(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("虚空幼龙", gameEvent.hostCard.isGold);
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
    /// 黑龙领主死亡之翼
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("将你手牌中所有的龙牌置入战场")]
    [GoldDescription("将你手牌中所有的龙牌置入战场")]
    public static bool SummonAllDragonFromHand(GameEvent gameEvent)
    {
        List<Card> targetCards = gameEvent.player.handPile.Filter(card => card.cardType == CardType.Minion && card.IsMinionType(MinionType.Dragons));
        if (targetCards.Count != 0)
        {
            foreach (var card in targetCards)
            {
                if (gameEvent.player.battlePile.Count < Const.numOfBattlePile)
                {
                    gameEvent.player.RemoveMinionFromHandPile(card);
                    gameEvent.player.board.SummonMinion(new GameEvent()
                    {
                        hostCard = gameEvent.hostCard,
                        targetCard = card,
                        player = gameEvent.player
                    });
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 深蓝刃麟龙人
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("随机使另一个友方随从获得+3攻击力")]
    [GoldDescription("随机使另一个友方随从获得+6攻击力")]
    public static bool GiveAnRandomAlly3Or6Attack(GameEvent gameEvent)
    {
        int value = gameEvent.hostCard.isGold ? 6 : 3;
        var targetCard = gameEvent.player.GetAllAllyMinion()
                        .Filter(card => card != gameEvent.hostCard)
                        .GetOneRandomly();
        if (targetCard != null)
        {
            targetCard.effectsStay.Add(new BodyPlusEffect(value, 0));
        }
        return true;
    }

    /// <summary>
    /// 时空破坏者
    /// </summary>
    [CommonDescription("如果你的手牌中有龙牌,对所有敌方随从造成3点伤害")]
    [GoldDescription("如果你的手牌中有龙牌,对所有敌方随从造成6点伤害")]
    public static bool Deal3Or6DamageToAllEnemy(GameEvent gameEvent)
    {
        if (gameEvent.player.handPile.Any(card => card.cardType == CardType.Minion && card.IsMinionType(MinionType.Dragons)))
        {
            List<GameEvent> gameEvents = new List<GameEvent>();
            foreach (Card minion in gameEvent.player.board.GetAnotherPlayer(gameEvent.player).GetAllAllyMinion())
            {
                gameEvents.Add(new GameEvent()
                {
                    hostCard = gameEvent.hostCard,
                    targetCard = minion,
                    player = gameEvent.player.board.GetPlayer(gameEvent.hostCard),
                    number = gameEvent.hostCard.isGold ? 6 : 3
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
        return false;
    }

    /// <summary>
    /// 龙语者
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使你手牌中所有龙牌获得+3/+3")]
    [GoldDescription("使你手牌中所有龙牌获得+6/+6")]
    public static bool GiveDragonInHand33Or66(GameEvent gameEvent)
    {
        var cards = gameEvent.player.handPile.Filter(card => card.cardType == CardType.Minion && card.IsMinionType(MinionType.Dragons));
        int dvalue = gameEvent.hostCard.isGold ? 6 : 3;
        foreach (var card in cards)
        {            
            card.effectsStay.Add(new BodyPlusEffect(dvalue, dvalue));
        }
        return true;
    }

    /// <summary>
    /// 拉佐格尔的子嗣
    /// </summary>
    [CommonDescription("便获得+2/+2")]
    [GoldDescription("便获得+4/+4")]
    public static bool Gain22Or44TurnEndInhand(GameEvent gameEvent)
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
    /// 诺兹多姆
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("返回你的手牌")]
    [GoldDescription("返回你的手牌")]
    public static bool ReturnToHand(GameEvent gameEvent)
    {
        var targetCard = CardBuilder.SearchCardByName(gameEvent.hostCard.name, gameEvent.hostCard.isGold);
        if (targetCard != null)
        {
            gameEvent.player.AddMinionToHandPile(targetCard.NewCard());            
        }

        return true;
    }

    /// <summary>
    /// 龙人使者
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("从手牌中随机召唤一个龙并使其获得+2/+2")]
    [GoldDescription("从手牌中随机召唤两个龙并使其获得+4/+4")]
    public static bool SummonDragonFromHandAndGive22(GameEvent gameEvent)
    {
        var targetCard = gameEvent.player.handPile.Filter(card => card.cardType == CardType.Minion && card.IsMinionType(MinionType.Dragons)).GetOneRandomly();
        int dvalue = gameEvent.hostCard.isGold ? 4 : 2;
        if (targetCard != null && gameEvent.player.battlePile.Count < Const.numOfBattlePile)
        {
            gameEvent.player.RemoveMinionFromHandPile(targetCard);
            gameEvent.player.board.SummonMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = targetCard,
                player = gameEvent.player
            });
            targetCard.effectsStay.Add(new BodyPlusEffect(dvalue, dvalue));
            if (gameEvent.hostCard.isGold)
            {
                targetCard = gameEvent.player.handPile.Filter(card => card.cardType == CardType.Minion && card.IsMinionType(MinionType.Dragons)).GetOneRandomly();
                if (targetCard != null && gameEvent.player.battlePile.Count < Const.numOfBattlePile)
                {
                    gameEvent.player.RemoveMinionFromHandPile(targetCard);
                    gameEvent.player.board.SummonMinion(new GameEvent()
                    {
                        hostCard = gameEvent.hostCard,
                        targetCard = targetCard,
                        player = gameEvent.player
                    });
                    targetCard.effectsStay.Add(new BodyPlusEffect(dvalue, dvalue));
                    return true;
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 空军指挥官
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("你手牌中每有一张龙牌，便获得+2攻击力")]
    [GoldDescription("你手牌中每有一张龙牌，便获得+4攻击力")]
    public static bool Plus2AttackOr4ForEachDragonInYourHand(GameEvent gameEvent)
    {
        foreach (Card ally in gameEvent.player.handPile)
        {
            if (ally == gameEvent.hostCard) continue;
            if (ally.IsMinionType(MinionType.Dragons))
            {
                gameEvent.hostCard.effects.Add(new BodyPlusEffect(gameEvent.hostCard.isGold ? 4 : 2, 0));
            }
        }
        return true;
    }

    /// <summary>
    /// 时间管理者
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使你手牌中所有随从牌获得+1/+1")]
    [GoldDescription("使你手牌中所有随从牌获得+2/+2")]
    public static bool GiveMinionsInHand11Or22(GameEvent gameEvent)
    {
        var cards = gameEvent.player.handPile.Filter(card => card.cardType == CardType.Minion);
        int dvalue = gameEvent.hostCard.isGold ? 2 : 1;
        foreach (var card in cards)
        {
            card.effectsStay.Add(new BodyPlusEffect(dvalue, dvalue));
        }
        return true;
    }

    /// <summary>
    /// 骸骨巨龙
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("将一张随机龙牌置入你的手牌")]
    [GoldDescription("将两张随机龙牌置入你的手牌")]
    public static bool Add1Or2DragonsToYourHand(GameEvent gameEvent)
    {
        gameEvent.player.board.AddToHandPile(
                gameEvent.player.board.cardPile.RandomlyGetCardByFilterAndReduceIt(
                    card => card.IsMinionType(gameEvent.hostCard.type) 
                    && card.name != gameEvent.hostCard.name).NewCard());
        if (gameEvent.hostCard.isGold)
        {
            gameEvent.player.board.AddToHandPile(
                gameEvent.player.board.cardPile.RandomlyGetCardByFilterAndReduceIt(
                    card => card.IsMinionType(gameEvent.hostCard.type)
                    && card.name != gameEvent.hostCard.name).NewCard());
        }
        return true;
    }    
}
