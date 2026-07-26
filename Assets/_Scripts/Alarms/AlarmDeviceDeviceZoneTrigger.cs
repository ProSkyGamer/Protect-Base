#region

using System;
using UnityEngine;
using Zenject;

#endregion

public class AlarmDeviceDeviceZoneTrigger : MonoBehaviour, IAlarmDeviceTriggerer
{
    #region Events

    public event Action<int> OnAlarmDeviceTriggered;

    #endregion

    #region Variables & References

    [SerializeField] private int _triggeringAlarmDeviceIndex;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(AlarmsManager alarmsManager)
    {
        int minDeviceNumber = alarmsManager.MinDeviceNumber;
        int maxDeviceNumber = alarmsManager.MaxDeviceNumber;

        if (_triggeringAlarmDeviceIndex > maxDeviceNumber)
            _triggeringAlarmDeviceIndex = maxDeviceNumber;
        else if (_triggeringAlarmDeviceIndex < minDeviceNumber)
            _triggeringAlarmDeviceIndex = minDeviceNumber;
    }

    #endregion

    #region Collision

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out EnemyController _))
            OnAlarmDeviceTriggered?.Invoke(_triggeringAlarmDeviceIndex);
    }

    #endregion
}