using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

// All visual grid concerns: tiles, entity models, ability previews/target highlights, grid lines,
// camera framing, and the selection outline. Reacts to GridManager's lifecycle like a StateView:
// build on GameStarted, refresh entity models on StateChanged. Reads state via _gridManager; entity
// visuals come from the global IdRegistry<EntityAssets>.
public class GridRenderer : LoggableBehaviour, IGridRenderer
{
    [SerializeField] private GridManager _gridManager;

    [SerializeField] private CinemachineCamera _cinemachineCamera;
    [SerializeField] private Grid _grid;
    [SerializeField] private GameObject _gridLines;
    [SerializeField] private LineRenderer _pressOutline;
    [SerializeField] private GameObject _cubePrefab;

    [SerializeField] private SpriteRenderer _highlightSquare;
    // [SerializeField] private Color _selectionPreviewColor = new Color(255f, 255f, 255f, 64f);
    // [SerializeField] private Color _selectionActiveColor = new Color(255f, 255f, 255f, 128f);
    [SerializeField] private Color _highlightAvailableEntities = new Color(64f, 255f, 64f, 64f);
    [SerializeField] private Color _highlightAbilityRange = new Color(255f, 255f, 255f, 64f);
    [SerializeField] private Color _highlightSelectableTargets = new Color(255f, 64f, 64f, 64f);

    private SpriteRenderer[] _highlightSquares;
    
    private float _gridGroundLevel;
    private GameObject[] _cubePrefabs;
    private GameObject[] _entityModels;

    public Grid Grid => _grid;
    private IReadOnlyGridState GridState => _gridManager.GridState;

    private void Start()
    {
        _gridManager.GameStarted += OnGameStarted;
        _gridManager.StateChanged += RefreshEntityModelPositions;
        if (_gridManager.IsGameStarted) OnGameStarted();   // sticky: match already started → build now
    }

    private void OnDestroy()
    {
        if (_gridManager == null) return;
        _gridManager.GameStarted -= OnGameStarted;
        _gridManager.StateChanged -= RefreshEntityModelPositions;
        if (_gridManager.Input != null) _gridManager.Input.OnSelectionChanged -= MovePressOutline;
    }

    private void OnGameStarted() => Build();

    // Build the visual grid. Idempotent — StartGame calls this synchronously before the input FSM
    // starts (the FSM drives highlights on enter), and the GameStarted subscription may call it again.
    public void Build()
    {
        if (_highlightSquares != null) return;   // already built
        _grid.gameObject.SetActive(true);
        InitCamera();
        InitRendering();
        InstantiateEntityModels();
        _gridManager.Input.OnSelectionChanged += MovePressOutline;
    }

    // Reposition/toggle the press outline as selection changes (was GridManager.OnInputChanged).
    private void MovePressOutline(QueryContext? ctx)
    {
        if (!ctx.HasValue)
        {
            _pressOutline.gameObject.SetActive(false);
            return;
        }
        var (x, y) = ctx.Value.SourcePosition;
        var worldPos = _grid.CellToWorld(new Vector3Int(x, y, 0));
        _pressOutline.transform.position = new Vector3(worldPos.x, _gridGroundLevel + 0.05f, worldPos.z);
        _pressOutline.gameObject.SetActive(true);
    }

    private void InitCamera()
    {
        var targetScreenHeight = Math.Max(GridState.X / 2.0f, GridState.Y);
        if (_cinemachineCamera.Target.TrackingTarget.Equals(_gridLines.transform))
        {
            _cinemachineCamera.GetComponent<CinemachineFollow>().FollowOffset = new Vector3(0f, targetScreenHeight, 0f);
        }
    }

