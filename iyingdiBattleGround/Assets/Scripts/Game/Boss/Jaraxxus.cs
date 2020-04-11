using UnityEngine;

public class Jaraxxus : Enemy
{
    public Jaraxxus() : base()
    {
        player = new Player(CardBuilder.SearchCardByName("加拉克苏斯大王").NewCard());
    }

    public override void StrenthenHeroPower()
    {
        foreach (var item in player.hero.GetProxysByEffect(ProxyEnum.AfterMinionDeath))
        {
            item.Counter += 2;
        }
    }

    public override void InitCardPile(CardPile cardPile)
    {
        this.cardPile = cardPile;
    }

    public override void InitTag()
    {
        AddToMap(minionTypes, MinionType.Demons, 3);
        AddToMap(tags, "恶魔流", 5);
        //AddToMap(proxys, ProxyEnum.Deathrattle, 1);
    }

    public override void InitUpgrade()
    {
        turnOfUpgrade = new int[5] { 2, 5, 7, 11, 15 };
    }
}
