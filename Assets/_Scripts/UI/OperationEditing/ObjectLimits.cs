#region

using UnityEngine;

#endregion

public class ObjectLimits
{
    public ObjectLimits(Vector2 minPoint, Vector2 maxPoint)
    {
        MinPoint = minPoint;
        MaxPoint = maxPoint;
    }

    public readonly Vector2 MinPoint;
    public readonly Vector2 MaxPoint;

    public bool IsPointWithinBoundaries(Vector2 checkingPoint)
    {
        return checkingPoint.x < MaxPoint.x &&
               checkingPoint.x > MinPoint.x &&
               checkingPoint.y < MaxPoint.y &&
               checkingPoint.y > MinPoint.y;
    }

    public bool IsRectWithinBoundaries(Vector2 bottomLeftPosition, Vector2 topRightPosition)
    {
        if (bottomLeftPosition.x > topRightPosition.x)
            (bottomLeftPosition.x, topRightPosition.x) = (topRightPosition.x, bottomLeftPosition.x);

        if (bottomLeftPosition.y > topRightPosition.y)
            (bottomLeftPosition.y, topRightPosition.y) = (topRightPosition.y, bottomLeftPosition.y);

        return bottomLeftPosition.x > MinPoint.x &&
               bottomLeftPosition.y > MinPoint.y &&
               topRightPosition.x < MaxPoint.x &&
               topRightPosition.y < MaxPoint.y;
    }
}