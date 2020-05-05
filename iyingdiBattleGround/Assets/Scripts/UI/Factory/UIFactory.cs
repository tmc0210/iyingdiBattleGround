using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIFactory : MonoBehaviour
{
    private static UIFactory instance;
    public static UIFactory Instance {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UIFactory>();
            }
            return instance;
        }
    }

    protected Dictionary<string, GameObject> factoryDict = new Dictionary<string, GameObject>();

    protected Dictionary<string, Stack<GameObject>> objectPoolDict = new Dictionary<string, Stack<GameObject>>();

    protected string loadPath = "Prefabs/UI";


    public GameObject GetItem(string itemName, GameObject prefab)
    {
        GameObject itemGO = null;
        if (objectPoolDict.ContainsKey(itemName))//对象池包含路径
        {
            if (objectPoolDict[itemName].Count == 0)//对象池没有
            {
                itemGO = Instantiate(prefab);
            }
            else//有
            {
                itemGO = objectPoolDict[itemName].Pop();
                itemGO.SetActive(true);
            }
        }
        else//不包含
        {
            objectPoolDict.Add(itemName, new Stack<GameObject>());
            itemGO = Instantiate(prefab);
        }
        if (itemGO == null)
        {
            Debug.LogWarning("资源获取失败，路径：" + itemName);
        }

        return itemGO;
    }

    public GameObject GetItem(string itemName)
    {
        return GetItem(itemName, GetResource(itemName));
    }

    public void PushItem(string itemName, GameObject item)
    {
        item.SetActive(false);
        if (objectPoolDict.ContainsKey(itemName))
        {
            objectPoolDict[itemName].Push(item);
        }
        else
        {
            Debug.LogWarning("当前对象池字典没有栈" + itemName);
        }
    }

    //取资源
    private GameObject GetResource(string itemName)
    {
        GameObject itemGO = null;
        string itemLoadPath = loadPath + itemName;
        if (factoryDict.ContainsKey(itemName))
        {
            itemGO = factoryDict[itemName];
        }
        else
        {
            itemGO = Resources.Load<GameObject>(itemLoadPath);
            factoryDict.Add(itemName, itemGO);
        }
        if (itemGO == null)
        {
            Debug.LogWarning("资源获取失败，路径：" + itemLoadPath);
        }
        return itemGO;
    }
}
