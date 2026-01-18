using System;
using System.Collections.Generic;
using UnityEngine;

public class MansionRouter : MonoBehaviour, IDestinationProvider
{

    [Header("Special Rooms")]
    public string startRoomId = "Start";
    public string ballroomRoomId = "Ballroom";

    // Rooms that get shuffled into the remaining cells (besides Start/Ballroom)
    [Header("Room Pools")]
    [SerializeField]
    private string[] randomizeRooms =
        { "Kitchen", "Basement", "Lounge", "GameRoom", "Theater", "Conservatory", "Patio" };

    // Rooms that should NOT move once placed (optional)
    [SerializeField] private string[] stableRooms = { };

    // 3x3 grid. We store room indices (ints) internally; -1 means empty.
    [Header("Grid")]
    public int[,] gameMap = new int[3, 3];

    [Header("Torch anchoring")]
    [SerializeField] private int maxTorchesPerRoom = 4;

    [Header("References")]
    [Tooltip("Assign your RoomLoader here so MansionRouter can detect the current room and torch states.")]
    [SerializeField] private RoomLoader roomLoader;

    [Header("Debug")]
    [SerializeField] private bool debugPrintOnStart = true;
    [SerializeField] private bool debugPrintOnReshuffle = true;

    // (roomId, exitDirection) -> destinationRoomId
    private readonly Dictionary<(string, Direction), string> doorMap = new();

    // roomId -> (row,col)
    private readonly Dictionary<string, Vector2Int> roomPositions = new();

    // rooms that never move once anchored
    private readonly HashSet<string> anchoredRooms = new();

    // index <-> roomId conversion tables
    private readonly List<string> indexToRoomId = new();
    private readonly Dictionary<string, int> roomIdToIndex = new();

    // Fixed positions (row 0 = top)
    private readonly Vector2Int startPos = new Vector2Int(2, 1);    // bottom middle
    private readonly Vector2Int ballroomPos = new Vector2Int(0, 2); // top right

    // Optional override if you don't want to reference RoomLoader for current room
    private string currentRoomIdOverride = "";

    void Start()
    {
        InitializeNewRun();

        if (debugPrintOnStart)
        {
            DebugPrintGrid("[MansionRouter] After InitializeNewRun()");
            DebugPrintDoorRoutesFrom(startRoomId);
        }
    }

    /// <summary>
    /// Doors call this to figure out where they lead.
    /// </summary>
    public string GetDestinationRoomId(string currentRoomId, Direction exitDirection)
    {
        var key = (currentRoomId, exitDirection);

        if (!doorMap.TryGetValue(key, out var dest) || string.IsNullOrEmpty(dest))
        {
            Debug.LogWarning($"[MansionRouter] No mapping for {currentRoomId} {exitDirection}. Falling back to {startRoomId}.");
            return startRoomId;
        }

        return dest;
    }

    /// <summary>
    /// Call this when a key is collected. Rebuilds the layout while keeping anchors + current room fixed.
    /// </summary>
    public void Reshuffle()
    {
        Debug.Log("[MansionRouter] Reshuffling mansion layout...");

        // Anchor rooms that are fully lit (torch rule)
        UpdateAnchorsFromTorches();

        // Current room should NOT move during this reshuffle
        string currentRoomId = GetCurrentRoomIdSafe();

        ShuffleMovableRooms(currentRoomId);
        RebuildDoorMapFromGrid();

        if (debugPrintOnReshuffle)
        {
            DebugPrintGrid($"[MansionRouter] After Reshuffle() (current='{currentRoomId}')");
            if (!string.IsNullOrEmpty(currentRoomId))
                DebugPrintDoorRoutesFrom(currentRoomId);
        }
    }

    /// <summary>
    /// Permanently anchor/unanchor a room (it will never move while anchored).
    /// </summary>
    public void SetRoomAnchored(string roomId, bool anchored)
    {
        if (string.IsNullOrWhiteSpace(roomId)) return;

        if (anchored) anchoredRooms.Add(roomId);
        else anchoredRooms.Remove(roomId);
    }

    /// <summary>
    /// Useful if you want to force the "current room" from another script instead of using RoomLoader.
    /// </summary>
    public void SetCurrentRoomOverride(string roomIdOrEmpty)
    {
        currentRoomIdOverride = roomIdOrEmpty ?? "";
    }

    /// <summary>
    /// Full reset for player death etc.
    /// </summary>
    public void ResetMansion()
    {
        anchoredRooms.Clear();
        currentRoomIdOverride = "";
        InitializeNewRun();

        if (debugPrintOnStart)
            DebugPrintGrid("[MansionRouter] After ResetMansion()");
    }

