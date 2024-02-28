using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkillBridge.Message;
using UnityEngine;

namespace Entities
{
    public class Character : Entity
    {
        // Character 角色类 ，包含所有玩家的 character
        public NCharacterInfo Info; //保存 从服务器上同步到客户端的NCharacterInfo

        public Common.Data.CharacterDefine Define;

        public int Id //DB_id
        {
            get { return this.Info.Id; }
        }

        public string Name
        {
            get
            {
                if (this.Info.Type == CharacterType.Player)
                    return this.Info.Name;
                else
                    return this.Define.Name;
            }
        }

        public bool IsPlayer //是否是玩家（非怪物）？
        {
            get
            {
                return this.Info.Type == CharacterType.Player;
            }
        }

        public bool IsCurrentPlayer //是否是当前玩家（由自己控制）
        {
            get
            {
                if (!IsPlayer) return false; 
                return this.Info.Id == Models.User.Instance.CurrentCharacter.Id;
            }
        }

        public Character(NCharacterInfo info) : base(info.Entity)
        {
            this.Info = info;
            this.Define = DataManager.Instance.Characters[info.ConfigId];
        }

        public void MoveForward()
        {
            Debug.LogFormat("MoveForward");
            this.speed = this.Define.Speed;
        }

        public void MoveBack()
        {
            Debug.LogFormat("MoveBack");
            this.speed = -this.Define.Speed;
        }

        public void Stop()
        {
            Debug.LogFormat("Stop");
            this.speed = 0;
        }

        public void SetDirection(Vector3Int direction)
        {
            Debug.LogFormat("SetDirection:{0}", direction);
            this.direction = direction;
        }

        public void SetPosition(Vector3Int position)
        {
            Debug.LogFormat("SetPosition:{0}", position);
            this.position = position;
        }
    }
}
