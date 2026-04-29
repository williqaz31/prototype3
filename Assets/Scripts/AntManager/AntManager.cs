using System.Collections.Generic;
using System.IO;
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
    private List<int> antIds = new();

    private Dictionary<int, Vector2Int> antTargets = new Dictionary<int, Vector2Int>();
    private Dictionary<int, Queue<Vector2Int>> antPaths = new Dictionary<int, Queue<Vector2Int>>();

    private Dictionary<int, float> antMoveTimers = new Dictionary<int, float>();
    public float moveInterval = 0.2f; //<------

    private Dictionary<int, Vector2Int> ants = new();
    private Vector2Int queenPos;

    private Dictionary<int, AntState> antStates = new Dictionary<int, AntState>();
    
  

    [SerializeField] public PopCounter popCounter;
    enum AntState
    {
        Idle,
        GoingToDig,
        CarryingToExit
    }

    void Start()
    {
        
        simulation = popCounter.GetState();

        if (!mapLoader.queenFound)
        {
            Debug.Log("Queen not loaded yet!");
            mapLoader.LoadDefault();
        }

        queenPos = mapLoader.queenPos;
        //SpawnAnts(0);
        DrawAnts();
    }
    void Update()
    {
        
        SyncAntCount();
        AssignJobs();
        UpdateAnts();

        ForceIdleRecheck();

        if (simulation == null)
        {
            Debug.Log("NO SIMULATION");
            return;
        }

        //Debug.Log("ANTS TARGET POP: " + simulation.Colonie.Pop());
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
    void SpawnAnts(int count)
    {
        for (int i = 0; i < count; i++)
        {
            //ants.Add(queenPos);
            antStates[i] = AntState.Idle;
            antMoveTimers[i] = 0f;
          //  Debug.Log("ant added at" + queenPos);
        }
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
        //Debug.Log("ant added at" + queenPos);
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

        return tile == '0' || tile == '4';
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
    void AssignJobs()
    {
        foreach (var job in mapLoader.jobs)
        {
            if (job.taken) continue;

            int antIndex = GetFreeAnt();
            if (antIndex == -1) return;

            var adjacent = GetAccessibleNeighbor(job.target);
            if (adjacent == null) continue;

            var path = FindPath(ants[antIndex], adjacent.Value);
            if (path == null) continue;

            job.taken = true;
            antTargets[antIndex] = job.target;
            antPaths[antIndex] = new Queue<Vector2Int>(path);
            antStates[antIndex] = AntState.GoingToDig;

            //Debug.Log($"Trying job at {job.target}");
            //Debug.Log($"Path found? {path != null}");
        }
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
                OnAntArrived(id, antTargets.ContainsKey(id) ? antTargets[id] : ants[id]);
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
            // DIG
            mapLoader.GetMapData()[pos.y, pos.x] = '0';
            mapLoader.UpdateOddsAround(pos.x, pos.y);
            mapLoader.RefreshTilemap();

            // NOW GO TO EXIT
            var exit = GetNearestExitAdjacent(ants[antIndex]);

            if (exit == null)
            {
                antStates[antIndex] = AntState.Idle;
                antTargets.Remove(antIndex);
                return;
            }

            var path = FindPath(ants[antIndex], exit.Value);

            if (path == null)
            {
                antStates[antIndex] = AntState.Idle;
                antTargets.Remove(antIndex);
                return;
            }

            antPaths[antIndex] = new Queue<Vector2Int>(path);
            antStates[antIndex] = AntState.CarryingToExit;
        }
        else if (state == AntState.CarryingToExit)
        {
            // DROP DIRT

            antStates[antIndex] = AntState.Idle;
            antTargets.Remove(antIndex);
            antPaths.Remove(antIndex);
        }

        //InvalidateAllPaths();
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
    }
}