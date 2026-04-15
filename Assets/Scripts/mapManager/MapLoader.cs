using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapLoader : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Camera mainCamera;

    public TileBase DarkDirt; // 1
    public TileBase Dirt;     // 0
    public TileBase Sky;      // 3
    public TileBase DarkGrass;// 2
    public TileBase Grass;    // 4
    public TileBase Rock;     // 5
    public TileBase Food;     // 6
    public TileBase Queen;    // 9

    public string mapFile = "/Tiles/Maps.txt";
    public string expOddsFile = "/Tiles/ExpansionOdds.txt";

    public float updateInterval = 0.1f;
    public bool debugMode = false;

    private char[,] mapData;
    private int[,] oddsGrid;
    private int rows;
    private int cols;

    private float timer;

    private static readonly int[] dx = { 0, 0, -1, 1 };
    private static readonly int[] dy = { -1, 1, 0, 0 };

    public Vector2Int queenPos;
    public bool queenFound;
    public List<DigJob> jobs = new List<DigJob>();

    void Start()
    {
        InitializeMap();
        UpdateOddsMap();
        RefreshTilemap();
        AdjustCamera();
        Debug.Log("Queen found at: " + queenPos);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= updateInterval)
        {
            if (DigWeightedTile())
            {
                UpdateOddsMap();
                ExpandMapIfNeeded();
                RefreshTilemap();
                //FindFirstObjectByType<AntManager>().DrawAnts();
            }

            timer = 0f;
        }
    }
    void InitializeMap()
    {
        string path = Application.dataPath + mapFile;

        if (!File.Exists(path))
        {
            Debug.LogError("Map file not found: " + path);
            return;
        }

        string[] lines = File.ReadAllLines(path);

        rows = lines.Length;
        cols = lines[0].Length;

        mapData = new char[rows, cols];
        oddsGrid = new int[rows, cols];

        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                mapData[y, x] = lines[y][x];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
                if (mapData[y, x] == '9')
                {
                    queenPos = new Vector2Int(x, y);
                    queenFound = true;
                    Debug.Log(queenFound);
                }
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
                int baseOdds = GetBaseOdds(zeroNeighbors);
                int nearby = CountNearbyZeros(x, y, 3);

                oddsGrid[y, x] = ApplyTunnelPenalty(baseOdds, nearby);
            }
        }

        if (debugMode)
            WriteOddsToFile();
    }
    public Vector3Int MapToTilePos(Vector2Int p)
    {
        return new Vector3Int(p.x - cols / 2, -p.y + rows / 2, 0);
    }
    int GetBaseOdds(int zeroNeighbors)
    {
        if (zeroNeighbors == 1) return 80;
        if (zeroNeighbors == 2) return 40;
        if (zeroNeighbors >= 3) return 5;
        return 2;
    }

    int ApplyTunnelPenalty(int baseOdds, int nearbyTunnels)
    {
        float factor = 1f - 0.15f * nearbyTunnels;
        factor = Mathf.Clamp(factor, 0.1f, 1f);
        return Mathf.RoundToInt(baseOdds * factor);
    }

    void WriteOddsToFile()
    {
        StringBuilder sb = new StringBuilder();

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int digit = Mathf.Clamp(oddsGrid[y, x] / 10, 0, 9);
                sb.Append(digit);
            }
            sb.AppendLine();
        }

        File.WriteAllText(Application.dataPath + expOddsFile, sb.ToString());
    }
    bool DigWeightedTile()
    {
        var frontTiles = GetTunnelFront();
        var candidates = new List<Vector2Int>();
        var weights = new List<int>();

        foreach (var front in frontTiles)
        {
            int dir = GetWeightedDirection();
            TryAddCandidate(front, dir, candidates, weights);

            if (Random.value < 0.1f)
            {
                int branchDir = (dir + Random.Range(1, 4)) % 4;
                TryAddCandidate(front, branchDir, candidates, weights);
            }
        }

        if (candidates.Count == 0) return false;

        int chosenIndex = GetWeightedIndex(weights);
        Vector2Int chosen = candidates[chosenIndex];

        jobs.Add(new DigJob(chosen));
        return true;
    }
    public void UpdateOddsAround(int cx, int cy, int radius = 3)
    {
        for (int y = cy - radius; y <= cy + radius; y++)
        {
            for (int x = cx - radius; x <= cx + radius; x++)
            {
                if (!IsInside(x, y)) continue;

                if (mapData[y, x] != '1')
                {
                    oddsGrid[y, x] = 0;
                    continue;
                }

                int zeroNeighbors = CountZeroNeighbors(x, y);
                int baseOdds = GetBaseOdds(zeroNeighbors);
                int nearby = CountNearbyZeros(x, y, 2);

                oddsGrid[y, x] = ApplyTunnelPenalty(baseOdds, nearby);
            }
        }
    }
    void TryAddCandidate(Vector2Int origin, int dir, List<Vector2Int> candidates, List<int> weights)
    {
        int nx = origin.x + dx[dir];
        int ny = origin.y + dy[dir];

        if (!IsInside(nx, ny)) return;
        if (mapData[ny, nx] != '1') return;
        if (CountZeroNeighbors(nx, ny) != 1) return;

        int nearby = CountNearbyZeros(nx, ny, 2);
        int adjusted = ApplyTunnelPenalty(oddsGrid[ny, nx], nearby);

        candidates.Add(new Vector2Int(nx, ny));
        weights.Add(adjusted);
    }

    int GetWeightedDirection()
    {
        int[] weights = { 10, 50, 20, 20 };
        return GetWeightedIndex(weights);
    }

    int GetWeightedIndex(IList<int> weights)
    {
        int total = 0;
        foreach (int w in weights) total += w;

        int rand = Random.Range(0, total);
        int sum = 0;

        for (int i = 0; i < weights.Count; i++)
        {
            sum += weights[i];
            if (rand < sum)
                return i;
        }

        return 0;
    }
    List<Vector2Int> GetTunnelFront()
    {
        var front = new List<Vector2Int>();

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (mapData[y, x] != '0') continue;

                int adjacentDirt = 0;

                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];

                    if (IsInside(nx, ny) && mapData[ny, nx] == '1')
                        adjacentDirt++;
                }

                if (adjacentDirt > 0)
                    front.Add(new Vector2Int(x, y));
            }
        }

        return front;
    }

    int CountZeroNeighbors(int x, int y)
    {
        int count = 0;

        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];

            if (IsInside(nx, ny) && mapData[ny, nx] == '0')
                count++;
        }

        return count;
    }

    int CountNearbyZeros(int x, int y, int radius)
    {
        int count = 0;

        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                int nx = x + dx;
                int ny = y + dy;

                if (IsInside(nx, ny) && mapData[ny, nx] == '0')
                    count++;
            }
        }

        return count;
    }

    bool IsInside(int x, int y)
    {
        return x >= 0 && x < cols && y >= 0 && y < rows;
    }
    void ExpandMapIfNeeded()
    {
        bool left = false, right = false, bottom = false;

        for (int y = 0; y < rows; y++)
        {
            if (mapData[y, 0] == '0') left = true;
            if (mapData[y, cols - 1] == '0') right = true;
        }

        for (int x = 0; x < cols; x++)
            if (mapData[rows - 1, x] == '0') bottom = true;

        if (!left && !right && !bottom) return;

        int newRows = rows + (bottom ? 1 : 0);
        int newCols = cols + (left ? 1 : 0) + (right ? 1 : 0);

        var newMap = new char[newRows, newCols];
        var newOdds = new int[newRows, newCols];

        for (int y = 0; y < newRows; y++)
            for (int x = 0; x < newCols; x++)
                newMap[y, x] = '1';

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int nx = x + (left ? 1 : 0);
                newMap[y, nx] = mapData[y, x];
                newOdds[y, nx] = oddsGrid[y, x];
            }
        }

        if (queenFound)
        {
            if (left) queenPos.x += 1;
            Debug.Log("Shifted" + queenPos);
        }

        mapData = newMap;
        oddsGrid = newOdds;
        rows = newRows;
        cols = newCols;

        Vector2Int shift = new Vector2Int(
    left ? 1 : 0,
    0
);

        if (shift != Vector2Int.zero)
        {
            FindObjectOfType<AntManager>().ShiftAll(shift);
        }

        AdjustCamera();
    }

    public void RefreshTilemap()
    {
        groundTilemap.ClearAllTiles();

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector3Int pos = MapToTilePos(new Vector2Int(x, y));
                groundTilemap.SetTile(pos, GetTile(mapData[y, x]));
            }
        }
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

        public DigJob(Vector2Int t)
        {
            target = t;
            taken = false;
        }
    }
    void AdjustCamera()
    {
        groundTilemap.CompressBounds();
        Bounds bounds = groundTilemap.localBounds;

        if (mainCamera == null) return;

        float padding = 1f;
        float sizeY = bounds.size.y / 2f + padding;
        float sizeX = (bounds.size.x / 2f + padding) / mainCamera.aspect;

        mainCamera.orthographicSize = Mathf.Max(sizeY, sizeX);
        mainCamera.transform.position = bounds.center + new Vector3(0, 0, -10);
    }

    public char[,] GetMapData()
    {
        return mapData;
    }

    public int Rows
    {
        get { return rows; }
    }

    public int Cols
    {
        get { return cols; }
    }
}