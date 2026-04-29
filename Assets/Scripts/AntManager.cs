using System.Collections.Generic;
using System.Linq;
using SimulationFourmiliere;
using UnityEngine;
using UnityEngine.Tilemaps;


public class AntManager : MonoBehaviour
{
    public MapLoader mapLoader;
    public Tilemap AntColonie;
    public SimulationState simulation;

    public TileBase AntTile;

    private int nextAntId = 0;
    public List<int> antIds = new();

    public Dictionary<int, Vector2Int> antTargets = new Dictionary<int, Vector2Int>();
    public Dictionary<int, Queue<Vector2Int>> antPaths = new Dictionary<int, Queue<Vector2Int>>();

    private Dictionary<int, float> digTimers = new Dictionary<int, float>();
    private Dictionary<int, float> digDurations = new Dictionary<int, float>();

    private Dictionary<int, float> antMoveTimers = new Dictionary<int, float>();
    public float moveInterval = 0.2f; //<------

    public Dictionary<int, Vector2Int> ants = new();
    public Vector2Int queenPos;

    private Dictionary<int, AntState> antStates = new Dictionary<int, AntState>();

    [SerializeField] public popCounter popCounter;
    enum AntState
    {
        Idle,
        GoingToDig,
        Digging,
        CarryingToExit
    }

    void Start()
    {
        simulation = popCounter.GetState();

        if (!mapLoader.queenFound)
        {
            Debug.LogError("Queen not loaded yet!");
            return;
        }

        queenPos = mapLoader.queenPos;
        //SpawnAnts(0);
        DrawAnts();
    }

    void RemoveAnt()
    {
        int index = ants.Count - 1;
        int id = antIds[index];

        antIds.RemoveAt(index);

        ants.Remove(id);
        antStates.Remove(id);
        antMoveTimers.Remove(id);
        antPaths.Remove(id);
        antTargets.Remove(id);
    }
    void SyncAntCount()
    {
        if (simulation == null) return;

        int target = simulation.Colonie.Pop();

        while (ants.Count < target)
            SpawnSingleAnt();

        while (ants.Count > target)
            RemoveAnt();

        //Debug.Log("TARGET POP = " + simulation.Colonie.Pop());
        //Debug.Log("CURRENT ANTS = " + ants.Count);
    }
    void SpawnSingleAnt()
    {
        int id = nextAntId++;

        antIds.Add(id);
        ants[id] = queenPos;

        antStates[id] = AntState.Idle;
        antMoveTimers[id] = 0f;

        digTimers[id] = 0f;
        digDurations[id] = 0f;

        Debug.Log("ant added at" + queenPos);
    }
    Vector2Int? GetNearestExitAdjacent(Vector2Int from)
    {
        var mapData = mapLoader.GetMapData();

        int bestDist = int.MaxValue;
        Vector2Int? best = null;

        Vector2Int[] dirs = {
        new Vector2Int(1,0),
        new Vector2Int(-1,0),
        new Vector2Int(0,1),
        new Vector2Int(0,-1)
    };

        for (int y = 0; y < mapLoader.Rows; y++)
        {
            for (int x = 0; x < mapLoader.Cols; x++)
            {
                if (mapData[y, x] != '4') continue;

                Vector2Int grass = new Vector2Int(x, y);

                foreach (var d in dirs)
                {
                    var neighbor = grass + d;

                    if (!IsWalkable(neighbor)) continue;

                    int dist = Mathf.Abs(from.x - neighbor.x) + Mathf.Abs(from.y - neighbor.y);

                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        best = neighbor;
                    }
                }
            }
        }

