#region

using UnityEngine;

#endregion

public static class BallisticsHelper
{
    private const float DefaultGravity = 9.81f;
    private const float OptimalAngle = 45f;

    public static Vector3 GetTrajectoryPoint(float currentTime, float totalTime, float distance,
        float gravity = DefaultGravity)
    {
        float angle = Mathf.Atan(gravity * totalTime * totalTime / (2 * distance));
        float speed = gravity * totalTime / (2 * Mathf.Sin(angle));

        float x = speed * Mathf.Cos(angle) * currentTime;
        float y = speed * Mathf.Sin(angle) * currentTime - 0.5f * gravity * currentTime * currentTime;

        return new Vector3(0f, y, x);
    }

    public static float CalculateFlightTime(float distance, float gravity = DefaultGravity)
    {
        // Используем оптимальный угол 45 градусов для максимальной дальности
        float angleInRad = OptimalAngle * Mathf.Deg2Rad;

        // Формула времени полета: t = sqrt(2 * d * sin(θ) / g)
        float flightTime = Mathf.Sqrt(2 * distance * Mathf.Sin(angleInRad) / gravity);

        return flightTime;
    }
}