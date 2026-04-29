using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapLoader : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Camera mainCamera;
    public GameObject textPrefab;
    private Dictionary<Vector2Int, TMP_Text> textMap = new Dictionary<Vector2Int, TMP_Text>();

    public TileBase DarkDirt;
    public TileBase Dirt;
    public TileBase Sky;
    public TileBase DarkGrass;
    public TileBase Grass;
    public TileBase Rock;
    public TileBase Food;
    public TileBase Queen;

    public string mapFile = "/Tiles/Maps.txt";
    public string expOddsFile = "/Tiles/ExpansionOdds.txt";

    public float updateInterval = 0.1f;
    public bool debugMode = false;

    private char[,] plannedMap;
    private char[,] mapData;
    private float[,] oddsGrid;
    private int rows;
    private int cols;

    private float timer;

    private static readonly int[] dx = { 0, 0, -1, 1 };
    private static readonly int[] dy = { -1, 1, 0, 0 };

    public Vector2Int queenPos;
    public bool queenFound;
    public List<DigJob> jobs = new List<DigJob>();

    public int minJobBuffer = 10;

    void Start()
    {
        InitializeMap();
        plannedMap = (char[,])mapData.Clone();
        UpdateOddsMap();
        RefreshTilemap();
        Debug.Log("Queen found at: " + queenPos);
    }

    void Update()
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

    void InitializeMap()
    {
        string path = Application.dataPath + mapFile;
        if (!File.Exists(path)) return;

        string[] lines = File.ReadAllLines(path);
        rows = lines.Length;
        cols = lines[0].Length;

        mapData = new char[rows, cols];
        oddsGrid = new float[rows, cols];

        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                mapData[y, x] = lines[y][x];

        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                if (mapData[y, x] == '9')
                {
                    queenPos = new Vector2Int(x, y);
                    queenFound = true;
                }
    }

    void UpdateOddsMap()
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (mapData[y, x] != '1')
                {
                    oddsGrid[y, x] = 0;
                    continue;
                }
                int zeroNeighbors = CountZeroNeighbors(x, y);
                float baseOdds = GetBaseOdds(zeroNeighbors);
                int nearby = CountNearbyZeros(x, y, 3);
                oddsGrid[y, x] = ApplyTunnelPenalty(baseOdds, nearby);
            }
        }
    }

    public Vector3Int MapToTilePos(Vector2Int p)
    {
        return new Vector3Int(p.x - cols / 2, -p.y + rows / 2, 0);
    }

    float GetBaseOdds(int zeroNeighbors)
    {
        if (zeroNeighbors == 1) return 80;
        if (zeroNeighbors == 2) return 40;
        if (zeroNeighbors >= 3) return 5;
        return 2;
    }

    public void RefillJobQueue()
    {
        int unjobbed = jobs.Count(j => !j.taken);
        if (unjobbed >= minJobBuffer) return;

        DigWeightedTileOnPlannedMap();

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (plannedMap[y, x] == '0' && mapData[y, x] == '1')
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (!jobs.Exists(j => j.target == pos))
                    {
                        jobs.Add(new DigJob(pos));
                        return;
                    }
                }
            }
        }
    }

    void DigWeightedTileOnPlannedMap()
    {
        List<Vector2Int> frontTiles = GetTunnelFrontFromPlanned();
        List<Vector2Int> candidates = new List<Vector2Int>();
        List<int> weights = new List<int>();

        foreach (var front in frontTiles)
        {
            int[] dirWeights = { 10, 50, 20, 20 };
            int totalDirWeight = 100;
            int rand = Random.Range(0, totalDirWeight);
            int chosenDir = 0;
            int sum = 0;

            for (int i = 0; i < 4; i++)
            {
                sum += dirWeights[i];
                if (rand < sum) { chosenDir = i; break; }
            }

            int nx = front.x + dx[chosenDir];
            int ny = front.y + dy[chosenDir];

            if (IsInside(nx, ny) && plannedMap[ny, nx] == '1' && CountZeroNeighborsPlanned(nx, ny) == 1)
            {
                int nearby = CountNearbyZerosPlanned(nx, ny, 2);
                float penalty = Mathf.Clamp(1f - 0.15f * nearby, 0.1f, 1f);
                candidates.Add(new Vector2Int(nx, ny));
                weights.Add(Mathf.RoundToInt(GetBaseOdds(1) * penalty));
            }
        }

        if (candidates.Count == 0) return;

        int totalWeight = weights.Sum();
        int randomValue = Random.Range(0, totalWeight);
        int sumWeight = 0;

        for (int i = 0; i < candidates.Count; i++)
        {
            sumWeight += weights[i];
            if (randomValue < sumWeight)
            {
                plannedMap[candidates[i].y, candidates[i].x] = '0';
                break;
            }
        }
    }

    List<Vector2Int> GetTunnelFrontFromPlanned()
    {
        List<Vector2Int> front = new List<Vector2Int>();
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
            {
                if (plannedMap[y, x] != '0' && plannedMap[y, x] != '9') continue;
                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dx[i], ny = y + dy[i];
                    if (IsInside(nx, ny) && plannedMap[ny, nx] == '1') { front.Add(new Vector2Int(x, y)); break; }
                }
            }
        return front;
    }

    int CountZeroNeighborsPlanned(int x, int y)
    {
        int count = 0;
        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i], ny = y + dy[i];
            if (IsInside(nx, ny) && (plannedMap[ny, nx] == '0' || plannedMap[ny, nx] == '9')) count++;
        }
        return count;
    }

    int CountNearbyZerosPlanned(int x, int y, int radius)
    {
        int count = 0;
        for (int yy = -radius; yy <= radius; yy++)
            for (int xx = -radius; xx <= radius; xx++)
            {
                int nx = x + xx, ny = y + yy;
                if (IsInside(nx, ny) && (plannedMap[ny, nx] == '0' || plannedMap[ny, nx] == '9')) count++;
            }
        return count;
    }

    float ApplyTunnelPenalty(float baseOdds, int nearbyTunnels)
    {
        return baseOdds * Mathf.Clamp(1f - 0.15f * nearbyTunnels, 0.1f, 1f);
    }

    public void UpdateOddsAround(int cx, int cy, int radius = 3)
    {
        for (int y = cy - radius; y <= cy + radius; y++)
            for (int x = cx - radius; x <= cx + radius; x++)
            {
                if (!IsInside(x, y) || mapData[y, x] != '1') continue;
                oddsGrid[y, x] = ApplyTunnelPenalty(GetBaseOdds(CountZeroNeighbors(x, y)), CountNearbyZeros(x, y, 2));
            }
    }

    int CountZeroNeighbors(int x, int y)
    {
        int count = 0;
        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i], ny = y + dy[i];
            if (IsInside(nx, ny) && (mapData[ny, nx] == '0' || mapData[ny, nx] == '9')) count++;
        }
        return count;
    }

    int CountNearbyZeros(int x, int y, int radius)
    {
        int count = 0;
        for (int yy = -radius; yy <= radius; yy++)
            for (int xx = -radius; xx <= radius; xx++)
            {
                int nx = x + xx, ny = y + yy;
                if (IsInside(nx, ny) && (mapData[ny, nx] == '0' || mapData[ny, nx] == '9')) count++;
            }
        return count;
    }

    bool IsInside(int x, int y) => x >= 0 && x < cols && y >= 0 && y < rows;

    void ExpandMapIfNeeded()
    {
        bool left = false, right = false, bottom = false;
        for (int y = 0; y < rows; y++)
        {
            if (mapData[y, 0] == '0' || mapData[y, 1] == '0') left = true;
            if (mapData[y, cols - 1] == '0' || mapData[y, cols - 2] == '0') right = true;
        }
        for (int x = 0; x < cols; x++)
            if (mapData[rows - 1, x] == '0' || mapData[rows - 2, x] == '0') bottom = true;

        if (!left && !right && !bottom) return;

        int newRows = rows + (bottom ? 1 : 0);
        int newCols = cols + (left ? 1 : 0) + (right ? 1 : 0);

        char[,] newMap = new char[newRows, newCols];
        char[,] newPlannedMap = new char[newRows, newCols];
        float[,] newOdds = new float[newRows, newCols];

        for (int y = 0; y < newRows; y++)
            for (int x = 0; x < newCols; x++)
            {
                char type = (Random.Range(0, 20) == 0) ? '5' : '1';
                newMap[y, x] = type;
                newPlannedMap[y, x] = type;
            }

        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
            {
                int nx = x + (left ? 1 : 0);
                newMap[y, nx] = mapData[y, x];
                newPlannedMap[y, nx] = plannedMap[y, x];
                newOdds[y, nx] = oddsGrid[y, x];
            }

        if (left && queenFound) queenPos.x += 1;

        mapData = newMap;
        plannedMap = newPlannedMap;
        oddsGrid = newOdds;
        rows = newRows;
        cols = newCols;

        if (left) FindObjectOfType<AntManager>().ShiftAll(new Vector2Int(1, 0));
    }

    public void RefreshTilemap()
    {
        groundTilemap.ClearAllTiles();
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                groundTilemap.SetTile(MapToTilePos(new Vector2Int(x, y)), GetTile(mapData[y, x]));
    }

    TileBase GetTile(char c)
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

    public class DigJob
    {
        public Vector2Int target;
        public bool taken;
        public DigJob(Vector2Int t) { target = t; taken = false; }
    }

    public char[,] GetMapData() => mapData;
    public int Rows => rows;
    public int Cols => cols;

    public char[] ExportMap()
    {
        char[] data = new char[rows * cols];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                data[y * cols + x] = mapData[y, x];
            }
        }

        return data;
    }

    public int[] ExportOdds()
    {
        int[] odds = new int[rows * cols];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                odds[y * cols + x] = Mathf.RoundToInt(oddsGrid[y, x]);
            }
        }

        return odds;
    }

    public void LoadMap(char[] data, int rows, int cols)
    {
        this.rows = rows;
        this.cols = cols;

        mapData = new char[rows, cols];
        plannedMap = new char[rows, cols];

        queenFound = false;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                char tile = data[y * cols + x];

                mapData[y, x] = tile;
                plannedMap[y, x] = tile;

                if (tile == '9')
                {
                    queenPos = new Vector2Int(x, y);
                    queenFound = true;
                }
            }
        }

        RefreshTilemap();
    }

    public void LoadOdds(int[] odds, int rows, int cols)
    {
        this.rows = rows;
        this.cols = cols;

        oddsGrid = new float[rows, cols];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                oddsGrid[y, x] = odds[y * cols + x];
            }
        }
    }

    public void LoadDefault()
    {
        InitializeMap();

        plannedMap = (char[,])mapData.Clone();

        UpdateOddsMap();
        RefreshTilemap();

        Debug.Log("Default map loaded.");
    }

    public void RemoveJobAt(Vector2Int target)
    {
        for (int i = jobs.Count - 1; i >= 0; i--)
        {
            if (jobs[i].target == target)
            {
                jobs.RemoveAt(i);
                return;
            }
        }
    }
}