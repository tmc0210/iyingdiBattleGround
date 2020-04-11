using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        BattlePile<object> battlePile = new BattlePile<object>(7);
        battlePile[0] = 0;
        battlePile[1] = 1;
        battlePile[2] = 2;
        battlePile[3] = 3;
        battlePile[4] = null;
        battlePile[5] = 5;
        battlePile[6] = null;

        battlePile.AddMinion(10,3);

        for (int i = 0; i < battlePile.fixedNumber; i++)
        {
            Debug.Log(battlePile[i] + "");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
