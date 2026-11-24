using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class TerrainView : StateView
{
    [SerializeField] private RectTransform _swatchSlot;
    [SerializeField] private SquarePortrait _swatchPrefab;
    [SerializeField] private PortraitElements _swatchElements = PortraitElements.Counter;
    [SerializeField] private TooltipStack _tooltips;
    [SerializeField] private int _snapshotResolution = 256;
    [SerializeField] private Vector3 _studioPosition = new(0f, -1000f, 0f);

    private readonly Dictionary<TerrainType, Texture2D> _snapshots = new();
    private SquarePortrait _swatch;
    private RawImage _swatchTexture;
    private GridPosition? _tooltipCell;

    private GridPosition? ShownCell => _gridManager.Player.CurrentSelection?.Position;

    protected override void OnGameStarted()
    {
        _swatch = Instantiate(_swatchPrefab, _swatchSlot);
        var swatchRect = (RectTransform)_swatch.transform;
        swatchRect.anchorMin = swatchRect.anchorMax = new Vector2(0.5f, 0.5f);
        swatchRect.anchoredPosition = Vector2.zero;
        _swatch.Configure(_swatchElements);
        _swatchTexture = new GameObject("Texture", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
        var textureRect = _swatchTexture.rectTransform;
        textureRect.SetParent(_swatch.Image.rectTransform, false);
        textureRect.anchorMin = Vector2.zero;
        textureRect.anchorMax = Vector2.one;
        textureRect.offsetMin = Vector2.zero;
        textureRect.offsetMax = Vector2.zero;
        _swatchTexture.raycastTarget = false;
        _swatch.Button.OnClickCompleted += ToggleTooltip;
        _gridManager.Player.SelectionChanged += OnSelectionChanged;
        base.OnGameStarted();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        foreach (var snapshot in _snapshots.Values) Destroy(snapshot);
        if (_gridManager == null) return;
        if (_gridManager.Player != null) _gridManager.Player.SelectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged(GridSource? source) => RenderShown();

    protected override void Render(GridState shown)
    {
        var cell = ShownCell;
        if (!Equals(cell, _tooltipCell)) CloseTooltip();
        _swatch.gameObject.SetActive(cell.HasValue);
        if (!cell.HasValue) return;

        shown.TryGetTerrain(cell.Value, out var terrain);
        IdRegistry<TerrainAssets>.TryGet(terrain.ToString(), out var assets);
        var turns = shown.GetTerrainTurns(cell.Value);
        _swatchTexture.texture = SnapshotFor(terrain, assets);
        _swatch.Init(null);
        _swatch.SetImageColor(Color.black);
        _swatch.SetCount(turns > 0 ? turns : null);
    }

    private void ToggleTooltip()
    {
        var rect = (RectTransform)_swatch.transform;
        if (_tooltips.IsOpenFor(this, rect) || ShownCell is not { } cell)
        {
            CloseTooltip();
            return;
        }
        _gridManager.Player.DeselectAbility();
        var shown = _gridManager.DisplayedState;
        shown.TryGetTerrain(cell, out var terrain);
        _tooltips.OpenRoot(this, TooltipText.ForTerrain(terrain, shown, shown.GetTerrainTurns(cell)), rect, TooltipSide.Right);
        _tooltipCell = cell;
    }

    private void CloseTooltip()
    {
        _tooltips.Close(this);
        _tooltipCell = null;
    }

    private Texture2D SnapshotFor(TerrainType terrain, TerrainAssets assets)
    {
        if (_snapshots.TryGetValue(terrain, out var cached)) return cached;
        var snapshot = new Texture2D(_snapshotResolution, _snapshotResolution, TextureFormat.RGBA32, false);
        _snapshots[terrain] = snapshot;
        StartCoroutine(RenderSnapshot(snapshot, terrain, assets));
        return snapshot;
    }

    private IEnumerator RenderSnapshot(Texture2D snapshot, TerrainType terrain, TerrainAssets assets)
    {
        var cube = Instantiate(_gridManager.Renderer.PositionCubePrefab, _studioPosition, Quaternion.identity);
        cube.SetTerrainEmission(terrain == TerrainType.Plain ? null : assets);
        var top = _studioPosition + Vector3.up * (cube.transform.localScale.y * 0.5f);
        var prop = assets != null && assets.Prop != null ? Instantiate(assets.Prop, top, Quaternion.identity) : null;
        var target = new RenderTexture(_snapshotResolution, _snapshotResolution, 24);

        var camera = new GameObject("TerrainSnapshotCamera").AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 0.5f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 4f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.transform.SetPositionAndRotation(top + Vector3.up * 2f, Quaternion.Euler(90f, 0f, 0f));
        camera.GetUniversalAdditionalCameraData().renderShadows = false;
        camera.targetTexture = target;
        camera.aspect = 1f;

        yield return null;

        var previous = RenderTexture.active;
        RenderTexture.active = target;
        snapshot.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
        snapshot.Apply();
        RenderTexture.active = previous;

        camera.targetTexture = null;
        Destroy(camera.gameObject);
        Destroy(cube.gameObject);
        if (prop != null) Destroy(prop);
        target.Release();
        Destroy(target);
    }
}
