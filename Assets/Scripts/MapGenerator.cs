using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    
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
        lineRenderer.positionCount = allDoors.Count;
        RandomlyAssignDoors();
    }

    /// <summary>
    /// Randomly assigns doors to create connected maze
    /// </summary>
    public void RandomlyAssignDoors()
    {
        int doorsLeft = allDoors.Count;
        int randNum = Random.Range(0, allDoors.Count); // FOR TESTING ONLY
        Door root = allDoors[randNum];
        Debug.Log(randNum); // FOR TESTING ONLY
        Door next;
        int pos = 0;
        while(doorsLeft > 0)
        {
            randNum = Random.Range(0, allDoors.Count);
            next = allDoors[Random.Range(0, allDoors.Count)];
            if(!next.visited && next != root) //To ensure no infinite loops
            {
                if (next.nextDoor == null || next.nextDoor != root)
                {
                    Debug.Log(randNum); // FOR TESTING ONLY
                    lineRenderer.SetPosition(pos, root.transform.position);
                    pos++;
                    root.nextDoor = next;
                    next.visited = true;
                    doorsLeft--;
                    root = next;
                }
            }
        }
    }
}
