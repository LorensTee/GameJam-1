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
    private bool isFrozen;

    [Header("Detection & Combat")]
    public float timeBetweenAttacks;
    private bool alreadyAttacked;
    private bool isRespawning;

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
        if (path != null)
        {
            agent.SetDestination(path.GetCurrentWayPoint());
        }
    }

    private void Update()
    {
        if (isRespawning || isFrozen) return;

        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        bool playerInSightSphere = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        bool playerInAttackSphere = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        playerInSightRange = playerInSightSphere && HasLineOfSight();
        playerInAttackRange = playerInAttackSphere && HasLineOfSight();

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

        waypointWaitTimer = 0f;
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.isStopped = true;

        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(targetPosition);

        if (!alreadyAttacked)
        {
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
        if (isRespawning) return;

        bool isPlayer = other.CompareTag("Player") ||
            (other.transform.parent != null &&
            other.transform.parent.CompareTag("Player"));

        if (!isPlayer) return;

        print("Enemy caught the player!");

        HUDController hud = FindAnyObjectByType<HUDController>();
        if (hud != null)
        {
            hud.TriggerJumpscareUI();
        }

        PlayerHealth playerHealth = GameObject.Find("PlayerRoot").GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.Decrement();
        }

        StartCoroutine(RespawnSequence());
    }

    private IEnumerator RespawnSequence()
    {
        isRespawning = true;
        
        agent.isStopped = true;
        agent.ResetPath();

        agent.Warp(respawnPoint);

        yield return new WaitForSeconds(1f);

        if (path != null)
        {
            agent.SetDestination(path.GetCurrentWayPoint());
        }

        waypointWaitTimer = 0f;
        alreadyAttacked = false;
        agent.isStopped = false;
        isRespawning = false;
    }

    public void SetFrozen(bool frozen)
    {
        if (isFrozen == frozen)
        {
            return;
        }

        isFrozen = frozen;

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = frozen;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}