using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class CameraRig : LoggableBehaviour
{
    private const float TopElevation = 90f;
    private const float QuarterTurn = 90f;

    [SerializeField] private GridManager _gridManager;
    [SerializeField] private CinemachineCamera[] _playerCameras;
    [SerializeField] private bool _faceLocalPlayer = false;
    [SerializeField, FormerlySerializedAs("_minHeight")] private float _minDistance = 4f;
    [SerializeField, FormerlySerializedAs("_maxHeight")] private float _maxDistance = 20f;
    [SerializeField] private float _zoomStep = 1f;
    [SerializeField] private float _smoothTime = 0.15f;
    [SerializeField] private float _orbitDegreesPerPixel = 0.25f;
    [SerializeField] private float _minElevation = 0f;

    private Transform[] _targets;
    private CinemachineFollow[] _follows;
    private Bounds _bounds;
    private float _defaultDistance;
    private int _active;
    private Vector2? _lastDragPoint;
    private InputAction _panAction;
    private InputAction _pointAction;
    private InputAction _zoomAction;

    private Vector3 _focus;
    private Vector3 _shownFocus;
    private Vector3 _focusVelocity;
    private float _yaw;
    private float _shownYaw;
    private float _yawVelocity;
    private float _elevation = TopElevation;
    private float _shownElevation = TopElevation;
    private float _elevationVelocity;
    private float _distance;
    private float _shownDistance;
    private float _distanceVelocity;

    public bool TopDown { get; private set; } = true;

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
        _defaultDistance = _follows[0].FollowOffset.y;
        foreach (var camera in GetComponentsInChildren<CinemachineCamera>(true))
        {
            var index = System.Array.IndexOf(_playerCameras, camera);
            camera.Target.TrackingTarget = _targets[Mathf.Max(index, 0)];
            camera.PreviousStateIsValid = false;
        }
        _focus = _shownFocus = _bounds.center;
        _distance = _shownDistance = _defaultDistance;
        for (var i = 0; i < _targets.Length; i++) Apply(i);
        _panAction = InputSystem.actions.FindAction("Player/Pan");
        _pointAction = InputSystem.actions.FindAction("Player/Point");
        _zoomAction = InputSystem.actions.FindAction("Player/Zoom");
        FaceLocalPlayer();
        _gridManager.StateChanged += FaceLocalPlayer;
    }

    public void ResetView()
    {
        _focus = _bounds.center;
        _yaw = Mathf.Round(_yaw / 360f) * 360f;
        _distance = _defaultDistance;
        _elevation = TopElevation;
    }

    public void Rotate() => _yaw += QuarterTurn;

    public void ToggleTopDown()
    {
        TopDown = !TopDown;
        if (TopDown) _elevation = TopElevation;
    }

    private void Update()
    {
        if (_targets == null) return;
        ReadDrag();
        ReadZoom();
        Smooth();
        Apply(_active);
    }

    private void Smooth()
    {
        _shownFocus = Vector3.SmoothDamp(_shownFocus, _focus, ref _focusVelocity, _smoothTime);
        _shownYaw = Mathf.SmoothDamp(_shownYaw, _yaw, ref _yawVelocity, _smoothTime);
        _shownElevation = Mathf.SmoothDamp(_shownElevation, _elevation, ref _elevationVelocity, _smoothTime);
        _shownDistance = Mathf.SmoothDamp(_shownDistance, _distance, ref _distanceVelocity, _smoothTime);
    }

    private void Apply(int index)
    {
        var yaw = BaseYaw(index) + _shownYaw;
        _targets[index].SetPositionAndRotation(_shownFocus, Quaternion.Euler(0f, yaw, 0f));
        var elevation = _shownElevation * Mathf.Deg2Rad;
        var height = _shownDistance * Mathf.Sin(elevation);
        var back = _shownDistance * Mathf.Cos(elevation);
        _follows[index].FollowOffset = new Vector3(0f, height, -back);
    }

    private static float BaseYaw(int index) => index == 1 ? 180f : 0f;

    private void FaceLocalPlayer()
    {
        var active = _faceLocalPlayer ? Mathf.Clamp(_gridManager.LocalSeat - 1, 0, _playerCameras.Length - 1) : 0;
        _active = active;
        for (var i = 0; i < _playerCameras.Length; i++) _playerCameras[i].Priority = i == _active ? 0 : -1;
    }

    private void ReadDrag()
    {
        if (!_panAction.IsPressed())
        {
            _lastDragPoint = null;
            return;
        }
        var point = _pointAction.ReadValue<Vector2>();
        if (_lastDragPoint.HasValue) Drag(point - _lastDragPoint.Value);
        else if (_gridManager.Input.IsPointerOverUI) return;
        _lastDragPoint = point;
    }

    private void Drag(Vector2 screenDelta)
    {
        if (TopDown) Pan(screenDelta);
        else Orbit(screenDelta);
    }

    private void ReadZoom()
    {
        var scroll = _zoomAction.ReadValue<float>();
        if (scroll != 0f && !_gridManager.Input.IsPointerOverUI) Zoom(scroll);
    }

    private void Pan(Vector2 screenDelta)
    {
        var camera = _gridManager.MainCamera;
        var unitsPerPixel = 2f * _shownDistance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) / Screen.height;
        var right = Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized;
        var up = Vector3.ProjectOnPlane(camera.transform.up, Vector3.up).normalized;
        var position = _focus - (right * screenDelta.x + up * screenDelta.y) * unitsPerPixel;
        position.x = Mathf.Clamp(position.x, _bounds.min.x, _bounds.max.x);
        position.z = Mathf.Clamp(position.z, _bounds.min.z, _bounds.max.z);
        _focus = position;
        _shownFocus = position;
        _focusVelocity = Vector3.zero;
    }

    private void Orbit(Vector2 screenDelta)
    {
        _yaw += screenDelta.x * _orbitDegreesPerPixel;
        _elevation = Mathf.Clamp(_elevation - screenDelta.y * _orbitDegreesPerPixel, _minElevation, TopElevation);
        _shownYaw = _yaw;
        _shownElevation = _elevation;
        _yawVelocity = 0f;
        _elevationVelocity = 0f;
    }

    private void Zoom(float amount)
    {
        _distance = Mathf.Clamp(_distance - amount * _zoomStep, _minDistance, _maxDistance);
    }
}
