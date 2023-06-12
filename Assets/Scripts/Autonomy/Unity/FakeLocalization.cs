using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     A fake localization - 
///     Simulate 2D localization result by adding gaussian error
/// </summary>
public class FakeLocalization : Localization
{
    [SerializeField] private GameObject robot;
    [SerializeField] private float positionErrorStd = 0.2f;
    [SerializeField] private float rotationErrorStd = 0.2f;
    [SerializeField] private float updatePositionThreshold = 1.0f;
    [SerializeField] private float updateRotationThreshold = 30f;

    private System.Random rand = new();
    private Vector3 prevPosition;
    private Vector3 prevRotation;
    private Vector3 positionError;
    private Vector3 rotationError;
    
    void Start()
    {
        Position = robot.transform.position;
        RotationEuler = robot.transform.rotation.eulerAngles;
        prevPosition = Position;
        prevRotation = RotationEuler;
    }

    // void Update() {}

    public override void UpdateLocalization()
    {
        // Generate a new localization error if needed
        if (((robot.transform.position - prevPosition).magnitude 
             > updatePositionThreshold) ||
            ((robot.transform.rotation.eulerAngles - prevRotation).magnitude 
             > updateRotationThreshold))
        {
            prevPosition = robot.transform.position;
            prevRotation = robot.transform.rotation.eulerAngles;
            UpdateLocalizationError();
        }

        // Get a fake localization result
        Position = robot.transform.position + positionError;
        RotationEuler = robot.transform.rotation.eulerAngles + rotationError;
    }

    private void UpdateLocalizationError()
    {
        // update position
        positionError = new Vector3(
            GenerateGaussian(0f, positionErrorStd), 
            0f, 
            GenerateGaussian(0f, positionErrorStd)
        );
        // update rotation
        rotationError = new Vector3(
            0f, 
            GenerateGaussian(0f, rotationErrorStd), 
            0f
        );
    }

    // Box-Muller transform
    private float GenerateGaussian(float mean, float std)
    {
        // uniform (0,1] random doubles
        float u1 = 1.0f - (float) rand.NextDouble(); 
        float u2 = 1.0f - (float) rand.NextDouble();
        // random normal (0,1)
        float randStdNormal = (
            Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2)
        );
        
        return mean + std * randStdNormal;
    }
}
