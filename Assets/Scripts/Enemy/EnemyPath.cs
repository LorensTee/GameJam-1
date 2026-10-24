using UnityEngine;

public class EnemyPath : MonoBehaviour
{
    public enum PathType { Loop, PingPong }

    public Transform[] waypoints;
    public PathType pathType = PathType.Loop;

    private int direction = 1;
    int index;

    public Vector3 GetCurrentWayPoint()
    {
        return waypoints[index].position;
    }

    public Vector3 GetNextWaypoint()
    {
        if (waypoints.Length == 0) return transform.position;
        
        index = GetNextWaypointIndex();
        Vector3 nextWaypoint = waypoints[index].position;

        return nextWaypoint;
    }

    private int GetNextWaypointIndex()
    {
        index += direction;
        if (PathType.Loop == pathType)
        {
            index %= waypoints.Length;
        }
        else if (PathType.PingPong == pathType)
        {
            if (index >= waypoints.Length || index < 0)
            {
                direction *= -1;
                index += direction * 2;
            }
        }
        return index;
    }

    private void OnDrawGizmos()
{
    if (waypoints == null || waypoints.Length == 0) return;

    // Draw red spheres at each waypoint position
    Gizmos.color = Color.red;
    foreach (Transform waypoint in waypoints)
    {
        if (waypoint != null)
            Gizmos.DrawSphere(waypoint.position, 0.3f);
    }

    // Draw white lines connecting sequential waypoints
    Gizmos.color = Color.white;
    for (int i = 0; i < waypoints.Length - 1; i++)
    {
        if (waypoints[i] != null && waypoints[i + 1] != null)
        {
            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }

    // Draw loop closing line if PathType is set to Loop
    if (pathType == PathType.Loop && waypoints.Length > 1)
    {
        if (waypoints[waypoints.Length - 1] != null && waypoints[0] != null)
        {
            Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
        }
    }
}
}
