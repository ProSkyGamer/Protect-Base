#region

using UnityEngine;

#endregion

public class FiringMachineVisuals : MonoBehaviour
{
    #region Variables & References

    [SerializeField] private Transform _firingMachineRotationTransform;
    [SerializeField] private Transform _firingMachineHorizontalOnlyRotationTransform;
    [SerializeField] private Transform _visualsFiringMachineTopPart;
    [SerializeField] private Transform _visualsFiringMachineBottomPart;

    #endregion

    #region Visuals

    public void SetFiringMachineRotationVisuals(Vector3 newRotation)
    {
        _firingMachineRotationTransform.localEulerAngles = newRotation;
        _visualsFiringMachineTopPart.localEulerAngles = newRotation;

        Vector3 horizontalOnlyRotationEulerAngles = new(0f, newRotation.y, 0f);
        _firingMachineHorizontalOnlyRotationTransform.localEulerAngles = horizontalOnlyRotationEulerAngles;
        _visualsFiringMachineBottomPart.localEulerAngles = horizontalOnlyRotationEulerAngles;
    }

    #endregion
}