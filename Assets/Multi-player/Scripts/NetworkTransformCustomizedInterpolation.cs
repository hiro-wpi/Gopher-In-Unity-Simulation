using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Netcode.Components
{
    /// <summary>
    /// A NetworkTransform that allows to change the interpolation time.
    /// It is ServerAuthoritative
    /// 
    /// The NetworkTransform is not working well with the current
    /// Movable objects settings. This script is a simple workaround.
    /// </summary>
    public class NetworkTransformCustomizedInterpolation : NetworkBehaviour
    {
        // Network object
        [SerializeField] private bool SyncPosition = true;
        [SerializeField] private bool SyncRotation = true;

        // Network status
        [SerializeField, ReadOnly]
        private NetworkVariable<Vector3> position = 
            new NetworkVariable<Vector3>(Vector3.zero);
        [SerializeField, ReadOnly]
        private NetworkVariable<Quaternion> rotation = 
            new NetworkVariable<Quaternion>(Quaternion.identity);

        // Thresholds
        [SerializeField] private float PositionThreshold = 0.001f;
        [SerializeField] private float RotAngleThreshold = 0.01f;

        [SerializeField] private float interpolationTime = 0.02f;
        private Vector3 positionVelocity = Vector3.zero;
        private Quaternion rotationVelocity = Quaternion.identity;

        void FixedUpdate()
        {
            if (!IsSpawned)
            {
                return;
            }

            // Server, update state
            if (IsServer)
            {
                UpdateValueServer();
            }

            // Non-server, update the transform based on the value
            else
            {
                UpdateValueClient();
            }
        }

        private void UpdateValueServer()
        {
            // Read and store values
            if (SyncPosition)
            {
                position.Value = transform.position;
            }
            if (SyncRotation)
            {
                rotation.Value = transform.rotation;
            }
        }

        private void UpdateValueClient()
        {
            // Update position
            if (
                SyncPosition
                && Vector3.Distance(transform.position, position.Value)
                > PositionThreshold
            ) {
                transform.position = Vector3.SmoothDamp(
                    transform.position, 
                    position.Value, 
                    ref positionVelocity, 
                    interpolationTime
                );
            }
            
            // Update rotation
            if (
                SyncRotation
                && Quaternion.Angle(transform.rotation, rotation.Value)
                > RotAngleThreshold
            ) {
                transform.rotation = Utils.QuaternionSmoothDamp(
                    transform.rotation,
                    rotation.Value,
                    ref rotationVelocity,
                    interpolationTime
                );
            }
        }
    }
}