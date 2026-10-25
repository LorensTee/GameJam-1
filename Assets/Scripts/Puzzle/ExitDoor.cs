using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class ExitDoor : MonoBehaviour
{
    [Header("Exit Cell")]
    [Tooltip("Used when the exit cell is not supplied by PuzzleManager. X and Z are grid coordinates.")]
    [SerializeField] private GridCell exitCellXZ;
    [Tooltip("When enabled, read the exit cell from this object's world position after GridManager starts.")]
    [SerializeField] private bool readCellFromTransform = true;

    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GridMover playerGridMover;

    [Header("Blocking Parts (the wall stays visible when open)")]
    [Tooltip("Only colliders that block the passage. Tile-tagged floor colliders are excluded automatically.")]
    [SerializeField] private Collider[] blockingColliders;
    [Tooltip("Nav Mesh Obstacles to disable when the passage opens.")]
    [SerializeField] private NavMeshObstacle[] navMeshObstacles;

    public bool IsActive { get; private set; }
    public bool IsUnlocked { get; private set; }
    public bool IsOpen => IsActive && IsUnlocked;
    public Vector2Int ExitCell => new Vector2Int(exitCellXZ.X, exitCellXZ.Z);
    public event Action<ExitDoor> OnPlayerReachedOpenExit;

    private bool explicitlyConfigured;
    private bool gridInitialized;
    private bool startRoutineRunning;
    private GridMover subscribedPlayerMover;

    private void Awake()
    {
        if (gridManager == null)
            gridManager = FindAnyObjectByType<GridManager>();

        CacheDoorParts();
    }

    private void OnEnable()
    {
        HookPlayerMover();
    }

    private void OnDisable()
    {
        UnhookPlayerMover();
    }

    private IEnumerator Start()
    {
        startRoutineRunning = true;
        
        yield return null;

        CompleteInitialization();
        startRoutineRunning = false;
    }
    
    public void Initialize(GridManager manager, GridMover playerMover, GridCell cellXZ, bool isSelectedExit)
    {
        explicitlyConfigured = true;
        gridManager = manager;
        playerGridMover = playerMover;
        exitCellXZ = cellXZ;
        readCellFromTransform = false;
        IsActive = isSelectedExit;
        IsUnlocked = false;

        HookPlayerMover();

        if (gridInitialized)
            ApplyCurrentState();
        else if (!startRoutineRunning && isActiveAndEnabled)
            StartCoroutine(InitializeAfterGridStart());
    }
    
    public void SetActiveExit(bool isActive)
    {
        IsActive = isActive;
        IsUnlocked = false;

        if (gridInitialized)
            ApplyCurrentState();
    }
    
    public void SetUnlocked(bool unlocked)
    {
        if (unlocked && !IsActive)
        {
            Debug.LogWarning($"{name}: Cannot unlock an inactive exit. Select it first.", this);
            return;
        }

        IsUnlocked = unlocked;

        if (gridInitialized)
            ApplyCurrentState();
    }
    
    public void ResetDoor()
    {
        IsActive = false;
        IsUnlocked = false;

        if (gridInitialized)
            ApplyCurrentState();
    }

    private IEnumerator InitializeAfterGridStart()
    {
        yield return null;
        CompleteInitialization();
    }

    private void CompleteInitialization()
    {
        if (gridManager == null)
            gridManager = FindAnyObjectByType<GridManager>();

        if (gridManager == null)
        {
            Debug.LogError($"{name}: ExitDoor could not find a GridManager.", this);
            return;
        }

        if (!explicitlyConfigured && readCellFromTransform)
        {
            Vector2Int cell = gridManager.WorldToCell(transform.position);
            exitCellXZ = new GridCell(cell.x, cell.y);
        }

        if (playerGridMover == null)
        {
            PlayerGridAdapter playerAdapter = FindAnyObjectByType<PlayerGridAdapter>();
            if (playerAdapter != null)
                playerGridMover = playerAdapter.GetComponent<GridMover>();
        }

        CacheDoorParts();
        HookPlayerMover();
        gridInitialized = true;
        ApplyCurrentState();
    }

    private void HookPlayerMover()
    {
        if (subscribedPlayerMover != null)
            subscribedPlayerMover.OnMoveCompleted -= HandlePlayerMoveCompleted;

        subscribedPlayerMover = null;

        if (isActiveAndEnabled && playerGridMover != null)
        {
            subscribedPlayerMover = playerGridMover;
            subscribedPlayerMover.OnMoveCompleted += HandlePlayerMoveCompleted;
        }
    }

    private void UnhookPlayerMover()
    {
        if (subscribedPlayerMover != null)
            subscribedPlayerMover.OnMoveCompleted -= HandlePlayerMoveCompleted;

        subscribedPlayerMover = null;
    }

    private void HandlePlayerMoveCompleted(Vector2Int destination)
    {
        if (!IsOpen || destination != ExitCell)
            return;

        OnPlayerReachedOpenExit?.Invoke(this);
    }

    private void ApplyCurrentState()
    {
        bool open = IsOpen;
        
        SetComponentsEnabled(blockingColliders, !open);
        SetComponentsEnabled(navMeshObstacles, !open);

        if (gridManager == null)
            return;

        Node node = gridManager.GetNode(ExitCell);
        if (node == null)
        {
            Debug.LogError($"{name}: Exit cell {ExitCell} is outside the current GridManager bounds.", this);
            return;
        }

        if (open)
        {
            node.IsWalkable = true;
            node.OccupantType = CellType.Empty;
            node.Occupant = null;
            node.IsReserved = false;
            node.IsLocked = false;
        }
        else
        {
            node.IsWalkable = false;
            node.OccupantType = CellType.Wall;
            node.Occupant = null;
            node.IsReserved = false;
            node.IsLocked = false;
        }
    }

    private void CacheDoorParts()
    {
        if (blockingColliders == null || blockingColliders.Length == 0)
            blockingColliders = GetComponentsInChildren<Collider>(true);
        
        if (blockingColliders != null)
        {
            blockingColliders = Array.FindAll(
                blockingColliders,
                collider => collider != null && !IsPartOfFloorTile(collider.transform));
        }

        if (navMeshObstacles == null || navMeshObstacles.Length == 0)
            navMeshObstacles = GetComponentsInChildren<NavMeshObstacle>(true);
    }

    private bool IsPartOfFloorTile(Transform item)
    {
        Transform current = item;
        while (current != null)
        {
            if (current.CompareTag("Tile"))
                return true;

            if (current == transform)
                break;

            current = current.parent;
        }

        return false;
    }

    private static void SetComponentsEnabled(Collider[] components, bool enabled)
    {
        if (components == null)
            return;

        foreach (Collider component in components)
        {
            if (component != null)
                component.enabled = enabled;
        }
    }

    private static void SetComponentsEnabled(NavMeshObstacle[] components, bool enabled)
    {
        if (components == null)
            return;

        foreach (NavMeshObstacle component in components)
        {
            if (component != null)
                component.enabled = enabled;
        }
    }
}
