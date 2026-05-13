using System.Collections;
using System.Collections.Generic;
using SimulationFourmiliere;
using UnityEngine;

public class ForagerManager : MonoBehaviour
{
    public AntManager antManager;
    public MapLoader mapLoader;

    [Header("Forager Settings")] public float forageReturnDelay = 3f;

    public int minFood = 2;
    public int maxFood = 6;
    public float distributeRange = 1.5f; // world units to check for hungry ants
    public float moveInterval = 0.2f;

    // How many ants should be foragers based on food stock
    public int targetForagerCount = 2;
    private readonly Dictionary<int, int> foodCarried = new();
    private readonly Dictionary<int, float> moveTimers = new();
    private readonly Dictionary<int, Queue<Vector2Int>> paths = new();
    private readonly Dictionary<int, float> returnTimers = new();

    // Per-forager state
    private readonly Dictionary<int, ForagerState> states = new();
    public SimulationState simulation { get; private set; }

    private IEnumerator Start()
    {
        Debug.Log("ForagerManager Start");

        yield return new WaitUntil(() =>
            antManager != null &&
            antManager.simulation != null &&
            antManager.antIds.Count > 0
        );

        simulation = antManager.simulation;

        Debug.Log("ForagerManager linked simulation OK");
    }

    private void Update()
    {
        if (simulation == null)
            simulation = antManager.simulation;

        if (antManager == null || antManager.simulation == null)
            //Debug.Log("Waiting for AntManager simulation...");
            return;

        SyncForagerCount();
        UpdateForagers();

        if (Time.frameCount % 60 == 0) LogForagerStatus();
    }

    private void SyncForagerCount()
    {
        if (antManager == null || simulation == null)
        {
            Debug.LogWarning("Missing references in SyncForagerCount");
            return;
        }

        // Decide how many foragers needed based on food stock vs colony size
        var pop = simulation.Colonie.Pop();
        var stock = simulation.StockNourriture;

        // If food is low relative to colony, push more foragers
        if (stock < pop)
            targetForagerCount = Mathf.Max(3, pop / 2);
        else if (stock < pop * 2)
            targetForagerCount = Mathf.Max(2, pop / 3);
        else
            targetForagerCount = Mathf.Max(1, pop / 5);

        // Count current foragers
        var currentForagers = 0;
        foreach (var id in antManager.antIds)
            if (antManager.roles.TryGetValue(id, out var r) && r == AntManager.AntRole.Forager)
                currentForagers++;

        // Promote idle miners to foragers if needed
        if (currentForagers < targetForagerCount)
            foreach (var id in antManager.antIds)
            {
                if (currentForagers >= targetForagerCount) break;

                var isForager = antManager.roles.TryGetValue(id, out var r)
                                && r == AntManager.AntRole.Forager;
                if (isForager) continue;

                // Only pull idle ants
                if (antManager.antStates.TryGetValue(id, out var state) && state != AntManager.AntState.Idle)
                    continue;
                if (antManager.antPaths.ContainsKey(id)) continue;

                PromoteToForager(id);
                currentForagers++;
            }

        // Demote excess foragers that are idle back to miners
        if (currentForagers > targetForagerCount)
            foreach (var id in antManager.antIds)
            {
                if (currentForagers <= targetForagerCount) break;

                if (!antManager.roles.TryGetValue(id, out var r)
                    || r != AntManager.AntRole.Forager) continue;

                // Only demote if currently outside or just returned with no food
                if (states.TryGetValue(id, out var fs) && fs == ForagerState.Outside) continue;
                if (foodCarried.TryGetValue(id, out var f) && f > 0) continue;

                DemoteToMiner(id);
                currentForagers--;
            }

        //Debug.Log($"Population={pop}, Food={stock}, TargetForagers={targetForagerCount}");
    }

    private void PromoteToForager(int id)
    {
        antManager.roles[id] = AntManager.AntRole.Forager;

        antManager.antTargets.Remove(id);
        antManager.antPaths.Remove(id);

        states[id] = ForagerState.WalkingToExit;
        moveTimers[id] = 0f;

        // ensure visible when becoming forager
        antManager.hiddenAnts.Remove(id);

        SendToExit(id);

        //Debug.Log($"Ant {id} promoted to Forager");
    }

