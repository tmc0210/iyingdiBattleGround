using UnityEngine;

public class SilverbackPatriarch : Enemy
{
    public SilverbackPatriarch() : base()
    {
        player = new Player(CardBuilder.SearchCardByName("银背族长").NewCard());
    }

    public override void InitCardPile(CardPile cardPile)
    {
        this.cardPile = cardPile;
    }

    public override void InitTag()
    {
        AddToMap(tags, "亡语流", 100);
        AddToMap(minionTypes, MinionType.Beasts, 20);
        AddToMap(proxys, ProxyEnum.Deathrattle, 10);
    }

    public override void InitUpgrade()
    {
        turnOfUpgrade = new int[5] { 2, 5, 7, 9, 13 };
    }
}
