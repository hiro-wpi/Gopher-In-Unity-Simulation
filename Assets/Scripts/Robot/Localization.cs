using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Localization parent class
/// </summary>
public abstract class Localization : MonoBehaviour
{
    [field:SerializeField, ReadOnly]
    public Vector3 Position { get; protected set; }
    [field:SerializeField, ReadOnly]
    public Vector3 Rotation { get; protected set; }

    void Start() {}

    void Update() {}

    public abstract void UpdateLocalization();
}
