using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class Test : MonoBehaviour
{

    int indice = 3;
    [SerializeField] List<Tile> tiles;
    [SerializeField] BoundsInt bounds;
    private TileBase[] allTiles;

    // Start is called before the first frame update
    void Start()
    {
        Tilemap tilemap = GetComponent<Tilemap>();

        BoundsInt bounds = tilemap.cellBounds;
        allTiles = tilemap.GetTilesBlock(bounds);

        TileBase[,] tileMatrix = new TileBase[bounds.size.x, bounds.size.y];

        Dictionary<string, TileData> dict = new Dictionary<string, TileData>();

        ExtractTileData(bounds, allTiles, tileMatrix, dict);

        StartCoroutine(GenerateTileMap(tilemap, dict));


    }

    private void ExtractTileData(BoundsInt bounds, TileBase[] allTiles, TileBase[,] tileMatrix, Dictionary<string, TileData> dict)
    {
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    if (!dict.ContainsKey(tile.name))
                        dict.Add(tile.name, new TileData() { name = tile.name, patterns = new List<Pattern>() });

                    tileMatrix[x, y] = tile;
                }
            }
        }
        ExtractPatterns(bounds, allTiles, dict);
    }


    private void ExtractPatterns(BoundsInt bounds, TileBase[] allTiles, Dictionary<string, TileData> dict)
    {
        foreach (var key in dict.Keys)
        {
            var tileData = dict.GetValueOrDefault(key);
            for (int x = 0; x < bounds.size.x; x++)
            {
                for (int y = 0; y < bounds.size.y; y++)
                {
                    TileBase tile = allTiles[x + y * bounds.size.x];
                    if (tile.name == key)
                    {
                        tileData.patterns.Add(FindPattern(x, y, bounds));
                    }
                }
            }
        }
    }

    private Pattern FindPattern(int x, int y, BoundsInt bounds)
    {
        TileBase tile = allTiles[x + y * bounds.size.x];
        Pattern pattern = new Pattern(indice);

        for (int i = -indice; i < indice + 1; i++)
        {
            for (int j = -indice; j < indice + 1; j++)
            {
                var valid = !((x + i) < 0 || (y + j) < 0 || (x + i) > bounds.size.x - 1 || (y + j) > bounds.size.y - 1);
                pattern[i, j] = valid ? allTiles[(x + i) + (y + j) * bounds.size.x].name : "";
            }
        }

        return pattern;
    }

    private IEnumerator GenerateTileMap(Tilemap tilemap, Dictionary<string, TileData> dict)
    {

        var newAllTiles = new TileBase[(bounds.size.x) + (bounds.size.y - 1) * bounds.size.x];
        var newAllPos = new Vector3Int[(bounds.size.x) + (bounds.size.y - 1) * bounds.size.x];

        tilemap.ClearAllTiles();

        //Set possibility for each tiles 
        GeneratedTile[,] loadingMap = new GeneratedTile[bounds.size.x, bounds.size.y];
        List<GeneratedTile> loadingList = new List<GeneratedTile>();

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                loadingMap[x, y] = new GeneratedTile() { availableTiles = dict.Keys.ToList(), x = x, y = y };
                loadingList.Add(loadingMap[x, y]);
            }
        }

        //Start with one, then loop
        var rndX = Random.Range(0, bounds.size.x - 1);
        var rndY = Random.Range(0, bounds.size.y - 1);

        loadingMap[rndX, rndY].tile = FindTileByName(loadingMap[rndX, rndY].availableTiles[Random.Range(0, loadingMap[rndX, rndY].availableTiles.Count - 1)]);

        newAllTiles[rndX + rndY * bounds.size.x] = loadingMap[rndX, rndY].tile;
        newAllPos[rndX + rndY * bounds.size.x] = new Vector3Int(tilemap.cellBounds.xMin + rndX, tilemap.cellBounds.yMin + rndY, 0);

        var result = RecalculateAvailableTiles(rndX, rndY, loadingMap, bounds, dict);


        while (loadingList.Where((g) => g.tile == null).Count() != 0 && result)
        {
            var min = loadingList.Min((g) => g.availableTiles.Count);
            if (min == 0) break;
            var noTileList = loadingList.Where((g) => g.tile == null && g.availableTiles.Count == min).ToList();

            var selectedTile = noTileList[Random.Range(0, noTileList.Count - 1)];
            selectedTile.tile = FindTileByName(selectedTile.availableTiles[selectedTile.availableTiles.Count == 1 ? 0 : Random.Range(0, selectedTile.availableTiles.Count - 1)]);

            newAllTiles[selectedTile.x + selectedTile.y * bounds.size.x] = selectedTile.tile;
            newAllPos[selectedTile.x + selectedTile.y * bounds.size.x] = new Vector3Int(tilemap.cellBounds.xMin + selectedTile.x, tilemap.cellBounds.yMin + selectedTile.y, 0);

            result = RecalculateAvailableTiles(selectedTile.x, selectedTile.y, loadingMap, bounds, dict);

            //yield return new WaitForSeconds(.05f);
            yield return new WaitForEndOfFrame();

            tilemap.SetTiles(newAllPos, newAllTiles);
        }
    }

    private bool RecalculateAvailableTiles(int x, int y, GeneratedTile[,] loadingMap, BoundsInt bounds, Dictionary<string, TileData> dict)
    {
        var tileData = dict.GetValueOrDefault(loadingMap[x, y].tile.name);

        for (int i = -indice; i < indice + 1; i++)
        {
            for (int j = -indice; j < indice + 1; j++)
            {
                var validNb = !((x + i) < 0 || (y + j) < 0 || (x + i) > bounds.size.x - 1 || (y + j) > bounds.size.y - 1);
                if (validNb && loadingMap[x + i, y + j].tile == null)
                {
                    List<string> possibilities = new List<string>();
                    foreach (var pattern in tileData.patterns)
                    {
                        if (!possibilities.Contains(pattern[i, j]))
                            possibilities.Add(pattern[i, j]);
                    }

                    var availables = new List<string>(loadingMap[x + i, y + j].availableTiles);

                    foreach (var tile in availables)
                    {
                        if (!possibilities.Contains(tile))
                            loadingMap[x + i, y + j].availableTiles.Remove(tile);
                    }

                    var res = true;

                    if (loadingMap[x + i, y + j].availableTiles.Count == 0)
                        return false;
                    else if (loadingMap[x + i, y + j].availableTiles.Count == 1)
                    {
                        loadingMap[x + i, y + j].tile = FindTileByName(loadingMap[x + i, y + j].availableTiles.FirstOrDefault());
                        res = RecalculateAvailableTiles(x + i, y + j, loadingMap, bounds, dict);
                        if (!res)
                            return false;
                    }

                }

            }
        }
        return true;
    }


    Tile FindTileByName(string name)
    {
        Debug.Log("WFC map, FindTileByName : value = " + name);
        return tiles.Where<Tile>((t) => t.name == name).FirstOrDefault();
    }

    public class TileData
    {
        public string name;
        public List<Pattern> patterns;
    }
    public class GeneratedTile
    {
        public Tile tile;
        public int x;
        public int y;
        public List<string> availableTiles;
    }


    public class Pattern
    {
        private int size;
        private int offset;

        public string[,] pattern { get; private set; }

        public Pattern(int size)
        {
            this.size = (2 * size + 1);
            this.offset = size;
            pattern = new string[(2 * size + 1), (2 * size + 1)];
        }

        public string this[int x, int y]
        {
            get { return pattern[x + (size / 2), y + (size / 2)]; }
            set { pattern[x + offset, y + offset] = value; }
        }

    }
}
