using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TilesetData", menuName = "WFC/Tileset Data", order = 1)]
public class TilesetData : ScriptableObject
{
    public Texture2D[] tileTextures; // Texture for each tile
    public List<TileSubsetData> subsets; // List of tile subsets
}

[System.Serializable]
public class TileSubsetData
{
    public string name; // Name of the subset
    public List<int> tileIndices; // Indices of tiles in this subset
}
