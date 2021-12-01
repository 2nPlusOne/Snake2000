using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Critter Types", menuName = "Scriptable Objects/Critter Types")]
public class CritterTypesSO : ScriptableObject
{
    public List<Transform> Critters = new List<Transform>();
}