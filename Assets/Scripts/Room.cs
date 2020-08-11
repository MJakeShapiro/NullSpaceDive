using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField]
    private List<Door> doors;

    private void Awake()
    {
        doors = new List<Door>(gameObject.GetComponentsInChildren<Door>());
    }
}
