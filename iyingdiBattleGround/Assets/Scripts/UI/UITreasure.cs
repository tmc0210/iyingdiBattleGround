using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITreasure : MonoBehaviour
{
    public UICard uiCard;
    public Text txt_Name;
    public Text txt_Type;
    public Text txt_Effect;
    public Button button;


    public void SetTreasure(Card card)
    {
        uiCard.SetTreasure(card);
        //TODO
    }

    public void Click()
    {

    }

    public void CancelClick()
    {
        
    }
}
