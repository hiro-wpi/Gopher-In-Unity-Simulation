using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Simulate 2D localization result with gaussian error
/// </summary>
public class Localization : MonoBehaviour
{
    public float updateRate = 10f;

    public GameObject robot;
    public Vector3 position;
    public Vector3 rotation;
    private Vector3 prevPosition;
    private Vector3 prevRotation;

    private System.Random rand = new System.Random();
    public float positionErrorStd = 0.2f;
    public float rotationErrorStd = 0.2f;
    private Vector3 positionError;
    private Vector3 rotationError;
    
    public float updatePositionDiff = 1.0f;
    public float updateRotationDiff = 30f;

    void Start()
    {
        position = robot.transform.position;
        rotation = robot.transform.rotation.eulerAngles;
        prevPosition = robot.transform.position;
        prevRotation = robot.transform.rotation.eulerAngles;
    }

    void FixedUpdate()
    {
        // Update error if needed
        if (((robot.transform.position - prevPosition).magnitude 
             > updatePositionDiff) ||
            ((robot.transform.rotation.eulerAngles - prevRotation).magnitude 
             > updateRotationDiff)
           )
        {
            prevPosition = robot.transform.position;
            prevRotation = robot.transform.rotation.eulerAngles;
            UpdateLocalizationError();
        }

        position = robot.transform.position + positionError;
        rotation = robot.transform.rotation.eulerAngles + rotationError;
    }

    private void UpdateLocalizationError()
    {
        // Update position
        positionError = new Vector3(GenerateGaussian(0f, positionErrorStd), 
                                    0f, GenerateGaussian(0f, positionErrorStd));
        // Update rotation
        rotationError = new Vector3(0f, GenerateGaussian(0f, rotationErrorStd), 0f);
    }


    // Box-Muller transform from
    // https://stackoverflow.com/questions/218060/random-gaussian-variables
    private float GenerateGaussian(float mean, float std)
    {
        // uniform (0,1] random doubles
        float u1 = 1.0f - (float) rand.NextDouble(); 
        float u2 = 1.0f - (float) rand.NextDouble();
        // random normal (0,1)
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                              Mathf.Sin(2.0f * Mathf.PI * u2);
        
        return mean + std * randStdNormal;
    }
}
