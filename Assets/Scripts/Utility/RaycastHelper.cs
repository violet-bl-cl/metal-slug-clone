using UnityEngine;
public static class RaycastHelper
{
    public static bool CheckCircleSide(this Transform transform, Vector2 direction, float raidus, float distance, LayerMask layerMask)
    {
        RaycastHit2D hitInfo = Physics2D.CircleCast(transform.position, raidus, direction, distance, layerMask);
        return hitInfo.collider != null;
    }
    public static bool CheckBoxSide(this Transform transform, Vector2 direction, float distance, Vector2 size, LayerMask layerMask)
    {
        RaycastHit2D hitInfo = Physics2D.BoxCast(transform.position, size, 0f, direction, distance, layerMask);
        return hitInfo.collider != null;
    }
    public static bool CheckLineSide(this Transform transform, Vector2 direction, float distance, LayerMask layerMask)
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, direction, distance, layerMask);
        return hitInfo.collider != null;
    }
}