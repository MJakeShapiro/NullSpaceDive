using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public LineRenderer lineRenderer; //FOR TESTING ONLY

    public List<Door> allDoors = new List<Door>();

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
        RandomlyAssignDoors();
    }


    /// <summary>
    /// Randomly assigns doors to create connected maze
    /// </summary>
    public void RandomlyAssignDoors()
    {
        int pos = 0; // FOR TESTING ONLY

        int doorsLeft = allDoors.Count;
        Door root = allDoors[Random.Range(0, allDoors.Count)], tempRoot = root, next;

        while(doorsLeft > 1)
        {
            tempRoot.visited = true;
            next = allDoors[Random.Range(0, allDoors.Count)];

            if(!next.visited && next != tempRoot) //To ensure no infinite loops
            {
                if (next.nextDoor == null || next.nextDoor != tempRoot)
                {
                    lineRenderer.SetPosition(pos, tempRoot.transform.position); //FOR TESTING ONLY
                    pos++; //FOR TESTING ONLY
                    tempRoot.nextDoor = next;
                    doorsLeft--;
                    tempRoot = next;
                }
            }
        }
        tempRoot.nextDoor = root; //tempRoot is final door, which is to be connected to the original root
        tempRoot.visited = true;
    }
}
