using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;
using BIF;

public class CardPileTracker : EditorWindow
{

    public CardPileTracker()
    {
        titleContent = new GUIContent("游戏测试面板");
    }

    [MenuItem("Test/牌堆追踪")]
    static void CreateTestWindows()
    {
        GetWindow(typeof(CardPileTracker));
    }

    private SelectedType selectedType;
    private MinionType minionType;
    private Keyword keywordValue;
    Vector2 scrollPosition;
    string tagValue;

    public enum SelectedType
    {
        MinionType,
        Tag,
        Keyword,
    }

    private void OnEnable()
    {
        selectedType = (SelectedType)PlayerPrefs.GetInt("selectedType", 0);
        minionType = (MinionType)PlayerPrefs.GetInt("minionType", 0);
        keywordValue = (Keyword)PlayerPrefs.GetInt("keywordValue", 0);
        scrollPosition = Vector2.zero;
        tagValue = "";
    }

    private void SaveLastCard(int cardId)
    {
        List<int> nums = GetAllLastCard();
        if (nums.Contains(cardId)) return;
        if (nums.Count > 4)
        {
            nums.RemoveAt(nums.Count-1);
        }
        nums.Insert(0, cardId);
        PlayerPrefs.SetString("cardsLastClicked", nums.Serialize());
    }
    private List<int> GetAllLastCard()
    {
        string cardsStr = PlayerPrefs.GetString("cardsLastClicked", "");
        return cardsStr.ParseListInt();
    }

    private void OnGUI()
    {

        if (CardBuilder.AllCards != null && Application.isPlaying && GameAnimationSetting.instance.board != null)
        {
            EditorGUILayout.HelpBox("在对战中点击卡牌后在Log中查看剩余张数", MessageType.Info);

            selectedType = (SelectedType)EditorGUILayout.EnumPopup(new GUIContent("分类依据："), selectedType);
            PlayerPrefs.SetInt("selectedType", (int)selectedType);

            if (selectedType == SelectedType.Tag)
            {
                List<string> tags = new HashSet<string>(GameAnimationSetting.instance.board.cardPile.cardPile.Keys.SelectMany(card => card.tag))
                    .OrderBy(x => x).ToList();
                if (string.IsNullOrEmpty(tagValue)) tagValue = tags.GetOne();

                GUILayout.BeginHorizontal();
                GUILayout.Label("选择tag:");
                if (EditorGUILayout.DropdownButton(new GUIContent(tagValue), FocusType.Keyboard))
                {
                    GenericMenu genericMenu = new GenericMenu();
                    foreach (var tag in tags)
                    {
                        genericMenu.AddItem(new GUIContent(tag), tag.Equals(tagValue), value =>
                        {
                            tagValue = value.ToString();
                        }, tag);
                    }
                    genericMenu.ShowAsContext();
                }
                GUILayout.EndHorizontal();

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                GameAnimationSetting.instance.board.cardPile.cardPile.FilterKey(card => card.tag.Contains(tagValue) && card.isGold == false)
                    .OrderByDescending(card => card.star)
                    .Map(card =>
                    {
                        if (GUILayout.Button(card.name + "******".Substring(0, card.star)))
                        {
                            GetNum(card);
                            SaveLastCard(card.id);
                        }
                    });
                GUILayout.EndScrollView();

            }
            else if (selectedType == SelectedType.MinionType)
            {
                minionType = (MinionType)EditorGUILayout.EnumPopup(new GUIContent("种族："), minionType);

                PlayerPrefs.SetInt("minionType", (int)minionType);

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                GameAnimationSetting.instance.board.cardPile.cardPile.FilterKey(card => card.IsMinionType(minionType) && card.isGold == false)
                    .OrderByDescending(card => card.star)
                    .Map(card =>
                    {
                        if (GUILayout.Button(card.name + "******".Substring(0, card.star)))
                        {
                            GetNum(card);
                            SaveLastCard(card.id);
                        }
                    });
                GUILayout.EndScrollView();
            }
            else if (selectedType == SelectedType.Keyword)
            {
                keywordValue = (Keyword)EditorGUILayout.EnumPopup(new GUIContent("关键字："), keywordValue);
                PlayerPrefs.SetInt("keywordValue", (int)keywordValue);


                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                GameAnimationSetting.instance.board.cardPile.cardPile.FilterKey(card => card.HasKeyword(keywordValue) && card.isGold == false)
                    .OrderByDescending(card => card.star)
                    .Map(card =>
                    {
                        if (GUILayout.Button(card.name + "******".Substring(0, card.star)))
                        {
                            GetNum(card);
                            SaveLastCard(card.id);
                        }
                    });
                GUILayout.EndScrollView();
            }

            List<int> cards = GetAllLastCard();
            if (cards.Count != 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("最近使用过", MessageType.Info);
                foreach (var id in cards)
                {
                    var card = CardBuilder.GetCard(id);
                    if (GUILayout.Button(card.name + "******".Substring(0, card.star)))
                    {
                        GetNum(card);
                    }
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("请启动游戏并开始对战", MessageType.Info);
        }
    }


    private void GetNum(Card card)
    {
        if (GameAnimationSetting.instance.board != null)
        {
            if (GameAnimationSetting.instance.board.cardPile.cardPile.ContainsKey(card))
            {
                Debug.Log(card.name + "剩余" + GameAnimationSetting.instance.board.cardPile.cardPile[card] + "张");
            }
            else
            {
                Debug.Log("牌堆中没有" + card.name);
            }
        }
        else
        {
            Debug.Log("board为空");
        }
    }
}