    private void InitRendering()
    {
        _gridGroundLevel = _cubePrefab.transform.localScale.y;
        _highlightSquares = new SpriteRenderer[GridState.Size];
        _cubePrefabs = new GameObject[GridState.Size];
        for (var x = 0; x < GridState.X; x++)
        {
            for (var y = 0; y < GridState.Y; y++)
            {
                _highlightSquares[GridState.ToPosition1D(x, y)] = Instantiate(_highlightSquare, _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel + 0.01f, 0.5f), Quaternion.Euler(90f, 0f, 0f), gameObject.transform);
                _cubePrefabs[GridState.ToPosition1D(x, y)] = Instantiate(_cubePrefab, _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel / 2.0f, 0.5f), Quaternion.identity, gameObject.transform);
            }
        }

        _gridLines.SetActive(true);
        _gridLines.transform.position = new Vector3(GridState.X/2.0f, _gridGroundLevel + 0.01f, GridState.Y/2.0f);
        _gridLines.transform.localScale = new Vector3(GridState.X/10f, 1, GridState.Y/10f);
        var material = _gridLines.GetComponent<MeshRenderer>().material;
        material.SetVector("_Size", new Vector2(GridState.X, GridState.Y));
    }

    private void InstantiateEntityModels()
    {
        _entityModels = new GameObject[GridState.Size];
        foreach (var position in GridState.GetOccupiedTilesPositionSet())
        {
            var entity = GridState.GetEntity(position);
            if (!IdRegistry<EntityAssets>.TryGet(entity.Id, out var assets)) continue;
            if (assets.Model3D == null) continue;

            var (x, y) = GridState.ToPosition2D(position);
            var worldPos = _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel, 0.5f);
            var rotation = Quaternion.identity;
            if (entity.TryGetComponent<ControlComponent>(out var control) && control.PlayerController == 2)
                rotation = Quaternion.Euler(0, 180f, 0);
            _entityModels[position] = Instantiate(assets.Model3D, worldPos, rotation, gameObject.transform);
        }
    }

    public void RefreshEntityModelPositions()
    {
        var entityToModel = new Dictionary<Entity, GameObject>();
        for (var i = 0; i < _entityModels.Length; i++)
        {
            if (_entityModels[i] == null) continue;
            var entity = GridState.GetEntity(i);
            if (entity == null)
            {
                Destroy(_entityModels[i]);
                _entityModels[i] = null;
                continue;
            }
            entityToModel[entity] = _entityModels[i];
            _entityModels[i] = null;
        }
        foreach (var (entity, model) in entityToModel)
        {
            var (x, y) = GridState.ToPosition2D(entity.Position);
            model.transform.position = _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel, 0.5f);
            _entityModels[entity.Position] = model;
        }
        foreach (var pos in GridState.GetOccupiedTilesPositionSet())
        {
            if (_entityModels[pos] != null) continue;
            var entity = GridState.GetEntity(pos);
            if (!IdRegistry<EntityAssets>.TryGet(entity.Id, out var assets)) continue;
            if (assets.Model3D == null) continue;
            var (x, y) = GridState.ToPosition2D(pos);
            var worldPos = _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel, 0.5f);
            var rotation = Quaternion.identity;
            if (entity.TryGetComponent<ControlComponent>(out var control) && control.PlayerController == 2)
                rotation = Quaternion.Euler(0, 180f, 0);
            _entityModels[pos] = Instantiate(assets.Model3D, worldPos, rotation, gameObject.transform);
        }
    }
    
    // public void ShowAbilityPreview(Ability ability, QueryContext ctx)
    // {
    //     var (areas, _) = ability.Selection.GetSelectablePositions(ctx);
    //     ShowTiles(areas.ToHashSet(), true);
    // }
    //
    // public void ClearAbilityPreview()
    // {
    //     ShowTiles(new HashSet<int>(), true);
    // }
    //
    // public void HighlightTargets(IReadOnlyList<(int, int)> targets)
    // {
    //     ShowTiles(targets.ToHashSet(), false);
    // }
    //
    // public void ClearTargetHighlight()
    // {
    //     ShowTiles(new HashSet<int>(), false);
    // }
    
    public void HighlightAvailableEntities()
    {
        // TODO: check abilities for available parameter in GetControllableEntities()
        HighlightPositions(GridState.GetControllableEntities(), _highlightAvailableEntities);
    }
    
    public void HighlightAbilityRange(Ability ability, QueryContext ctx)
    {
        var (areas, _) = ability.Selection.GetSelectablePositions(ctx);
        HighlightPositions(areas, _highlightAbilityRange);
    }
    
    public void HighlightSelectableTargets(Ability ability, QueryContext ctx)
    {
        var (areas, _) = ability.Selection.GetSelectablePositions(ctx);
        HighlightPositions(areas, _highlightSelectableTargets);
    }
    
    public void ClearHighlights()
    {
        foreach (var square in _highlightSquares) square.gameObject.SetActive(false);
    }
    
    private void HighlightPositions(List<(int, int)> positions, Color color)
    {
        ClearHighlights();
        foreach (var position in positions)
        {
            _highlightSquares[GridState.ToPosition1D(position)].gameObject.SetActive(true);
            _highlightSquares[GridState.ToPosition1D(position)].material.SetColor(UnityUtil.MaterialBaseColorId, color);
        }
    }
    
    private void HighlightPositions(List<int> positions, Color color)
    {
        ClearHighlights();
        foreach (var position in positions)
        {
            _highlightSquares[position].gameObject.SetActive(true);
            _highlightSquares[position].material.SetColor(UnityUtil.MaterialBaseColorId, color);
        }
    }

    // private void ShowTiles(HashSet<int> tiles, bool preview)
    // {
    //     for (int i = 0; i < _highlightSquares.Length; i++)
    //         _highlightSquares[i].gameObject.SetActive(false);
    //     foreach (var tile in tiles)
    //     {
    //         if (GridState.IsValidPosition(tile))
    //         {
    //             _highlightSquares[tile].color = preview ? _selectionPreviewColor : _selectionActiveColor;
    //             _highlightSquares[tile].gameObject.SetActive(true);
    //         }
    //     }
    // }
    //
    // private void ShowTiles(HashSet<(int, int)> tiles, bool preview)
    // {
    //     var positions = new HashSet<int>();
    //     foreach (var tile in tiles)
    //         positions.Add(GridState.ToPosition1D(tile));
    //     ShowTiles(positions, preview);
    // }
}
