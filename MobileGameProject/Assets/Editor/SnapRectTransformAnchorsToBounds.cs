using UnityEngine;
using UnityEditor;

public class SnapRectTransformAnchorsToBounds : MonoBehaviour
{
    [MenuItem("CONTEXT/RectTransform/Snap Anchors")]
    private static void SnapAnchors(MenuCommand cmd)
    {
        RectTransform target = cmd.context as RectTransform;
        RectTransform targetParent = target.parent as RectTransform;

        Vector2 parentSize = targetParent.rect.size;
        Vector2 parentMin = targetParent.rect.min;
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        Vector2 bottomLeft = corners[0];
        //Vector2 topLeft = corners[1];
        Vector2 topRight = corners[2];
        //Vector2 bottomRight = corners[3];

        // void CreateMarker(string name, Vector3 position)
        // {
        //     GameObject marker = new GameObject(name, typeof(RectTransform));
        //     marker.transform.SetParent(targetParent, false);
        //     Vector2 localPos = targetParent.InverseTransformPoint(position);
        //     ((RectTransform)marker.transform).anchoredPosition = localPos;
        // }

        //CreateMarker(nameof(topLeft), topLeft);
        //CreateMarker(nameof(topRight), topRight);
        //CreateMarker(nameof(bottomRight), bottomRight);
        //CreateMarker(nameof(bottomLeft), bottomLeft);

        Vector2 localMin = targetParent.InverseTransformPoint(bottomLeft);
        Vector2 localMax = targetParent.InverseTransformPoint(topRight);

        Undo.RecordObject(target, "Snap Anchors");

        target.anchorMin = (localMin - parentMin) / parentSize;
        target.anchorMax = (localMax - parentMin) / parentSize;

        target.sizeDelta = target.anchoredPosition = Vector2.zero;
    }

    [MenuItem("CONTEXT/RectTransform/Snap Anchors", validate = true)]
    private static bool Validate(MenuCommand command)
    {
        RectTransform rectTransform = command.context as RectTransform;
        return (rectTransform.parent as RectTransform) != null && !PrefabUtility.IsPartOfImmutablePrefab(rectTransform);
    }
}
