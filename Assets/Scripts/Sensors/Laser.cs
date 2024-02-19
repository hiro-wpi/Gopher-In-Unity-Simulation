using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Collections;
using Unity.Jobs;

/// <summary>
///     Detect the surrounding objects using raycast.
///     Obstacle can be splited into obstacles & human.
///
///     Send batches of raycasts to be processed in jobs,
///     which is shown in https://github.com/LotteMakesStuff/SimplePhysicsDemo
/// </summary>
public class Laser : MonoBehaviour
{
    // General
    [SerializeField] private GameObject laserGameObject;
    private Vector3 lastScanPosition;
    private Quaternion lastScanRotation;
    [SerializeField] private bool enableHumanDetection = true;
    [SerializeField] private bool debugVisualization = false;

    // Scan parameters
    [SerializeField] private int updateRate = 10;
    private Timer timer = new Timer(10);
    [SerializeField] private int samples = 180;
    [SerializeField] private float angleMin = -1.5708f;
    [SerializeField] private float angleMax = 1.5708f;
    private float angleIncrement;
    [SerializeField] private float rangeMin = 0.1f;
    [SerializeField] private float rangeMax = 5.0f;
    public (int, int, float, float, float, float) GetLaserScanParameters()
    {
        return (updateRate, samples, angleMin, angleMax, rangeMin, rangeMax);
    }

    // Gameobject to ignore
    public GameObject filteredGraspedObject = null;

    // Scan sending
    private NativeArray<Quaternion> raycastRotations;
    private NativeArray<RaycastCommand> raycastCommands;
    // create a batch of RaycastCommands to detect collisions
    private struct PrepareRaycastCommands : IJobParallelFor
    {
        public Vector3 Position;
        public Vector3 Forward;
        public float MaxDistance;
        public NativeArray<Quaternion> RaycastRotations;
        // result
        public NativeArray<RaycastCommand> RaycastCommands;
        public void Execute(int i)
        {
            RaycastCommands[i] = new RaycastCommand(
                Position,
                RaycastRotations[i] * Forward,
                QueryParameters.Default,
                distance: MaxDistance
            );
        }
    }
    // Scan reading
    private NativeArray<RaycastHit> raycastHits;

    // Scan processing
    private JobHandle ProcessHitsJobHandle;
    private NativeArray<float> obstacleDistances;
    private NativeArray<float> humanDistances;
    // create a batch for collision callbacks
    private struct ProcessHits : IJobParallelFor
    {
        public NativeArray<RaycastHit> RaycastHits;
        public float MinDistance;
        // ignore gameobject
        // public NativeArray<char> ignoredGameObjectName;
        // result
        public NativeArray<float> ObstacleDistances;
        public NativeArray<float> HumanDistances;

        public void Execute(int i)
        {
            RaycastHit hit = RaycastHits[i];

            // No hit or is the object we want to ignore
            if (hit.distance < MinDistance || hit.distance == 0f)
            {
                ObstacleDistances[i] = float.PositiveInfinity;
                HumanDistances[i] = float.PositiveInfinity;
            }
            // Scan hit
            else
            {
                ObstacleDistances[i] = hit.distance;
            }
        }
    };

    // Results
    [field: SerializeField, ReadOnly]
    public float[] Directions { get; private set; }
    [field: SerializeField, ReadOnly]
    public float[] ObstacleRanges { get; private set; }
    [field: SerializeField, ReadOnly]
    public float[] HumanRanges { get; private set; }

    // Event
    public delegate void ScanFinishedHandler();
    public event ScanFinishedHandler ScanFinishedEvent;

    void Start()
    {
        // Initialize result containers
        ObstacleRanges = new float[samples];
        HumanRanges = new float[samples];

        // Update rate
        timer = new Timer(updateRate);
    }

    void OnEnable()
    {
        Directions = new float[samples];

        // Calculate angles based on angle limit and number of samples
        raycastRotations = new NativeArray<Quaternion>(samples, Allocator.Persistent);
        angleIncrement = (angleMax - angleMin) / (samples - 1);
        for (int i = 0; i < samples; ++i)
        {
            Directions[i] = angleMin + i * angleIncrement;
            raycastRotations[i] = Quaternion.Euler(
                new Vector3(0f, Directions[i] * Mathf.Rad2Deg, 0f)
            );
        }

        // Initialize result containers
        obstacleDistances = new(samples, Allocator.Persistent);
        humanDistances = new(samples, Allocator.Persistent);
    }

