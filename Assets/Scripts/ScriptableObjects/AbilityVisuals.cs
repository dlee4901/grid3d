using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/AbilityVisuals")]
public class AbilityVisuals : ScriptableObject, INameId
{
    [field: SerializeField] public string Id { get; private set; }
    public Sprite Icon;
    public CasterAction CasterAction;
}