        return best;
    }

    public void DrawAnts()
    {
        AntColonie.ClearAllTiles();
        foreach (int id in antIds)
        {
            Vector3Int pos = mapLoader.MapToTilePos(ants[id]);
            AntColonie.SetTile(pos, AntTile);
        }
    }

    List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        if (start == goal) return new List<Vector2Int>();

        var queue = new Queue<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        Vector2Int[] dirs = {
        new Vector2Int(1,0),
        new Vector2Int(-1,0),
        new Vector2Int(0,1),
        new Vector2Int(0,-1)
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
        queenPos += shift;
    }
    bool IsWalkable(Vector2Int p)
    {
        var mapData = mapLoader.GetMapData();

        if (p.x < 0 || p.y < 0 ||
            p.y >= mapLoader.Rows || p.x >= mapLoader.Cols)
            return false;

        char tile = mapData[p.y, p.x];

        return tile == '0' || tile == '4' || tile == '9';
    }
    Vector2Int? GetAccessibleNeighbor(Vector2Int target)
    {
        Vector2Int[] dirs = {
        new Vector2Int(1,0),
        new Vector2Int(-1,0),
        new Vector2Int(0,1),
        new Vector2Int(0,-1)
    };

        foreach (var d in dirs)
        {
            var n = target + d;

            if (IsWalkable(n))
                return n;
        }

        return null;
    }
    public void AssignJobs()
    {
        // Find the current dig target (the deepest job, taken or not)
        MapLoader.DigJob deepestJob = null;
        int deepestDist = -1;

        foreach (var job in mapLoader.jobs)
        {
            int dist = Mathf.Abs(job.target.x - queenPos.x) +
                       Mathf.Abs(job.target.y - queenPos.y);
            if (dist > deepestDist)
            {
                deepestDist = dist;
                deepestJob = job;
            }
        }

        foreach (int id in antIds)
        {
            // Skip ants already moving
            if (antPaths.ContainsKey(id) || antTargets.ContainsKey(id)) continue;
            if (antStates[id] != AntState.Idle) continue;

            // First try to assign an untaken job
            MapLoader.DigJob assigned = null;
            foreach (var job in mapLoader.jobs)
            {
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
            }

            // If no untaken job, walk toward the deepest job anyway
            // so the ant is already in position when the tile opens
            if (assigned == null && deepestJob != null)
            {
                var adj = GetAccessibleNeighbor(deepestJob.target);
                // If no neighbor yet (tile not dug), walk toward the nearest open tile
                // adjacent to the job chain
                Vector2Int? staging = adj ?? GetNearestOpenTileToward(ants[id], deepestJob.target);
                if (staging == null) continue;

                var path = FindPath(ants[id], staging.Value);
                if (path == null) continue;

                // Don't mark job taken — ant is just staging
                antPaths[id] = new Queue<Vector2Int>(path);
                // No antTargets entry = ant will reassign when it arrives
            }
        }
    }
    Vector2Int? GetNearestOpenTileToward(Vector2Int from, Vector2Int target)
    {
        var mapData = mapLoader.GetMapData();
        Vector2Int best = from;
        int bestDist = int.MaxValue;

        for (int y = 0; y < mapLoader.Rows; y++)
        {
            for (int x = 0; x < mapLoader.Cols; x++)
            {
                if (!IsWalkable(new Vector2Int(x, y))) continue;

                // Pick tiles that are closer to target than current ant position
                int distToTarget = Mathf.Abs(x - target.x) + Mathf.Abs(y - target.y);
                if (distToTarget < bestDist)
                {
                    bestDist = distToTarget;
                    best = new Vector2Int(x, y);
                }
            }
        }

        return best == from ? null : best;
    }
    int GetFreeAnt()
    {
        foreach (int id in antIds)
        {
            if (!antTargets.ContainsKey(id))
                return id;
        }
        return -1;
    }
    void Update()
    {
        SyncAntCount();
        AssignJobs();
        UpdateAnts();

        int freeAnts = antIds.Count(id => !antTargets.ContainsKey(id));
        int openJobs = mapLoader.jobs.Count(j => !j.taken);
        int totalJobs = mapLoader.jobs.Count;
        Debug.Log($"Ants: {ants.Count} | Free: {freeAnts} | Jobs total: {totalJobs} | Jobs open: {openJobs}");

        if (simulation == null)
        {
            Debug.Log("NO SIMULATION");
            return;
        }
    }
    void ForceIdleRecheck()
    {
        foreach (int id in antIds)
        {
            if (!antPaths.ContainsKey(id) &&
                antStates[id] == AntState.Idle)
            {
                AssignJobs();
                break;
            }
        }
    }
    public void UpdateAnts()
    {
        foreach (int id in antIds)
        {
            // =========================
            // DIGGING STATE (NEW LOGIC)
            // =========================
            if (antStates[id] == AntState.Digging)
            {
                digTimers[id] += Time.deltaTime;

                if (digTimers[id] >= digDurations[id])
                {
                    Vector2Int target = antTargets[id];

                    // ACTUAL DIG HAPPENS HERE
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

            // =========================
            // NORMAL MOVEMENT LOGIC
            // =========================
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

            Vector2Int next = path.Dequeue();
            ants[id] = next;
        }

        DrawAnts();
    }

    void TryReassignJob(int i)
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
    void OnAntArrived(int antIndex, Vector2Int pos)
    {
        var state = antStates[antIndex];

        if (state == AntState.GoingToDig)
        {
            // START DIG DELAY (no instant digging anymore)
            antStates[antIndex] = AntState.Digging;

            digTimers[antIndex] = 0f;
            digDurations[antIndex] = Random.Range(0f, 0.5f);

            return;
        }
        else if (state == AntState.CarryingToExit)
        {
            // DROP DIRT
            antStates[antIndex] = AntState.Idle;
            antTargets.Remove(antIndex);
            antPaths.Remove(antIndex);
        }
    }

    void InvalidateAllPaths()
    {
        antPaths.Clear();
        antTargets.Clear();

        foreach (int id in antIds)
            antStates[id] = AntState.Idle;
    }
    public void ShiftAll(Vector2Int delta)
    {
        foreach (int id in antIds)
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
    }
}