using UnityEngine;

public class EnemyPath : MonoBehaviour
{
    public enum PathType { Loop, PingPong, Random }

    public Transform[] waypoints;
    public PathType pathType = PathType.Loop;

    private int direction = 1;
    private int index = 0;

    public Vector3 GetCurrentWayPoint()
    {
        if (waypoints == null || waypoints.Length == 0) return transform.position;
        return waypoints[index].position;
    }

    public Vector3 GetNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return transform.position;
        index = GetNextWaypointIndex();
        return waypoints[index].position;
    }

    private int GetNextWaypointIndex()
    {
        if (waypoints.Length <= 1) return 0;

        if (pathType == PathType.Random)
        {
            int nextIndex;
            // Select a random index that is different from the current one
            do
            {
                nextIndex = Random.Range(0, waypoints.Length);
            } 
            while (nextIndex == index);

            return nextIndex;
        }

        // Sequential movement (Loop / PingPong)
        index += direction;
        
        if (pathType == PathType.Loop)
        {
            index %= waypoints.Length;
        }
        else if (pathType == PathType.PingPong)
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

        // Draw connecting lines based on PathType
        if (pathType == PathType.Random)
        {
            // Draw yellow star-burst lines from every waypoint to every other waypoint to visualize random links
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Length; i++)
            {
                for (int j = i + 1; j < waypoints.Length; j++)
                {
                    if (waypoints[i] != null && waypoints[j] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[j].position);
                    }
                }
            }
        }
        else
        {
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
}