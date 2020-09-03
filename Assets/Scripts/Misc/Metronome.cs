using NaughtyAttributes;
using UnityEngine;

public class Metronome : MonoBehaviour
{
    public static Metronome instance;

    [MinValue(1)]
    public int beats = 4;
    [OnValueChanged("ApplyOffBeats")] [MinValue(0),MaxValue(4)]
    public int offBeats = 1;


    [OnValueChanged("ApplyBPM")] [MinValue(30),MaxValue(240)] [SerializeField]
    private float bpm = default; // (beat/min)
    [ReadOnly] [SerializeField]
    private float timeBetweenBeats = default; // (s/beat)


    public delegate void onBeat();
    public static event onBeat OnBeat;
    public delegate void offBeat();
    public static event offBeat OffBeat;
    public delegate void newBar();
    public static event newBar NewBar;
    public delegate void onBeatLast();
    public static event onBeatLast OnBeatLast;
    public delegate void offBeatLast();
    public static event offBeatLast OffBeatLast;

    public bool active;

    [HorizontalLine]
    public bool debugMode;

    [ShowIf("debugMode")] [SerializeField]
    public bool showTiming = default;

    [ShowIf("debugMode")]  [SerializeField]
    public bool visualizeBeat = default;

    [ShowIf(EConditionOperator.And,"debugMode","visualizeBeat")] [SerializeField]
    public Transform beatGraphic = default;


    private float lastBeat = 0; // (s)
    private float pauseOffset = 1; // (s)

    void Awake()
    {
        active = false;
        ApplyBPM();
        previousOffBeats = offBeats;
        previousTimeBetweenBeats = timeBetweenBeats;
        active = true;
    }

    void Start()
    {
        if (instance == null)
            instance = this;
        else
            DestroyImmediate(this);
    }

    void OnEnable()
    {
        InvokeRepeating("Beat", pauseOffset, timeBetweenBeats / (offBeats + 1));
        if (showTiming) Debug.Log("Enabling Metronome\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
    }

    void OnDisable()
    {
        pauseOffset = (timeBetweenBeats/(offBeats+1)) - (Time.time-lastBeat);
        CancelInvoke();
        if (showTiming) Debug.Log("Disabling Metronome\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
    }

    protected int lastMainIndex = int.MaxValue-1;
    protected int lastOffIndex = int.MaxValue-1;

    protected void Beat () // Sequences beats in the propper order and triggers their events. Invoked by OnEnable()
    {
        lastBeat = Time.time;

        if (showTiming) Debug.Log(lastMainIndex + "-" + lastOffIndex + ":\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
        if (visualizeBeat && beatGraphic) if (beatGraphic.localScale == Vector3.one) beatGraphic.localScale *= 2; else beatGraphic.localScale = Vector3.one; // Displays the beat on-screen directly

        if (++lastOffIndex > offBeats) // Next Main Beat
        {
            lastOffIndex = 0;
            if (++lastMainIndex > beats) // Next Bar
            {
                lastMainIndex = 0;
                NewBar?.Invoke();
            }
            OnBeat?.Invoke();
            if (lastMainIndex == beats)
                OnBeatLast?.Invoke();
        }
        else // Next Off Beat
        {
            OffBeat?.Invoke();
            if (lastOffIndex == offBeats/* && lastMainIndex == beats*/)
                OffBeatLast?.Invoke();
        }
    }

    public bool SetBPM(float newBPM) // Public method for other classes to change the obpm
    {
        if (newBPM < 30 || newBPM > 240)
            return false;

        bpm = newBPM;
        ApplyBPM();
        return true;
    }

    protected float previousTimeBetweenBeats = 0;
    protected void ApplyBPM() // NaughtyAttributes handler for changing beats during runtime through the Unity Editor
    {
        bpm = Mathf.Clamp(bpm, 30, 240);
        timeBetweenBeats = (60 / bpm);

        if (UnityEditor.EditorApplication.isPlaying && active)
        {
            CancelInvoke();
            float offset = (previousTimeBetweenBeats / (offBeats + 1)) - (Time.time - lastBeat);
            InvokeRepeating("Beat", offset, timeBetweenBeats / (offBeats + 1));
        }
        previousTimeBetweenBeats = timeBetweenBeats;
    }

    public bool SetBeats (int n) // Public method for other classes to change the beat count
    {
        if (n < 1)
            return false;

        beats = n;
        return true;
    }

    public bool SetOffBeats(int n) // Public method for other classes to change the offbeat count
    {
        if (n < 0 || n > 4)
            return false;

        offBeats = n;
        ApplyOffBeats();
        return true;
    }

    protected int previousOffBeats = 0;
    public void ApplyOffBeats() // NaughtyAttributes handler for changing offbeats during runtime through the Unity Editor
    {
        if (!UnityEditor.EditorApplication.isPlaying)
            return;

        offBeats = Mathf.Clamp(offBeats, 0, 4);

        CancelInvoke();
        float offset = (timeBetweenBeats / (previousOffBeats + 1)) - (Time.time - lastBeat);
        InvokeRepeating("Beat", offset, timeBetweenBeats / (offBeats+1));
        previousOffBeats = offBeats;
    }
}


public enum BeatType
{
    OnBeat = 1,
    OffBeat = 2,
    NewBar = 3,
    OnBeatLast = 4,
    OffBeatLast = 5,
}


/*public enum BeatType
{
    OnBeat1 = 0,
    OffBeat1 = 1,
    OffBeat1b = 2,
    OffBeat1c = 3,
    OffBeat1d = 4,
    OnBeat2 = 5,
    OffBeat2 = 6,
    OffBeat2b = 7,
    OffBeat2c = 8,
    OffBeat2d = 9,
    OnBeat3 = 10,
    OffBeat3 = 11,
    OffBeat3b = 12,
    OffBeat3c = 13,
    OffBeat3d = 14,
    OnBeat4 = 15,
    OffBeat4 = 16,
    OffBeat4b = 17,
    OffBeat4c = 18,
    OffBeat4d = 19,
    OnBeat5 = 20,
    OffBeat5 = 21,
    OffBeat5b = 22,
    OffBeat5c = 23,
    OffBeat5d = 24,
    OnBeat6 = 25,
    OffBeat6 = 26,
    OffBeat6b = 27,
    OffBeat6c = 28,
    OffBeat6d = 29,
    OnBeat7 = 30,
    OffBeat7 = 31,
    OffBeat7b = 32,
    OffBeat7c = 33,
    OffBeat7d = 34,
    OnBeat8 = 35,
    OffBeat8 = 36,
    OffBeat84b = 37,
    OffBeat8c = 38,
    OffBeat8d = 39,
    _______ = 40,
    OnBeat = 41,
    OffBeat = 42,
    OnBeatLast = 43,
    OffBeatLast = 44,
    OffBeata = 45,
    OffBeatb = 46,
    OffBeatc = 47,
    OffBeatd = 48,
}*/