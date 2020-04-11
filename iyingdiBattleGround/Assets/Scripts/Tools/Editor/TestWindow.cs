using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

public class TestWindow : EditorWindow
{

    public TestWindow()
    {
        titleContent = new GUIContent("游戏测试面板");
    }

    [MenuItem("Test/Test窗口")]
    static void CreateTestWindows()
    {
        GetWindow(typeof(TestWindow));
    }

    private SelectedType selectedType;
    private MinionType minionType;
    private Keyword keywordValue;
    Vector2 scrollPosition;
    bool isToken;
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
        isToken = false;
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

        if (CardBuilder.AllCards != null && Application.isPlaying)
        {
            EditorGUILayout.HelpBox("在对战中点击卡牌生成卡牌\n在对战前点击宝藏获得宝藏", MessageType.Info);
            if (GUILayout.Button("增加十块钱"))
            {
                AddCoins();
            }

            if (GUILayout.Button("解锁所有英雄"))
            {
                UnlockAllHero(true);
            }
            if (GUILayout.Button("锁定所有英雄"))
            {
                UnlockAllHero(false);
            }

            if (GUILayout.Button("解锁所有卡牌"))
            {
                LockAllCard(true);
            }
            if (GUILayout.Button("锁定所有卡牌"))
            {
                LockAllCard(false);
            }
            if (GUILayout.Button("直接获胜"))
            {
                AutoWin();
            }
            if (GUILayout.Button("直接失败"))
            {
                AutoLose();
            }
            EditorGUILayout.Space();

            selectedType = (SelectedType)EditorGUILayout.EnumPopup(new GUIContent("分类依据："), selectedType);
            PlayerPrefs.SetInt("selectedType", (int)selectedType);

            if (selectedType == SelectedType.Tag)
            {
                List<string> tags = new HashSet<string>(CardBuilder.AllCards.Values.SelectMany(card => card.tag))
                    .OrderBy(x=>x).ToList();
                if (string.IsNullOrEmpty(tagValue)) tagValue = tags.GetOne();

                GUILayout.BeginHorizontal();
                GUILayout.Label("选择tag:");
                if (EditorGUILayout.DropdownButton(new GUIContent(tagValue), FocusType.Keyboard))
                {
                    GenericMenu genericMenu = new GenericMenu();
                    foreach (var tag in tags)
                    {
                        genericMenu.AddItem(new GUIContent(tag), tag.Equals(tagValue), value => {
                            tagValue = value.ToString();
                        }, tag);
                    }
                    genericMenu.ShowAsContext();
                }
                GUILayout.EndHorizontal();

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                CardBuilder.AllCards.FilterValue(card => card.tag.Contains(tagValue) && card.isGold == false)
                    .OrderByDescending(card => card.star)
                    .Map(card => {
                        if (GUILayout.Button(card.name + "******".Substring(0, card.star)))
                        {
                            CreateNewCard(card);
                            SaveLastCard(card.id);
                        }
                    });
                GUILayout.EndScrollView();

            }
            else if (selectedType == SelectedType.MinionType)
            {
                minionType = (MinionType)EditorGUILayout.EnumPopup(new GUIContent("种族："), minionType);

                PlayerPrefs.SetInt("minionType", (int)minionType);

                isToken = EditorGUILayout.Toggle(new GUIContent("Token?"), isToken);


                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                CardBuilder.AllCards.FilterValue(card => card.IsMinionType(minionType) && card.isToken == isToken && card.isGold == false)
                    .OrderByDescending(card => card.star)
                    .Map(card => {
                        if (GUILayout.Button(card.name + "******".Substring(0, card.star)))
                        {
                            CreateNewCard(card);
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
                CardBuilder.AllCards.FilterValue(card => card.HasKeyword(keywordValue) && card.isGold == false)
                    .OrderByDescending(card => card.star)
                    .Map(card => {
                        if (GUILayout.Button(card.name + "******".Substring(0, card.star)))
                        {
                            CreateNewCard(card);
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
                        CreateNewCard(card);
                    }
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("请启动游戏", MessageType.Info);
        }
    }

    private void UnlockAllHero(bool lockCard)
    {
        CardBuilder.AllCards.FilterValue(card => card.cardType == CardType.Hero)
            .Map(card=> card.Lock = !lockCard);

        GameController.SaveLockCard();
        GameController.LoadLockCard();
    }

    private void LockAllCard(bool lockCard)
    {
        CardBuilder.AllCards.Values
            .Map(card => card.Lock = !lockCard);

        GameController.SaveLockCard();
        GameController.LoadLockCard();
    }

    private static void AddCoins()
    {
        GameAnimationSetting.instance.board?.AddCoin(10);
    }

    private static void CreateNewCard(Card card)
    {
        if (card.tag.Contains("宝藏"))
        {
            GameAnimationSetting.instance.gameController.player?.treasures.Add(card.NewCard());
        }
        else
        {
            GameAnimationSetting.instance.board?.CreateCard(card);
        }

    }
    private static void AutoWin()
    {
        if (GameAnimationSetting.instance.board != null)
        {
            GameAnimationSetting.instance.BattleBoard.GameEndWindow.Win();
        }
    }
    private static void AutoLose()
    {
        if (GameAnimationSetting.instance.board != null)
        {
            GameAnimationSetting.instance.BattleBoard.GameEndWindow.Lose();
        }
    }
}
