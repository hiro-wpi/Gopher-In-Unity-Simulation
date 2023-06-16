//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Moveit
{
    [Serializable]
    public class PlanningSceneMsg : Message
    {
        public const string k_RosMessageName = "moveit_msgs/PlanningScene";
        public override string RosMessageName => k_RosMessageName;

        //  name of planning scene
        public string name;
        //  full robot state
        public RobotStateMsg robot_state;
        //  The name of the robot model this scene is for
        public string robot_model_name;
        // additional frames for duplicating tf (with respect to the planning frame)
        public Geometry.TransformStampedMsg[] fixed_frame_transforms;
        // full allowed collision matrix
        public AllowedCollisionMatrixMsg allowed_collision_matrix;
        //  all link paddings
        public LinkPaddingMsg[] link_padding;
        //  all link scales
        public LinkScaleMsg[] link_scale;
        //  Attached objects, collision objects, even the octomap or collision map can have
        //  colors associated to them. This array specifies them.
        public ObjectColorMsg[] object_colors;
        //  the collision map
        public PlanningSceneWorldMsg world;
        //  Flag indicating whether this scene is to be interpreted as a diff with respect to some other scene
        public bool is_diff;

        public PlanningSceneMsg()
        {
            this.name = "";
            this.robot_state = new RobotStateMsg();
            this.robot_model_name = "";
            this.fixed_frame_transforms = new Geometry.TransformStampedMsg[0];
            this.allowed_collision_matrix = new AllowedCollisionMatrixMsg();
            this.link_padding = new LinkPaddingMsg[0];
            this.link_scale = new LinkScaleMsg[0];
            this.object_colors = new ObjectColorMsg[0];
            this.world = new PlanningSceneWorldMsg();
            this.is_diff = false;
        }

        public PlanningSceneMsg(string name, RobotStateMsg robot_state, string robot_model_name, Geometry.TransformStampedMsg[] fixed_frame_transforms, AllowedCollisionMatrixMsg allowed_collision_matrix, LinkPaddingMsg[] link_padding, LinkScaleMsg[] link_scale, ObjectColorMsg[] object_colors, PlanningSceneWorldMsg world, bool is_diff)
        {
            this.name = name;
            this.robot_state = robot_state;
            this.robot_model_name = robot_model_name;
            this.fixed_frame_transforms = fixed_frame_transforms;
            this.allowed_collision_matrix = allowed_collision_matrix;
            this.link_padding = link_padding;
            this.link_scale = link_scale;
            this.object_colors = object_colors;
            this.world = world;
            this.is_diff = is_diff;
        }

        public static PlanningSceneMsg Deserialize(MessageDeserializer deserializer) => new PlanningSceneMsg(deserializer);

        private PlanningSceneMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.name);
            this.robot_state = RobotStateMsg.Deserialize(deserializer);
            deserializer.Read(out this.robot_model_name);
            deserializer.Read(out this.fixed_frame_transforms, Geometry.TransformStampedMsg.Deserialize, deserializer.ReadLength());
            this.allowed_collision_matrix = AllowedCollisionMatrixMsg.Deserialize(deserializer);
            deserializer.Read(out this.link_padding, LinkPaddingMsg.Deserialize, deserializer.ReadLength());
            deserializer.Read(out this.link_scale, LinkScaleMsg.Deserialize, deserializer.ReadLength());
            deserializer.Read(out this.object_colors, ObjectColorMsg.Deserialize, deserializer.ReadLength());
            this.world = PlanningSceneWorldMsg.Deserialize(deserializer);
            deserializer.Read(out this.is_diff);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.name);
            serializer.Write(this.robot_state);
            serializer.Write(this.robot_model_name);
            serializer.WriteLength(this.fixed_frame_transforms);
            serializer.Write(this.fixed_frame_transforms);
            serializer.Write(this.allowed_collision_matrix);
            serializer.WriteLength(this.link_padding);
            serializer.Write(this.link_padding);
            serializer.WriteLength(this.link_scale);
            serializer.Write(this.link_scale);
            serializer.WriteLength(this.object_colors);
            serializer.Write(this.object_colors);
            serializer.Write(this.world);
            serializer.Write(this.is_diff);
        }

        public override string ToString()
        {
            return "PlanningSceneMsg: " +
            "\nname: " + name.ToString() +
            "\nrobot_state: " + robot_state.ToString() +
            "\nrobot_model_name: " + robot_model_name.ToString() +
            "\nfixed_frame_transforms: " + System.String.Join(", ", fixed_frame_transforms.ToList()) +
            "\nallowed_collision_matrix: " + allowed_collision_matrix.ToString() +
            "\nlink_padding: " + System.String.Join(", ", link_padding.ToList()) +
            "\nlink_scale: " + System.String.Join(", ", link_scale.ToList()) +
            "\nobject_colors: " + System.String.Join(", ", object_colors.ToList()) +
            "\nworld: " + world.ToString() +
            "\nis_diff: " + is_diff.ToString();
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