    void OnDisable()
    {
        raycastRotations.Dispose();
        obstacleDistances.Dispose();
        humanDistances.Dispose();
    }

    void FixedUpdate()
    {
        // Time reached check
        timer.UpdateTimer(Time.fixedDeltaTime);
        if (!timer.ShouldProcess || !ProcessHitsJobHandle.IsCompleted)
        {
            return;
        }
        timer.ShouldProcess = false;

        // Schedule jobs to send raycast commands
        raycastCommands = new(samples, Allocator.TempJob);
        PrepareRaycastCommands setupRaycastsJob = new()
        {
            Position = laserGameObject.transform.position,
            Forward = laserGameObject.transform.forward,
            RaycastRotations = raycastRotations,
            MaxDistance = rangeMax,
            RaycastCommands = raycastCommands
        };
        JobHandle setupDenpendency = setupRaycastsJob.Schedule(samples, 32);

        // Schedule jobs to read raycast results
        raycastHits = new(samples, Allocator.TempJob);
        JobHandle raycastsDependency = RaycastCommand.ScheduleBatch(
            raycastCommands, raycastHits, 32, setupDenpendency
        );

        // Schedule jobs to process raycast results
        var processHitsJob = new ProcessHits()
        {
            RaycastHits = raycastHits,
            MinDistance = rangeMin,
            // ignoredGameObjectName = new NativeArray<char>("".ToCharArray(), Allocator.TempJob),
            ObstacleDistances = obstacleDistances,
            HumanDistances = humanDistances
        };
        ProcessHitsJobHandle = processHitsJob.Schedule(
            samples, 32, raycastsDependency
        );

        // End job
        ProcessHitsJobHandle.Complete();
        raycastCommands.Dispose();

        // Human detection involves tag/string checking
        // which is not supported in burst/jobs
        if (enableHumanDetection)
        {
            for (int i = 0; i < samples; ++i)
            {
                RaycastHit hit = raycastHits[i];
                if (hit.collider?.gameObject.tag != "Human")
                {
                    humanDistances[i] = hit.distance;
                }
            }
        }

        // filtering the object we want to ignore
        if (filteredGraspedObject != null)
        {
            for (int i = 0; i < samples; ++i)
            {
                RaycastHit hit = raycastHits[i];
                if (hit.collider != null)
                {
                    if (hit.collider.transform.IsChildOf(filteredGraspedObject.transform))
                    {
                        obstacleDistances[i] = float.PositiveInfinity;
                        humanDistances[i] = float.PositiveInfinity;
                    }
                }

            }
        }
        raycastHits.Dispose();

        // Convert result to regular array
        ObstacleRanges = obstacleDistances.ToArray();
        HumanRanges = humanDistances.ToArray();
        // Trigger event
        ScanFinishedEvent?.Invoke();

        lastScanPosition = laserGameObject.transform.position;
        lastScanRotation = laserGameObject.transform.rotation;
        // Visualization
        if (debugVisualization)
        {
            DrawLaserScan(ObstacleRanges, Color.red);
            DrawLaserScan(HumanRanges, Color.blue);
        }
    }

    private void DrawLaserScan(float[] ranges, Color color)
    {
        // Draw laser scan
        for (int i = 0; i < samples; ++i)
        {
            Vector3 rotation = (
                raycastRotations[i] * laserGameObject.transform.forward
            );
            if (ranges[i] != float.PositiveInfinity && ranges[i] != 0.0f)
            {
                Debug.DrawRay(
                    laserGameObject.transform.position,
                    ranges[i] * rotation,
                    color,
                    1f / updateRate
                );
            }
        }
    }

    public (bool[], Vector3[]) GetObstaclePositions()
    {
        Vector3[] positions = new Vector3[samples];
        bool[] detected = new bool[samples];
        for (int i = 0; i < samples; ++i)
        {
            Vector3 rotation = (
                raycastRotations[i] * (lastScanRotation * Vector3.forward)
            );

            // Check if the range is valid (not infinity or zero)
            if (
                ObstacleRanges[i] != float.PositiveInfinity
                && ObstacleRanges[i] != 0.0f
            )
            {
                // Calculate the position based on the laser's position,
                // the direction, and the range
                positions[i] = lastScanPosition + rotation * ObstacleRanges[i];
                detected[i] = true;
            }
            else
            {
                detected[i] = false;
            }
        }
        return (detected, positions);
    }
}
