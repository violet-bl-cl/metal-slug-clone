using UnityEngine;

public static class DrawUtility {
    
    private static Transform objectTransform;
    
    public static void SetTransform(Transform targetTransform){
        objectTransform = targetTransform;
    }
    public static void DrawRayBox(Vector2 direction, float distance, Vector2 boxSize)
    {
        Gizmos.color = Color.green;
        Vector2 start = (Vector2)objectTransform.position;
        Vector2 end = start + direction * distance;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireCube(end, boxSize);
    }
    public static void DrawRaySphere(Vector2 direction, float distance, float sphereRadius)
    {
        Gizmos.color = Color.red;
        Vector2 start = (Vector2)objectTransform.position;
        Vector2 end = start + direction * distance;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(end, sphereRadius);
    }
    public static void DrawyRayLine(Vector2 direction, float distance){
        Gizmos.color = Color.cyan;
        Vector2 start = (Vector2)objectTransform.position;
        Vector2 end = start + direction * distance;
        Gizmos.DrawLine(start,end);
    }
}