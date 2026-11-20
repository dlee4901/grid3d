using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/TerrainAssets")]
public class TerrainAssets : ScriptableObject, INameId
{
    [field: SerializeField] public string Id { get; private set; }
    public string DisplayName;
    [ColorUsage(false, true)] public Color TopEmission = Color.black;
    [ColorUsage(false, true)] public Color SideEmission = Color.black;
    public GameObject Prop;
}