    public bool TryGetRoomPosition(string roomId, out Vector2Int pos) =>
        roomPositions.TryGetValue(roomId, out pos);

    // -------------------------
    // Core initialization
    // -------------------------

    private void InitializeNewRun()
    {
        BuildRoomIndexTables();
        ClearGrid();

        // Place fixed rooms
        PlaceRoomAt(startRoomId, startPos.x, startPos.y);
        PlaceRoomAt(ballroomRoomId, ballroomPos.x, ballroomPos.y);

        // Place stable rooms (optional) into first empty cells
        foreach (var stable in stableRooms)
        {
            if (string.IsNullOrWhiteSpace(stable)) continue;
            if (stable == startRoomId || stable == ballroomRoomId) continue;
            PlaceRoomInFirstEmptyCell(stable);
        }

        // Place random rooms into remaining empty cells (shuffled order)
        var rooms = new List<string>(randomizeRooms);
        ShuffleList(rooms);

        foreach (var r in rooms)
        {
            if (string.IsNullOrWhiteSpace(r)) continue;
            if (r == startRoomId || r == ballroomRoomId) continue;
            PlaceRoomInFirstEmptyCell(r);
        }

        RebuildDoorMapFromGrid();
    }

    private void BuildRoomIndexTables()
    {
        indexToRoomId.Clear();
        roomIdToIndex.Clear();

        AddRoomIdIfMissing(startRoomId);
        AddRoomIdIfMissing(ballroomRoomId);

        foreach (var s in stableRooms) AddRoomIdIfMissing(s);
        foreach (var r in randomizeRooms) AddRoomIdIfMissing(r);

        void AddRoomIdIfMissing(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            if (roomIdToIndex.ContainsKey(id)) return;

            roomIdToIndex[id] = indexToRoomId.Count;
            indexToRoomId.Add(id);
        }
    }

    private void ClearGrid()
    {
        roomPositions.Clear();

        for (int row = 0; row < gameMap.GetLength(0); row++)
            for (int col = 0; col < gameMap.GetLength(1); col++)
                gameMap[row, col] = -1;
    }

    private void PlaceRoomAt(string roomId, int row, int col)
    {
        if (!roomIdToIndex.TryGetValue(roomId, out int idx))
        {
            Debug.LogError($"[MansionRouter] Unknown roomId '{roomId}'. Add it to randomizeRooms/stableRooms.");
            return;
        }

        gameMap[row, col] = idx;
        roomPositions[roomId] = new Vector2Int(row, col);
    }

    private void PlaceRoomInFirstEmptyCell(string roomId)
    {
        for (int row = 0; row < gameMap.GetLength(0); row++)
        {
            for (int col = 0; col < gameMap.GetLength(1); col++)
            {
                if (gameMap[row, col] == -1)
                {
                    PlaceRoomAt(roomId, row, col);
                    return;
                }
            }
        }

        Debug.LogWarning($"[MansionRouter] No empty cell to place '{roomId}'. Grid too small?");
    }

    private string RoomIdAt(int row, int col)
    {
        int idx = gameMap[row, col];
        if (idx < 0 || idx >= indexToRoomId.Count) return null;
        return indexToRoomId[idx];
    }

    // -------------------------
    // Anchoring logic
    // -------------------------

    /// <summary>
    /// If a room has all torches lit (>= maxTorchesPerRoom), anchor it permanently.
    /// Reads states from RoomLoader.
    /// </summary>
    private void UpdateAnchorsFromTorches()
    {
        if (roomLoader == null) return;

        // We only know about rooms that exist on the grid right now
        RebuildRoomPositionsFromGrid();

        foreach (var kvp in roomPositions)
        {
            string roomId = kvp.Key;
            if (string.IsNullOrEmpty(roomId)) continue;

            // Start/Ballroom are effectively fixed anyway
            if (roomId == startRoomId || roomId == ballroomRoomId) continue;

            // RoomState state = roomLoader.GetRoomState(roomId);
            // if (state != null && state.litTorches != null && state.litTorches.Count >= maxTorchesPerRoom)
            // {
            //     anchoredRooms.Add(roomId);
            // }
        }
    }

    // -------------------------
    // Shuffle logic (keep fixed/anchored/current)
    // -------------------------

