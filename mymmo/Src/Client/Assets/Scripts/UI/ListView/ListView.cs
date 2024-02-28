using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class ListView : MonoBehaviour
{
    //只要UI中有列表、列表项，就可以使用的通用模板
    public UnityAction<ListViewItem> onItemSelected;//当列表项被选中时触发的动作

    //ListViewItem表示列表中的每一项
    public class ListViewItem : MonoBehaviour, IPointerClickHandler //实现鼠标点击选中OnPointerClick
    {
        public ListView owner;//拥有该列表项的 ListView 实例

        private bool selected;//表示列表项是否被选中
        public bool Selected //获取或设置列表项的选中状态，并在状态变化时触发 onSelected 方法
        {
            get { return selected; }
            set
            {
                selected = value;
                onSelected(selected);
            }
        }
        public virtual void onSelected(bool selected)
        {

        }

        public void OnPointerClick(PointerEventData eventData) //任务列表中 的任务项，要能够点击选中，并且在右侧面板显示 详细信息
        {
            //if (!this.selected)
            //{
            //    this.Selected = true; //只将当前项设为选中状态
            //}
            this.Selected = !this.Selected;
            if (this.Selected)
            {
                if (owner != null)// && owner.SelectedItem != this
                {
                    owner.SelectedItem = this;
                }
            }
            SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
        }
    }

    List<ListViewItem> items = new List<ListViewItem>();//存储所有列表项

    private ListViewItem selectedItem = null;//当前选中的列表项
    public ListViewItem SelectedItem
    {
        get { return selectedItem; }
        private set
        {//private set 确保属性只能在类的内部进行修改
            if (selectedItem != null && selectedItem != value)
            {
                selectedItem.Selected = false; //先将原先选中的项设为非选中状态
            }
            //当selectedItem ==null时，触发选中通知会报异常
            selectedItem = value;//然后再设置新的选中项
            if (onItemSelected != null)
            {
                onItemSelected.Invoke((ListViewItem)value);
            }

        }
    }

    public void AddItem(ListViewItem item)//将一个列表项添加到列表中
    {
        item.owner = this;
        this.items.Add(item);
    }

    public void RemoveAll()//清除所有的列表项
    {
        foreach (var it in items)
        {
            Destroy(it.gameObject);
        }
        items.Clear();
    }
}
