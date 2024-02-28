
using UnityEngine;
using UnityEngine.UI;

public class SpriteManager : MonoSingleton<SpriteManager> {
	//作为Mono单例脚本，绑定在Loading场景中的 SpriteManager 游戏物体上
	public Sprite[] classIcons; //战士、法师、弓箭手 图标

}
