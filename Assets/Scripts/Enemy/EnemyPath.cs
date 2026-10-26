using UnityEngine;

public class EnemyPath : MonoBehaviour
{
    public enum PathType { Loop, PingPong, Random }

    public Transform[] waypoints;
    public PathType pathType = PathType.Loop;

    private int direction = 1;
    private int index = 0;

    private void Awake()
    {
        //random start idx
        if (pathType == PathType.Random && waypoints != null && waypoints.Length > 0)
        {
            index = Random.Range(0, waypoints.Length);
        }
    }

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
            do
            {
                nextIndex = Random.Range(0, waypoints.Length);
            } 
            while (nextIndex == index);

            return nextIndex;
        }

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

        Gizmos.color = Color.red;
        foreach (Transform waypoint in waypoints)
        {
            if (waypoint != null)
                Gizmos.DrawSphere(waypoint.position, 0.3f);
        }

        if (pathType == PathType.Random)
        {
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
            Gizmos.color = Color.white;
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }

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