using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PuzzleManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Transform ordinaryCratesParent;
    [SerializeField] private Transform keyCandidatesParent;
    [SerializeField] private Transform keyholeTransform;
    [SerializeField] private Transform playerSpawnTransform;
    [SerializeField] private GridMover playerGridMover;
    [SerializeField] private ExitDoor[] exitDoors = new ExitDoor[2];

    [Header("Initialization")]
    [SerializeField, Min(1f)] private float initializationTimeoutSeconds = 10f;

    [Header("Event Channels")]
    [SerializeField] private GameEventChannelSO playerWonEvent;

    private sealed class CrateStartData
    {
        public GridMover Mover;
        public CrateController Controller;
        public Vector2Int StartCell;
    }

    private readonly List<CrateStartData> allCrates = new List<CrateStartData>();
    private readonly List<CrateController> keyCandidates = new List<CrateController>();
    private readonly HashSet<GridMover> knownMovers = new HashSet<GridMover>();

    private Vector2Int keyholeCell;
    private Vector2Int playerSpawnCell;
    private bool initialized;
    private bool startRequestedReset;
    private CrateController currentKeyCrate;
    private ExitDoor activeExit;
    
    public CrateController CurrentKeyCrate => currentKeyCrate;
    
    public ExitDoor ActiveExit => activeExit;

    private IEnumerator Start()
    {
        float elapsed = 0f;

        while (true)
        {
            ResolveSceneReferences();

            if (HasRequiredReferences() && AreGridMoversInitialized())
                break;

            elapsed += Time.unscaledDeltaTime;
            if (elapsed >= initializationTimeoutSeconds)
            {
                Debug.LogError(
                    $"{name}: PuzzleManager could not initialize. Check its Inspector references, " +
                    "make sure every crate has GridMover and CrateController components, and confirm " +
                    "the crates and player are registered by GridManager.", this);
                yield break;
            }

            yield return null;
        }

        if (!CacheStartingPositions())
            yield break;

        keyholeCell = gridManager.WorldToCell(keyholeTransform.position);
        playerSpawnCell = gridManager.WorldToCell(playerSpawnTransform.position);

        SubscribeToExitEvents();
        initialized = true;
        StartNewPuzzleRun();

        if (startRequestedReset)
            ResetPuzzle();
    }

    private void OnDestroy()
    {
        foreach (CrateController candidate in keyCandidates)
        {
            if (candidate != null)
                candidate.OnLockedInSlot -= HandleKeyCrateLockedInSlot;
        }

        if (exitDoors != null)
        {
            foreach (ExitDoor door in exitDoors)
            {
                if (door != null)
                    door.OnPlayerReachedOpenExit -= HandlePlayerReachedExit;
            }
        }
    }
    
    public void ResetPuzzle()
    {
        if (!initialized)
        {
            startRequestedReset = true;
            return;
        }

        RestoreStartingPositions();
        StartNewPuzzleRun();
    }

    private void ResolveSceneReferences()
    {
        if (gridManager == null)
            gridManager = FindAnyObjectByType<GridManager>();

        Transform level = GameObject.Find("Level") != null
            ? GameObject.Find("Level").transform
            : null;

        if (level != null)
        {
            if (ordinaryCratesParent == null)
                ordinaryCratesParent = level.Find("Crate");
            if (keyCandidatesParent == null)
                keyCandidatesParent = level.Find("Keys");
            if (keyholeTransform == null)
                keyholeTransform = level.Find("Special_Tiles/Keyhole");
            if (playerSpawnTransform == null)
                playerSpawnTransform = level.Find("Special_Tiles/PlayerSpawn");
        }

        if (playerGridMover == null)
        {
            PlayerGridAdapter playerAdapter = FindAnyObjectByType<PlayerGridAdapter>();
            if (playerAdapter != null)
                playerGridMover = playerAdapter.GetComponent<GridMover>();
        }

        bool hasMissingDoor = exitDoors == null || exitDoors.Length < 2;
        if (!hasMissingDoor)
        {
            for (int i = 0; i < exitDoors.Length; i++)
            {
                if (exitDoors[i] == null)
                {
                    hasMissingDoor = true;
                    break;
                }
            }
        }

        if (hasMissingDoor)
        {
            ExitDoor[] foundDoors = FindObjectsByType<ExitDoor>();
            ExitDoor northEast = null;
            ExitDoor southWest = null;

            foreach (ExitDoor door in foundDoors)
            {
                if (door.name == "NorthEast") northEast = door;
                else if (door.name == "SouthWest") southWest = door;
            }

            if (northEast != null && southWest != null)
                exitDoors = new[] { northEast, southWest };
        }
    }

    private bool HasRequiredReferences()
    {
        if (gridManager == null || ordinaryCratesParent == null || keyCandidatesParent == null ||
            keyholeTransform == null || playerSpawnTransform == null || playerGridMover == null ||
            exitDoors == null || exitDoors.Length < 2)
            return false;

        for (int i = 0; i < exitDoors.Length; i++)
        {
            if (exitDoors[i] == null)
                return false;
        }

        return true;
    }

    private bool AreGridMoversInitialized()
    {
        if (!IsMoverRegistered(playerGridMover))
            return false;

        GridMover[] ordinaryMovers = ordinaryCratesParent.GetComponentsInChildren<GridMover>(true);
        CrateController[] candidates = keyCandidatesParent.GetComponentsInChildren<CrateController>(true);

        if (candidates.Length == 0)
            return false;

        foreach (GridMover mover in ordinaryMovers)
        {
            if (mover == null || !IsMoverRegistered(mover))
                return false;
        }

        foreach (CrateController candidate in candidates)
        {
            if (candidate == null)
                return false;

            GridMover mover = candidate.GetComponent<GridMover>();
            if (mover == null || !IsMoverRegistered(mover))
                return false;
        }

        return true;
    }

    private bool IsMoverRegistered(GridMover mover)
    {
        if (mover == null || gridManager == null)
            return false;

        Node node = gridManager.GetNode(mover.CurrentCell);
        return node != null && node.Occupant == mover;
    }

    private bool CacheStartingPositions()
    {
        allCrates.Clear();
        keyCandidates.Clear();
        knownMovers.Clear();

        GridMover[] ordinaryMovers = ordinaryCratesParent.GetComponentsInChildren<GridMover>(true);
        foreach (GridMover mover in ordinaryMovers)
        {
            if (mover == null || !knownMovers.Add(mover))
                continue;

            allCrates.Add(new CrateStartData
            {
                Mover = mover,
                Controller = mover.GetComponent<CrateController>(),
                StartCell = mover.CurrentCell
            });
        }

        CrateController[] candidates = keyCandidatesParent.GetComponentsInChildren<CrateController>(true);
        foreach (CrateController candidate in candidates)
        {
            if (candidate == null)
                continue;

            GridMover mover = candidate.GetComponent<GridMover>();
            if (mover == null)
            {
                Debug.LogError($"{candidate.name}: Key candidate has no GridMover component.", candidate);
                return false;
            }

            keyCandidates.Add(candidate);
            candidate.OnLockedInSlot -= HandleKeyCrateLockedInSlot;
            candidate.OnLockedInSlot += HandleKeyCrateLockedInSlot;

            if (knownMovers.Add(mover))
            {
                allCrates.Add(new CrateStartData
                {
                    Mover = mover,
                    Controller = candidate,
                    StartCell = mover.CurrentCell
                });
            }
        }

        if (keyCandidates.Count == 0)
        {
            Debug.LogError("PuzzleManager: No CrateController components were found under Level/Keys.", this);
            return false;
        }
        
        HashSet<Vector2Int> occupiedStartingCells = new HashSet<Vector2Int>();
        foreach (CrateStartData data in allCrates)
        {
            if (!IsMoverRegistered(data.Mover))
            {
                Debug.LogError($"{data.Mover.name}: Its starting grid cell is not registered correctly.", data.Mover);
                return false;
            }

            if (!occupiedStartingCells.Add(data.StartCell))
            {
                Debug.LogError($"More than one crate starts at grid cell {data.StartCell}. Fix the crate positions before play.", this);
                return false;
            }
        }

        return true;
    }

    private void StartNewPuzzleRun()
    {
        currentKeyCrate = null;
        activeExit = null;

        foreach (CrateController candidate in keyCandidates)
        {
            if (candidate != null)
                candidate.SetAsOrdinaryCrate();
        }

        foreach (ExitDoor door in exitDoors)
        {
            if (door != null)
                door.ResetDoor();
        }

        List<CrateController> validCandidates = GetValidKeyCandidates();
        if (validCandidates.Count == 0)
        {
            Debug.LogError("PuzzleManager: No valid key crate candidates can reach the keyhole through the walkable grid.", this);
            return;
        }

        List<ExitDoor> validExits = GetValidExitDoors();
        if (validExits.Count == 0)
        {
            Debug.LogError("PuzzleManager: No exit door has a valid grid cell.", this);
            return;
        }

        currentKeyCrate = validCandidates[UnityEngine.Random.Range(0, validCandidates.Count)];
        activeExit = validExits[UnityEngine.Random.Range(0, validExits.Count)];

        foreach (ExitDoor door in exitDoors)
        {
            Vector2Int exitCell = gridManager.WorldToCell(door.transform.position);
            door.Initialize(
                gridManager,
                playerGridMover,
                new GridCell(exitCell.x, exitCell.y),
                door == activeExit);
        }
        
        currentKeyCrate.SetAsKeyCrate(keyholeCell);
        Debug.Log($"Puzzle ready. Key crate: {currentKeyCrate.name}; active exit: {activeExit.name}.", this);
    }

    private List<CrateController> GetValidKeyCandidates()
    {
        List<CrateController> valid = new List<CrateController>();
        Node goalNode = gridManager.GetNode(keyholeCell);

        if (goalNode == null || !goalNode.IsWalkable)
        {
            Debug.LogError($"PuzzleManager: Keyhole cell {keyholeCell} is missing or blocked in GridManager.", this);
            return valid;
        }

        HashSet<Vector2Int> seenStartCells = new HashSet<Vector2Int>();
        foreach (CrateController candidate in keyCandidates)
        {
            if (candidate == null)
                continue;

            GridMover mover = candidate.GetComponent<GridMover>();
            if (mover == null)
                continue;

            Vector2Int startCell = mover.CurrentCell;
            Node startNode = gridManager.GetNode(startCell);

            bool validCell = startNode != null && startNode.IsWalkable &&
                             startNode.Occupant == mover && startCell != keyholeCell;
            bool uniqueCell = seenStartCells.Add(startCell);
            bool connected = validCell && CanReachKeyholeThroughWalkableCells(startCell);

            if (validCell && uniqueCell && connected)
            {
                valid.Add(candidate);
            }
            else
            {
                Debug.LogWarning(
                    $"PuzzleManager: Key candidate '{candidate.name}' was excluded. Check its starting cell " +
                    $"{startCell}, grid walkability, duplicate positions, and path to keyhole {keyholeCell}.",
                    candidate);
            }
        }

        return valid;
    }
    
    private bool CanReachKeyholeThroughWalkableCells(Vector2Int startCell)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        queue.Enqueue(startCell);
        visited.Add(startCell);

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (queue.Count > 0)
        {
            Vector2Int cell = queue.Dequeue();
            if (cell == keyholeCell)
                return true;

            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = cell + direction;
                if (visited.Contains(next))
                    continue;

                Node node = gridManager.GetNode(next);
                if (node == null || !node.IsWalkable || node.OccupantType == CellType.Wall)
                    continue;

                visited.Add(next);
                queue.Enqueue(next);
            }
        }

        return false;
    }

    private List<ExitDoor> GetValidExitDoors()
    {
        List<ExitDoor> valid = new List<ExitDoor>();
        HashSet<Vector2Int> seenCells = new HashSet<Vector2Int>();

        foreach (ExitDoor door in exitDoors)
        {
            if (door == null)
                continue;

            Vector2Int cell = gridManager.WorldToCell(door.transform.position);
            Node node = gridManager.GetNode(cell);

            if (node == null)
            {
                Debug.LogError($"{door.name}: Exit cell {cell} is outside GridManager bounds.", door);
                continue;
            }

            if (!seenCells.Add(cell))
            {
                Debug.LogError($"Two exit doors use the same grid cell {cell}.", door);
                continue;
            }

            valid.Add(door);
        }

        return valid;
    }

    private void RestoreStartingPositions()
    {
        foreach (CrateController candidate in keyCandidates)
        {
            if (candidate != null)
                candidate.ResetCrateState();
        }

        ClearMoverOccupancy(playerGridMover);
        foreach (CrateStartData data in allCrates)
            ClearMoverOccupancy(data.Mover);
        
        foreach (ExitDoor door in exitDoors)
        {
            if (door != null)
                door.ResetDoor();
        }

        foreach (CrateStartData data in allCrates)
        {
            if (data.Mover == null)
                continue;

            data.Mover.CancelAndSnapTo(data.StartCell);
            gridManager.RegisterOccupant(data.StartCell, data.Mover, CellType.Crate);
        }

        if (playerGridMover != null)
        {
            playerGridMover.CancelAndSnapTo(playerSpawnCell);
            gridManager.RegisterOccupant(playerSpawnCell, playerGridMover, CellType.Player);
        }
    }

    private void ClearMoverOccupancy(GridMover mover)
    {
        if (mover == null)
            return;

        Node node = gridManager.GetNode(mover.CurrentCell);
        if (node == null || node.Occupant != mover)
            return;

        node.OccupantType = CellType.Empty;
        node.Occupant = null;
        node.IsReserved = false;
        node.IsLocked = false;
    }

    private void SubscribeToExitEvents()
    {
        foreach (ExitDoor door in exitDoors)
        {
            if (door == null)
                continue;

            door.OnPlayerReachedOpenExit -= HandlePlayerReachedExit;
            door.OnPlayerReachedOpenExit += HandlePlayerReachedExit;
        }
    }

    private void HandleKeyCrateLockedInSlot(CrateController crate)
    {
        if (crate == null || crate != currentKeyCrate || activeExit == null)
            return;

        if (!crate.IsKeyCrate || !crate.IsLockedInSlot)
            return;

        activeExit.SetUnlocked(true);
        Debug.Log($"Key crate locked in the keyhole. Exit '{activeExit.name}' is unlocked.", this);
    }

    private void HandlePlayerReachedExit(ExitDoor door)
    {
        if (door == null || door != activeExit || !door.IsOpen)
            return;

        playerWonEvent?.RaiseEvent();
    }
}
