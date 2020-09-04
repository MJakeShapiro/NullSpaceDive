using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;
using TMPro.EditorUtilities;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    static ObjectPooler instance;
    [Tooltip("Prefab to be instantiated, must contain a Projectile component")]
    public GameObject projectilePrefab;
    private List<GameObject> projectiles = new List<GameObject>();

    [Tooltip("Generates pool in Awake() if true")]
    public bool generateOnAwake = true;
    [Tooltip("How many objects to initially generate")]
    public int startingObjects = 20;
    [Tooltip("Instantiates new objects if none are free, up to maxObjects")]
    public bool createMoreOnFull = true;
    [Tooltip("Maximum allowed objects in pool. -1 for unlimited size")]
    public int maxObjects = -1;

    protected void Awake()
    {
        if (instance)
        {
            Debug.LogWarning("Multiple object poolers found in scene!");
            Destroy(this);
        }
        else
            instance = this;

        if (maxObjects>=0 && startingObjects>maxObjects)
            maxObjects = startingObjects;

        GenerateInitialPool();
    }

    private void GenerateInitialPool ()
    {
        for (int i=0; i<startingObjects; i++)
        {
            GenerateNewObject();
        }
    }

    private GameObject GenerateNewObject ()
    {
        GameObject newObj = Instantiate(projectilePrefab, transform.position, transform.rotation, transform);
        projectiles.Add(newObj);
        newObj.SetActive(false);
        return newObj;
    }

    private void RemoveProjectileEarly (GameObject obj)
    {
        obj.GetComponent<Projectile>()?.OnRangeMet();
        obj.SetActive(false);
    }

    private void DestroyObject (GameObject obj)
    {
        projectiles.Remove(obj);
        Destroy(obj);
    }

    /// <summary>
    /// Gets a free object from the pool
    /// <para> Returns null if none are available </para>
    /// </summary>
    public static GameObject GetFreeObject ()
    {
        GameObject myObj;

        for (int i = 0; i < instance.projectiles.Count; i++)
            if (!instance.projectiles[i].activeInHierarchy)
            {
                myObj = instance.projectiles[i];
                instance.projectiles.RemoveAt(i);
                instance.projectiles.Add(myObj);
                return myObj;
            }

        if (instance.createMoreOnFull && (instance.maxObjects<0 || instance.projectiles.Count<instance.maxObjects))
            myObj = instance.GenerateNewObject();
        else
            myObj = null;

        return myObj;
    }

    /// <summary>
    /// Gets a free object from the pool, and sets its transform
    /// </summary>
    /// <param name="_position">Position for the new object</param>
    /// <param name="_rotation">Rotation for the new object</param>
    public static GameObject SetFreeObject (Vector3 _position, Quaternion _rotation)
    {
        GameObject myObj = GetFreeObject();
        myObj.transform.position = _position;
        myObj.transform.rotation = _rotation;
        return myObj;
    }

    /// <summary>
    /// Gets a free object from the pool, and sets its transform
    /// </summary>
    /// <param name="_transform">Transform for the new object to copy</param>
    public static GameObject SetFreeObject (Transform _transform)
    {
        GameObject myObj = GetFreeObject();
        myObj.transform.position = _transform.position;
        myObj.transform.rotation = _transform.rotation;
        return myObj;
    }

    /// <summary>
    /// Gets an object from the pool
    /// <para> WARNING: Will take the oldest active projectile if none are free </para>
    /// </summary>
    public static GameObject ForceGetObject () // Forceably re-uses objects if cannot make more
    {
        GameObject myObj;

        for (int i = 0; i < instance.projectiles.Count; i++)
            if (!instance.projectiles[i].activeInHierarchy)
            {
                myObj = instance.projectiles[i];
                instance.projectiles.RemoveAt(i);
                instance.projectiles.Add(myObj);
                return myObj;
            }

        if (instance.createMoreOnFull && (instance.maxObjects<0 || instance.projectiles.Count<instance.maxObjects))
            myObj = instance.GenerateNewObject();
        else
        {
            myObj = instance.projectiles[0];
            instance.RemoveProjectileEarly(myObj);
            instance.projectiles.RemoveAt(0);
            instance.projectiles.Add(myObj);
        }

        return myObj;
    }

    /// <summary>
    /// Gets an object from the pool, and sets its transform
    /// <para> WARNING: Will take the oldest active projectile if none are free </para>
    /// </summary>
    /// <param name="_position">Position for the new object</param>
    /// <param name="_rotation">Rotation for the new object</param>
    public static GameObject ForceSetObject (Vector3 _position, Quaternion _rotation)
    {
        GameObject myObj = ForceGetObject();
        myObj.transform.position = _position;
        myObj.transform.rotation = _rotation;
        return myObj;
    }

    /// <summary>
    /// Gets an object from the pool, and sets its transform
    /// <para> WARNING: Will take the oldest active projectile if none are free </para>
    /// </summary>
    /// <param name="_transform">Transform for the new object to copy</param>
    public static GameObject ForceSetObject (Transform _transform)
    {
        GameObject myObj = ForceGetObject();
        myObj.transform.position = _transform.position;
        myObj.transform.rotation = _transform.rotation;
        return myObj;
    }
}