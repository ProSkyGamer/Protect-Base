#region

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class EnemyMainInfoMarkerPage : MarkerPage
{
    #region Variables & References

    [SerializeField] private Image _currentEnemyTypeIcon;
    [SerializeField] private Image _currentHealthBarImage;
    [SerializeField] private TextMeshProUGUI _currentHealthText;

    private EnumTranslationValuesSO _enumTranslationValuesSO;
    private EnemyController _currentFollowingEnemy;
    private StringFormatsSO _stringFormatsSO;

    #endregion

    #region Intitialization

    [Inject]
    public void Construct(EnumTranslationValuesSO enumTranslationValuesSO, StringFormatsSO stringFormatsSO)
    {
        _enumTranslationValuesSO = enumTranslationValuesSO;
        _stringFormatsSO = stringFormatsSO;
    }

    public override void InitializePage(Transform followingObject)
    {
        _currentFollowingEnemy = followingObject.GetComponent<EnemyController>();

        base.InitializePage(followingObject);
    }

    #endregion

    #region Visuals

    public override void UpdateVisuals()
    {
        _currentEnemyTypeIcon.sprite =
            _enumTranslationValuesSO.GetEnemyTypeSprite(_currentFollowingEnemy.EnemyType);

        float currentEnemyHealth = _currentFollowingEnemy.HealthComponent.CurrentHealth;
        float maxEnemyHealth = _currentFollowingEnemy.HealthComponent.MaxHealth;

        _currentHealthBarImage.fillAmount = currentEnemyHealth / maxEnemyHealth;
        string currentHealthString = string.Format(_stringFormatsSO.CurrentHealthFormatString, currentEnemyHealth, maxEnemyHealth);
        _currentHealthText.text = currentHealthString;
    }

    #endregion
}