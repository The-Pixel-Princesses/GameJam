// using UnityEngine;

// public class RoomLoader_Minimal : MonoBehaviour
// {
//     public Transform gridRoot;
//     public Transform player;

//     public GameObject[] roomPrefabs;

//     private GameObject currentRoom;
//     private int currentIndex = 0;

//     void Start()
//     {
//         LoadRoom(0);
//     }

//     public void LoadNextRoom()
//     {
//         currentIndex = (currentIndex + 1) % roomPrefabs.Length;
//         LoadRoom(currentIndex);
//     }

//     void LoadRoom(int index)
//     {
//         if (currentRoom != null)
//             Destroy(currentRoom);

//         var prefab = roomPrefabs[index];
//         Debug.Log($"[RoomLoader_Minimal] Loading {prefab.name}");

//         currentRoom = Instantiate(prefab, gridRoot);
//         currentRoom.transform.localPosition = Vector3.zero;
//         currentRoom.transform.localRotation = Quaternion.identity;
//         currentRoom.transform.localScale = Vector3.one;

//         // Find spawn point
//         var spawn = currentRoom.transform.Find("Spawn");
//         if (spawn == null)
//         {
//             Debug.LogWarning("[RoomLoader_Minimal] No Spawn child found; using room origin.");
//             spawn = currentRoom.transform;
//         }

//         // Move player (preserve Z!)
//         player.position = new Vector3(
//             spawn.position.x,
//             spawn.position.y,
//             player.position.z
//         );
//     }
// }
