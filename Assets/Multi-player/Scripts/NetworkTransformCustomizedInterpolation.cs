using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Netcode.Components
{
    /// <summary>
    /// A NetworkTransform that allows to change the interpolation time.
    /// It is ServerAuthoritative.
    /// 
    /// The NetworkTransform is not working well with the current
    /// Movable objects settings. This script is a simple workaround.
    /// </summary>
    public class NetworkTransformCustomizedInterpolation : NetworkBehaviour
    {
        // Network robot status
        [SerializeField, ReadOnly] 
        private NetworkVariable<Vector3> position = 
            new NetworkVariable<Vector3>(Vector3.zero);
        [SerializeField, ReadOnly] 
        private NetworkVariable<Quaternion> rotation = 
            new NetworkVariable<Quaternion>(Quaternion.identity);

        [SerializeField] private float PositionThreshold = 0.001f;
        [SerializeField] private float RotAngleThreshold = 0.01f;

        [SerializeField] private float interpolationTime = 0.05f;
        private Vector3 positionVelocity = Vector3.zero;
        private Quaternion rotationVelocity = Quaternion.identity;

        void FixedUpdate()
        {
            // Server, update state
            if (IsServer)
            {
                // read joint angles from articulation body
                position.Value = transform.position;
                rotation.Value = transform.rotation;
            }

            // Non-server, update the transform based on the value
            else
            {
                // Update position
                if (
                    Vector3.Distance(transform.position, position.Value)
                    > PositionThreshold
                )
                {
                    transform.position = Vector3.SmoothDamp(
                        transform.position, 
                        position.Value, 
                        ref positionVelocity, 
                        interpolationTime
                    );
                }
                
                // Update rotation
                if (
                    Quaternion.Angle(transform.rotation, rotation.Value)
                    > RotAngleThreshold
                )
                {
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
}