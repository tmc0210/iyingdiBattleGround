using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FocusShow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject ShowingGo;

    private void Awake()
    {
        ShowingGo.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowingGo.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ShowingGo.SetActive(false);
    }
}
