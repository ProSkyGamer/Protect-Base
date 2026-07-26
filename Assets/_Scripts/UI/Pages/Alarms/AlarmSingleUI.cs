#region

using TMPro;
using UnityEngine;
using Zenject;

#endregion

public class AlarmSingleUI : BaseSelectedItemSingleUI
{
    #region Variables & References

    [SerializeField] private TextMeshProUGUI _deviceNumberText;
    [SerializeField] private TextMeshProUGUI _alarmNumberText;
    [SerializeField] private TextMeshProUGUI _firingMechanismNumberText;
    [SerializeField] private TextMeshProUGUI _preSettingNumberText;
    [SerializeField] private TextMeshProUGUI _firingMechanismEnableTypeText;
    [SerializeField] private TextMeshProUGUI _alarmTypeText;
    [SerializeField] private TextMeshProUGUI _alarmStatusText;
    [SerializeField] private string _alarmNormalStatusString = "Норма";

    private EnumTranslationValuesSO _enumTranslationValuesSO;

    #endregion

    #region Iniailization

    [Inject]
    public void Construct(EnumTranslationValuesSO enumTranslationValuesSO, AlarmSingle alarmSingle)
    {
        _enumTranslationValuesSO = enumTranslationValuesSO;

        Initialize();
        Initialize(alarmSingle);
    }

    private void Initialize(AlarmSingle alarmSingle)
    {
        _deviceNumberText.text = alarmSingle.DeviceNumber.ToString();
        _alarmNumberText.text = alarmSingle.AlarmNumber.ToString();
        _firingMechanismNumberText.text = alarmSingle.FiringMachineNumber.ToString();
        _preSettingNumberText.text = alarmSingle.PreSettingNumber.ToString();

        _firingMechanismEnableTypeText.text = "text";

        _firingMechanismEnableTypeText.text =
            _enumTranslationValuesSO.GetFiringMachineEnableTypeShortText(alarmSingle
                .FiringMachineEnableType);

        _alarmTypeText.text = _enumTranslationValuesSO.GetAlarmTypeShortString(alarmSingle.AlarmType);

        _alarmStatusText.text = _alarmNormalStatusString;
    }

    #endregion
}