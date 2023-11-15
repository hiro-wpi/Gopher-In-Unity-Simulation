using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Locomotion provider that allows the user to smoothly move their rig continuously over time
    /// using a specified input action.
    /// </summary>
    /// <seealso cref="LocomotionProvider"/>
    public class TreadmillMoveProvider : ContinuousMoveProviderBase
    {
        [SerializeField] private TreadmillReader treadmillReader;

        /// <inheritdoc />
        protected override Vector2 ReadInput()
        {
            return treadmillReader.GetVelocity();
        }
    }
}
