using UnityEngine;

[CreateAssetMenu(fileName = "NewDinoInfo", menuName = "Dictionary/DinoInfo")]
public class DinoInfo : ScriptableObject
{
    public string dinoName;  // °ø·æ ÀÌ¸§
    [TextArea] public string description;  // °ø·æ ¼³¸í
}
