using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;

    [Header("Patrolling / Waypoints")]
    [SerializeField] private EnemyPath path;
    [SerializeField] private float waitTimeOnWaypoint = 0.5f;
    [SerializeField] private Vector3 respawnPoint;
    private float waypointWaitTimer = 0f;

    [Header("Detection & Combat")]
    public float timeBetweenAttacks;
    private bool alreadyAttacked;
    private bool isRespawning; // Prevents multiple trigger calls during the pause

    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    [Header("Layer Masks")]
    public LayerMask whatIsGround, whatIsPlayer, whatIsObstacle;

    private Animator animator;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (player == null)
        {
            GameObject playerObj = GameObject.Find("PlayerRoot");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    private void Start()
    {
        // Initialize path destination if a path is assigned
        if (path != null)
        {
            agent.SetDestination(path.GetCurrentWayPoint());
        }
    }

    private void Update()
    {
        // Ignore update logic while paused/respawning
        if (isRespawning) return;

        // Safety check to ensure agent is placed on a NavMesh
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        // Check if player is within sphere range
        bool playerInSightSphere = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        bool playerInAttackSphere = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // Verify line of sight (no obstacles in between)
        playerInSightRange = playerInSightSphere && HasLineOfSight();
        playerInAttackRange = playerInAttackSphere && HasLineOfSight();

        // State Machine
        if (!playerInSightRange && !playerInAttackRange) Patroling();
        else if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        else if (playerInSightRange && playerInAttackRange) AttackPlayer();
    }

    private bool HasLineOfSight()
    {
        if (player == null) return false;

        Vector3 eyeLevel = transform.position + Vector3.up * 1.5f;
        Vector3 directionToPlayer = (player.position - eyeLevel).normalized;
        float distanceToPlayer = Vector3.Distance(eyeLevel, player.position);

        return !Physics.Raycast(eyeLevel, directionToPlayer, distanceToPlayer, whatIsObstacle);
    }

    private void Patroling()
    {
        if (agent.isStopped) 
            agent.isStopped = false;

        if (path == null) return;

        // Check if the agent has reached the waypoint
        if (!agent.pathPending && agent.remainingDistance <= 0.2f)
        {
            waypointWaitTimer += Time.deltaTime;

            if (waypointWaitTimer >= waitTimeOnWaypoint)
            {
                waypointWaitTimer = 0f;
                agent.SetDestination(path.GetNextWaypoint());
            }
        }
    }

    private void ChasePlayer()
    {
        if (agent.isStopped) 
            agent.isStopped = false;

        waypointWaitTimer = 0f; // Reset waypoint wait timer while chasing
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.isStopped = true;

        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(targetPosition);

        if (!alreadyAttacked)
        {
            /// Attack code / Animation trigger here

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isRespawning) return; // Ignore collisions while already respawning

        bool isPlayer = other.CompareTag("Player") || 
                    (other.transform.parent != null && other.transform.parent.CompareTag("Player"));

        if (isPlayer)
        {
            print("Enemy caught the player!");
            // lessen health by 1 here

            StartCoroutine(RespawnSequence());
        }
    }

    private IEnumerator RespawnSequence()
    {
        isRespawning = true;
        
        // Stop movement and reset pathfinding
        agent.isStopped = true;
        agent.ResetPath();

        // Teleport agent safely using Warp to keep it attached to the NavMesh
        agent.Warp(respawnPoint);

        // Pause for 1 second
        yield return new WaitForSeconds(1f);

        // Resume patrol pathing
        if (path != null)
        {
            agent.SetDestination(path.GetCurrentWayPoint());
        }

        waypointWaitTimer = 0f;
        alreadyAttacked = false;
        agent.isStopped = false;
        isRespawning = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}