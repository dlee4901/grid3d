using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 GetMouseWorldPosition()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit) ? hit.point : Vector3.zero;
    }
}
