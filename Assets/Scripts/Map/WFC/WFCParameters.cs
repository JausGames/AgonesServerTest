using UnityEngine;

[CreateAssetMenu(fileName = "WFCParameters", menuName = "WFC/Parameters", order = 2)]
public class WFCParameters : ScriptableObject
{
    public int width;
    public int height;
    public bool periodic;
    public TilesetData tilesetData; // Reference to the TilesetData scriptable object
}
