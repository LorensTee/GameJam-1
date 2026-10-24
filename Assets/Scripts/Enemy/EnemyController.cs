using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer, whatIsObstacle;

    // Patrolling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;

    // States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Check if player is within sphere range
        bool playerInSightSphere = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        bool playerInAttackSphere = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // Verify line of sight
        playerInSightRange = playerInSightSphere && HasLineOfSight();
        playerInAttackRange = playerInAttackSphere && HasLineOfSight();

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInSightRange && playerInAttackRange) AttackPlayer();
    }

    private bool HasLineOfSight()
    {
        Vector3 eyeLevel = transform.position + Vector3.up * 1.5f;
        Vector3 directionToPlayer = (player.position - eyeLevel).normalized;
        float distanceToPlayer = Vector3.Distance(eyeLevel, player.position);

        return !Physics.Raycast(eyeLevel, directionToPlayer, distanceToPlayer, whatIsObstacle);
    }

    private void Patroling()
    {
        // Ensure agent is active and unpaused when entering patrol state
        if (agent.isStopped) 
            agent.isStopped = false;

        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        // Calculate distance ignoring vertical Y differences (so height mismatches don't break patrol)
        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        distanceToWalkPoint.y = 0;

        // Walkpoint reached or path blocked/stuck
        if (distanceToWalkPoint.magnitude < 1.5f || agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            walkPointSet = false;
        }
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        Vector3 randomPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        NavMeshHit hit;
        // Search in a slightly wider radius (e.g. walkPointRange) on the NavMesh
        if (NavMesh.SamplePosition(randomPoint, out hit, walkPointRange, NavMesh.AllAreas))
        {
            walkPoint = hit.position;
            walkPointSet = true;
        }
    }

    private void ChasePlayer()
    {
        if (agent.isStopped) 
            agent.isStopped = false;

        agent.SetDestination(player.position);
        
        // Reset walkPointSet so when the enemy loses sight, it immediately picks a NEW patrol point
        walkPointSet = false; 
    }

    private void AttackPlayer()
    {
        // Stop movement during attack
        agent.isStopped = true;

        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(targetPosition);

        if (!alreadyAttacked)
        {
            /// Attack code here

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }

        // Reset walkPointSet so patrol generates a fresh point when player runs away
        walkPointSet = false; 
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}