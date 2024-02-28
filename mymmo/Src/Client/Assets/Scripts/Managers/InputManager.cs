using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common.Data;
using Models;
using Services;
using SkillBridge.Message;
using UnityEngine.Events;

namespace Managers
{
    class InputManager : MonoSingleton<InputManager>
    {//作为Mono单例脚本，绑定在Loading场景中的 InputManager 游戏物体上
        public bool IsInputMode = false; //当前是否是聊天输入模式 (为了聊天时，不影响角色的操作，例如：聊天框中输入W，而不使角色向前移动）

        void Start()
        {

        }

        void Update()
        {

        }
    }
}
