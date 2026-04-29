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

    private int currentHeading = 1;
    private int headingStreak = 0;
    public int minJobBuffer = 10;


    void Start()
    {
        InitializeMap();

        // Hidden planner map uses old digging logic
        plannedMap = (char[,])mapData.Clone();

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

        if (!File.Exists(path))
        {
            Debug.LogError("Map file not found: " + path);
            return;
        }

        string[] lines = File.ReadAllLines(path);

        rows = lines.Length;
        cols = lines[0].Length;

        mapData = new char[rows, cols];
        oddsGrid = new float[rows, cols];

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
                float baseOdds = GetBaseOdds(zeroNeighbors);
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

        if (unjobbed >= minJobBuffer)
            return;

        // Run OLD tunnel generation on hidden planner map
        DigWeightedTileOnPlannedMap();

        // Convert planner differences into dig jobs
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (plannedMap[y, x] == '0' &&
                    mapData[y, x] == '1')
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    if (!jobs.Exists(j => j.target == pos))
                    {
                        jobs.Add(new DigJob(pos));

                        // VERY IMPORTANT:
                        // only 1 new job per cycle
                        // exactly like old DigWeightedTile()
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

        int[] localDx = { 0, 0, -1, 1 };
        int[] localDy = { -1, 1, 0, 0 };

        foreach (var front in frontTiles)
        {
            int[] dirWeights = { 10, 50, 20, 20 };
            // up, down, left, right

            int totalDirWeight = 0;
            foreach (int w in dirWeights)
                totalDirWeight += w;

            int rand = Random.Range(0, totalDirWeight);

            int chosenDir = 0;
            int sum = 0;

            for (int i = 0; i < 4; i++)
            {
                sum += dirWeights[i];

                if (rand < sum)
                {
                    chosenDir = i;
                    break;
                }
            }

            int nx = front.x + localDx[chosenDir];
            int ny = front.y + localDy[chosenDir];

            if (IsInside(nx, ny))
            {
                if (plannedMap[ny, nx] == '1' &&
                    CountZeroNeighborsPlanned(nx, ny) == 1)
                {
                    int nearbyTunnels = CountNearbyZerosPlanned(nx, ny, 2);

                    float penaltyFactor = 1f - 0.15f * nearbyTunnels;
                    penaltyFactor = Mathf.Clamp(penaltyFactor, 0.1f, 1f);

                    int zeroNeighbors = CountZeroNeighborsPlanned(nx, ny);
                    int baseOdds = 0;

                    if (zeroNeighbors == 1) baseOdds = 80;
                    else if (zeroNeighbors == 2) baseOdds = 40;
                    else if (zeroNeighbors >= 3) baseOdds = 5;
                    else baseOdds = 2;

                    int adjustedOdds =
                        Mathf.RoundToInt(baseOdds * penaltyFactor);

                    candidates.Add(new Vector2Int(nx, ny));
                    weights.Add(adjustedOdds);
                }
            }

            // small branch chance EXACTLY like old code
            if (Random.value < 0.1f)
            {
                int branchDir =
                    (chosenDir + Random.Range(1, 4)) % 4;

                int bx = front.x + localDx[branchDir];
                int by = front.y + localDy[branchDir];

                if (IsInside(bx, by))
                {
                    if (plannedMap[by, bx] == '1' &&
                        CountZeroNeighborsPlanned(bx, by) == 1)
                    {
                        int nearbyTunnels =
                            CountNearbyZerosPlanned(bx, by, 2);

                        float penaltyFactor =
                            1f - 0.15f * nearbyTunnels;

                        penaltyFactor =
                            Mathf.Clamp(penaltyFactor, 0.1f, 1f);

                        int zeroNeighbors =
                            CountZeroNeighborsPlanned(bx, by);

                        int baseOdds = 0;

                        if (zeroNeighbors == 1) baseOdds = 80;
                        else if (zeroNeighbors == 2) baseOdds = 40;
                        else if (zeroNeighbors >= 3) baseOdds = 5;
                        else baseOdds = 2;

                        int adjustedOdds =
                            Mathf.RoundToInt(baseOdds * penaltyFactor);

                        candidates.Add(new Vector2Int(bx, by));
                        weights.Add(adjustedOdds);
                    }
                }
            }
        }

        if (candidates.Count == 0)
            return;

        int totalWeight = 0;
        foreach (int w in weights)
            totalWeight += w;

        int randomValue = Random.Range(0, totalWeight);
        int sumWeight = 0;

        for (int i = 0; i < candidates.Count; i++)
        {
            sumWeight += weights[i];

            if (randomValue < sumWeight)
            {
                Vector2Int chosen = candidates[i];

                // ONLY planner digs here
                plannedMap[chosen.y, chosen.x] = '0';

                break;
            }
        }
    }

    List<Vector2Int> GetTunnelFrontFromPlanned()
    {
        List<Vector2Int> front = new List<Vector2Int>();

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (plannedMap[y, x] != '0' &&
                    plannedMap[y, x] != '9')
                    continue;

                int adjacentDirt = 0;

                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];

                    if (!IsInside(nx, ny))
                        continue;

                    if (plannedMap[ny, nx] == '1')
                        adjacentDirt++;
                }

                if (adjacentDirt > 0)
                    front.Add(new Vector2Int(x, y));
            }
        }

        return front;
    }

    int CountZeroNeighborsPlanned(int x, int y)
    {
        int count = 0;

        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];

            if (IsInside(nx, ny) &&
                (plannedMap[ny, nx] == '0' ||
                 plannedMap[ny, nx] == '9'))
            {
                count++;
            }
        }

        return count;
    }

    int CountNearbyZerosPlanned(int x, int y, int radius)
    {
        int count = 0;

        for (int yy = -radius; yy <= radius; yy++)
        {
            for (int xx = -radius; xx <= radius; xx++)
            {
                int nx = x + xx;
                int ny = y + yy;

                if (IsInside(nx, ny) &&
                    (plannedMap[ny, nx] == '0' ||
                     plannedMap[ny, nx] == '9'))
                {
                    count++;
                }
            }
        }

        return count;
    }

    List<Vector2Int> GetTunnelFront()
    {
        List<Vector2Int> front = new List<Vector2Int>();

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (mapData[y, x] != '0' && mapData[y, x] != '9')
                    continue;

                int adjacentDirt = 0;

                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];

                    if (!IsInside(nx, ny)) continue;

                    if (mapData[ny, nx] == '1')
                        adjacentDirt++;
                }

                if (adjacentDirt > 0)
                    front.Add(new Vector2Int(x, y));
            }
        }

        return front;
    }

    Vector2Int GetTunnelTip()
    {
        // Prefer the deepest untaken job as the planning tip
        DigJob deepest = null;
        foreach (var j in jobs)
        {
            if (deepest == null || j.target.y > deepest.target.y)
                deepest = j;
        }
        if (deepest != null) return deepest.target;

        // Fall back to deepest open tile
        Vector2Int tip = queenPos;
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                if ((mapData[y, x] == '0' || mapData[y, x] == '9') && y >= tip.y)
                    tip = new Vector2Int(x, y);

        return tip;
    }

    int GetWeightedDirectionFrom(Vector2Int from)
    {
        // Build candidates weighted by odds, same as old code
        var candidates = new List<int>();
        var weights = new List<int>();

        int[] dirWeights = { 2, 70, 14, 14 }; // up, down, left, right

        for (int dir = 0; dir < 4; dir++)
        {
            int nx = from.x + dx[dir];
            int ny = from.y + dy[dir];

            if (!IsInside(nx, ny)) continue;
            if (mapData[ny, nx] != '1') continue;
            if (CountZeroNeighborsIncludingPending(nx, ny) != 1) continue;

            int nearby = CountNearbyZeros(nx, ny, 2);
            float penalty = Mathf.Clamp(1f - 0.15f * nearby, 0.1f, 1f);
            int weight = Mathf.RoundToInt(dirWeights[dir] * penalty);

            candidates.Add(dir);
            weights.Add(weight);
        }

        if (candidates.Count == 0) return currentHeading; // keep going same way if stuck

        return candidates[GetWeightedIndex(weights)];
    }

    int CountZeroNeighborsIncludingPending(int x, int y)
    {
        int count = 0;
        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];
            if (!IsInside(nx, ny)) continue;

            bool isOpen = mapData[ny, nx] == '0' || mapData[ny, nx] == '9';
            bool isPending = jobs.Exists(j => j.target == new Vector2Int(nx, ny));

            if (isOpen || isPending) count++;
        }
        return count;
    }
    float ApplyTunnelPenalty(float baseOdds, int nearbyTunnels)
    {
        float factor = 1f - 0.15f * nearbyTunnels;
        factor = Mathf.Clamp(factor, 0.1f, 1f);
        return baseOdds * factor;
    }

    void WriteOddsToFile()
    {
        if (debugMode)
        {
            StringBuilder sb = new StringBuilder();

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    float digit = Mathf.Clamp(oddsGrid[y, x] / 10, 0, 9);
                    sb.Append(digit);
                }
                sb.AppendLine();
            }

            File.WriteAllText(Application.dataPath + expOddsFile, sb.ToString());
        }
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
                float baseOdds = GetBaseOdds(zeroNeighbors);
                int nearby = CountNearbyZeros(x, y, 2);

                oddsGrid[y, x] = ApplyTunnelPenalty(baseOdds, nearby);
            }
        }
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

    int CountZeroNeighbors(int x, int y)
    {
        int count = 0;

        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];

            if (IsInside(nx, ny) && (mapData[ny, nx] == '0' || mapData[ny, nx] == '9'))
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

                if (IsInside(nx, ny) && (mapData[ny, nx] == '0' || mapData[ny, nx] == '9'))
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
            if (mapData[y, 0] == '0' || mapData[y, 1] == '0')
                left = true;

            if (mapData[y, cols - 1] == '0' || mapData[y, cols - 2] == '0')
                right = true;
        }

        for (int x = 0; x < cols; x++)
        {
            if (mapData[rows - 1, x] == '0' ||
                mapData[rows - 2, x] == '0')
            {
                bottom = true;
            }
        }

        if (!left && !right && !bottom)
            return;

        int newRows = rows + (bottom ? 1 : 0);
        int newCols = cols + (left ? 1 : 0) + (right ? 1 : 0);

        char[,] newMap = new char[newRows, newCols];
        char[,] newPlannedMap = new char[newRows, newCols];
        float[,] newOdds = new float[newRows, newCols];

        // Fill defaults
        for (int y = 0; y < newRows; y++)
        {
            for (int x = 0; x < newCols; x++)
            {
                newMap[y, x] = '1';
                newPlannedMap[y, x] = '1';
            }
        }

        // Copy old data
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int nx = x + (left ? 1 : 0);

                newMap[y, nx] = mapData[y, x];
                newPlannedMap[y, nx] = plannedMap[y, x];
                newOdds[y, nx] = oddsGrid[y, x];
            }
        }

        if (queenFound)
        {
            if (left)
                queenPos.x += 1;

            FindObjectOfType<AntManager>().queenPos = queenPos;
        }

        mapData = newMap;
        plannedMap = newPlannedMap;
        oddsGrid = newOdds;

        rows = newRows;
        cols = newCols;

        Vector2Int shift = new Vector2Int(left ? 1 : 0, 0);

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
                Vector2Int gridPos = new Vector2Int(x, y);
                Vector3Int tilePos = MapToTilePos(gridPos);

                groundTilemap.SetTile(tilePos, GetTile(mapData[y, x]));

                if (debugMode)
                {
                    UpdateTileText(gridPos, tilePos);
                }
            }
        }
    }

    public void RemoveJobAt(Vector2Int target)
    {
        jobs.RemoveAll(j => j.target == target);
    }

    void UpdateTileText(Vector2Int gridPos, Vector3Int tilePos)
    {
        float odds = oddsGrid[gridPos.y, gridPos.x];

        // Optional: hide zeros
        if (odds == 0)
        {
            if (textMap.ContainsKey(gridPos))
                textMap[gridPos].text = "";
            return;
        }

        TMP_Text text;

        if (!textMap.TryGetValue(gridPos, out text))
        {
            GameObject obj = Instantiate(textPrefab, groundTilemap.transform);
            text = obj.GetComponent<TMP_Text>();

            Renderer r = text.GetComponent<Renderer>();
            r.sortingLayerName = "TileText";
            r.sortingOrder = 20;

            textMap[gridPos] = text;
        }

        Vector3 worldPos = groundTilemap.GetCellCenterWorld(tilePos);
        text.transform.position = worldPos;

        text.text = odds.ToString();
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