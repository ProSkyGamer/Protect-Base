#region

using TMPro;
using UnityEngine;

#endregion

public class EnemyStatsInfoMarkerPage : MarkerPage
{
    #region Variables & References

    [SerializeField] private TextMeshProUGUI _currentEnemyAttackText;
    [SerializeField] private TextMeshProUGUI _currentEnemySpeedText;

    private EnemyController _currentFollowingEnemy;

    #endregion

    #region Initialization

    public override void InitializePage(Transform followingObject)
    {
        _currentFollowingEnemy = followingObject.GetComponent<EnemyController>();

        base.InitializePage(followingObject);
    }

    #endregion

    #region Visuals

    public override void UpdateVisuals()
    {
        int currentEnemySpeed = Mathf.RoundToInt(_currentFollowingEnemy.CurrentSpeed);
        _currentEnemySpeedText.text = currentEnemySpeed.ToString();

        int currentEnemyAttack = Mathf.RoundToInt(_currentFollowingEnemy.CurrentAtk);
        _currentEnemyAttackText.text = currentEnemyAttack.ToString();
    }

    #endregion
}