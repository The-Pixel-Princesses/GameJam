using System.Collections.Generic;

[System.Serializable]
public class RoomState
{
    public bool keyCollected;
    public bool anchored;

    // Which chest slot (0..3) is the real one
    public int realChestIndex = -1;

    // Random ID assignment for each chest slot (length 4)
    public int[] chestIdByIndex;

    // Runtime-only is fine (not shown in inspector)
    public HashSet<int> litTorches = new HashSet<int>();

}