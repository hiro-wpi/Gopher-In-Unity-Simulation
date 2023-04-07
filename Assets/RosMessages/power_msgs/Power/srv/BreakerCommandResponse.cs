//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Power
{
    [Serializable]
    public class BreakerCommandResponse : Message
    {
        public const string k_RosMessageName = "power_msgs/BreakerCommand";
        public override string RosMessageName => k_RosMessageName;

        public BreakerStateMsg status;

        public BreakerCommandResponse()
        {
            this.status = new BreakerStateMsg();
        }

        public BreakerCommandResponse(BreakerStateMsg status)
        {
            this.status = status;
        }

        public static BreakerCommandResponse Deserialize(MessageDeserializer deserializer) => new BreakerCommandResponse(deserializer);

        private BreakerCommandResponse(MessageDeserializer deserializer)
        {
            this.status = BreakerStateMsg.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.status);
        }

        public override string ToString()
        {
            return "BreakerCommandResponse: " +
            "\nstatus: " + status.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize, MessageSubtopic.Response);
        }
    }
}
