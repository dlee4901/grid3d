using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/EntityAssets")]
public class EntityAssets : ScriptableObject, INameId
{
    [field: SerializeField] public string Id { get; private set; }
    public GameObject Prefab3D;
    public GameObject Prefab2D;
}
