//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class test_Login : MonoBehaviour
//{
//    // �ͻ���
//    void Start()
//    {
//        Network.NetClient.Instance.Init("127.0.0.1", 8000);//��ʼ�������÷����� IP �Ͷ˿�
//        Network.NetClient.Instance.Connect();//�ͻ������ӣ�������

//        //������Ϣ
//        SkillBridge.Message.NetMessage msg = new SkillBridge.Message.NetMessage();//��Ϣ�ķ�װ������Դ˸�ʽ����
//        msg.Request = new SkillBridge.Message.NetMessageRequest();
//        msg.Request.firstRequest = new SkillBridge.Message.FirstTestRequest();//�����Զ�����Ϣ
//        msg.Request.firstRequest.Helloworld = "Hello World!"; //�����Ϣ����
//        Network.NetClient.Instance.SendMessage(msg); //����send

//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }
//}
