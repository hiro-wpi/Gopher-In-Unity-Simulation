using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFootSolver : MonoBehaviour
{
    public bool isMovingForward;

    [SerializeField] LayerMask terrainLayer = default;
    [SerializeField] Transform body = default;
    [SerializeField] Transform footTarget = default;
    [SerializeField] IKFootSolver otherFoot = default;
    [SerializeField] float speed = 4;
    [SerializeField] float stepDistance = .2f;
    [SerializeField] float stepLength = .2f;
    [SerializeField] float sideStepLength = .1f;

    [SerializeField] float stepHeight = .3f;
    [SerializeField] Vector3 footOffset = default;

    public Vector3 footRotOffset;
    public float footYPosOffset = 0.1f;

    public float rayStartYOffset = 0;
    public float rayLength = 1.5f;
    
    float footSpacing;
    Vector3 oldPosition, currentPosition, newPosition;
    Vector3 oldNormal, currentNormal, newNormal;
    float lerp;

    private void Start()
    {
        if (footTarget == null)
            footTarget = this.transform;

        footSpacing = footTarget.transform.localPosition.x;
        currentPosition = newPosition = oldPosition = footTarget.transform.position;
        currentNormal = newNormal = oldNormal = footTarget.transform.up;
        lerp = 1;
    }

    // Update is called once per frame

    void LateUpdate()
    {
        footTarget.transform.position = currentPosition + Vector3.up * footYPosOffset;
        footTarget.transform.localRotation = Quaternion.Euler(footRotOffset);

        Ray ray = new Ray(body.position + (body.right * footSpacing) + Vector3.up * rayStartYOffset, Vector3.down);

        Debug.DrawRay(body.position + (body.right * footSpacing) + Vector3.up * rayStartYOffset, Vector3.down);
            
        if (Physics.Raycast(ray, out RaycastHit info, rayLength, terrainLayer.value))
        {
            if (Vector3.Distance(newPosition, info.point) > stepDistance && !otherFoot.IsMoving() && lerp >= 1)
            {
                lerp = 0;
                Vector3 direction = Vector3.ProjectOnPlane(info.point - currentPosition,Vector3.up).normalized;

                float angle = Vector3.Angle(body.forward, body.InverseTransformDirection(direction));

                isMovingForward = angle < 50 || angle > 130;

                if(isMovingForward)
                {
                    newPosition = info.point + direction * stepLength + footOffset;
                    newNormal = info.normal;
                }
                else
                {
                    newPosition = info.point + direction * sideStepLength + footOffset;
                    newNormal = info.normal;
                }

            }
        }

        if (lerp < 1)
        {
            Vector3 tempPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
            tempPosition.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            currentPosition = tempPosition;
            currentNormal = Vector3.Lerp(oldNormal, newNormal, lerp);
            lerp += Time.deltaTime * speed;
        }
        else
        {
            oldPosition = newPosition;
            oldNormal = newNormal;
        }
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(newPosition, 0.1f);
    }



    public bool IsMoving()
    {
        return lerp < 1;
    }



}
