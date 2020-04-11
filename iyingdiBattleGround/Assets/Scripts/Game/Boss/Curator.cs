using BIF;
using UnityEngine;

public class Curator : Enemy
{
    public Curator()
    {
        player = new Player(CardBuilder.SearchCardByName("馆长").NewCard());
        //player.hero.effectsStay.Add(new BodyPlusEffect(0, 10 * (player.board.level - 1)));
    }

    public override void InitCardPile(CardPile cardPile)
    {
        this.cardPile = cardPile;
    }

    public override void InitTag()
    {
        AddToMap(tags, "机械流", 1);
        AddToMap(tags, "野兽流", 1);
        AddToMap(tags, "恶魔流", 1);
    }

    public override void InitUpgrade()
    {
        turnOfUpgrade = new int[5] { 2, 5, 7, 10, 14 };
    }

    public override void StrenthenHeroPower()
    {
        if (player.maxCoins < Const.MaxCoin)
        {
            player.maxCoins += 2;
        }
        player.leftCoins = player.maxCoins;
    }

    public override void SpecialAction()
    {
        foreach (Card ally in player.GetAllAllyMinion())
        {
            ally.type = MinionType.Any;
        }
        foreach (Card ally in player.handPile)
        {
            if (ally.cardType == CardType.Minion)
            {
                ally.type = MinionType.Any;
            }
        }
    }
}
