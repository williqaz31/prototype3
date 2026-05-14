using System.Collections.Generic;
using System.Linq;
using SimulationFourmiliere;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AntManager : MonoBehaviour
{
    // Different jobs an ant can have
    public enum AntRole
    {
        Miner,
        Forager
    }

    // Current activity/state of an ant
    public enum AntState
    {
        Idle,
        GoingToDig,
        Digging,
        CarryingToExit,
        Distributing
    }

    public MapLoader mapLoader;
    public Tilemap AntColonie;

    public TileBase AntTile;
    public TileBase AntWithFoodTile;
    public TileBase AntDistributingTile;

    public List<int> antIds = new();

    public float moveInterval = 0.2f; //<------

    [SerializeField] public PopCounter popCounter;

    public GameObject antCountLabelPrefab;

    // Stores labels showing ant counts on crowded tiles
    private readonly Dictionary<Vector3Int, GameObject> antCountLabels = new();

    private readonly Dictionary<int, float> antMoveTimers = new();

    // Path each ant is currently following
    public Dictionary<int, Queue<Vector2Int>> antPaths = new();

    // Position of every ant
    public Dictionary<int, Vector2Int> ants = new();

    public Dictionary<int, AntState> antStates = new();
    public Dictionary<int, Vector2Int> antTargets = new();

    private readonly Dictionary<int, float> digDurations = new();
    private readonly Dictionary<int, float> digTimers = new();

    public Dictionary<int, int> foodCarried = new();

    // Hidden ants are not rendered
    public HashSet<int> hiddenAnts = new();

    public Dictionary<int, int> hunger = new();

    private int nextAntId;

    public Dictionary<int, AntRole> roles = new();

    public SimulationState simulation;

    private void Start()
    {
        if (popCounter == null)
        {
            Debug.LogError("popCounter not assigned!");
            return;
        }

        simulation = popCounter.GetState();

        if (simulation == null)
        {
            Debug.LogError("SimulationState is NULL from popCounter");
            return;
        }

        if (!mapLoader.queenFound)
        {
            Debug.LogError("Queen not loaded yet!");
            return;
        }

        AntColonie.GetComponent<TilemapRenderer>().mode = TilemapRenderer.Mode.Individual;

        mapLoader.queenPos = mapLoader.queenPos;

        DrawAnts();
    }

    private void Update()
    {
        SyncAntCount();
        AssignJobs();
        UpdateAnts();

        var freeAnts = antIds.Count(id => !antTargets.ContainsKey(id));
        var openJobs = mapLoader.jobs.Count(j => !j.taken);
        var totalJobs = mapLoader.jobs.Count;

        if (simulation == null)
            return;

        if (Time.frameCount % 60 == 0)
        {
            //LogForagerCount();
        }
    }

    public bool IsHungry(int id)
    {
        if (!hunger.ContainsKey(id))
            return false;

        return hunger[id] > 0;
    }

    // Reduces hunger level when an ant eats
    public void FeedAnt(int id)
    {
        hunger[id] = Mathf.Max(0, hunger[id] - 1);
    }

    // Removes the last ant from the colony
    private void RemoveAnt()
    {
        var index = ants.Count - 1;
        var id = antIds[index];

        antIds.RemoveAt(index);

        ants.Remove(id);
        antStates.Remove(id);
        antMoveTimers.Remove(id);
        antPaths.Remove(id);
        antTargets.Remove(id);
    }

    public void LogForagerCount()
    {
        var foragers = 0;

        foreach (var id in antIds)
            if (roles.TryGetValue(id, out var role) &&
                role == AntRole.Forager)
                continue;

        Debug.Log($"Foragers: {foragers} / Total ants: {antIds.Count}");
    }

    // Keeps the ant count synced with the simulation population
    private void SyncAntCount()
    {
        if (simulation == null) return;

        var target = simulation.Colonie.Pop();

        while (ants.Count < target)
            SpawnSingleAnt();

        while (ants.Count > target)
            RemoveAnt();

        //Debug.Log("TARGET POP = " + simulation.Colonie.Pop());
        //Debug.Log("CURRENT ANTS = " + ants.Count);
    }

    // Creates a new ant at the queen position
    private void SpawnSingleAnt()
    {
        var id = nextAntId++;

        antIds.Add(id);

        ants[id] = mapLoader.queenPos;

        antStates[id] = AntState.Idle;
        antMoveTimers[id] = 0f;

        digTimers[id] = 0f;
        digDurations[id] = 0f;

        hunger[id] = Random.Range(1, 4);

        roles[id] = AntRole.Miner;

        //Debug.Log("ant added at " + mapLoader.queenPos);
    }

    // Finds the nearest walkable tile beside the colony exit
    public Vector2Int? GetNearestExitAdjacent(Vector2Int from)
    {
        var mapData = mapLoader.GetMapData();

        var bestDist = int.MaxValue;
        Vector2Int? best = null;

        Vector2Int[] dirs =
        {
            new(1, 0),
            new(-1, 0),
            new(0, 1),
            new(0, -1)
        };

        for (var y = 0; y < mapLoader.Rows; y++)
            for (var x = 0; x < mapLoader.Cols; x++)
            {
                if (mapData[y, x] != '4') continue;

                var grass = new Vector2Int(x, y);

                foreach (var d in dirs)
                {
                    var neighbor = grass + d;

                    if (!IsWalkable(neighbor)) continue;

                    var dist = Mathf.Abs(from.x - neighbor.x) + Mathf.Abs(from.y - neighbor.y);

                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        best = neighbor;
                    }
                }
            }

        return best;
    }

    public void SetCarrying(int id, bool carrying, int amount = 1)
    {
        if (carrying)
            foodCarried[id] = amount;
        else
            foodCarried.Remove(id);
    }

    // Draws all visible ants onto the tilemap
    public void DrawAnts()
    {
        AntColonie.ClearAllTiles();

        ClearAntCountLabels();

        var cellCounts = new Dictionary<Vector3Int, List<int>>();

        foreach (var id in antIds)
        {
            if (!ants.TryGetValue(id, out var gridPos)) continue;
            if (hiddenAnts.Contains(id)) continue;

            var pos = mapLoader.MapToTilePos(gridPos);

            if (!cellCounts.ContainsKey(pos))
                cellCounts[pos] = new List<int>();

            cellCounts[pos].Add(id);
        }

        foreach (var kvp in cellCounts)
        {
            var pos = kvp.Key;
            var ids = kvp.Value;

            var tileToUse = AntTile;

            // Select tile appearance depending on ant state
            foreach (var id in ids)
            {
                roles.TryGetValue(id, out var role);
                foodCarried.TryGetValue(id, out var f);
                antStates.TryGetValue(id, out var state);

                if (role == AntRole.Forager && f > 0)
                {
                    tileToUse = AntWithFoodTile;
                    break;
                }

                if (role == AntRole.Forager && state == AntState.Distributing)
                    tileToUse = AntDistributingTile;
            }

            AntColonie.SetTile(pos, tileToUse);

            AntColonie.SetTileFlags(pos, TileFlags.None);
            AntColonie.SetColor(pos, Color.white);

            // Changes tile color based on crowd density
            if (ids.Count >= 8)
                AntColonie.SetColor(pos, new Color(1f, 0.2f, 0.2f)); // red
            else if (ids.Count >= 5)
                AntColonie.SetColor(pos, new Color(1f, 0.6f, 0.2f)); // orange
            else if (ids.Count >= 2)
                AntColonie.SetColor(pos, new Color(1f, 1f, 0.3f)); // yellow
            else
                AntColonie.SetColor(pos, Color.white); // normal

            if (ids.Count > 1)
                ShowAntCountLabel(pos, ids.Count);
        }
    }

    private void ClearAntCountLabels()
    {
        foreach (var go in antCountLabels.Values)
            if (go != null)
                go.SetActive(false);
    }

    // Displays the number of ants stacked on one tile
    private void ShowAntCountLabel(Vector3Int tilePos, int count)
    {
        if (antCountLabelPrefab == null) return;

        if (!antCountLabels.TryGetValue(tilePos, out var go) || go == null)
        {
            go = Instantiate(antCountLabelPrefab, AntColonie.transform);
            antCountLabels[tilePos] = go;
        }

        go.SetActive(true);

        go.transform.position =
            AntColonie.GetCellCenterWorld(tilePos) + new Vector3(0.2f, 0.2f, 0f);

        var tmp = go.GetComponent<TMP_Text>();

        if (tmp != null)
        {
            tmp.text = count.ToString();
            tmp.fontSize = 3f;
            tmp.color = Color.white;
        }
    }

    public List<Vector2Int> FindPathPublic(Vector2Int start, Vector2Int goal)
    {
        return FindPath(start, goal);
    }

    // Breadth-first pathfinding
    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        if (start == goal) return new List<Vector2Int>();

        var queue = new Queue<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        Vector2Int[] dirs =
        {
            new(1, 0),
            new(-1, 0),
            new(0, 1),
            new(0, -1)
        };

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == goal)
                break;

            foreach (var d in dirs)
            {
                var next = current + d;

                if (visited.Contains(next)) continue;
                if (!IsWalkable(next)) continue;

                visited.Add(next);

                cameFrom[next] = current;

                queue.Enqueue(next);
            }
        }

        if (!cameFrom.ContainsKey(goal))
            return null;

        var path = new List<Vector2Int>();
        var step = goal;

        while (step != start)
        {
            path.Add(step);
            step = cameFrom[step];
        }

        path.Reverse();

        return path;
    }

    public void OnMapShift(Vector2Int shift)
    {
        mapLoader.queenPos += shift;
    }

    // Checks if a tile can be walked on
    private bool IsWalkable(Vector2Int p)
    {
        var mapData = mapLoader.GetMapData();

        if (p.x < 0 || p.y < 0 ||
            p.y >= mapLoader.Rows || p.x >= mapLoader.Cols)
            return false;

        var tile = mapData[p.y, p.x];

        return tile == '0' || tile == '4' || tile == '9';
    }

    // Finds an accessible tile beside a dig target
    private Vector2Int? GetAccessibleNeighbor(Vector2Int target)
    {
        Vector2Int[] dirs =
        {
            new(1, 0),
            new(-1, 0),
            new(0, 1),
            new(0, -1)
        };

        foreach (var d in dirs)
        {
            var n = target + d;

            if (IsWalkable(n))
                return n;
        }

        return null;
    }

    // Assigns digging jobs to idle miner ants
    public void AssignJobs()
    {
        MapLoader.DigJob deepestJob = null;

        var deepestDist = -1;

        foreach (var job in mapLoader.jobs)
        {
            var dist = Mathf.Abs(job.target.x - mapLoader.queenPos.x) +
                       Mathf.Abs(job.target.y - mapLoader.queenPos.y);

            if (dist > deepestDist)
            {
                deepestDist = dist;
                deepestJob = job;
            }
        }

        foreach (var id in antIds)
        {
            if (roles.TryGetValue(id, out var role) &&
                role == AntRole.Forager)
                continue;

            if (antPaths.ContainsKey(id) || antTargets.ContainsKey(id)) continue;
            if (antStates[id] != AntState.Idle) continue;

            MapLoader.DigJob assigned = null;

            foreach (var job in mapLoader.jobs)
                if (!job.taken)
                {
                    var adj = GetAccessibleNeighbor(job.target);

                    if (adj == null) continue;

                    var path = FindPath(ants[id], adj.Value);

                    if (path == null) continue;

                    job.taken = true;

                    antTargets[id] = job.target;
                    antPaths[id] = new Queue<Vector2Int>(path);

                    antStates[id] = AntState.GoingToDig;

                    assigned = job;

                    break;
                }

            // If no job is available, move toward the deepest unfinished tunnel
            if (assigned == null && deepestJob != null)
            {
                var adj = GetAccessibleNeighbor(deepestJob.target);

                var staging =
                    adj ?? GetNearestOpenTileToward(ants[id], deepestJob.target);

                if (staging == null) continue;

                var path = FindPath(ants[id], staging.Value);

                if (path == null) continue;

                antPaths[id] = new Queue<Vector2Int>(path);
            }
        }
    }

    private Vector2Int? GetNearestOpenTileToward(Vector2Int from, Vector2Int target)
    {
        var mapData = mapLoader.GetMapData();

        var best = from;
        var bestDist = int.MaxValue;

        for (var y = 0; y < mapLoader.Rows; y++)
            for (var x = 0; x < mapLoader.Cols; x++)
            {
                if (!IsWalkable(new Vector2Int(x, y))) continue;

                var distToTarget = Mathf.Abs(x - target.x) + Mathf.Abs(y - target.y);

                if (distToTarget < bestDist)
                {
                    bestDist = distToTarget;
                    best = new Vector2Int(x, y);
                }
            }

        return best == from ? null : best;
    }

    // Main ant update loop
    public void UpdateAnts()
    {
        foreach (var id in antIds)
        {
            // Handles digging timer
            if (antStates[id] == AntState.Digging)
            {
                digTimers[id] += Time.deltaTime;

                if (digTimers[id] >= digDurations[id])
                {
                    var target = antTargets[id];

                    mapLoader.RemoveJobAt(target);

                    mapLoader.GetMapData()[target.y, target.x] = '0';

                    mapLoader.UpdateOddsAround(target.x, target.y);

                    mapLoader.RefreshTilemap();

                    var exit = GetNearestExitAdjacent(ants[id]);

                    if (exit == null)
                    {
                        antStates[id] = AntState.Idle;
                        antTargets.Remove(id);
                        continue;
                    }

                    var path2 = FindPath(ants[id], exit.Value);

                    if (path2 == null)
                    {
                        antStates[id] = AntState.Idle;
                        antTargets.Remove(id);
                        continue;
                    }

                    antPaths[id] = new Queue<Vector2Int>(path2);

                    antStates[id] = AntState.CarryingToExit;
                }

                continue;
            }

            if (!antPaths.ContainsKey(id))
            {
                TryReassignJob(id);
                continue;
            }

            antMoveTimers[id] += Time.deltaTime;

            if (antMoveTimers[id] < moveInterval)
                continue;

            antMoveTimers[id] = 0f;

            var path = antPaths[id];

            if (path.Count == 0)
            {
                if (antTargets.ContainsKey(id))
                    OnAntArrived(id, antTargets[id]);
                else
                    antStates[id] = AntState.Idle;

                antPaths.Remove(id);

                continue;
            }

            var next = path.Dequeue();

            ants[id] = next;
        }

        DrawAnts();
    }

    // Rebuilds paths if an ant lost its path unexpectedly
    private void TryReassignJob(int i)
    {
        if (antStates[i] == AntState.Idle)
            return;

        if (antStates[i] == AntState.GoingToDig)
        {
            var adj = GetAccessibleNeighbor(antTargets[i]);

            if (adj == null) return;

            var path = FindPath(ants[i], adj.Value);

            if (path == null) return;

            antPaths[i] = new Queue<Vector2Int>(path);
        }
        else if (antStates[i] == AntState.CarryingToExit)
        {
            var exit = GetNearestExitAdjacent(ants[i]);

            if (exit == null) return;

            var path = FindPath(ants[i], exit.Value);

            if (path == null) return;

            antPaths[i] = new Queue<Vector2Int>(path);
        }
    }

    // Handles what happens when an ant reaches its destination
    private void OnAntArrived(int antIndex, Vector2Int pos)
    {
        var state = antStates[antIndex];

        if (state == AntState.GoingToDig)
        {
            antStates[antIndex] = AntState.Digging;

            digTimers[antIndex] = 0f;
            digDurations[antIndex] = Random.Range(0f, 0.5f);

            return;
        }

        if (state == AntState.CarryingToExit)
        {
            antStates[antIndex] = AntState.Idle;

            antTargets.Remove(antIndex);
            antPaths.Remove(antIndex);
        }

        DrawAnts();
    }

    // Shifts all ants and targets when the map expands
    public void ShiftAll(Vector2Int delta)
    {
        foreach (var id in antIds)
            ants[id] += delta;

        var keys = new List<int>(antTargets.Keys);

        foreach (var k in keys)
            antTargets[k] += delta;

        foreach (var path in antPaths.Values)
        {
            var newQueue = new Queue<Vector2Int>();

            foreach (var p in path)
                newQueue.Enqueue(p + delta);

            path.Clear();

            foreach (var p in newQueue)
                path.Enqueue(p);
        }

        foreach (var job in mapLoader.jobs)
            job.target += delta;

        var fm = FindObjectOfType<ForagerManager>();

        if (fm != null)
            fm.ResetBrokenForagersOnly();
    }
}
