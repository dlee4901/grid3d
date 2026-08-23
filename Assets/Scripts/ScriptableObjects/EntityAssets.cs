using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/EntityAssets")]
public class EntityAssets : ScriptableObject, INameId
{
    [field: SerializeField] public string Id { get; private set; }
    public GameObject Model3D;
    public Sprite Model2D;
    public List<Sprite> AbilityIcons;
}
