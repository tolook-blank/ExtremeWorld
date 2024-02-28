using Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    class MinimapManager : Singleton<MinimapManager>
    {
        private UIMinimap minimap;
        public UIMinimap Minimap //MinimapManager 管理 UIMinimap 小地图UI
        {
            get { return minimap; }
            set {
                minimap = value;
                Debug.LogWarningFormat("MinimapManager.Instance.Minimap[{0}] Set", minimap.GetInstanceID());
            }
        }

        private Collider minimapBoundingBox;//使用顶视图，用一个包围盒Cube 恰好覆盖地图场景，则 Cube的长宽 就是 地图的长宽
        public Collider MinimapBoundingBox
        {
            get { return minimapBoundingBox; }
        }

        public Transform PlayerTransform //当前玩家坐标
        {
            get
            {
                if (User.Instance.CurrentCharacterObject == null)
                {
                    return null;
                }
                return User.Instance.CurrentCharacterObject.transform;
            }
        }

        public Sprite LoadCurrentMinimap()//从 Resources/UI/Minimap/ 目录下，加载当前地图Sprite资源
        {
            return Resloader.Load<Sprite>("UI/Minimap/" + User.Instance.CurrentMapData.Minimap);
        }

        public void UpdateMinimap(Collider minimapBoundingBox)//当地图发送变化时，调用UpdateMinimap，更新包围盒
        {
            this.minimapBoundingBox = minimapBoundingBox;//先更新包围盒
            if(this.minimap != null)
            {
                this.minimap.UpdateMap();
            }
        }
    }
}
