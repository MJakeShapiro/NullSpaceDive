using NaughtyAttributes;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool runDataTestsOnAwake = true;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Debug.LogError("A second GameManager was detected! Time: " + Time.time + "\n" + this);
            Destroy(gameObject);
        }

        if (runDataTestsOnAwake)
            RunDataTests();
    }

    /// <summary>
    /// Runs a series of tests to ensure the games files are set-up properly.
    /// <para>Logs the results in Unitys Debug window</para>
    /// </summary>
    [Button]
    void RunDataTests ()
    {
        Debug.Log("===================\nStarting Tests!");
        System.DateTime startTime = System.DateTime.Now;

        System.DateTime equipmentTime = System.DateTime.Now;
        if (!EquipmentManager.TestAllWeapons(out int infractions, out int totalCases))
            Debug.LogError($"Weapons Set-up: Failed {infractions}/{totalCases} cases\nTimeElapsed: " + System.DateTime.Now.Subtract(equipmentTime).TotalMilliseconds + "ms");
        else
            Debug.Log("Weapons Set-up: Passed\nTimeElapsed: " + System.DateTime.Now.Subtract(equipmentTime).TotalMilliseconds + "ms");

        double elapsedTime = System.DateTime.Now.Subtract(startTime).TotalMilliseconds;
        Debug.Log("Tests complete! Elapsed time: " + elapsedTime + "ms\n===================");
    }
}