using UnityEngine;

class GMInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RuntimeInit()
    {
        if (Object.FindObjectOfType<GameManager>() != null)
            return;

        var go = new GameObject { name = "[GameManager]" };
        go.AddComponent<GameManager>();
        Object.DontDestroyOnLoad(go);
    }
}