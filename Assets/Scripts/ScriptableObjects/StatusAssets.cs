using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/StatusAssets")]
public class StatusAssets : ScriptableObject, INameId
{
    [field: SerializeField] public string Id { get; private set; }
    public Sprite Icon;
}
