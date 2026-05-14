using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapLoader : MonoBehaviour
{
    // Directions used for neighbor checks
    private static readonly int[] dx = { 0, 0, -1, 1 };
    private static readonly int[] dy = { -1, 1, 0, 0 };

    public Tilemap groundTilemap;
    public Camera mainCamera;
    public GameObject textPrefab;

    public TileBase DarkDirt;
    public TileBase Dirt;
    public TileBase Sky;
    public TileBase DarkGrass;
    public TileBase Grass;
    public TileBase Rock;
    public TileBase Food;
    public TileBase Queen;

    public string mapFilePath = "Tiles/Maps";
    public string expOddsFile = "/Tiles/ExpansionOdds.txt";

    public float updateInterval = 0.1f;
    public bool debugMode;

    public Vector2Int queenPos;
    public bool queenFound;

    public int minJobBuffer = 10;

    // Queue of digging jobs for ants
    public List<DigJob> jobs = new();

    private char[,] mapData;
    private float[,] oddsGrid;

    // Used to predict future tunnel expansion
    private char[,] plannedMap;

    private Dictionary<Vector2Int, TMP_Text> textMap = new();

    private float timer;

    public int Rows { get; private set; }
    public int Cols { get; private set; }

    private void Start()
    {
        InitializeMap();
        plannedMap = (char[,])mapData.Clone();

        UpdateOddsMap();
        RefreshTilemap();

        Debug.Log("Queen found at: " + queenPos);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= updateInterval)
        {
            RefillJobQueue();
            ExpandMapIfNeeded();

            RefreshTilemap();

            FindObjectOfType<AntManager>().AssignJobs();

            timer = 0f;
        }
    }

    // Loads the map from the text file
    private void InitializeMap()
    {
        // No file extension needed for Resources.Load
        TextAsset mapAsset = Resources.Load<TextAsset>(mapFilePath);

        if (mapAsset == null)
        {
            Debug.LogError($"File not found at Assets/Resources/{mapFilePath}.txt");
            return;
        }

        // Split the text into lines
        string[] lines = mapAsset.text.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        Rows = lines.Length;
        Cols = lines[0].Length;

        mapData = new char[Rows, Cols];
        oddsGrid = new float[Rows, Cols];

        for (var y = 0; y < Rows; y++)
            for (var x = 0; x < Cols; x++)
                mapData[y, x] = lines[y][x];

        // Find queen position
        for (var y = 0; y < Rows; y++)
            for (var x = 0; x < Cols; x++)
                if (mapData[y, x] == '9')
                {
                    queenPos = new Vector2Int(x, y);
                    queenFound = true;
                }
    }

    // Recalculates digging odds for every wall tile
    private void UpdateOddsMap()
    {
        for (var y = 0; y < Rows; y++)
            for (var x = 0; x < Cols; x++)
            {
                if (mapData[y, x] != '1')
                {
                    oddsGrid[y, x] = 0;
                    continue;
                }

                var zeroNeighbors = CountZeroNeighbors(x, y);
                var baseOdds = GetBaseOdds(zeroNeighbors);
                var nearby = CountNearbyZeros(x, y, 3);

                oddsGrid[y, x] = ApplyTunnelPenalty(baseOdds, nearby);
            }
    }

    // Converts map coordinates into tilemap coordinates
    public Vector3Int MapToTilePos(Vector2Int p)
    {
        return new Vector3Int(p.x - Cols / 2, -p.y + Rows / 2, 0);
    }

    private float GetBaseOdds(int zeroNeighbors)
    {
        if (zeroNeighbors == 1) return 80;
        if (zeroNeighbors == 2) return 40;
        if (zeroNeighbors >= 3) return 5;

        return 2;
    }

    // Ensures there are always enough dig jobs available
    public void RefillJobQueue()
    {
        var unjobbed = jobs.Count(j => !j.taken);

        if (unjobbed >= minJobBuffer) return;

        DigWeightedTileOnPlannedMap();

        for (var y = 0; y < Rows; y++)
            for (var x = 0; x < Cols; x++)
                if (plannedMap[y, x] == '0' && mapData[y, x] == '1')
                {
                    var pos = new Vector2Int(x, y);

                    if (!jobs.Exists(j => j.target == pos))
                    {
                        jobs.Add(new DigJob(pos));
                        return;
                    }
                }
    }

    // Selects a future tunnel tile using weighted randomness
    private void DigWeightedTileOnPlannedMap()
    {
        var frontTiles = GetTunnelFrontFromPlanned();

        var candidates = new List<Vector2Int>();
        var weights = new List<int>();

        foreach (var front in frontTiles)
        {
            int[] dirWeights = { 10, 50, 20, 20 };

            var totalDirWeight = 100;
            var rand = Random.Range(0, totalDirWeight);

            var chosenDir = 0;
            var sum = 0;

            for (var i = 0; i < 4; i++)
            {
                sum += dirWeights[i];

                if (rand < sum)
                {
                    chosenDir = i;
                    break;
                }
            }

            var nx = front.x + dx[chosenDir];
            var ny = front.y + dy[chosenDir];

            if (IsInside(nx, ny) && plannedMap[ny, nx] == '1' && CountZeroNeighborsPlanned(nx, ny) == 1)
            {
                var nearby = CountNearbyZerosPlanned(nx, ny, 2);

                var penalty = Mathf.Clamp(1f - 0.15f * nearby, 0.1f, 1f);

                candidates.Add(new Vector2Int(nx, ny));
                weights.Add(Mathf.RoundToInt(GetBaseOdds(1) * penalty));
            }
        }

        if (candidates.Count == 0) return;

        var totalWeight = weights.Sum();
        var randomValue = Random.Range(0, totalWeight);

        var sumWeight = 0;

        for (var i = 0; i < candidates.Count; i++)
        {
            sumWeight += weights[i];

            if (randomValue < sumWeight)
            {
                plannedMap[candidates[i].y, candidates[i].x] = '0';
                break;
            }
        }
    }

    // Finds tunnel edge tiles in the planned map
    private List<Vector2Int> GetTunnelFrontFromPlanned()
    {
        var front = new List<Vector2Int>();

        for (var y = 0; y < Rows; y++)
            for (var x = 0; x < Cols; x++)
            {
                if (plannedMap[y, x] != '0' && plannedMap[y, x] != '9') continue;

                for (var i = 0; i < 4; i++)
                {
                    int nx = x + dx[i], ny = y + dy[i];

                    if (IsInside(nx, ny) && plannedMap[ny, nx] == '1')
                    {
                        front.Add(new Vector2Int(x, y));
                        break;
                    }
                }
            }

        return front;
    }

    private int CountZeroNeighborsPlanned(int x, int y)
    {
        var count = 0;

        for (var i = 0; i < 4; i++)
        {
            int nx = x + dx[i], ny = y + dy[i];

            if (IsInside(nx, ny) && (plannedMap[ny, nx] == '0' || plannedMap[ny, nx] == '9'))
                count++;
        }

        return count;
    }

    private int CountNearbyZerosPlanned(int x, int y, int radius)
    {
        var count = 0;

        for (var yy = -radius; yy <= radius; yy++)
            for (var xx = -radius; xx <= radius; xx++)
            {
                int nx = x + xx, ny = y + yy;

                if (IsInside(nx, ny) && (plannedMap[ny, nx] == '0' || plannedMap[ny, nx] == '9'))
                    count++;
            }

        return count;
    }

    // Reduces digging odds if there are already many nearby tunnels
    private float ApplyTunnelPenalty(float baseOdds, int nearbyTunnels)
    {
        return baseOdds * Mathf.Clamp(1f - 0.15f * nearbyTunnels, 0.1f, 1f);
    }

    // Updates digging odds around a modified tile
    public void UpdateOddsAround(int cx, int cy, int radius = 3)
    {
        for (var y = cy - radius; y <= cy + radius; y++)
            for (var x = cx - radius; x <= cx + radius; x++)
            {
                if (!IsInside(x, y) || mapData[y, x] != '1') continue;

                oddsGrid[y, x] = ApplyTunnelPenalty(
                    GetBaseOdds(CountZeroNeighbors(x, y)),
                    CountNearbyZeros(x, y, 2)
                );
            }
    }

    private int CountZeroNeighbors(int x, int y)
    {
        var count = 0;

        for (var i = 0; i < 4; i++)
        {
            int nx = x + dx[i], ny = y + dy[i];

            if (IsInside(nx, ny) && (mapData[ny, nx] == '0' || mapData[ny, nx] == '9'))
                count++;
        }

        return count;
    }

    private int CountNearbyZeros(int x, int y, int radius)
    {
        var count = 0;

        for (var yy = -radius; yy <= radius; yy++)
            for (var xx = -radius; xx <= radius; xx++)
            {
                int nx = x + xx, ny = y + yy;

                if (IsInside(nx, ny) && (mapData[ny, nx] == '0' || mapData[ny, nx] == '9'))
                    count++;
            }

        return count;
    }

    private bool IsInside(int x, int y)
    {
        return x >= 0 && x < Cols && y >= 0 && y < Rows;
    }

    // Dynamically expands the map when tunnels reach borders
    private void ExpandMapIfNeeded()
    {
        bool left = false, right = false, bottom = false;

        for (var y = 0; y < Rows; y++)
        {
            if (mapData[y, 0] == '0' || mapData[y, 1] == '0') left = true;
            if (mapData[y, Cols - 1] == '0' || mapData[y, Cols - 2] == '0') right = true;
        }

        for (var x = 0; x < Cols; x++)
            if (mapData[Rows - 1, x] == '0' || mapData[Rows - 2, x] == '0')
                bottom = true;

        if (!left && !right && !bottom) return;

        var newRows = Rows + (bottom ? 1 : 0);
        var newCols = Cols + (left ? 1 : 0) + (right ? 1 : 0);

        var newMap = new char[newRows, newCols];
        var newPlannedMap = new char[newRows, newCols];
        var newOdds = new float[newRows, newCols];

        // Fill new area with dirt and occasional rocks
        for (var y = 0; y < newRows; y++)
            for (var x = 0; x < newCols; x++)
            {
                var type = Random.Range(0, 50) == 0 ? '5' : '1';

                newMap[y, x] = type;
                newPlannedMap[y, x] = type;
            }

        for (var y = 0; y < Rows; y++)
            for (var x = 0; x < Cols; x++)
            {
                var nx = x + (left ? 1 : 0);

                newMap[y, nx] = mapData[y, x];
                newPlannedMap[y, nx] = plannedMap[y, x];
                newOdds[y, nx] = oddsGrid[y, x];
            }

        if (left && queenFound)
            queenPos.x += 1;

        mapData = newMap;
        plannedMap = newPlannedMap;
        oddsGrid = newOdds;

        Rows = newRows;
        Cols = newCols;

        if (left)
            FindObjectOfType<AntManager>().ShiftAll(new Vector2Int(1, 0));
    }

    // Draws every tile onto the tilemap
    public void RefreshTilemap()
    {
        groundTilemap.ClearAllTiles();

        for (var y = 0; y < Rows; y++)
            for (var x = 0; x < Cols; x++)
                groundTilemap.SetTile(
                    MapToTilePos(new Vector2Int(x, y)),
                    GetTile(mapData[y, x])
                );
    }

    // Converts tile characters into actual TileBase objects
    private TileBase GetTile(char c)
    {
        switch (c)
        {
            case '0': return Dirt;
            case '1': return DarkDirt;
            case '2': return DarkGrass;
            case '3': return Sky;
            case '4': return Grass;
            case '5': return Rock;
            case '6': return Food;
            case '9': return Queen;

            default: return null;
        }
    }

    public char[,] GetMapData()
    {
        return mapData;
    }

    // Exports map data into a 1D array for saving
    public char[] ExportMap()
    {
        var data = new char[Rows * Cols];

        for (var y = 0; y < Rows; y++)
            for (var x = 0; x < Cols; x++)
                data[y * Cols + x] = mapData[y, x];

        return data;
    }

    public int[] ExportOdds()
    {
        var odds = new int[Rows * Cols];

        for (var y = 0; y < Rows; y++)
            for (var x = 0; x < Cols; x++)
                odds[y * Cols + x] = Mathf.RoundToInt(oddsGrid[y, x]);

        return odds;
    }

    // Loads a saved map
    public void LoadMap(char[] data, int rows, int cols)
    {
        this.Rows = rows;
        this.Cols = cols;

        mapData = new char[rows, cols];
        plannedMap = new char[rows, cols];

        queenFound = false;

        for (var y = 0; y < rows; y++)
            for (var x = 0; x < cols; x++)
            {
                var tile = data[y * cols + x];

                mapData[y, x] = tile;
                plannedMap[y, x] = tile;

                if (tile == '9')
                {
                    queenPos = new Vector2Int(x, y);
                    queenFound = true;
                }
            }

        RefreshTilemap();
    }

    public void LoadOdds(int[] odds, int rows, int cols)
    {
        this.Rows = rows;
        this.Cols = cols;

        oddsGrid = new float[rows, cols];

        for (var y = 0; y < rows; y++)
            for (var x = 0; x < cols; x++)
                oddsGrid[y, x] = odds[y * cols + x];
    }

    // Reloads the original default map
    public void LoadDefault()
    {
        InitializeMap();

        plannedMap = (char[,])mapData.Clone();

        UpdateOddsMap();
        RefreshTilemap();

        Debug.Log("Default map loaded.");
    }

    // Removes a dig job after completion
    public void RemoveJobAt(Vector2Int target)
    {
        for (var i = jobs.Count - 1; i >= 0; i--)
            if (jobs[i].target == target)
            {
                jobs.RemoveAt(i);
                return;
            }
    }

    public class DigJob
    {
        public bool taken;
        public Vector2Int target;

        public DigJob(Vector2Int t)
        {
            target = t;
            taken = false;
        }
    }
}
