#region

using System;

#endregion

public interface IPreSettingTriggerer
{
    public event Action<int> PreSettingTriggered;
}