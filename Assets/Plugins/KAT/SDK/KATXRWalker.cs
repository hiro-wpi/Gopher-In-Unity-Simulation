using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class KATXRWalker : MonoBehaviour
{
    [Range(0.5f,10.0f)]
    public float speedMul = 1.0f;

    public GameObject xr;
    public GameObject eye;

    public enum ExecuteMethod
    {
        RigidBody,
        CharactorController,
        MovePosition
    }

    public ExecuteMethod executeMethod = ExecuteMethod.RigidBody;

    protected Vector3 lastPosition = Vector3.zero;
    //protected Vector3 defaultEyeOffset = Vector3.zero;

    protected float yawCorrection;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        var ws = KATNativeSDK.GetWalkStatus();

        if (!ws.connected)
        {
            return;
        }

        //Calibration Stage 
        if(ws.deviceDatas[0].btnPressed)
        {
            var hmdYaw = eye.transform.eulerAngles.y;
            var bodyYaw = ws.bodyRotationRaw.eulerAngles.y;

            yawCorrection = bodyYaw - hmdYaw;

            var pos = transform.position;
            var eyePos = eye.transform.position;
            pos.x = eyePos.x;
            pos.z = eyePos.z;
            transform.position = pos;
            lastPosition = transform.position;
            return;
        }

        transform.rotation = ws.bodyRotationRaw * Quaternion.Inverse( Quaternion.Euler(new Vector3(0,yawCorrection,0)));

        if (Input.GetKey(KeyCode.W))
        {
            ws.moveSpeed = new Vector3(0, 0, 1);
        }

        switch(executeMethod)
        {
            case ExecuteMethod.CharactorController: 
                {
                    var ch = GetComponent<CharacterController>();
                    ch.SimpleMove(transform.rotation * ws.moveSpeed);
                }
                break;
            case ExecuteMethod.MovePosition:
                {
                    transform.position += (transform.rotation * ws.moveSpeed * Time.fixedDeltaTime);
                }
                break;
            case ExecuteMethod.RigidBody:
                {
                    var r = GetComponent<Rigidbody>();
                    r.velocity = transform.rotation * ws.moveSpeed;
                }
                break;
        } 
    }


    void LateUpdate()
    {
        var offset = transform.position - lastPosition;
        offset.y = 0;
        xr.transform.position += offset;

        lastPosition = transform.position;
    }
}
