using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageCollection : MonoBehaviour
{
    public Sprite[] sprites;




    public Sprite GetSpriteByName(string name)
    {
        foreach (var sprite in sprites)
        {
            if (sprite.name == name)
            {
                return sprite;
            }
        }
        return null;
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
        if (string.IsNullOrEmpty(str)) return sprites[0];
        if (int.TryParse(str, out int value))
        {
            return GetSpriteByIndex(value);
        }
        return GetSpriteByName(str);
    }
}
