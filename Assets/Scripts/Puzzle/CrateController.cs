using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(GridMover))]
public class CrateController : MonoBehaviour
{
    [Header("Key Crate Visual")]
    [SerializeField] private Renderer[] visualRenderers;
    [SerializeField] private Color keyCrateColor = new Color(1f, 0.72f, 0.12f, 1f);
    [SerializeField] private float bloomIntensity = 3.0f;
    
    private GridMover gridMover;
    private GridManager gridManager;
    private Renderer[] cachedRenderers;
    private MaterialPropertyBlock[] originalPropertyBlocks;
    private MaterialPropertyBlock propertyBlock;
    private Light crateLight;

    private Vector2Int keyholeCell;
    private Vector2Int lockedCell;
    private bool hasLockedCell;
    
    public bool IsKeyCrate { get; private set; }

    public bool IsLockedInSlot { get; private set; }

    public event Action<CrateController> OnLockedInSlot;

    private void Awake()
    {
        gridMover = GetComponent<GridMover>();
        gridManager = FindAnyObjectByType<GridManager>();
        propertyBlock = new MaterialPropertyBlock();
        crateLight = GetComponent<Light>();
        CacheVisuals();
    }

    private void OnEnable()
    {
        if (gridMover == null)
            gridMover = GetComponent<GridMover>();

        if (gridMover != null)
            gridMover.OnMoveCompleted += HandleMoveCompleted;
    }

    private void OnDisable()
    {
        if (gridMover != null)
            gridMover.OnMoveCompleted -= HandleMoveCompleted;
    }
    
    public void SetAsKeyCrate(Vector2Int keyholeCell)
    {
        ResetCrateState();

        this.keyholeCell = keyholeCell;
        IsKeyCrate = true;
        ApplyKeyCrateVisual();
        
        if (gridMover != null && !gridMover.IsMoving && gridMover.CurrentCell == keyholeCell)
            TryLockInSlot(keyholeCell);
    }
    
    public void SetAsOrdinaryCrate()
    {
        ResetCrateState();
    }
    
    public void ResetCrateState()
    {
        GridManager manager = GetGridManager();
        
        if (hasLockedCell && manager != null)
        {
            Node lockedNode = manager.GetNode(lockedCell);
            if (lockedNode != null)
                lockedNode.IsLocked = false;
        }

        IsKeyCrate = false;
        IsLockedInSlot = false;
        hasLockedCell = false;
        RestoreOriginalVisual();
    }

    private void HandleMoveCompleted(Vector2Int completedCell)
    {
        if (!IsKeyCrate || IsLockedInSlot)
            return;

        if (completedCell == keyholeCell)
            TryLockInSlot(completedCell);
    }
    
    public bool TryLockInSlot(Vector2Int slotCell)
    {
        if (!IsKeyCrate || IsLockedInSlot || gridMover == null || gridMover.IsMoving)
            return false;

        if (gridMover.CurrentCell != slotCell || slotCell != keyholeCell)
            return false;

        GridManager manager = GetGridManager();
        if (manager == null)
        {
            Debug.LogError($"{name}: Cannot lock key crate because no GridManager was found.", this);
            return false;
        }

        Node slotNode = manager.GetNode(slotCell);
        if (slotNode == null)
        {
            Debug.LogError($"{name}: Keyhole cell {slotCell} is outside the current grid.", this);
            return false;
        }
        
        if (slotNode.Occupant != gridMover)
        {
            Debug.LogWarning($"{name}: Cannot lock because this crate is not registered in cell {slotCell}.", this);
            return false;
        }

        slotNode.IsLocked = true;
        lockedCell = slotCell;
        hasLockedCell = true;
        IsLockedInSlot = true;

        OnLockedInSlot?.Invoke(this);
        return true;
    }

    private GridManager GetGridManager()
    {
        if (gridManager == null)
            gridManager = FindAnyObjectByType<GridManager>();

        return gridManager;
    }

    private void CacheVisuals()
    {
        cachedRenderers = visualRenderers != null && visualRenderers.Length > 0
            ? visualRenderers
            : GetComponentsInChildren<Renderer>(true);

        originalPropertyBlocks = new MaterialPropertyBlock[cachedRenderers.Length];
        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            originalPropertyBlocks[i] = new MaterialPropertyBlock();
            cachedRenderers[i].GetPropertyBlock(originalPropertyBlocks[i]);
        }
    }

    private void ApplyKeyCrateVisual()
    {
        if (cachedRenderers == null)
            CacheVisuals();

        Color hdrColor = keyCrateColor * bloomIntensity;

        foreach (Renderer targetRenderer in cachedRenderers)
        {
            if (targetRenderer == null)
                continue;

            targetRenderer.material.EnableKeyword("_EMISSION");
            targetRenderer.material.SetColor("_EmissionColor", hdrColor);

            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", keyCrateColor);
            propertyBlock.SetColor("_Color", keyCrateColor);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }

        if (crateLight != null)
        {
            crateLight.color = keyCrateColor;
            crateLight.intensity = bloomIntensity;
            crateLight.enabled = true;
        }
    }

    private void RestoreOriginalVisual()
    {
        if (cachedRenderers == null || originalPropertyBlocks == null)
            return;

        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            if (cachedRenderers[i] != null)
            {
                cachedRenderers[i].material.DisableKeyword("_EMISSION");
                cachedRenderers[i].material.SetColor("_EmissionColor", Color.black);
            
                cachedRenderers[i].SetPropertyBlock(originalPropertyBlocks[i]);
            }

            if (crateLight != null)
            {
                crateLight.enabled = false;
            }
        }
    }
}
