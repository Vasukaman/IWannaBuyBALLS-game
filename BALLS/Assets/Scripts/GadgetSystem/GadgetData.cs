using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/GadgetData")]
public class GadgetData : ScriptableObject
{
    public string displayName;
    public int price;
    public GameObject prefab;
    public Sprite icon; // optional, good for UI/shop
}
