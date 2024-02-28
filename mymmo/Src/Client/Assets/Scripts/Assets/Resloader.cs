using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Resloader
{
    public static T Load<T>(string path) where T : UnityEngine.Object
    {
        return Resources.Load<T>(path);//加载存储在Resources 文件夹中的 path 处的资源，省略扩展名
    }
}