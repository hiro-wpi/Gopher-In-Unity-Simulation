using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Detect the surrounding objects 
///     Obstacle can be splited into obstacles & human.
/// </summary>
public class SurroundingDetection : MonoBehaviour
{
    // general
    public GameObject centerObject;
    protected Vector3 rayStartPosition;
    protected Vector3 rayStartForward;
    public bool seperateHumanDetection = true;
    public bool debugVisualization = false;
    // scan params
    public int updateRate = 10;
    protected float scanTime;
    public int samples = 180;
    public float angleMin = -1.5708f;
    public float angleMax = 1.5708f;
    protected float angleIncrement;
    public float rangeMin = 0.1f;
    public float rangeMax = 5.0f;
    // containers
    protected RaycastHit[] raycastHits;
    protected Quaternion[] rayRotations;
    public float[] directions;
    public float[] obstacleRanges;
    public float[] humanRanges;
    

    protected void Start()
    {
        // Containers
        rayRotations = new Quaternion[samples];
        directions = new float[samples];
        obstacleRanges = new float[samples];
        humanRanges = new float[samples];

        // Calculate resolution based on angle limit and number of samples
        angleIncrement = (angleMax - angleMin) / (samples - 1);
        for (int i = 0; i < samples; ++i)
        {
            directions[i] = angleMin + i * angleIncrement;
            rayRotations[i] = 
                Quaternion.Euler(new Vector3(0f, directions[i] * Mathf.Rad2Deg, 0f));
        }

        // Start scanning
        scanTime = 1f / updateRate;
        InvokeRepeating("Scan", 1f, scanTime);
    }

    private void Scan()
    {
        obstacleRanges = new float[samples];
        humanRanges = new float[samples];

        // Cast rays towards diffent directions to find colliders
        rayStartPosition = centerObject.transform.position;
        rayStartForward = centerObject.transform.forward;
        for (int i = 0; i < samples; ++i)
        {
            // Ray angle
            Vector3 rotation = rayRotations[i] * rayStartForward;
            // Check if hit colliders within distance
            raycastHits = new RaycastHit[samples];
            if (Physics.Raycast(rayStartPosition, rotation, out raycastHits[i], rangeMax) 
                && (raycastHits[i].distance >= rangeMin)
                && (!raycastHits[i].collider.isTrigger) )
            {
                // Human detection seperated
                if (seperateHumanDetection &&
                    raycastHits[i].collider.gameObject.tag == "Human")
                {
                    humanRanges[i] = raycastHits[i].distance;
                }
                // Regular scan
                else
                {
                    obstacleRanges[i] = raycastHits[i].distance;
                }

                // Visualization
                if (debugVisualization)
                {
                    Debug.DrawRay(rayStartPosition, 
                                  obstacleRanges[i] * rotation, 
                                  Color.red, scanTime);
                    Debug.DrawRay(rayStartPosition, 
                                  humanRanges[i] * rotation, 
                                  Color.blue, scanTime);
                }
            }
        }
    }

    public Vector3[] GetScanResultPositions(int minIndex = 0, int maxIndex = int.MaxValue, 
                                            bool getHuman = false)
    {
        if (maxIndex == int.MaxValue)
            maxIndex = samples;

        // Get position that has valid scanning results
        List<Vector3> positions = new List<Vector3>();
        for (int i = minIndex; i < maxIndex; ++i)
        {
            Vector3 rotation = rayRotations[i] * rayStartForward;
            if (!getHuman)
                positions.Add(rayStartPosition + obstacleRanges[i] * rotation);
            else
                positions.Add(rayStartPosition + humanRanges[i] * rotation);
        }
        return positions.ToArray();
    }
}