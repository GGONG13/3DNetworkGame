using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TerrainCircleDrawer : MonoBehaviour
{
    public Transform circleCenter;
    public float radius = 5.0f;
    public int segments = 50;
    public LayerMask terrainLayer;

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        DrawCircle();
    }

    void DrawCircle()
    {
        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = true;

        float angle = 0f;
        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            Vector3 pointPosition = circleCenter.position + new Vector3(x, 0, z);
            RaycastHit hit;
            if (Physics.Raycast(pointPosition + Vector3.up * 50, Vector3.down, out hit, 100, terrainLayer))
            {
                pointPosition.y = hit.point.y;
            }
            lineRenderer.SetPosition(i, pointPosition);

            angle += (360f / segments);
        }
    }
}