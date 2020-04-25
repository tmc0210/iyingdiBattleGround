using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ImageCollection : MonoBehaviour
{
    public Sprite[] sprites;

    public List<Sprite> spritesIngame;

    #region singleton
    public static ImageCollection instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion



    public Sprite GetSpriteByName(string name)
    {
        foreach (var sprite in spritesIngame)
        {
            if (sprite.name == name)
            {
                return sprite;
            }
        }
        return sprites[0];
    }
    public Sprite GetSpriteByIndex(int n)
    {
        if (sprites.Length > n)
        {
            return sprites[n];
        }
        return null;
    }
    public Sprite GetSpriteByString(string str)
    {
        //if (string.IsNullOrEmpty(str)) return sprites[0];
        //if (int.TryParse(str, out int value))
        //{
        //    return GetSpriteByIndex(value);
        //}
        return GetSpriteByName(str);
    }
}
