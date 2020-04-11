using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 顺滑卡牌移动,并且添加稍许旋转效果
/// 必需有父节点
/// </summary>
public class LagPosition : MonoBehaviour
{
    [Tooltip("X轴向旋转程度")]
    public float rotateX = 0.1f;
    [Tooltip("Y轴向旋转程度")]
    public float rotateY = 0.1f;
    [Tooltip("X轴向移动速度")]
    public float speedX = 10f;
    [Tooltip("Y轴向移动速度")]
    public float speedY = 10f;

    private void OnEnable()
    {
        
    }
    private void OnDisable()
    {
        
    }

    private void FixedUpdate()
    {
        
    }
}
