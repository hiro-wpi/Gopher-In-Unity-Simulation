using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///    This script simulates a IMU sensor
/// </summary>
public class IMU : MonoBehaviour
{
    // Robot
    [SerializeField] private GameObject robot;
    private Rigidbody rb;

    // Parameter
    [SerializeField] private int updateRate = 30;
    private float scanTime;
    private float elapsedTime = 0f;

    // Noise
    [SerializeField] private float linearBiasMean = 0.1f;
    [SerializeField] private float linearBiasStd = 0.001f;
    [SerializeField] private float angularBiasMean = 7.5e-6f;
    [SerializeField] private float angularBiasStd = 8.0e-7f;

    // Readings
    [field: SerializeField] 
    public Quaternion Orientation { get; private set; }
    [field: SerializeField] 
    public Vector3 LinearAcceleration { get; private set; }
    [field: SerializeField] 
    public Vector3 AngularVelocity { get; private set; }
    private Vector3 linearVelocity;
    private Vector3 lastPosition;
    private Vector3 lastRotationEuler;
    private Vector3 lastLinearVelocity;

    // Event
    public delegate void DataUpdatedHandler();
    public event DataUpdatedHandler DataUpdatedEvent;

    void Start()
    {
        rb = robot.GetComponentInChildren<Rigidbody>();
        if (rb == null)
        {
            // Debug.LogWarning("No RigidBody found on robot. Using transform instead.");
            lastPosition = robot.transform.position;
        }

        // update rate
        scanTime = 1.0f / updateRate;
    }

    void FixedUpdate()
    {
        // Time reached check
        elapsedTime += Time.fixedDeltaTime;
        if (elapsedTime < scanTime)
        {
            return;
        }
        elapsedTime -= scanTime;

        // Orientation
        Orientation = robot.transform.rotation;

        // Linear & Angular velocity
        if (rb != null)
        {
            linearVelocity = rb.velocity;
            AngularVelocity = rb.angularVelocity;
        }
        else
        {
            // lienar and angular velocity from transform
            linearVelocity = (
                robot.transform.position - lastPosition
            ) / Time.fixedDeltaTime;
            lastPosition = robot.transform.position;

            AngularVelocity = (
                robot.transform.rotation.eulerAngles - lastRotationEuler
            ) / Time.fixedDeltaTime;
            lastRotationEuler = robot.transform.rotation.eulerAngles;
        }

        // Linear acceleration
        linearVelocity = robot.transform.InverseTransformDirection(linearVelocity);
        LinearAcceleration = (linearVelocity - lastLinearVelocity) / Time.fixedDeltaTime;
        lastLinearVelocity = linearVelocity;

        // Add noise
        LinearAcceleration = ApplyBiasNoise(
            LinearAcceleration, linearBiasMean, linearBiasStd
        );
        AngularVelocity = ApplyBiasNoise(
            AngularVelocity, angularBiasMean, angularBiasStd
        );

        // Trigger event
        DataUpdatedEvent?.Invoke();
    }

    private Vector3 ApplyBiasNoise(Vector3 value, float mean, float std)
    {
        return value + new Vector3(
            Utils.RandomFlip(Utils.GenerateGaussianRandom(mean, std)),
            Utils.RandomFlip(Utils.GenerateGaussianRandom(mean, std)),
            Utils.RandomFlip(Utils.GenerateGaussianRandom(mean, std))
        );
    }
}
