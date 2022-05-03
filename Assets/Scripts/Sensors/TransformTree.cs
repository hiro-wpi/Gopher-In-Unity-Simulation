using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.Core;
using Unity.Robotics.UrdfImporter;
using RosMessageTypes.Geometry;

/// <summary>
///     This script reads the tree structures of all
///     URDF links.
/// </summary>
public class TransformTree
{
    public readonly GameObject SceneObject;
    public readonly List<TransformTree> Children;
    public Transform Transform => SceneObject.transform;
    public string name => SceneObject.name;
    public bool IsALeafNode => Children.Count == 0;

    public TransformTree(GameObject sceneObject)
    {
        SceneObject = sceneObject;
        Children = new List<TransformTree>();
        PopulateChildNodes(this);
    }

    public static TransformStampedMsg ToTransformStamped(TransformTree node)
    {

        return node.Transform.ToROSTransformStamped(Clock.time, node.name);
    }

    private static void PopulateChildNodes(TransformTree tfNode)
    {
        var parentTransform = tfNode.Transform;
        for (var childIndex = 0; childIndex < parentTransform.childCount; ++childIndex)
        {
            var childTransform = parentTransform.GetChild(childIndex);
            var childGO = childTransform.gameObject;

            // If game object has a URDFLink attached, it's a link in the transform tree
            if (childGO.TryGetComponent(out UrdfLink _))
            {
                var childNode = new TransformTree(childGO);
                tfNode.Children.Add(childNode);
            }
        }
    }
}
