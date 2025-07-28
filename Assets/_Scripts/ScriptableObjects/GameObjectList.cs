using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/GameObjectList")]
public class GameObjectList : ScriptableObject
{
    public List<GameObject> GameObjects;
}
