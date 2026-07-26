#region

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

#endregion

public class TimeDutyModeTab : DutyModeTab, IDisposable
{
    #region Variables & References

    [SerializeField] private TextMeshProUGUI _currentTimeText;

    private readonly float _updatingTimeInterval = 1f;
    private readonly CancellationTokenSource _updateCancellationToken = new();

    private ICurrentDateTimeProvider _dateTimeProvider;

    #endregion

    #region Properties

    public override DutyModeTabType DutyModeTabType => DutyModeTabType.Time;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(ICurrentDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public override void Initialize()
    {
        ContinuouslyUpdateTimeAsync(_updateCancellationToken.Token).Forget();
    }

    #endregion

    #region Update

    private async UniTaskVoid ContinuouslyUpdateTimeAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            await UniTask.WaitForSeconds(_updatingTimeInterval, cancellationToken: cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return;

            UpdateCurrentTime();
        }
    }

    #endregion

    #region Visuals

    private void UpdateCurrentTime()
    {
        _currentTimeText.text = _dateTimeProvider.GetTimeFormattedString(_dateTimeProvider.CurrentDateTime);
    }

    public override void UpdateTabVisual(IFiringMachineDataProvider currentFiringMachineDataProvider)
    {
        UpdateCurrentTime();
    }

    #endregion

    public override void Dispose()
    {
        _updateCancellationToken.Cancel();
    }
}