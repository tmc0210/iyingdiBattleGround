using UnityEngine;

public class OldMurkEye : Enemy
{
    public OldMurkEye() : base()
    {
        player = new Player(CardBuilder.SearchCardByName("老蓟皮").NewCard());
    }

    public override void InitCardPile(CardPile cardPile)
    {
        this.cardPile = cardPile;
    }

    public override void InitTag()
    {
        AddToMap(tags, "鱼人流", 2);
        AddToMap(minionTypes, MinionType.Murlocs, 2);
    }

    public override void InitUpgrade()
    {
        turnOfUpgrade = new int[5] { 2, 5, 7, 9, 13 };
    }
}
