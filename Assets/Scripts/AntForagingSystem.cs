using System.Collections;
using System.Collections.Generic;
using SimulationFourmiliere;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ForagerManager : MonoBehaviour
{
    public AntManager antManager;
    public MapLoader mapLoader;
    public SimulationState simulation { get; private set; }

    [Header("Forager Settings")]
    public float forageReturnDelay = 3f;
    public int minFood = 2;
    public int maxFood = 6;
    public float distributeRange = 1.5f;   // world units to check for hungry ants
    public float moveInterval = 0.2f;

    // How many ants should be foragers based on food stock
    public int targetForagerCount = 2;

    // Per-forager state
    private Dictionary<int, ForagerState> states = new();
    private Dictionary<int, int> foodCarried = new();
    private Dictionary<int, float> returnTimers = new();
    private Dictionary<int, Queue<Vector2Int>> paths = new();
    private Dictionary<int, float> moveTimers = new();

    private enum ForagerState
    {
        WalkingToExit,
        Outside,
        ReturningToColony,
        Distributing
    }

    void Start()
    {
        Debug.Log("ForagerManager Start");

        if (antManager == null)
        {
            Debug.LogError("antManager is NULL");
            return;
        }

        simulation = antManager.simulation;

        if (simulation == null)
        {
            Debug.LogError("simulation is NULL (from antManager)");
        }
    }

    void Update()
    {
        if (simulation == null)
            simulation = antManager.simulation;

        if (simulation == null)
        {
            Debug.Log("ForagerManager waiting for simulation...");
            return;
        }

        SyncForagerCount();
        UpdateForagers();

        if (Time.frameCount % 60 == 0)
        {
            LogForagerStatus();
        }
    }

    void SyncForagerCount()
    {
        if (antManager == null || simulation == null)
        {
            Debug.LogWarning("Missing references in SyncForagerCount");
            return;
        }

        // Decide how many foragers needed based on food stock vs colony size
        int pop = simulation.Colonie.Pop();
        int stock = simulation.StockNourriture;

        // If food is low relative to colony, push more foragers
        if (stock < pop)
            targetForagerCount = Mathf.Max(3, pop / 2);
        else if (stock < pop * 2)
            targetForagerCount = Mathf.Max(2, pop / 3);
        else
            targetForagerCount = Mathf.Max(1, pop / 5);

        // Count current foragers
        int currentForagers = 0;
        foreach (int id in antManager.antIds)
            if (antManager.roles.TryGetValue(id, out var r) && r == AntManager.AntRole.Forager)
                currentForagers++;

        // Promote idle miners to foragers if needed
        if (currentForagers < targetForagerCount)
        {
            foreach (int id in antManager.antIds)
            {
                if (currentForagers >= targetForagerCount) break;

                bool isForager = antManager.roles.TryGetValue(id, out var r)
                                 && r == AntManager.AntRole.Forager;
                if (isForager) continue;

                // Only pull idle ants
                if (antManager.antStates.TryGetValue(id, out var state) && state != AntManager.AntState.Idle)
                    continue;
                if (antManager.antPaths.ContainsKey(id)) continue;

                PromoteToForager(id);
                currentForagers++;
            }
        }

        // Demote excess foragers that are idle back to miners
        if (currentForagers > targetForagerCount)
        {
            foreach (int id in antManager.antIds)
            {
                if (currentForagers <= targetForagerCount) break;

                if (!antManager.roles.TryGetValue(id, out var r)
                    || r != AntManager.AntRole.Forager) continue;

                // Only demote if currently outside or just returned with no food
                if (states.TryGetValue(id, out var fs) && fs == ForagerState.Outside) continue;
                if (foodCarried.TryGetValue(id, out int f) && f > 0) continue;

                DemoteToMiner(id);
                currentForagers--;
            }
        }

        Debug.Log($"Population={pop}, Food={stock}, TargetForagers={targetForagerCount}");
    }

    void PromoteToForager(int id)
    {
        antManager.roles[id] = AntManager.AntRole.Forager;

        // Remove from miner job system
        antManager.antTargets.Remove(id);
        antManager.antPaths.Remove(id);

        states[id] = ForagerState.WalkingToExit;
        moveTimers[id] = 0f;
        SendToExit(id);
    }

    void DemoteToMiner(int id)
    {
        antManager.roles[id] = AntManager.AntRole.Miner;
        states.Remove(id);
        foodCarried.Remove(id);
        returnTimers.Remove(id);
        paths.Remove(id);
        moveTimers.Remove(id);
    }

    void UpdateForagers()
    {
        var toRemove = new List<int>();

        foreach (int id in antManager.antIds)
        {
            if (!antManager.roles.TryGetValue(id, out var role)
                || role != AntManager.AntRole.Forager) continue;

            if (!states.ContainsKey(id))
            {
                states[id] = ForagerState.WalkingToExit;
                SendToExit(id);
            }

            switch (states[id])
            {
                case ForagerState.WalkingToExit:
                    MoveAlongPath(id);
                    CheckReachedExit(id);
                    break;

                case ForagerState.Outside:
                    returnTimers[id] -= Time.deltaTime;
                    if (returnTimers[id] <= 0f)
                    {
                        int food = Random.Range(minFood, maxFood + 1);
                        foodCarried[id] = food;
                        antManager.SetCarrying(id, true);
                        states[id] = ForagerState.ReturningToColony;
                        SendToQueen(id);
                    }
                    break;

                case ForagerState.ReturningToColony:
                    MoveAlongPath(id);
                    CheckReachedQueen(id);
                    break;

                case ForagerState.Distributing:
                    DistributeFood(id);
                    break;
            }
        }
    }

    void MoveAlongPath(int id)
    {
        if (!paths.ContainsKey(id)) return;

        moveTimers[id] += Time.deltaTime;
        if (moveTimers[id] < moveInterval) return;
        moveTimers[id] = 0f;

        var path = paths[id];
        if (path.Count == 0) return;

        Vector2Int next = path.Dequeue();
        antManager.ants[id] = next;
        antManager.DrawAnts();
    }

    void CheckReachedExit(int id)
    {
        if (!paths.ContainsKey(id) || paths[id].Count > 0) return;

        // Ant is at exit — "disappear" it by moving off map temporarily
        // We just mark it outside and start the timer
        states[id] = ForagerState.Outside;
        returnTimers[id] = forageReturnDelay + Random.Range(0f, 2f);

        // Hide the ant visually by moving it far off
        antManager.ants[id] = new Vector2Int(-999, -999);
        antManager.DrawAnts();
    }

    void CheckReachedQueen(int id)
    {
        if (!paths.ContainsKey(id) || paths[id].Count > 0) return;

        states[id] = ForagerState.Distributing;
    }

    void DistributeFood(int id)
    {
        if (!foodCarried.ContainsKey(id) || foodCarried[id] <= 0)
        {
            antManager.SetCarrying(id, false);
            // Out of food — go forage again
            states[id] = ForagerState.WalkingToExit;
            SendToExit(id);
            return;
        }

        // Find nearest hungry ant within range
        Vector2Int myPos = antManager.ants[id];
        int bestTarget = -1;
        float bestDist = float.MaxValue;

        foreach (int otherId in antManager.antIds)
        {
            if (otherId == id) continue;
            if (!antManager.IsHungry(otherId)) continue;

            Vector2Int otherPos = antManager.ants[otherId];
            float dist = Vector2Int.Distance(myPos, otherPos);

            if (dist <= distributeRange && dist < bestDist)
            {
                bestDist = dist;
                bestTarget = otherId;
            }
        }

        if (bestTarget != -1)
        {
            antManager.FeedAnt(bestTarget);
            foodCarried[id]--;
            simulation.StockNourriture += 1; // food enters the colony stock
            return;
        }

        // No hungry ant nearby — walk toward nearest hungry ant or queen
        int hungryTarget = FindHungriestAnt();
        if (hungryTarget == -1)
        {
            // No hungry ants at all — dump food into stock and go forage
            simulation.StockNourriture += foodCarried[id];
            foodCarried[id] = 0;
            states[id] = ForagerState.WalkingToExit;
            SendToExit(id);
            return;
        }

        // Walk one step toward hungriest ant
        Vector2Int targetPos = antManager.ants[hungryTarget];
        var path = antManager.FindPathPublic(myPos, targetPos);
        if (path != null && path.Count > 0)
        {
            paths[id] = new Queue<Vector2Int>(path);
            MoveAlongPath(id);
        }
    }

    void LogForagerStatus()
    {
        int currentForagers = 0;

        foreach (int id in antManager.antIds)
        {
            if (antManager.roles.TryGetValue(id, out var role) &&
                role == AntManager.AntRole.Forager)
            {
                currentForagers++;

                int carried = 0;
                if (foodCarried.ContainsKey(id))
                    carried = foodCarried[id];

                Debug.Log($"Forager {id} | Carrying Food: {carried} | State: {states[id]}");
            }
        }

        Debug.Log(
            $"FOOD STOCK: {simulation.StockNourriture} | " +
            $"Foragers: {currentForagers}/{targetForagerCount}"
        );
    }
    int FindHungriestAnt()
    {
        int best = -1;
        int bestHunger = 0;

        foreach (int id in antManager.antIds)
        {
            if (!antManager.hunger.TryGetValue(id, out int h)) continue;
            if (h > bestHunger)
            {
                bestHunger = h;
                best = id;
            }
        }

        return best;
    }

    void SendToExit(int id)
    {
        var exit = FindExitTile();
        if (exit == null) return;

        var path = antManager.FindPathPublic(antManager.ants[id], exit.Value);
        if (path == null) return;

        paths[id] = new Queue<Vector2Int>(path);
    }

    void SendToQueen(int id)
    {
        var path = antManager.FindPathPublic(antManager.ants[id], antManager.queenPos);
        if (path == null) return;

        paths[id] = new Queue<Vector2Int>(path);
    }

    Vector2Int? FindExitTile()
    {
        var mapData = mapLoader.GetMapData();

        for (int y = 0; y < mapLoader.Rows; y++)
            for (int x = 0; x < mapLoader.Cols; x++)
                if (mapData[y, x] == '4')
                    return new Vector2Int(x, y);

        return null;
    }
}