//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.KortexDriver
{
    [Serializable]
    public class SendGripperCommandRequest : Message
    {
        public const string k_RosMessageName = "kortex_driver/SendGripperCommand";
        public override string RosMessageName => k_RosMessageName;

        public GripperCommandMsg input;

        public SendGripperCommandRequest()
        {
            this.input = new GripperCommandMsg();
        }

        public SendGripperCommandRequest(GripperCommandMsg input)
        {
            this.input = input;
        }

        public static SendGripperCommandRequest Deserialize(MessageDeserializer deserializer) => new SendGripperCommandRequest(deserializer);

        private SendGripperCommandRequest(MessageDeserializer deserializer)
        {
            this.input = GripperCommandMsg.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.input);
        }

        public override string ToString()
        {
            return "SendGripperCommandRequest: " +
            "\ninput: " + input.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
