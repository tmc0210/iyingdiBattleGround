using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartSetting : MonoBehaviour
{
    #region get singleton
    public static GameStartSetting instance = null;

    private void Awake()
    {
        instance = this;
    }
    #endregion

    public void BeforeGameStart()
    {

    }
}
