using UnityEngine;

public static class SpriteHelper
{
    public static void ChangeSpritePosition(GameObject actionObject, bool flip, Vector2 changePos)
    {
        actionObject.GetComponent<SpriteRenderer>().flipX = flip;
        Vector2 topPosition = actionObject.transform.localPosition;
        topPosition.x = changePos.x;
        topPosition.y = changePos.y;
        actionObject.transform.localPosition = new Vector2(topPosition.x, topPosition.y);
    }
}