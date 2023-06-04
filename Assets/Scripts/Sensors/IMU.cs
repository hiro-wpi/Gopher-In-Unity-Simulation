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
    // [SerializeField] private int updateRate = 30;

    // Noise
    [SerializeField] private float linearBiasMean = 0.1f;
    [SerializeField] private float linearBiasStd = 0.001f;
    [SerializeField] private float angularBiasMean = 7.5e-6f;
    [SerializeField] private float angularBiasStd = 8.0e-7f;

    // Readings
    [SerializeField, ReadOnly] private Quaternion orientation;
    [SerializeField, ReadOnly] private Vector3 linearAcceleration;
    [SerializeField, ReadOnly] private Vector3 angularVelocity;
    private Vector3 linearVelocity;
    private Vector3 lastPosition;
    private Vector3 lastRotationEuler;
    private Vector3 lastLinearVelocity;

    void Start() 
    {
        rb = robot.GetComponentInChildren<Rigidbody>();
        if (rb == null)
        {
            // Debug.LogWarning("No RigidBody found on robot. Using transform instead.");
            lastPosition = robot.transform.position;
        }

        // TODO Include update rate
    }

    void FixedUpdate() 
    {
        // Orientation
        orientation = robot.transform.rotation;

        // Linear & Angular velocity
        if (rb != null)
        {
            linearVelocity = rb.velocity;
            angularVelocity = rb.angularVelocity;
        }
        else
        {
            // lienar and angular velocity from transform
            linearVelocity = (
                robot.transform.position - lastPosition
            ) / Time.fixedDeltaTime;
            lastPosition = robot.transform.position;

            angularVelocity = (
                robot.transform.rotation.eulerAngles - lastRotationEuler
            ) / Time.fixedDeltaTime;
            lastRotationEuler = robot.transform.rotation.eulerAngles;
        }

        // Linear acceleration
        linearVelocity = robot.transform.InverseTransformDirection(linearVelocity);
        linearAcceleration = (linearVelocity - lastLinearVelocity) / Time.fixedDeltaTime;
        lastLinearVelocity = linearVelocity;

        // Add noise
        linearAcceleration = ApplyBiasNoise(
            linearAcceleration, linearBiasMean, linearBiasStd
        );
        angularVelocity = ApplyBiasNoise(
            angularVelocity, angularBiasMean, angularBiasStd
        );
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
