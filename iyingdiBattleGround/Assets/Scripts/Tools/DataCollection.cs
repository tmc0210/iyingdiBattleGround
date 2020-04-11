using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataCollection
{
    public static void Init()
    {
        //TalkingDataPlugin.SetLogEnabled(true);
        //TalkingDataPlugin.SetExceptionReportEnabled(true);
        //TalkingDataPlugin.SessionStarted("0E1DA85143C649AD898F518575DF01F3", "phone");
    }

    public static void SelectHero(Card card)
    {
        //TalkingDataPlugin.TrackEventWithLabel("选择英雄", card.name);
    }
    public static void BuyCard(Card card)
    {

    }
    public static void SelectTreasure(Card card)
    {

    }

    public static void StayLevel(int n)
    {

    }
}
