public class Const
{
    public const int numOfBattlePile = 7;
    public const int numOfHandPile = 10;

    public static int InitialUpgradeCost = 6;
    public static int InitialFlushCost = 1;
    public static int InitialFreezeCost = 0;
    public static int InitialStar = 1;
    public static int InitialMaxCoins = 2;
    public static int InitialLeftCoins = 2;

    public static int InitialCoinCostToBuyMinion = 3;
    public static int InitialCoinGetBySellMinion = 1;

    public static int coinCostToBuyMinion = InitialCoinCostToBuyMinion;
    public static int coinGetBySellMinion = InitialCoinGetBySellMinion;
    public static int MaxCoin = 10;

    public static readonly int[] upgradeCosts = new int[5] { 6, 7, 8, 9, 10 };
    public static readonly int[] numOfMinionsInCardPile = new int[6] { 16, 15, 13, 11, 9, 7 };
    public static readonly int[] typeOfMinionsInCardPile = new int[6] { 9, 10, 13, 11, 10, 7 };


    public static int[] numOfMinionsOnSale = new int[6] { 3, 4, 4, 5, 5, 6 };

    public static readonly Keyword[] EvolveKeyWords = new Keyword[5] {Keyword.DivineShield,Keyword.Poisonous,Keyword.Reborn,Keyword.Taunt,Keyword.Windfury};

    public static void Reset()
    {
        coinCostToBuyMinion = InitialCoinCostToBuyMinion;
        coinGetBySellMinion = InitialCoinGetBySellMinion;
        numOfMinionsOnSale = new int[6] { 3, 4, 4, 5, 5, 6 };
    }
}
