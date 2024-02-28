using Models;
using UnityEngine;
using UnityEngine.UI;
using Managers;

public class UIMinimap : MonoBehaviour
{    //小地图，绑定在主城的 UIMain/Minimap上
    //地图资源制作：预渲染顶视图：使用顶视图，用一个包围盒Cube 恰好覆盖地图场景，则 Cube的长宽 就是 地图的长宽
    public Collider minimapBoundingBox; //地图盒（对比出地图的大小），用于实现小地图与场景的坐标映射
    public Image minimap; //地图
    public Image arrow;   //导航箭头，指示玩家在地图中的位置
    public Text mapName;  //地图名

    private Transform playerTransform; //玩家的坐标
                                       
    void Start()
    {
        MinimapManager.Instance.Minimap = this;//MinimapManager 管理 minimap 的信息，UIMinimap 从 MinimapManager 中请求到数据
        this.UpdateMap();
    }

    public void UpdateMap() //切换地图时，要更新为对应的小地图
    {
        this.mapName.text = User.Instance.CurrentMapData.Name;
        this.minimap.overrideSprite = MinimapManager.Instance.LoadCurrentMinimap(); //更新小地图

        this.minimap.SetNativeSize(); //设置小地图资源的尺寸
        this.minimap.transform.localPosition = Vector3.zero;
        this.minimapBoundingBox = MinimapManager.Instance.MinimapBoundingBox;
        this.playerTransform = null; //每当角色离开地图，就会销毁角色游戏对象（进入地图重新再创建），所以将playerTransform置为null

        //当角色删除后，再选择角色进入地图OnMapCharacterEnter时，Minimap可能先初始化了，但CreateCharacterObject还未执行到，CurrentCharacterObject还为null,就会引发CurrentCharacterObject.transform空异常
        //this.playerTransform = User.Instance.CurrentCharacterObject.transform;//初始化玩家坐标
    }

    //实现小地图与场景的坐标映射
    void Update() 
    {
        if (playerTransform == null)//当角色切换地图时，需要初始化玩家坐标 （因为角色离开地图，就销毁角色游戏对象，进入地图重新再创建 ）
        {
            playerTransform = MinimapManager.Instance.PlayerTransform;
        }

        //当角色离开、进入地图时，因为无法保证组件删除、添加的顺序，所以要添加安全检查
        if (minimapBoundingBox == null || playerTransform == null) { return; }

        float realWidth = minimapBoundingBox.bounds.size.x;//（顶视图平面）地图的宽度、高度，通过地图包围盒在 x 、z轴上的尺寸来获取
        float realHeight = minimapBoundingBox.bounds.size.z;

        //在顶视图平面地图中，以地图包围盒左下角为原点， 计算玩家的位置（x和z轴），地图左下角坐标为minimapBoundingBox.bounds.min.x/z
        float relaX = this.playerTransform.position.x - minimapBoundingBox.bounds.min.x;
        float relaY = this.playerTransform.position.z - minimapBoundingBox.bounds.min.z;

        //计算顶视图下， 玩家在地图上的pivot中心点的比例值，为 玩家坐标/地图长宽 ，
        float pivotX = relaX / realWidth;//
        float pivotY = relaY / realHeight;

        //小地图上显示的是中心点附近区域， 通过 玩家和小地图的坐标映射得到中心点（pivotX，pivotY），再 由中心点的移动，来实现小地图的对应移动
        //localPosition为自身矩形中心点 (Pivot)与其父节点矩形中心点 (Pivot)的相对位置坐标，是本物体相对于父物体位置的偏移信息
        this.minimap.rectTransform.pivot = new Vector2(pivotX, pivotY);//以顶视图下，玩家在地图上的中心点位置，更新小地图的中心点，此时小地图的localPosition的值相应变动，使小地图保持原来位置不变
        this.minimap.rectTransform.localPosition = Vector2.zero;//再始终将小地图的本地位置设置为0，只由中心点来控制移动， 才能使小地图跟随中心点移动显示

        //小箭头转动，将Unity游戏中 玩家绕Y轴旋转 转换成 小地图中 箭头绕Z轴旋转，坐标系转换
        this.arrow.transform.eulerAngles = new Vector3(0, 0, -playerTransform.eulerAngles.y);//小箭头arrow指示玩家的方向
    }
}
