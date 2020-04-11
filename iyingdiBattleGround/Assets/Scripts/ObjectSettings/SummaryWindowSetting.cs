using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SummaryWindowSetting : MonoBehaviour
{
    [Autohook]
    public CardSetting CardShowTreasure0;
    [Autohook]
    public CardSetting CardShowTreasure1;
    [Autohook]
    public CardSetting CardShowTreasure2;

    [Autohook]
    public HeroSetting Hero0;
    [Autohook]
    public HeroSetting Hero1;
    [Autohook]
    public HeroSetting Hero2;
    [Autohook]
    public HeroSetting Hero3;

    [Autohook]
    public TextMeshPro CardName0;
    [Autohook]
    public TextMeshPro CardName1;
    [Autohook]
    public TextMeshPro CardName2;

    [Autohook]
    public TextMeshPro HeroName0;
    [Autohook]
    public TextMeshPro HeroName1;
    [Autohook]
    public TextMeshPro HeroName2;
    [Autohook]
    public TextMeshPro HeroName3;


    public void SetTreasure(List<Card> treasures)
    {
        CardSetting[] cards = new CardSetting[] { CardShowTreasure0 , CardShowTreasure1, CardShowTreasure2};
        TextMeshPro[] texts = new TextMeshPro[] { CardName0, CardName1, CardName2};
        cards.Map(go => go.gameObject.SetActive(false));
        texts.Map(go => go.gameObject.SetActive(false));
        treasures.Take(3).Map((card, index) => {
            cards[index].gameObject.SetActive(true);
            texts[index].gameObject.SetActive(true);
            cards[index].SetByCard(card);
            texts[index].text = card.name;
            cards[index].EnableDrag(false);
            cards[index].EnableLongPress(false);
            cards[index].EnableTap(false);
        });
    }
    public void SetHero(List<Card> heros)
    {
        HeroSetting[] cards = new HeroSetting[] { Hero0, Hero1, Hero2, Hero3 };
        TextMeshPro[] texts = new TextMeshPro[] { HeroName0, HeroName1, HeroName2, HeroName3 };
        cards.Map(go => go.gameObject.SetActive(false));
        texts.Map(go => go.gameObject.SetActive(false));
        heros.Take(4).Map((card, index) => {
            cards[index].gameObject.SetActive(true);
            texts[index].gameObject.SetActive(true);
            cards[index].SetByCard(card);
            texts[index].text = card.name;
        });
    }
}
