using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;
using System.Linq;

public class WFCGenerator : MonoBehaviour
{
    public Tilemap outputTilemap; // Reference to the output Tilemap in the Inspector
    public WFCParameters wfcParameters; // Reference to the WFCParameters scriptable object

    private OverlappingModel wfcModel; // Your WFC model

    private void Start()
    {
        /*int width = wfcParameters.width;
        int height = wfcParameters.height;
        bool periodic = wfcParameters.periodic;

        // Create an instance of your WFC model with parameters from the scriptable object
        wfcModel = new SimpleTiledModel(wfcParameters.tilesetData, width, height, periodic, false, Heuristic.LowEntropy);

        // Perform the WFC algorithm to generate the pattern
        wfcModel.Run();

        // Replace the following loop with your code to fill the Tilemap based on the generated pattern
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int tileIndex = wfcModel.observed[x + y * width];
                Tile tile = wfcParameters.tilesetData.tileTextures[tileIndex] as Tile;
                outputTilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }*/
    }
}
