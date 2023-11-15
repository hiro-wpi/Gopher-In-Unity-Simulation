using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Locomotion provider that allows the user to smoothly rotate their rig continuously over time
    /// using a specified input action.
    /// </summary>
    /// <seealso cref="LocomotionProvider"/>
    public class TreadmillTurnProvider : ContinuousTurnProviderBase
    {
        [SerializeField] private TreadmillReader treadmillReader;

        /// <inheritdoc />
        protected override Vector2 ReadInput()
        {
            return new Vector2(treadmillReader.GetRotation(), 0);
        }
    }
}
