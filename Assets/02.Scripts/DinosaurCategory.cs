using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DinosaurCategory", menuName = "Pooling/DinosaurCategory")]
public class DinosaurCategory : ScriptableObject
{
    public GameObject[] large;
    public GameObject[] medium;
    public GameObject[] small;
}
