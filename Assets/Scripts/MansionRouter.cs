using System;
using System.Collections.Generic;
using UnityEngine;

public class MansionRouter : MonoBehaviour, IDestinationProvider
{
    [Header("Special Rooms")]
    public string startRoomId = "Start";
    public string ballroomRoomId = "Ballroom";

    [Header("Room Pools")]
    [SerializeField] private string[] randomizeRooms =
        { "Kitchen", "Basement", "Lounge", "GameRoom", "Theater", "Conservatory", "Patio" };

    [SerializeField] private string[] stableRooms = { };

    // Fixed positions (row 0 = top)
    private readonly Vector2Int startPos = new Vector2Int(2, 1);
    private readonly Vector2Int ballroomPos = new Vector2Int(0, 2);

    // 3x3 stores indices
    private int[,] gameMap = new int[3, 3];

    // (roomId, dir) -> destination roomId
    private readonly Dictionary<(string, Direction), string> doorMap = new();

    // index tables
    private readonly List<string> indexToRoomId = new();
    private readonly Dictionary<string, int> roomIdToIndex = new();

    [Header("Debug")]
    public bool debugPrintOnStart = true;

    private void Start()
    {
        ResetMansion();

        if (debugPrintOnStart)
        {
            DebugPrintGrid("[Router] After ResetMansion()");
            DebugPrintDoorRoutesFrom(startRoomId);
        }
    }

    public void ResetMansion()
    {
        BuildRoomIndexTables();
        ClearGrid();

        PlaceRoomAt(startRoomId, startPos.x, startPos.y);
        PlaceRoomAt(ballroomRoomId, ballroomPos.x, ballroomPos.y);

        foreach (var stable in stableRooms)
        {
            if (string.IsNullOrWhiteSpace(stable)) continue;
            if (stable == startRoomId || stable == ballroomRoomId) continue;
            PlaceRoomInFirstEmptyCell(stable);
        }

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

    public string GetDestinationRoomId(string currentRoomId, Direction exitDirection)
    {
        if (string.IsNullOrWhiteSpace(currentRoomId))
            return startRoomId;

        if (doorMap.TryGetValue((currentRoomId, exitDirection), out var dest) && !string.IsNullOrEmpty(dest))
            return dest;

        return startRoomId;
    }

    // ---- internal ----

    private void BuildRoomIndexTables()
    {
        indexToRoomId.Clear();
        roomIdToIndex.Clear();

        AddIfMissing(startRoomId);
        AddIfMissing(ballroomRoomId);
        foreach (var s in stableRooms) AddIfMissing(s);
        foreach (var r in randomizeRooms) AddIfMissing(r);

        void AddIfMissing(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            if (roomIdToIndex.ContainsKey(id)) return;
            roomIdToIndex[id] = indexToRoomId.Count;
            indexToRoomId.Add(id);
        }
    }

    private void ClearGrid()
    {
        for (int row = 0; row < 3; row++)
            for (int col = 0; col < 3; col++)
                gameMap[row, col] = -1;
    }

    private void PlaceRoomAt(string roomId, int row, int col)
    {
        if (!roomIdToIndex.TryGetValue(roomId, out int idx))
        {
            Debug.LogError($"[Router] Unknown roomId '{roomId}'");
            return;
        }
        gameMap[row, col] = idx;
    }

    private void PlaceRoomInFirstEmptyCell(string roomId)
    {
        for (int row = 0; row < 3; row++)
            for (int col = 0; col < 3; col++)
                if (gameMap[row, col] == -1)
                {
                    PlaceRoomAt(roomId, row, col);
                    return;
                }
    }

    private string RoomIdAt(int row, int col)
    {
        int idx = gameMap[row, col];
        if (idx < 0 || idx >= indexToRoomId.Count) return null;
        return indexToRoomId[idx];
    }

    private void RebuildDoorMapFromGrid()
    {
        doorMap.Clear();

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                var fromId = RoomIdAt(row, col);
                if (fromId == null) continue;

                foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                {
                    var (nr, nc) = Neighbor(row, col, dir);

                    string toId = null;
                    if (nr >= 0 && nr < 3 && nc >= 0 && nc < 3)
                        toId = RoomIdAt(nr, nc);

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

    private void DebugPrintGrid(string header)
    {
        Debug.Log(header);
        for (int row = 0; row < 3; row++)
        {
            string line = "| ";
            for (int col = 0; col < 3; col++)
                line += (RoomIdAt(row, col) ?? "EMPTY") + " | ";
            Debug.Log(line);
        }
    }

    private void DebugPrintDoorRoutesFrom(string roomId)
    {
        Debug.Log($"[Router] Routes from '{roomId}':");
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
            Debug.Log($"  {dir} -> {GetDestinationRoomId(roomId, dir)}");
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
