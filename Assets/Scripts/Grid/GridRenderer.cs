using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public enum GridHighlightType { AvailableEntities, AbilityRange, SelectableTargets, EffectPreview }

public class GridRenderer : LoggableBehaviour, IGridRenderer
{
    [SerializeField] private GridManager _gridManager;

    [SerializeField] private CinemachineCamera _cinemachineCamera;
    [SerializeField] private Grid _grid;
    [SerializeField] private GameObject _gridLines;
    [SerializeField] private LineRenderer _pressOutline;
    [SerializeField] private GameObject _cubePrefab;

    [SerializeField] private List<Sprite> _directionalArrowSprites;
    [SerializeField] private SpriteRenderer _highlightSquare;
    // [SerializeField] private Color _selectionPreviewColor = new Color(255f, 255f, 255f, 64f);
    // [SerializeField] private Color _selectionActiveColor = new Color(255f, 255f, 255f, 128f);
    [SerializeField] private Color _highlightAvailableEntities = new Color(64f, 255f, 64f, 64f);
    [SerializeField] private Color _highlightAbilityRange = new Color(255f, 255f, 255f, 64f);
    [SerializeField] private Color _highlightSelectableTargets = new Color(255f, 64f, 64f, 64f);
    [SerializeField] private Color _highlightEffectPreview = new Color(255f, 128f, 0f, 96f);

    private GameObject[] _cubePrefabs;
    private SpriteRenderer[] _highlightSquares;
    private SpriteRenderer[] _directionalArrows;
    
    private GameObject[] _entityModels;
    
    private float _gridGroundLevel;
    
    private IReadOnlyGridState GridState => _gridManager.GridState;
    
    private const float HighlightSquarePositionOffset = 0.01f;
    private const float UnidirectionalArrowPositionOffset = 0.02f;
    
    public Grid Grid => _grid;

    private void Start()
    {
        _gridManager.GameStarted += OnGameStarted;
        _gridManager.StateChanged += RefreshEntityModelPositions;
        if (_gridManager.IsGameStarted) OnGameStarted();
    }

    private void OnDestroy()
    {
        if (_gridManager == null) return;
        _gridManager.GameStarted -= OnGameStarted;
        _gridManager.StateChanged -= RefreshEntityModelPositions;
        if (_gridManager.Player != null) _gridManager.Player.SelectionChanged -= MovePressOutline;
    }

    private void OnGameStarted() => Build();

    public void Build()
    {
        if (_highlightSquares != null) return;
        _grid.gameObject.SetActive(true);
        InitCamera();
        InitRendering();
        InstantiateEntityModels();
        _gridManager.Player.SelectionChanged += MovePressOutline;
    }

