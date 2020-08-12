using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public bool noDupeRooms = false;
    public List<GameObject> roomTypes = new List<GameObject>();
    public List<Door> allDoors = new List<Door>();

    public LineRenderer lineRenderer; //FOR TESTING ONLY

    private static MapGenerator instance;
    public static MapGenerator Instance
    {
        get { return instance ?? (instance = new GameObject("MapGenerator").AddComponent<MapGenerator>()); }
    }


    private void OnEnable()
    {
        instance = this;
    }


    private void Start()
    {
        lineRenderer.positionCount = allDoors.Count; //FOR TESTING ONLY
        PlaceRooms();
        RandomlyAssignDoors();
    }


    public void PlaceRooms()
    {
        List<GameObject> roomsPlaced = new List<GameObject>();
        GameObject lastPlacedRoom, roomToPlace;

        if (noDupeRooms)
        {
            for (int i = 0; 0 < roomTypes.Count; i++)
            {

                if (roomsPlaced.Count == 0)  //No rooms placed so instantiate first and place at origin
                {
                    roomToPlace = Instantiate(roomTypes[i], new Vector3(0, 0, 0), Quaternion.identity);
                    roomsPlaced.Add(roomToPlace);
                }
                else
                {
                    lastPlacedRoom = roomsPlaced[roomsPlaced.Count - 1];

                    roomToPlace = Instantiate(roomTypes[i]);
                    float toPlaceXPos = lastPlacedRoom.transform.position.x + (lastPlacedRoom.GetComponent<SpriteRenderer>().bounds.size.x / 2)
                                                                            + (roomToPlace.GetComponent<SpriteRenderer>().bounds.size.x / 2);   //Find bounds of both rooms
                    roomToPlace.transform.position = new Vector3(toPlaceXPos, 0f, 0f);  //and place next room to the right of previous
                    roomsPlaced.Add(roomToPlace);
                }
            }
        }
        else
        {
            int randomRoom, numOfRooms = Random.Range(1, 6);
            for (int i = 0; i < numOfRooms; i++)
            {
                if (roomsPlaced.Count == 0)  //No rooms placed so instantiate first and place at origin
                {
                    randomRoom = Random.Range(0, roomTypes.Count);
                    roomToPlace = Instantiate(roomTypes[randomRoom], new Vector3(0, 0, 0), Quaternion.identity);
                    roomsPlaced.Add(roomToPlace);
                }
                else
                {
                    lastPlacedRoom = roomsPlaced[roomsPlaced.Count - 1];

                    roomToPlace = Instantiate(roomTypes[Random.Range(0, roomTypes.Count)]);
                    float toPlaceXPos = lastPlacedRoom.transform.position.x + (lastPlacedRoom.GetComponent<SpriteRenderer>().bounds.size.x / 2)
                                                                            + (roomToPlace.GetComponent<SpriteRenderer>().bounds.size.x / 2);   //Find bounds of both rooms
                    roomToPlace.transform.position = new Vector3(toPlaceXPos, 0f, 0f);  //and place next room to the right of previous
                    roomsPlaced.Add(roomToPlace);
                }
            }
        }
        
    }

    /// <summary>
    /// Randomly assigns doors to create connected maze
    /// </summary>
    public void RandomlyAssignDoors()
    {
        int pos = 0; // FOR TESTING ONLY

        int doorsLeft = allDoors.Count;
        Door next, root = allDoors[Random.Range(0, allDoors.Count)], tempRoot = root;

        while(doorsLeft > 1)
        {
            tempRoot.visited = true;
            next = allDoors[Random.Range(0, allDoors.Count)];

            if(!next.visited && next != tempRoot) //To ensure an infinite loop is not created between doors
            {
                if (next.nextDoor == null || next.nextDoor != tempRoot)
                {
                    lineRenderer.SetPosition(pos, tempRoot.transform.position); //FOR TESTING ONLY
                    Debug.Log("Position Set"); //FOR TESTING ONLY
                    pos++; //FOR TESTING ONLY
                    tempRoot.nextDoor = next;
                    doorsLeft--;
                    tempRoot = next;
                }
            }
        }
        tempRoot.nextDoor = root; //tempRoot is final door at end of loop, which is to be connected to the original root
        tempRoot.visited = true;
    }
}
