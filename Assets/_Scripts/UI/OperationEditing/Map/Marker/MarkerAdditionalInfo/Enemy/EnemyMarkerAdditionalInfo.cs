#region

using UnityEngine;

#endregion

public class EnemyMarkerAdditionalInfo : MarkerAdditionalInfo, IOperationUpdateListener
{
    #region Variables & References

    private EnemyController _currentFollowingEnemy;

    #endregion

    #region Initialize

    public void UpdateOperationsVisuals()
    {
        UpdateMainInfo();
    }

    #endregion

    #region Visuals

    public override void Show(Transform followingTransform, Vector3 additionalTabPosition)
    {
        DisplayEnemyInfo(followingTransform, additionalTabPosition);

        gameObject.SetActive(true);
        UpdateVisuals();
    }

    private void DisplayEnemyInfo(Transform followingTransform, Vector3 additionalInfoTabPosition)
    {
        _currentFollowingEnemy = followingTransform.GetComponent<EnemyController>();

        if (_currentFollowingEnemy == null)
            return;

        _mainPage.InitializePage(followingTransform);

        foreach (MarkerPage markerPage in _otherPages)
        {
            markerPage.InitializePage(followingTransform);
        }

        transform.position = additionalInfoTabPosition;

        _currentFollowingEnemy.HealthComponent.HealthDepleted += CurrentFollowingEnemy_OnEnemyDeath;
    }

    public override void Hide()
    {
        if (_currentFollowingEnemy != null)
            _currentFollowingEnemy.HealthComponent.HealthDepleted -= CurrentFollowingEnemy_OnEnemyDeath;

        gameObject.SetActive(false);
    }

    public override void UpdateVisuals()
    {
        UpdateMainInfo();
        UpdateStatsInfo();
    }

    private void UpdateMainInfo()
    {
        _mainPage.UpdateVisuals();
    }

    private void UpdateStatsInfo()
    {
        foreach (MarkerPage markerPage in _otherPages)
        {
            markerPage.UpdateVisuals();
        }
    }

    private void CurrentFollowingEnemy_OnEnemyDeath()
    {
        Hide();
        _currentFollowingEnemy = null;
    }

    #endregion
}