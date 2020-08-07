using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitioner : MonoBehaviour
{
    public static SceneTransitioner instance;

    public Animator[] transitions;

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public static void LoadImmediate (string sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public static bool LoadScene(string sceneToLoad)
    {
        if (instance != null)
        {
            instance.StartTransition(sceneToLoad);
            return true;
        }
        else
            return false;
    }

    public static bool LoadScene(string sceneToLoad, float duration)
    {
        if (instance != null)
        {
            instance.StartTransition(sceneToLoad);
            return true;
        }
        else
            return false;
    }

    public void StartTransition(string sceneToLoad)
    {
        StartCoroutine(Transition(sceneToLoad));
    }

    public void StartTransition(string sceneToLoad, float duration)
    {

        StartCoroutine(Transition(sceneToLoad, duration));
    }

    public IEnumerator Transition(string sceneToLoad)
    {
        foreach (Animator a in transitions)
            if (a.gameObject.activeSelf)
                a.SetTrigger("TransitionOut");

        yield return new WaitForSeconds(1);

        SceneManager.LoadScene(sceneToLoad);
    }

    public IEnumerator Transition(string sceneToLoad, float duration)
    {
        foreach (Animator a in transitions)
            if (a.gameObject.activeSelf)
            {
                a.SetFloat("Speed", 1 / duration);
                a.SetTrigger("TransitionOut");
            }

        yield return new WaitForSeconds(duration);

        SceneManager.LoadScene(sceneToLoad);
    }
}