using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRendererCollider : MonoBehaviour
{
    private float _colliderWidth; // Width of the collider
    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        _colliderWidth = lineRenderer.startWidth;
        CreateColliders();
    }

    private void CreateColliders()
    {
        var numPositions = lineRenderer.positionCount;

        // Loop through the points of the LineRenderer
        for (var i = 0; i < numPositions - 1; i++)
        {
            var startPos = lineRenderer.GetPosition(i);
            var endPos = lineRenderer.GetPosition(i + 1);

            // Create a capsule collider
            var collider = new GameObject(lineRenderer.name + "Collider").AddComponent<CapsuleCollider>();

            collider.tag = lineRenderer.tag;
            collider.gameObject.layer = lineRenderer.gameObject.layer;

            // Set parent to keep the hierarchy clean
            collider.transform.SetParent(transform);

            // Position collider between the start and end positions
            var colliderPos = (startPos + endPos) / 2;
            collider.transform.position = colliderPos;

            // Calculate the distance and set the collider's height
            var distance = Vector3.Distance(startPos, endPos);
            collider.height = distance;

            // Adjust the collider orientation
            var direction = (endPos - startPos).normalized;
            collider.transform.up = direction;

            // Set the radius of the collider based on the desired collider width
            collider.radius = _colliderWidth / 2;

            // Optionally, mark the collider as a trigger if needed
            collider.isTrigger = true;

            // Adjusting collider's direction
            collider.direction = 1; // 2 means "Z-axis" alignment in Unity's CapsuleCollider.
        }
    }
}