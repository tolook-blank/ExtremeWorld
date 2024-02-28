using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UIIconItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //背包道具 预制体
    public Image mainImage; //道具图标显示
    public Image SecondImage; //预留

    public Text Count;//右下角 道具数量

    public void SetMainIcon(string iconName, string count)//设置道具图标、道具数量
    {
        this.mainImage.overrideSprite = Resloader.Load<Sprite>(iconName);
        this.Count.text = count;
    }

    /// <summary>
    /// 实现拖动背包格子中的道具，实现位置变换
    /// </summary>

    private Transform beginParentTransform; //开始拖拽道具时，记录下此父级对象，后续用于位置变化        
    private Transform topOfUiT;
    void Start()
    {
        topOfUiT = GameObject.Find("TabView").transform;
    }

    /// <summary>
    /// 在开始拖拽时被调用。它首先检查当前道具的父级是否为UI顶层，如果是则直接返回; 否则
    /// </summary>
    public void OnBeginDrag(PointerEventData _)
    {
        if (transform.parent == topOfUiT) return;
        beginParentTransform = transform.parent;//开始拖拽道具时，记录该 起始道具格的位置
        transform.SetParent(topOfUiT);//移动当前道具 到topOfUiT子集下 暂时保存着
    }

    /// <summary>
    /// 在拖拽过程中被调用。它将道具的位置设置为当前鼠标位置，并关闭道具的射线检测，使得其他UI元素不会响应鼠标事件。
    /// </summary>
    public void OnDrag(PointerEventData _)
    {
        transform.position = Input.mousePosition;
        if (transform.GetComponent<Image>().raycastTarget) transform.GetComponent<Image>().raycastTarget = false;
    }

    /// <summary>
    /// 在拖拽结束时被调用。它首先判断拖拽结束时 光标位置下的游戏对象PointerEventData.pointerCurrentRaycast.gameObject 的标签
    /// 如果其标签是"Grid"，表示目标位置是一个空格子，那么就将道具的位置设置为该格子的位置，并将道具的父级设置为该格子。
    /// 如果其标签是"BagItem"，表示目标位置是另一个道具，互换位置：将当前道具 设置为 目标道具的格子的子级并归正位置，将目标道具 设置为 当前道具初始父级的子级。
    /// 如果是其他情况，则将道具的位置设置回初始位置。
    /// </summary>
    public void OnEndDrag(PointerEventData _)
    {
        GameObject go = _.pointerCurrentRaycast.gameObject; //拖拽结束时 光标位置下的游戏对象
        if (go.tag == "Grid" && go.GetComponent<Image>().color != Color.gray) //如果目标位置处 是已经解锁的空格子 ，
        {
            SetPosAndParent(transform, go.transform); //将当前道具移动到此空格子中
            transform.GetComponent<Image>().raycastTarget = true;
        }
        else if (go.tag == "BagItem") //如果目标位置处 是道具，互换位置
        {
            SetPosAndParent(transform, go.transform.parent); //当前拖拽道具 移动到 目标道具的格子下
            SetPosAndParent(go.transform, beginParentTransform);//目标道具 移动到 当前拖拽道具的格子下
            transform.GetComponent<Image>().raycastTarget = true; //将当前道具的Image组件的射线检测重新打开，使得其他UI元素可以与之交互。
        }
        else //其他任何情况，当前拖拽道具 回归原始格子   
        {
            SetPosAndParent(transform, beginParentTransform);
            transform.GetComponent<Image>().raycastTarget = true;
        }
    }

    // 设置ts的父级为parent，并将其位置归正为父级位置
    private void SetPosAndParent(Transform ts, Transform parent)
    {
        ts.SetParent(parent);
        ts.position = parent.position;
    }
}
