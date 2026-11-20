using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/TraitAssets")]
public class TraitAssets : ScriptableObject, INameId
{
    [field: SerializeField] public string Id { get; private set; }
    public Sprite Icon;
}
