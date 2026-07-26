#region

using System.Collections.Generic;
using UnityEngine;

#endregion

public class CameraSystemTrigger : MonoBehaviour, ISceneResettable
{
    #region Variables & References

    [SerializeField] private CameraSystemSingle _triggeringCameraSystem;
    [SerializeField] private bool _isLeftHalf;

    private readonly List<EnemyController> _allEnteredEnemies = new();
    private bool _isTriggered;

    #endregion

    #region Collision

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out EnemyController enteredEnemy) == false)
            return;

        TryTriggerCamera(enteredEnemy);
    }

    private void TryTriggerCamera(EnemyController enteredEnemy)
    {
        _allEnteredEnemies.Add(enteredEnemy);
        enteredEnemy.HealthComponent.HealthDepleted += EnteredEnemy_OnEnemyDeath;

        if (_isTriggered)
            return;

        _triggeringCameraSystem.TriggerCamera(_isLeftHalf);
        _isTriggered = true;
    }

    private void EnteredEnemy_OnEnemyDeath()
    {
        TryEndCameraTrigger();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out EnemyController _) == false)
            return;

        TryEndCameraTrigger();
    }

    private void TryEndCameraTrigger()
    {
        _allEnteredEnemies.RemoveAll(enteredEnemy => enteredEnemy.HealthComponent.IsDestroyed);

        if (_isTriggered && _allEnteredEnemies.Count == 0)
        {
            _triggeringCameraSystem.EndCameraTrigger(_isLeftHalf);
            _isTriggered = false;
        }
    }

    #endregion

    public void OnSceneReset()
    {
        _isTriggered = false;
        _allEnteredEnemies.Clear();
    }
}