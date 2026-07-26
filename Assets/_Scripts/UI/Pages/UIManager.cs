public class UIManager : ISceneResettable
{
    private IUIPage _currentShowingInterface;

    public void HideCurrentInterface()
    {
        if (_currentShowingInterface == null || _currentShowingInterface.IsCanHide == false) return;

        _currentShowingInterface.RequestHide();
        _currentShowingInterface = null;
    }

    public void ChangeCurrentInterface(IUIPage showingUIPage)
    {
        _currentShowingInterface?.Hide();

        showingUIPage.Show();

        _currentShowingInterface = showingUIPage;
    }

    public void OnSceneReset()
    {
        _currentShowingInterface = null;
    }
}