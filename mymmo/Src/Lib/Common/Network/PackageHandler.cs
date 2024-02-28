// RayMix Libs - RayMix的.Net库
// 版权所有 2018 Ray@raymix.net。保留所有权利。
// https://www.raymix.net
//
// 在源代码和二进制形式中的重新分发，无论是否进行修改，只要满足以下条件即可：

// * 必须保留源代码的版权声明、此条件列表和以下免责声明。
// * 以二进制形式重新分发时，必须在提供的文档和/或其他材料中复制上述版权声明、此条件列表和以下免责声明。
// * 未经特定事先书面许可，不得使用RayMix.net的名称或其贡献者的名称来认可或推广从本软件派生的产品。

// 本软件由版权持有人和贡献者提供，按原样提供，任何明示或暗示的担保，包括但不限于适销性和适用于特定目的的暗示担保均被拒绝。
// 在任何情况下，无论是合同、严格责任还是侵权行为，版权所有人或贡献者均不对任何直接、间接、偶然、特殊、惩罚性或后果性的损害
// （包括但不限于替代商品或服务的采购、使用、数据或利润的损失；或业务中断）负责，即使已被告知此类损害的可能性。

using System;
using System.IO;

namespace Network
{
    /// <summary>
    /// PackageHandler 是用于处理网络数据包的类，主要实现了数据包的接收、解析和打包等功能。
    /// PackageHandler 是简单的继承类，只是传递了 sender 给基类的构造函数
    /// </summary>
    public class PackageHandler : PackageHandler<object>
    {
        public PackageHandler(object sender) : base(sender)
        {
        }
    }

    /// <summary>
    /// PackageHandler<T>
    /// 数据包处理器
    /// </summary>
    /// <typeparam name="T">消息发送者类型</typeparam>
    public class PackageHandler<T>
    {
        //创建一个 MemoryStream 对象，用于在内存中存储接收到的数据包。初始容量为 64KB。
        //stream.Position 是指当前 MemoryStream 流的位置。在流中读写数据时，这个指针会移动，指向当前操作的位置。
        private MemoryStream stream = new MemoryStream(64 * 1024);

        private int readOffset = 0; //记录已经读取的数据包的偏移量

        private T sender; //消息的发送者

        public PackageHandler(T sender)
        {
            this.sender = sender;//初始化数据包处理器，并传递消息的发送者。
        }

        /// <summary>
        /// ReceiveData用于接收数据，将接收到的数据追加到 stream 中，并调用 ParsePackage 进行数据包解析。
        /// </summary>
        public void ReceiveData(byte[] data, int offset, int count)
        {
            if (stream.Position + count > stream.Capacity)//如果数据超出了 stream 的容量，将抛出异常
            {
                throw new Exception("PackageHandler write buffer overflow");
            }
            stream.Write(data, offset, count);//接收到的数据追加到 stream 流中

            ParsePackage();//数据包解析
        }

        /// <summary>
        /// 将网络协议消息打包成字节数组
        /// </summary>
        /// 使用 Protocol Buffers 序列化网络协议消息对象 message，将消息长度和消息内容拷贝到一个新的字节数组中，然后返回这个字节数组。
        public static byte[] PackMessage(SkillBridge.Message.NetMessage message)
        {
            byte[] package = null;
            using (MemoryStream ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, message);//序列化网络协议消息对象message
                package = new byte[ms.Length + 4];
                Buffer.BlockCopy(BitConverter.GetBytes(ms.Length), 0, package, 0, 4);//将待存储的数据包的大小，保存在package的前 4 个字节中，便于解析
                Buffer.BlockCopy(ms.GetBuffer(), 0, package, 4, (int)ms.Length);//package的第4个字节后，存储 message的序列化消息对象
            }
            return package;
        }

