//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class test_Login : MonoBehaviour
//{
//    // 客户端
//    void Start()
//    {
//        Network.NetClient.Instance.Init("127.0.0.1", 8000);//初始化，设置服务器 IP 和端口
//        Network.NetClient.Instance.Connect();//客户端连接，服务器

//        //发送消息
//        SkillBridge.Message.NetMessage msg = new SkillBridge.Message.NetMessage();//消息的封装，最后以此格式发送
//        msg.Request = new SkillBridge.Message.NetMessageRequest();
//        msg.Request.firstRequest = new SkillBridge.Message.FirstTestRequest();//创建自定义消息
//        msg.Request.firstRequest.Helloworld = "Hello World!"; //填充消息内容
//        Network.NetClient.Instance.SendMessage(msg); //调用send

//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }
//}
