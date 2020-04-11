using UnityEngine;

public class Millificent : Enemy
{
    public Millificent() : base()
    {
        player = new Player(CardBuilder.SearchCardByName("米尔菲斯·法力风暴").NewCard());
        //player.hero.effectsStay.Add(new BodyPlusEffect(0, 10 * (player.board.level - 1)));
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
        AddToMap(tags, "机械流", 2);
        AddToMap(minionTypes, MinionType.Mechs, 2);
    }

    public override void InitUpgrade()
    {
        turnOfUpgrade = new int[5] { 2, 5, 7, 11, 15 };
    }
}