    private void DemoteToMiner(int id)
    {
        antManager.roles[id] = AntManager.AntRole.Miner;
        states.Remove(id);
        foodCarried.Remove(id);
        returnTimers.Remove(id);
        paths.Remove(id);
        moveTimers.Remove(id);
    }

    private void UpdateForagers()
    {
        var toRemove = new List<int>();

        foreach (var id in antManager.antIds)
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
                        var food = Random.Range(minFood, maxFood + 1);
                        foodCarried[id] = food;

                        antManager.SetCarrying(id, true, food);

                        // NEW: unhide ant when returning
                        antManager.hiddenAnts.Remove(id);

                        states[id] = ForagerState.ReturningToColony;
                        SendToQueen(id);
                    }

                    break;

                case ForagerState.ReturningToColony:
                    if (!paths.ContainsKey(id) || paths[id].Count == 0)
                    {
                        SendToQueen(id);
                        return;
                    }

                    MoveAlongPath(id);
                    CheckReachedQueen(id);
                    break;

                case ForagerState.Distributing:
                    DistributeFood(id);
                    break;
            }
        }
    }

    private void MoveAlongPath(int id)
    {
        if (!paths.ContainsKey(id)) return;

        moveTimers[id] += Time.deltaTime;
        if (moveTimers[id] < moveInterval) return;
        moveTimers[id] = 0f;

        var path = paths[id];
        if (path.Count == 0) return;

        var next = path.Dequeue();
        antManager.ants[id] = next;
        antManager.DrawAnts();
    }

    private void CheckReachedExit(int id)
    {
        if (!paths.ContainsKey(id) || paths[id].Count > 0) return;

        states[id] = ForagerState.Outside;

        returnTimers[id] = forageReturnDelay + Random.Range(0f, 2f);

        antManager.hiddenAnts.Add(id);

        // clear path so no stale data
        paths.Remove(id);

        //Debug.Log($"Forager {id} left colony (hidden)");
    }

    private void CheckReachedQueen(int id)
    {
        if (!paths.ContainsKey(id) || paths[id].Count > 0) return;

        states[id] = ForagerState.Distributing;
    }

    private void DistributeFood(int id)
    {
        if (!foodCarried.ContainsKey(id) || foodCarried[id] <= 0)
        {
            antManager.SetCarrying(id, false, 0);
            foodCarried[id] = 0;
            // Out of food � go forage again
            states[id] = ForagerState.WalkingToExit;
            SendToExit(id);
            antManager.DrawAnts();
            return;
        }

        // Find nearest hungry ant within range
        var myPos = antManager.ants[id];
        var bestTarget = -1;
        var bestDist = float.MaxValue;

        foreach (var otherId in antManager.antIds)
        {
            if (otherId == id) continue;
            if (!antManager.IsHungry(otherId)) continue;

            var otherPos = antManager.ants[otherId];
            var dist = Vector2Int.Distance(myPos, otherPos);

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

        // No hungry ant nearby � walk toward nearest hungry ant or queen
        var hungryTarget = FindHungriestAnt();
        if (hungryTarget == -1)
        {
            // No hungry ants at all � dump food into stock and go forage
            simulation.StockNourriture += foodCarried[id];
            foodCarried[id] = 0;
            antManager.SetCarrying(id, false, 0);
            states[id] = ForagerState.WalkingToExit;
            paths.Remove(id);
            SendToExit(id);
            antManager.DrawAnts();
            return;
        }

        // Walk one step toward hungriest ant
        var targetPos = antManager.ants[hungryTarget];
        var path = antManager.FindPathPublic(myPos, targetPos);
        if (path != null && path.Count > 0)
        {
            paths[id] = new Queue<Vector2Int>(path);
            MoveAlongPath(id);
        }
    }

    public void ResetForagersToExit()
    {
        foreach (var id in antManager.antIds)
        {
            if (!antManager.roles.TryGetValue(id, out var role))
                continue;

            if (role != AntManager.AntRole.Forager)
                continue;

            var exit = FindExitTile();
            if (exit == null) continue;

            // TELEPORT to exit
            antManager.ants[id] = exit.Value;

            // clear movement state
            paths.Remove(id);
            moveTimers[id] = 0f;

            // restart state machine cleanly
            states[id] = ForagerState.Outside;
            returnTimers[id] = forageReturnDelay;

            // optional: hide/unhide safety reset
            antManager.hiddenAnts.Remove(id);

            antManager.DrawAnts();
        }
    }

    private void LogForagerStatus()
    {
        var currentForagers = 0;

        foreach (var id in antManager.antIds)
            if (antManager.roles.TryGetValue(id, out var role) &&
                role == AntManager.AntRole.Forager)
            {
                currentForagers++;

                var carried = 0;
                if (foodCarried.ContainsKey(id))
                    carried = foodCarried[id];
                //Debug.Log($"Forager {id} | Carrying Food: {carried} | State: {states[id]}");
            }

        //Debug.Log(
        //    $"FOOD STOCK: {simulation.StockNourriture} | " +
        //    $"Foragers: {currentForagers}/{targetForagerCount}"
        //);
    }

    private int FindHungriestAnt()
    {
        var best = -1;
        var bestHunger = 0;

        foreach (var id in antManager.antIds)
        {
            if (!antManager.hunger.TryGetValue(id, out var h)) continue;
            if (h > bestHunger)
            {
                bestHunger = h;
                best = id;
            }
        }

        return best;
    }

    private void SendToExit(int id)
    {
        // Make sure ant is visible and at a valid position first
        antManager.hiddenAnts.Remove(id);

        var currentPos = antManager.ants[id];

        // If position is invalid (was hidden off-map), snap to queen first
        if (currentPos.x < 0 || currentPos.y < 0 ||
            currentPos.x >= mapLoader.Cols || currentPos.y >= mapLoader.Rows)
        {
            antManager.ants[id] = mapLoader.queenPos;
            currentPos = mapLoader.queenPos;
        }

        var exit = FindExitTile();
        if (exit == null) return;

        var path = antManager.FindPathPublic(currentPos, exit.Value);
        if (path == null) return;

        paths[id] = new Queue<Vector2Int>(path);
    }

    private void SendToQueen(int id)
    {
        var currentPos = antManager.ants[id];

        var path = antManager.FindPathPublic(
            currentPos,
            mapLoader.queenPos
        );

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning($"Forager {id} failed to path to queen � resetting");
            ResetSingleForagerToExit(id);
            return;
        }

        paths[id] = new Queue<Vector2Int>(path);
    }

    private void ResetSingleForagerToExit(int id)
    {
        var exit = FindExitTile();
        if (exit == null) return;

        antManager.ants[id] = exit.Value;

        paths.Remove(id);
        moveTimers[id] = 0f;

        states[id] = ForagerState.Outside;
        returnTimers[id] = forageReturnDelay;

        antManager.hiddenAnts.Remove(id);
    }

    private Vector2Int? FindExitTile()
    {
        var mapData = mapLoader.GetMapData();

        for (var y = 0; y < mapLoader.Rows; y++)
        for (var x = 0; x < mapLoader.Cols; x++)
            if (mapData[y, x] == '4')
                return new Vector2Int(x, y);

        return null;
    }

    public void ResetBrokenForagersOnly()
    {
        foreach (var id in antManager.antIds)
        {
            if (!antManager.roles.TryGetValue(id, out var role) ||
                role != AntManager.AntRole.Forager)
                continue;

            var pos = antManager.ants[id];

            // Only reset if OUTSIDE map or stuck in invalid tile
            if (pos.x < 0 || pos.y < 0 ||
                pos.x >= mapLoader.Cols || pos.y >= mapLoader.Rows)
            {
                ResetSingleForagerToExit(id);
                continue;
            }

            // Optional: also reset if inside a wall
            var tile = mapLoader.GetMapData()[pos.y, pos.x];
            if (tile == '1') // wall
                ResetSingleForagerToExit(id);
        }
    }

    private enum ForagerState
    {
        WalkingToExit,
        Outside,
        ReturningToColony,
        Distributing
    }
}