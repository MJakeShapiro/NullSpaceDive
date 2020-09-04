using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Debug.LogError("A second GameManager was detected! Time: " + Time.time + "\n" + this);
            Destroy(gameObject);
        }
    }

    private void Start()
    {

    }
}