#region

using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class EnemyUpdatableMarkerSingleUI : BaseUpdatableMarkerSingleUI
{
    #region Variables & References

    [SerializeField] private Image _enemyMarkerImage;
    [SerializeField] private Image _selectedMarkerImage;

    private EnemyController _enemyController;
    private bool _isInitialized;

    private EnumTranslationValuesSO _enumTranslationValuesSO;
    private OperationTerritoryManager _operationTerritoryManager;

    #endregion

    #region Initialize

    [Inject]
    public void Construct(EnumTranslationValuesSO enumTranslationValuesSO,
        OperationTerritoryManager operationTerritoryManager)
    {
        _enumTranslationValuesSO = enumTranslationValuesSO;
        _operationTerritoryManager = operationTerritoryManager;
    }

    public override void Initialize()
    {
        _isInitialized = FollowingObjectTransform.TryGetComponent(out EnemyController baseEnemyController);

        if (_isInitialized == false)
        {
            Destroy(gameObject);

            return;
        }

        _enemyMarkerImage.sprite =
            _enumTranslationValuesSO.GetEnemyTypeSprite(baseEnemyController.EnemyType);

        _selectedMarkerImage.sprite =
            _enumTranslationValuesSO.GetEnemyTypeSprite(baseEnemyController.EnemyType);
    }

    #endregion

    #region Visuals

    protected override void UpdateVisuals()
    {
        if (_isInitialized == false)
            return;

        Vector3 objectWorldPosition = FollowingObjectTransform.position;
        Vector2 objectMapPosition = _operationTerritoryManager.GetMapPointFromWorldPoint(objectWorldPosition);
        transform.position = objectMapPosition;
    }

    #endregion

    public override void OnSceneReset()
    {
    }
}