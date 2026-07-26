#region

using UnityEngine;

#endregion

public interface IPathPointCoordinatesListener
{
    public void PathPointCoordinatesSelected(Vector2 screenCenteredPosition, Vector3 worldPointPosition);
    public void PathPointCoordinatesSelectionCanceled();
}