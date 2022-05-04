using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    // This script denotes a "Grabbable" object
    public Transform grabPoint;
    public Transform hoverPoint;

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(grabPoint.position, 0.025f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(hoverPoint.position, 0.025f);

        DrawAxes(grabPoint);
        DrawAxes(hoverPoint);
    }

    private void DrawAxes(Transform t)
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(t.position, t.position + t.forward * 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(t.position, t.position + t.up * 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(t.position, t.position + t.right * 0.1f);
    }
}