    private void MovePressOutline(QueryContext? ctx)
    {
        if (!ctx.HasValue)
        {
            _pressOutline.gameObject.SetActive(false);
            return;
        }
        var position = ctx.Value.SourcePosition;
        var worldPos = _grid.CellToWorld(new Vector3Int(position.Dim2.x, position.Dim2.y, 0));
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
        _cubePrefabs = new GameObject[GridState.Size];
        _highlightSquares = new SpriteRenderer[GridState.Size];
        _directionalArrows = new SpriteRenderer[GridState.Size];
        
        for (var x = 0; x < GridState.X; x++)
        {
            for (var y = 0; y < GridState.Y; y++)
            {
                var gridPosition = new GridPosition(GridState, (x, y));
                _cubePrefabs[gridPosition.Dim1] = Instantiate(_cubePrefab, _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel / 2.0f, 0.5f), Quaternion.identity, gameObject.transform);
                _highlightSquares[gridPosition.Dim1] = Instantiate(_highlightSquare, _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel + HighlightSquarePositionOffset, 0.5f), Quaternion.Euler(90f, 0f, 0f), gameObject.transform);
                var directionalArrow = new GameObject("UnidirectionalArrow").AddComponent<SpriteRenderer>();
                directionalArrow.transform.SetParent(gameObject.transform);
                directionalArrow.transform.SetPositionAndRotation(_grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel + UnidirectionalArrowPositionOffset, 0.5f), Quaternion.Euler(90f, 0f, 0f));
                _directionalArrows[gridPosition.Dim1] = directionalArrow;
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
        foreach (var position in GridState.GetOccupiedEntityPositions())
        {
            var entity = GridState.GetEntity(position);
            if (!IdRegistry<EntityAssets>.TryGet(entity.Id, out var assets)) continue;
            if (assets.Model3D == null) continue;
            
            var worldPos = _grid.CellToWorld(new Vector3Int(position.Dim2.x, position.Dim2.y, 0)) + new Vector3(0.5f, _gridGroundLevel, 0.5f);
            var rotation = Quaternion.identity;
            if (entity.TryGetComponent<ControlComponent>(out var control) && control.PlayerController == 2)
                rotation = Quaternion.Euler(0, 180f, 0);
            _entityModels[position.Dim1] = Instantiate(assets.Model3D, worldPos, rotation, gameObject.transform);
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
            var position = entity.Position;
            model.transform.position = _grid.CellToWorld(new Vector3Int(position.Dim2.x, position.Dim2.y, 0)) + new Vector3(0.5f, _gridGroundLevel, 0.5f);
            _entityModels[position.Dim1] = model;
        }
        foreach (var position in GridState.GetOccupiedEntityPositions())
        {
            if (_entityModels[position.Dim1] != null) continue;
            var entity = GridState.GetEntity(position);
            if (!IdRegistry<EntityAssets>.TryGet(entity.Id, out var assets)) continue;
            if (assets.Model3D == null) continue;
            var worldPos = _grid.CellToWorld(new Vector3Int(position.Dim2.x, position.Dim2.y, 0)) + new Vector3(0.5f, _gridGroundLevel, 0.5f);
            var rotation = Quaternion.identity;
            if (entity.TryGetComponent<ControlComponent>(out var control) && control.PlayerController == 2)
                rotation = Quaternion.Euler(0, 180f, 0);
            _entityModels[position.Dim1] = Instantiate(assets.Model3D, worldPos, rotation, gameObject.transform);
        }
    }
    
    public void ClearHighlights()
    {
        foreach (var square in _highlightSquares) square.gameObject.SetActive(false);
        foreach (var arrow in _directionalArrows) arrow.gameObject.SetActive(false);
    }
    
    public void HighlightPositions(HashSet<GridPosition> positions, GridHighlightType type)
    {
        var color = HighlightColor(type);
        foreach (var position in positions)
        {
            _highlightSquares[position.Dim1].gameObject.SetActive(true);
            _highlightSquares[position.Dim1].material.SetColor(UnityUtil.MaterialBaseColorId, color);
        }
    }
    
    public void HighlightPositions(GridSteps steps, GridHighlightType type)
    {
        if (_debug)
        {
            HighlightSteps(steps, type);
            return;
        }
        var color = HighlightColor(type);
        var positions = steps.GetPositions();
        foreach (var position in positions)
        {
            _highlightSquares[position.Dim1].gameObject.SetActive(true);
            _highlightSquares[position.Dim1].material.SetColor(UnityUtil.MaterialBaseColorId, color);
        }
    }
    
    private void HighlightSteps(GridSteps gridSteps, GridHighlightType type)
    {
        var color = HighlightColor(type);
        var steps = gridSteps.GetSteps();
        foreach (var step in steps)
        {
            var position = step.Position;
            _highlightSquares[position.Dim1].gameObject.SetActive(true);
            _highlightSquares[position.Dim1].material.SetColor(UnityUtil.MaterialBaseColorId, color);
            
            var direction = (int)step.Direction;
            _directionalArrows[position.Dim1].gameObject.SetActive(true);
            _directionalArrows[position.Dim1].sprite = _directionalArrowSprites[direction];
        }
    }
    
    // public void HighlightPositions(List<HashSet<GridStep>> steps, GridHighlightType type)
    // {
    //     var color = HighlightColor(type);
    //     foreach (var list in steps)
    //     foreach (var step in list)
    //     {
    //         var position = _gridManager.GridState.ToPosition1D(step.Position);
    //         _highlightSquares[position].gameObject.SetActive(true);
    //         if (_highlightSquares[position].material.GetColor(UnityUtil.MaterialBaseColorId) == color)
    //         {
    //             var colorMore = color;
    //             SetAlpha(colorMore, color.a * 2.0f);
    //             _highlightSquares[position].material.SetColor(UnityUtil.MaterialBaseColorId, colorMore);
    //         }
    //         else
    //         {
    //             _highlightSquares[position].material.SetColor(UnityUtil.MaterialBaseColorId, color);
    //         }
    //     }
    // }

    private Color HighlightColor(GridHighlightType type) => type switch
    {
        GridHighlightType.AvailableEntities => _highlightAvailableEntities,
        GridHighlightType.AbilityRange      => _highlightAbilityRange,
        GridHighlightType.SelectableTargets => _highlightSelectableTargets,
        GridHighlightType.EffectPreview     => _highlightEffectPreview,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
    
    private Color SetAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }
}
