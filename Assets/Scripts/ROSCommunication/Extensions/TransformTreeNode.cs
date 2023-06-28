using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.UrdfImporter;

/// <summary>
///     This script provides a tf node class.
/// </summary>
class TransformTreeNode
{
    public readonly GameObject NodeObject;
    public readonly List<TransformTreeNode> Children;

    public Transform Transform => NodeObject.transform;
    public string Name => NodeObject.name;
    public bool IsALeafNode => Children.Count == 0;

    public TransformTreeNode(GameObject nodeObject)
    {
        NodeObject = nodeObject;

        Children = new List<TransformTreeNode>();
        PopulateChildNodes(this);
    }

    private static void PopulateChildNodes(TransformTreeNode tfNode)
    {
        Transform parentTransform = tfNode.Transform;
        for (int childIndex = 0; childIndex < parentTransform.childCount; ++childIndex)
        {
            Transform childTransform = parentTransform.GetChild(childIndex);
            GameObject childGO = childTransform.gameObject;

            // If game object has a URDFLink attached,
            // it's a link in the transform tree
            if (childGO.TryGetComponent(out UrdfLink _))
            {
                TransformTreeNode childNode = new(childGO);
                tfNode.Children.Add(childNode);
            }
        }
    }
}
