using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class GameUtil : Singleton<GameUtil>
{
    public bool InScreen(Vector3 position)
    { //Camera.main.WorldToScreenPoint(position) 将世界坐标 转为-> 屏幕坐标
        return Screen.safeArea.Contains(Camera.main.WorldToScreenPoint(position));
    }
}