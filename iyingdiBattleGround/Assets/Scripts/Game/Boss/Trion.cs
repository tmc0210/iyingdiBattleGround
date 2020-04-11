using UnityEngine;

public class Trion : Enemy
{
    public Trion() : base()
    {
        player = new Player(CardBuilder.SearchCardByName("提里奥·弗丁").NewCard());
    }

    public override void StrenthenHeroPower()
    {
        player.hero.cost = 0;
    }

    public override void InitCardPile(CardPile cardPile)
    {
        this.cardPile = cardPile;
    }

    public override void InitTag()
    {
        AddToMap(tags, "中立流", 3);
        AddToMap(minionTypes, MinionType.General, 3);
        AddToMap(minionTypes, MinionType.Mechs, -5);
        AddToMap(minionTypes, MinionType.Beasts, -5);
        AddToMap(minionTypes, MinionType.Demons, -5);
        AddToMap(minionTypes, MinionType.Murlocs, -5);
    }

    public override void InitUpgrade()
    {
        turnOfUpgrade = new int[5] { 4, 6, 8, 10, 13 };
    }
}
