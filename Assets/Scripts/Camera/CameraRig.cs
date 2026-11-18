using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRig : LoggableBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private CinemachineCamera[] _playerCameras;
    [SerializeField] private bool _faceLocalPlayer = false;
    [SerializeField] private float _minHeight = 4f;
    [SerializeField] private float _maxHeight = 20f;
    [SerializeField] private float _zoomStep = 1f;
    [SerializeField] private float _resetDuration = 0.35f;

    private Transform[] _targets;
    private CinemachineFollow[] _follows;
    private Bounds _bounds;
    private float _defaultHeight;
    private int _active;
    private float _resetProgress = 1f;
    private Vector3 _resetFromPosition;
    private float _resetFromHeight;
    private Vector2? _lastPanPoint;
    private InputAction _panAction;
    private InputAction _pointAction;
    private InputAction _zoomAction;

    private void OnDestroy()
    {
        if (_gridManager != null) _gridManager.StateChanged -= FaceLocalPlayer;
    }

    public void Build(Bounds bounds)
    {
        _bounds = bounds;
        _targets = new Transform[_playerCameras.Length];
        _follows = new CinemachineFollow[_playerCameras.Length];
        for (var i = 0; i < _playerCameras.Length; i++)
        {
            _targets[i] = new GameObject($"CameraTarget P{i + 1}").transform;
            _targets[i].SetParent(transform);
            _follows[i] = _playerCameras[i].GetComponent<CinemachineFollow>();
        }
        _defaultHeight = _follows[0].FollowOffset.y;
        foreach (var camera in GetComponentsInChildren<CinemachineCamera>(true))
        {
            var index = System.Array.IndexOf(_playerCameras, camera);
            camera.Target.TrackingTarget = _targets[Mathf.Max(index, 0)];
            camera.PreviousStateIsValid = false;
        }
        for (var i = 0; i < _targets.Length; i++) SetView(i, _bounds.center, _defaultHeight);
        _panAction = InputSystem.actions.FindAction("Player/Pan");
        _pointAction = InputSystem.actions.FindAction("Player/Point");
        _zoomAction = InputSystem.actions.FindAction("Player/Zoom");
        FaceLocalPlayer();
        _gridManager.StateChanged += FaceLocalPlayer;
    }

    public void ResetView()
    {
        _resetFromPosition = _targets[_active].position;
        _resetFromHeight = _follows[_active].FollowOffset.y;
        _resetProgress = 0f;
    }

    private void StepReset()
    {
        _resetProgress = Mathf.MoveTowards(_resetProgress, 1f, Time.deltaTime / _resetDuration);
        var t = Mathf.SmoothStep(0f, 1f, _resetProgress);
        SetView(_active, Vector3.Lerp(_resetFromPosition, _bounds.center, t), Mathf.Lerp(_resetFromHeight, _defaultHeight, t));
    }

    private void SetView(int index, Vector3 position, float height)
    {
        _targets[index].position = position;
        _follows[index].FollowOffset = new Vector3(0f, height, 0f);
    }

    private void Update()
    {
        if (_targets == null) return;
        if (_resetProgress < 1f) StepReset();
        ReadPan();
        ReadZoom();
    }

    private void FaceLocalPlayer()
    {
        var active = _faceLocalPlayer ? Mathf.Clamp(_gridManager.LocalSeat - 1, 0, _playerCameras.Length - 1) : 0;
        if (active != _active) _resetProgress = 1f;
        _active = active;
        for (var i = 0; i < _playerCameras.Length; i++) _playerCameras[i].Priority = i == _active ? 0 : -1;
    }

    private void ReadPan()
    {
        if (!_panAction.IsPressed())
        {
            _lastPanPoint = null;
            return;
        }
        var point = _pointAction.ReadValue<Vector2>();
        if (_lastPanPoint.HasValue) Pan(point - _lastPanPoint.Value);
        else if (_gridManager.Input.IsPointerOverUI) return;
        _lastPanPoint = point;
    }

    private void ReadZoom()
    {
        var scroll = _zoomAction.ReadValue<float>();
        if (scroll != 0f && !_gridManager.Input.IsPointerOverUI) Zoom(scroll);
    }

    private void Pan(Vector2 screenDelta)
    {
        _resetProgress = 1f;
        var camera = _gridManager.MainCamera;
        var height = _follows[_active].FollowOffset.y;
        var unitsPerPixel = 2f * height * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) / Screen.height;
        var right = Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized;
        var up = Vector3.ProjectOnPlane(camera.transform.up, Vector3.up).normalized;
        var target = _targets[_active];
        var position = target.position - (right * screenDelta.x + up * screenDelta.y) * unitsPerPixel;
        position.x = Mathf.Clamp(position.x, _bounds.min.x, _bounds.max.x);
        position.z = Mathf.Clamp(position.z, _bounds.min.z, _bounds.max.z);
        target.position = position;
    }

    private void Zoom(float amount)
    {
        _resetProgress = 1f;
        var height = Mathf.Clamp(_follows[_active].FollowOffset.y - amount * _zoomStep, _minHeight, _maxHeight);
        _follows[_active].FollowOffset = new Vector3(0f, height, 0f);
    }
}
