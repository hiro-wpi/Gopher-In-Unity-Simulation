using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Localization parent class
/// </summary>
public abstract class Localization : MonoBehaviour
{
    [SerializeField] protected static float updateRate = 10f;
    private float updateTime = 1 / updateRate;
    private float elapsedTime = 0f;

    [field:SerializeField, ReadOnly]
    public Vector3 Position { get; protected set; }
    [field:SerializeField, ReadOnly]
    public Vector3 RotationEuler { get; protected set; }

    void Start() {}

    void FixedUpdate() 
    {
        elapsedTime += Time.fixedDeltaTime;
        if (elapsedTime >= updateTime)
        {
            UpdateLocalization();
            elapsedTime -= Time.fixedDeltaTime;
        }
    }

    public abstract void UpdateLocalization();
}
