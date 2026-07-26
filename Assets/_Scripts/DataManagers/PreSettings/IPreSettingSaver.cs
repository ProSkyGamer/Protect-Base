public interface IPreSettingSaver
{
    public bool IsEnteringPreSetting { get; }

    public void StartEnteringPreSettingNumber();

    public void StartSavingPreSettingNumber();

    public void ProcessPreSettingNumberInput(char addingNumber);

    public void FinishEnteringPreSettingNumber();

    public void ResetInteraction();
}