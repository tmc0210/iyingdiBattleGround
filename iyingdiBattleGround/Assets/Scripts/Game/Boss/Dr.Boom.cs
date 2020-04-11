using UnityEngine;

public class Boom : Enemy
{
    public Boom() : base()
    {
        player = new Player(CardBuilder.SearchCardByName("Dr.Boom").NewCard());
    }

    public override void StrenthenHeroPower()
    {
        foreach (var item in player.hero.GetProxysByEffect(ProxyEnum.Aura))
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
        AddToMap(tags, "亡语流", 100);
        AddToMap(minionTypes, MinionType.Mechs, 10);
        AddToMap(proxys, ProxyEnum.Deathrattle, 20);
    }

    public override void InitUpgrade()
    {
        turnOfUpgrade = new int[5] { 2, 5, 7, 9, 13 };
    }
}
