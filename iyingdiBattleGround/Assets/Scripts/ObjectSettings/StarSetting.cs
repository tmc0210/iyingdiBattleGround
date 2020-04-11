using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TouchScript.Gestures;
using UnityEngine;

public class StarSetting : MonoBehaviour
{
    private readonly Transform[] stars = new Transform[6];



    private void OnEnable()
    {
        string preStr = "星级";
        for(int i=0; i<6; i++)
        {
            stars[i] = transform.Find(preStr + (i + 1));
        }
    }

    private int star = -1;
    public void SetStar(int value)
    {
        value--;
        if (star >= 0 && star <= 5)
        {
            stars[star].gameObject.SetActive(false);
        }
        if (value >=0  && value <= 5)
        {
            star = value;
            stars[star].gameObject.SetActive(true);
        }
    }
    public int GetStar()
    {
        return star;
    }

}
