using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverExample : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// OnPointerEnter 方法在指针进入某个物体时被调用，其中 eventData.pointerEnter 表示指针当前悬停的物体
    /// eventData.hovered 是一个列表，包含了悬停栈中的所有物体，即指针当前下方的所有物体。
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Mouse entered: " + eventData.pointerEnter.name);

        // 获取悬停栈中的物体列表
        foreach (GameObject hoveredObject in eventData.hovered)
        {
            Debug.Log("Hovered object: " + hoveredObject.name);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Mouse exited");
    }
}
