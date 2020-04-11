using Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace BIF
{

    public static class BIFStaticTool
    {
        #region set color

        public static void SetAlphaRecursively(GameObject go,  float alpha)
        {
            Color newColor;

            var sprites = go.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sprite in sprites)
            {
                newColor = sprite.color;
                newColor.a = alpha;
                sprite.color = newColor;
            }

            var textMeshes = go.GetComponentsInChildren<TextMeshPro>();
            foreach (var text in textMeshes)
            {
                newColor = text.color;
                newColor.a = alpha;
                text.color = newColor;
            }
        }

        public static void SetColorRecursively(GameObject go, Color color)
        {
            var sprites = go.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sprite in sprites)
            {
                sprite.color = color;
            }
            //Debug.Log("num:" + sprites.Length);

            //var textMeshes = go.GetComponentsInChildren<TextMeshPro>();
            //foreach (var text in textMeshes)
            //{
            //    text.color = color;
            //}
            //Debug.Log("num:" + textMeshes.Length);
        }

        #endregion

        #region change layer

        public static void ChangeLayersRecursivelyBy(Transform gameObject, int dLayer)
        {
            SortingGroup sortingGroup = gameObject.GetComponent<SortingGroup>();
            if (sortingGroup != null)
            {
                sortingGroup.sortingOrder += dLayer;
                return;
            }

            foreach(var renderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
            {
                renderer.sortingOrder += dLayer;
            }
            foreach (var tmp in gameObject.GetComponentsInChildren<TextMeshPro>())
            {
                tmp.sortingOrder += dLayer;
            }
        }
        public static int GetMinLayerRecursively(Transform gameObject)
        {
            int min = int.MinValue;
            foreach (var renderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
            {
                min = Math.Max(min, renderer.sortingOrder);
            }
            foreach (var tmp in gameObject.GetComponentsInChildren<TextMeshPro>())
            {
                min = Math.Max(min, tmp.sortingOrder);
            }
            return min;
        }


        #endregion

        #region enum相关反射

        /// <summary>
        /// 获取枚举类型的Description。
        ///     例如：[Description("已准备")]Ready
        /// </summary>
        /// <typeparam name="T">该枚举类型</typeparam>
        /// <param name="enumValue">枚举值</param>
        /// <returns>Description string</returns>
        public static string GetEnumDescription<T>(T enumValue)
        {
            Type enumType = typeof(T);
            FieldInfo fieldInfo = enumType.GetField(Enum.GetName(enumType, enumValue));
            DescriptionAttribute attribute = Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute), false) as DescriptionAttribute;
            return attribute.Description;
        }

        public static string GetFieldDescription(FieldInfo fieldInfo)
        {
            DescriptionAttribute attribute = Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute), false) as DescriptionAttribute;
            return attribute.Description;
        }
        public static string GetFieldDescription<T>(string filedName)
        {
            Type type = typeof(T);
            FieldInfo fieldInfo = type.GetField(filedName);
            if (fieldInfo == null) return null;
            DescriptionAttribute attribute = Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute), false) as DescriptionAttribute;
            return attribute?.Description;
        }

        public static Map<string, string> GetEnumMemberAndDescription(Type type)
        {
            Map<string, string> retMap = new Map<string, string>();
            MemberInfo[] memberInfos = type.GetMembers();
            foreach (var pio in memberInfos)
            {
                DescriptionAttribute attribute = Attribute.GetCustomAttribute(pio, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    retMap.Add(pio.Name, attribute.Description);
                }
            }
            return retMap;
        }
        public static Map<T, string> GetEnumFieldAndDescription<T>()
        {
            Type enumType = typeof(T);
            T obj = (T)Activator.CreateInstance(enumType);
            Map<T, string> retMap = new Map<T, string>();
            FieldInfo[] fieldInfos = enumType.GetFields();
            foreach (var field in fieldInfos)
            {
                DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    retMap.Add((T)field.GetValue(obj), attribute.Description);
                }
            }
            return retMap;
        }

        static Map<Type, object>  EnumDescriptionSaves = new Map<Type, object>();

        /// <summary>
        /// 获取一个enum的描述字符串（第一次获取时存储所有描述到map，之后在map中读取）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetEnumDescriptionSaved<T>(T enumValue)
        {
            Type enumType = typeof(T);
            var maps = EnumDescriptionSaves.GetByDefault(enumType, null);
            if (maps == null)
            {
                maps = GetEnumFieldAndDescription<T>();
                EnumDescriptionSaves[enumType] = maps;
            }

            return (maps as Map<T, string>)[enumValue];
        }

        /// <summary>
        /// 根据描述字符串，获取一个enum（第一次获取时存储所有描述到map，之后在map中读取）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="description"></param>
        /// <param name="defalutValue"></param>
        /// <returns></returns>
        public static T GetEnumDescriptionEnumSaved<T>(string description, T defalutValue)
        {
            Type enumType = typeof(T);
            var maps = EnumDescriptionSaves.GetByDefault(enumType, null);
            if (maps == null)
            {
                maps = GetEnumFieldAndDescription<T>();
                EnumDescriptionSaves[enumType] = maps;
            }
            foreach(var pair in maps as Map<T, string>)
            {
                if (pair.Value == description)
                {
                    return pair.Key;
                }
            }
            return defalutValue;
        }

        #endregion

        #region enum相关反射2

        private static readonly Map<Type, object> enumNameAndDescriptionSavedCache = new Map<Type, object>();

        /// <summary>
        /// map中： pair.key为枚举值; pair.value.key为Name; pair.value.value为Description
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Map<T, KeyValuePair<string, string>> GetEnumNameAndDescriptionSaved<T>()
        {
            Type type = typeof(T);
            if (enumNameAndDescriptionSavedCache.ContainsKey(type))
                return (Map<T, KeyValuePair<string, string>>)enumNameAndDescriptionSavedCache[type];

            T obj = (T)Activator.CreateInstance(type);
            Map<T, KeyValuePair<string, string>> map = new Map<T, KeyValuePair<string, string>>();

            var fields = type.GetFields();
            foreach (var field in fields)
            {
                T t = (T)field.GetValue(obj);
                DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                map[t] = new KeyValuePair<string, string>(field.Name, attribute?.Description);
            }
            enumNameAndDescriptionSavedCache[type] = map;
            return map;
        }


        private static readonly Map<Type, object> enumNameAndCommonDescriptionSavedCache = new Map<Type, object>();

        /// <summary>
        /// map中： pair.key为枚举值; pair.value.key为Name; pair.value.value为Description
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Map<T, KeyValuePair<string, string>> GetEnumNameAndCommonDescriptionSaved<T>()
        {
            Type type = typeof(T);
            if (enumNameAndCommonDescriptionSavedCache.ContainsKey(type))
                return (Map<T, KeyValuePair<string, string>>)enumNameAndCommonDescriptionSavedCache[type];

            T obj = (T)Activator.CreateInstance(type);
            Map<T, KeyValuePair<string, string>> map = new Map<T, KeyValuePair<string, string>>();

            var fields = type.GetFields();
            foreach (var field in fields)
            {
                T t = (T)field.GetValue(obj);
                CommonDescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(CommonDescriptionAttribute)) as CommonDescriptionAttribute;
                map[t] = new KeyValuePair<string, string>(field.Name, attribute?.Description);
            }
            enumNameAndCommonDescriptionSavedCache[type] = map;
            return map;
        }

        #endregion

        #region prefab pool


        /// <summary>
        /// 初始化：PrefabPool("player", PlayerPrefab, HideArea)
        /// 提取：PrefabPool("player")
        /// 放回：GetPrefabInPool("player", playerObject)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="prefab"></param>
        /// <param name="hideArea">隐藏的GameObject存放地</param>
        /// <returns></returns>
        public static GameObject PrefabPool(string key, GameObject prefab = null, Transform hideArea = null)
        {
            if (prefab != null && hideArea != null)
            {
                List<GameObject> pool = prefabPool.SetByDefault(key, new List<GameObject>());
                pool.Clear();
                pool.Add(prefab);
                pool.Add(hideArea.gameObject);
            }
            else if (prefab != null && hideArea == null)
            {
                if (prefabPool.ContainsKey(key))
                {
                    List<GameObject> pool = prefabPool[key];
                    if (!pool.Contains(prefab))
                    {
                        pool.Add(prefab);
                        prefab.transform.SetParent(pool[1].transform, false);
                        prefab.SetActive(false);
                    }
                }
            }
            else
            {
                if (prefabPool.ContainsKey(key))
                {
                    List<GameObject> pool = prefabPool[key];
                    if (pool.Count >= 3)
                    {
                        GameObject ret = pool[pool.Count - 1];
                        pool.RemoveAt(pool.Count - 1);
                        ret.SetActive(true);
                        return ret;
                    }
                    else
                    {
                        return UnityEngine.Object.Instantiate(pool[0], Vector3.zero, Quaternion.identity, pool[1].transform);
                    }
                } else
                {
                    throw new Exception("Key "+key+" not seted yet!");
                }
            }
            return null;
        }

        /// <summary>
        /// 从对象池中取出object，并设置其父节点
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        static public GameObject GetPrefabInPool(string key, Transform parent)
        {
            GameObject gameObject = PrefabPool(key);
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }
        /// <summary>
        /// List中第一个元素存储原本的Prefab, 第二个存储hideArea
        /// </summary>
        static readonly Map<string, List<GameObject>> prefabPool = new Map<string, List<GameObject>>();


        #endregion

        #region read csv

        static public List<List<string>> ReadCSV(string filename)
        {
            List<List<string>> csvContent = new List<List<string>>();
            TextAsset binAsset = Resources.Load<TextAsset>(filename);
            if (binAsset == null)
            {
                throw new Exception("csv文件不存在");
            }
            string[] lines = binAsset.text.Split('\n');

            foreach (var line in lines)
            {
                csvContent.Add(new List<string>(line.Split(',')));
            }

            return csvContent;
        }

        public static int ParseInt(string s, int defaultValue = default)
        {
            if (int.TryParse(s, out int number))
            {
                return number;
            }
            return defaultValue;
        }


        #endregion read csv

        #region read excel

        static public List<List<string>> ReadExcel(string filename)
        {
            string path = Path.Combine(Application.streamingAssetsPath, filename);
            var tmp =  File.ReadAllText(path);
            Debug.Log(tmp);

            using (FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader excelDataReader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);
                DataSet result = excelDataReader.AsDataSet();

                // 获取表格有多少列 
                int columns = result.Tables[0].Columns.Count;
                // 获取表格有多少行 
                int rows = result.Tables[0].Rows.Count;
                // 根据行列依次打印表格中的每个数据 

                List<string> excelDta = new List<string>();

                //第一行为表头，不读取
                for (int i = 1; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        // 获取表格中指定行指定列的数据 
                        var value = result.Tables[0].Rows[i][j].ToString();
                        Debug.Log(value);
                    }
                }

            }

            return null;
        }

        #endregion
    }



    public class Map<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public Map<TKey, TValue> Update(Map<TKey, TValue> map)
        {
            foreach (var pair in map)
            {
                this[pair.Key] = pair.Value;
            }

            return this;
        }

        public TValue RemoveItem(TKey key)
        {
            if (ContainsKey(key))
            {
                TValue value = this[key];
                Remove(key);
                return value;
            }
            return default;
        }

        public TValue SetByDefault(TKey key, TValue defaultValue)
        {
            if (ContainsKey(key))
            {
                return this[key];
            } else
            {
                this[key] = defaultValue;
                return defaultValue;
            }
        }
        public TValue GetByDefault(TKey key, TValue defaultValue=default)
        {
            if (ContainsKey(key))
            {
                return this[key];
            }
            else
            {
                return defaultValue;
            }
        }

        public override string ToString()
        {
            string ret = "";
            foreach(var pair in this)
            {
                ret += "\""+pair.Key +  "\": \"" + pair.Value+"\",";
            }
            ret = "{" + ret + "}";
            return ret;
        }

        public List<TKey> GetKeys()
        {
            var keys = new List<TKey>();
            keys.AddRange(Keys);
            return keys;
        }
        public List<TValue> GetValues()
        {
            var values = new List<TValue>();
            values.AddRange(Values);
            return values;
        }

        public static explicit operator Map<TKey, TValue>(List<KeyValuePair<Card, int>> v)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class UnityStringEvent : UnityEvent<String>
    {

    }
    [Serializable]
    public class UnityIntEvent : UnityEvent<int>
    {

    }
}