    private void ShuffleMovableRooms(string currentRoomId)
    {
        // Fixed room IDs = start + ballroom + stable + anchored (+ current room for this reshuffle)
        var fixedIds = new HashSet<string>(anchoredRooms);

        fixedIds.Add(startRoomId);
        fixedIds.Add(ballroomRoomId);

        if (!string.IsNullOrWhiteSpace(currentRoomId))
            fixedIds.Add(currentRoomId);

        foreach (var s in stableRooms)
            if (!string.IsNullOrWhiteSpace(s)) fixedIds.Add(s);

        // Find fixed cells + movable rooms
        var fixedCells = new HashSet<Vector2Int>();
        var movableRooms = new List<string>();

        int rows = gameMap.GetLength(0);
        int cols = gameMap.GetLength(1);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var id = RoomIdAt(row, col);
                if (id == null) continue;

                var cell = new Vector2Int(row, col);

                if (fixedIds.Contains(id))
                    fixedCells.Add(cell);
                else
                    movableRooms.Add(id);
            }
        }

        // Collect movable cells = every cell that's not a fixed cell
        var movableCells = new List<Vector2Int>();
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var cell = new Vector2Int(row, col);
                if (!fixedCells.Contains(cell))
                    movableCells.Add(cell);
            }
        }

        // Clear movable cells
        foreach (var cell in movableCells)
            gameMap[cell.x, cell.y] = -1;

        // Shuffle movable rooms and put them back
        ShuffleList(movableRooms);

        int count = Mathf.Min(movableRooms.Count, movableCells.Count);
        for (int i = 0; i < count; i++)
        {
            var roomId = movableRooms[i];
            var cell = movableCells[i];
            PlaceRoomAt(roomId, cell.x, cell.y);
        }

        // Enforce Start/Ballroom are exactly where you designed them
        PlaceRoomAt(startRoomId, startPos.x, startPos.y);
        PlaceRoomAt(ballroomRoomId, ballroomPos.x, ballroomPos.y);

        // Ensure stable rooms exist somewhere (if they got lost due to setup changes)
        foreach (var s in stableRooms)
        {
            if (string.IsNullOrWhiteSpace(s)) continue;
            if (!roomPositions.ContainsKey(s))
                PlaceRoomInFirstEmptyCell(s);
        }

        RebuildRoomPositionsFromGrid();
    }

    private void RebuildRoomPositionsFromGrid()
    {
        roomPositions.Clear();
        for (int row = 0; row < gameMap.GetLength(0); row++)
        {
            for (int col = 0; col < gameMap.GetLength(1); col++)
            {
                var id = RoomIdAt(row, col);
                if (id != null)
                    roomPositions[id] = new Vector2Int(row, col);
            }
        }
    }

    // -------------------------
    // Door map building
    // -------------------------

    private void RebuildDoorMapFromGrid()
    {
        doorMap.Clear();
        RebuildRoomPositionsFromGrid();

        int rows = gameMap.GetLength(0);
        int cols = gameMap.GetLength(1);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var fromId = RoomIdAt(row, col);
                if (fromId == null) continue;

                foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                {
                    var (nr, nc) = Neighbor(row, col, dir);

                    string toId = null;
                    if (nr >= 0 && nr < rows && nc >= 0 && nc < cols)
                        toId = RoomIdAt(nr, nc);

                    // Fallback to Start if out of bounds or empty cell
                    if (string.IsNullOrEmpty(toId))
                        toId = startRoomId;

                    doorMap[(fromId, dir)] = toId;
                }
            }
        }
    }

    private (int nr, int nc) Neighbor(int row, int col, Direction dir)
    {
        return dir switch
        {
            Direction.N => (row - 1, col),
            Direction.E => (row, col + 1),
            Direction.S => (row + 1, col),
            Direction.W => (row, col - 1),
            _ => (row, col)
        };
    }

    // -------------------------
    // Current room detection
    // -------------------------

    private string GetCurrentRoomIdSafe()
    {
        if (!string.IsNullOrEmpty(currentRoomIdOverride))
            return currentRoomIdOverride;

        if (roomLoader != null)
            return roomLoader.GetCurrentRoomId();

        return "";
    }

    // -------------------------
    // Debug helpers
    // -------------------------

    private void DebugPrintGrid(string header)
    {
        Debug.Log(header);
        Debug.Log("Grid (row 0 = top):");

        int rows = gameMap.GetLength(0);
        int cols = gameMap.GetLength(1);

        for (int row = 0; row < rows; row++)
        {
            string line = "| ";
            for (int col = 0; col < cols; col++)
            {
                var id = RoomIdAt(row, col);
                line += (id ?? "EMPTY") + " | ";
            }
            Debug.Log(line);
        }
    }

    private void DebugPrintDoorRoutesFrom(string roomId)
    {
        Debug.Log($"[MansionRouter] Door routes from '{roomId}':");
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            string dest = GetDestinationRoomId(roomId, dir);
            Debug.Log($"  {dir} -> {dest}");
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}