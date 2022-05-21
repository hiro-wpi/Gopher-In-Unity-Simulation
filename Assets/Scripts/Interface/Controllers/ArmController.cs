using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine;

public class ArmController : MonoBehaviour
{
    public ArticulationJointController articulationJointController;
    public ArticulationGripperController articulationGripperController;
    public NewtonIK newtonIK;

    // ENUM for mode CONTROL or TARGET
    private enum Mode
    {
        Control,
        Target,
        Pending
    }

    private Mode _mode = Mode.Pending;

    // For CONTROL
    public float moveSpeed = 0.005f;
    public float rotateSpeed = 0.25f;
    private Vector3 _deltaPosition = Vector3.zero;
    private Vector3 _deltaRotation = Vector3.zero;
    private float _gripperPosition;

    // For TARGET
    public Grabbable target;

    // General
    private float[] _jointAngles;
    private Coroutine _targetCoroutine;

    private void Start()
    {
        StartCoroutine(SwitchToControl());
    }

    private IEnumerator SwitchToControl()
    {
        // wait for articulationJointController.HomeJointsCoroutine();
        yield return new WaitUntil(() => articulationJointController.HomeAndCheck());
        _jointAngles = articulationJointController.GetCurrentJointTargets();
        _mode = Mode.Control;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _deltaPosition = context.ReadValue<Vector3>() * moveSpeed;
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        _deltaRotation = context.ReadValue<Vector3>() * rotateSpeed;
    }

    public void OnGrip(InputAction.CallbackContext context)
    {
        _gripperPosition = context.ReadValue<float>();
    }

    public void FixedUpdate()
    {
        if (_mode == Mode.Control)
        {
            Quaternion deltaRotationQuaternion = Quaternion.Euler(_deltaRotation);
            _jointAngles = articulationJointController.GetCurrentJointTargets();
            _jointAngles = newtonIK.SolveVelocityIK(_jointAngles, _deltaPosition, deltaRotationQuaternion);
            articulationJointController.SetJointTargets(_jointAngles);
            articulationGripperController.SetGrippers(_gripperPosition);
        }
    }

    public void OnTarget(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_targetCoroutine != null) StopCoroutine(_targetCoroutine);
            _targetCoroutine = StartCoroutine(MoveToTargets(target));
        }
    }

    private IEnumerator LerpJoints(float[] currentAngles, float[] targetJointAngles, float seconds)
    {
        float elapsedTime = 0;
        // Keep track of starting angles
        var startingAngles = currentAngles.Clone() as float[];
        while (elapsedTime < seconds)
        {
            // lerp each joint angle in loop
            // calculate smallest difference between current and target joint angle
            // using atan2(sin(x-y), cos(x-y))
            for (var i = 0; i < targetJointAngles.Length; i++)
            {
                currentAngles[i] = Mathf.Lerp(startingAngles[i], targetJointAngles[i], (elapsedTime / seconds));
            }

            elapsedTime += Time.deltaTime;

            articulationJointController.SetJointTargets(currentAngles);

            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator MoveToTargets(Grabbable grabTarget)
    {
        // Lock controls
        _mode = Mode.Target;

        var targetPosition = grabTarget.hoverPoint.position;
        var targetRotation = grabTarget.hoverPoint.rotation;

        _jointAngles = articulationJointController.GetCurrentJointTargets();
        var (converged, targetJointAngles) =
            newtonIK.SolveIK(_jointAngles, targetPosition, targetRotation);
        if (!converged)
        {
            _mode = Mode.Control;
            yield break;
        }

        // Lerp between points
        yield return LerpJoints(_jointAngles, targetJointAngles, 5.0f);

        targetPosition = grabTarget.grabPoint.position;
        targetRotation = grabTarget.grabPoint.rotation;

        // Assume we got to the target
        _jointAngles = targetJointAngles;
        (converged, targetJointAngles) = newtonIK.SolveIK(_jointAngles, targetPosition, targetRotation);
        if (!converged)
        {
            _mode = Mode.Control;
            yield break;
        }

        yield return LerpJoints(_jointAngles, targetJointAngles, 5.0f);

        // close the gripper
        articulationGripperController.SetGrippers(1.0f);

        // move back to hover point
        targetPosition = grabTarget.hoverPoint.position;
        targetRotation = grabTarget.hoverPoint.rotation;

        // Assume we got to the target
        _jointAngles = targetJointAngles;
        (converged, targetJointAngles) = newtonIK.SolveIK(_jointAngles, targetPosition, targetRotation);
        if (!converged)
        {
            _mode = Mode.Control;
            yield break;
        }

        yield return LerpJoints(_jointAngles, targetJointAngles, 5.0f);
        _mode = Mode.Control;
    }
}