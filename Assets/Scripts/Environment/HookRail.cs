using UnityEngine;

public class HookRail : MonoBehaviour
{
    [Header("=== Rail Settings ===")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private bool loop = true;
    [SerializeField] private Color gizmoColor = Color.cyan;

    public Transform[] Waypoints => waypoints;
    public bool Loop => loop;

    private void Awake()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            waypoints = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                waypoints[i] = transform.GetChild(i);
            }
        }
    }

    public Vector3 GetWaypointPosition(int index)
    {
        if (waypoints == null || waypoints.Length == 0) return transform.position;
        index = Mathf.Clamp(index, 0, waypoints.Length - 1);
        return waypoints[index].position;
    }

    public int WaypointCount => waypoints != null ? waypoints.Length : 0;

    public float GetTotalLength()
    {
        if (waypoints == null || waypoints.Length < 2) return 0f;
        
        float length = 0f;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            length += Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);
        }
        
        if (loop && waypoints.Length > 1)
        {
            length += Vector3.Distance(waypoints[waypoints.Length - 1].position, waypoints[0].position);
        }
        
        return length;
    }

    private void OnDrawGizmos()
    {
        Transform[] points = waypoints;
        
        if (points == null || points.Length == 0)
        {
            points = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                points[i] = transform.GetChild(i);
            }
        }

        if (points == null || points.Length == 0) return;

        Gizmos.color = gizmoColor;

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == null) continue;
            
            Gizmos.DrawWireSphere(points[i].position, 0.3f);
            
            if (i < points.Length - 1 && points[i + 1] != null)
            {
                Gizmos.DrawLine(points[i].position, points[i + 1].position);
            }
        }

        if (loop && points.Length > 1 && points[0] != null && points[points.Length - 1] != null)
        {
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.5f);
            Gizmos.DrawLine(points[points.Length - 1].position, points[0].position);
        }
    }
}
