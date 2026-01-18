using UnityEngine;

public class GameSceneInitializer : MonoBehaviour
{
    private void Start()
    {
        var router = FindFirstObjectByType<MansionRouter>();
        var loader = FindFirstObjectByType<RoomLoader>();

        if (router == null || loader == null)
        {
            Debug.LogError("[GameSceneInitializer] Missing MansionRouter or RoomLoader.");
            return;
        }

        router.ResetMansion();
        loader.LoadRoom(router.startRoomId, Direction.S);
    }
}