        /// <summary>
        /// 解析字节数组中的消息。它将字节数组 packet 中，从 offset 位置开始的 length 长度的数据，
        /// 反序列化为 SkillBridge.Message.NetMessage 类型的消息对象，并返回。
        /// </summary>
        public static SkillBridge.Message.NetMessage UnpackMessage(byte[] packet, int offset, int length)
        {
            SkillBridge.Message.NetMessage message = null;
            using (MemoryStream ms = new MemoryStream(packet, offset, length))
            {
                message = ProtoBuf.Serializer.Deserialize<SkillBridge.Message.NetMessage>(ms);
            }
            return message;
        }

        /// <summary>
        /// 数据包解析，用于从 stream 中解析数据包
        /// 
        /// 它首先检查是否有足够的数据可用以形成完整的包。如果是，它会提取包的大小，然后解析出消息内容。
        /// 接着，它将消息传递给消息分发器 MessageDistributer<T>.Instance，以便将消息传递给正确的接收者。
        /// 最后，它更新 readOffset，指示已读取的数据包。如果还有剩余数据包，递归调用 ParsePackage 继续解析。
        /// 
        /// 如果还有部分数据包未接收完，会将未接收完的数据复制到 stream 的开头，然后截断 stream。
        /// 这样做是为了保证下一次数据接收时，能够正确解析数据包。（解决粘包的问题）
        /// </summary>
        bool ParsePackage()
        {
            if (readOffset + 4 < stream.Position)//检查在当前流的位置是否有足够的字节，以便解析出数据包的大小。数据包 的前 4 个字节表示数据包的大小
            {
                //BitConverter.ToInt32 用于将缓冲区字节数组的前四个字节转换为 int 类型，解析出数据包的大小。
                int packageSize = BitConverter.ToInt32(stream.GetBuffer(), readOffset);
                //packageSize 表示的是数据包的实际大小，不包括 用于表示数据包大小的前4个字节
                //在当前已读取的数据包的末尾 再加上一个可能存在的新的数据包的头部（4个字节） 是否<= 当前流的位置。
                if (packageSize + readOffset + 4 <= stream.Position)//检查是否有足够的数据可以形成一个完整的数据包，若条件不满足则需要等待更多数据的到来。
                {//若有足够的数据来形成一个有效数据包
                    //调用 UnpackMessage 方法来解析数据包，返回消息对象
                    SkillBridge.Message.NetMessage message = UnpackMessage(stream.GetBuffer(), this.readOffset + 4, packageSize);
                    if (message == null)//检查是否成功解析了消息
                    {
                        throw new Exception("PackageHandler ParsePackage faild,invalid package");
                    }
                    //如果消息成功解析，添加到服务器接收的消息队列中 。this.sender 表示消息的发送者。
                    MessageDistributer<T>.Instance.ReceiveMessage(this.sender, message);
                    this.readOffset += (packageSize + 4);//更新读取偏移量，指示已经读取的数据包的末尾位置。
                    return ParsePackage();//递归调用 ParsePackage 继续解析 剩余的数据包
                }
            }

            //当不足以解析出下一个数据包时，可能还有部分数据包未接收完
            if (this.readOffset > 0)
            {
                long size = stream.Position - this.readOffset;//计算未处理的数据大小，即数据流中剩余的数据
                if (this.readOffset < stream.Position)//将未处理的数据复制到数据流的开头，以便下次处理。
                { //Array.Copy 将未处理的数据从当前 readOffset 位置，复制到数据流的起始位置。
                    Array.Copy(stream.GetBuffer(), this.readOffset, stream.GetBuffer(), 0, stream.Position - this.readOffset);
                }
                //是确保下一次数据到达时，数据流处于正确的状态，可以正确解析新的数据包。
                this.readOffset = 0;    //重置读取偏移量，表示下次从头开始读取新的数据。
                stream.Position = size; //将数据流的位置设置为未处理数据的末尾，以便在这个位置开始接收新的数据。
                stream.SetLength(size); //设置数据流的长度为未处理数据的长度
            }
            // 数据包已经全部处理完
            return true;
        }
    }
}
