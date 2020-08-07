using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metronome : MonoBehaviour
{
    public static Metronome instance;

    public BeatMode mode = BeatMode.Four;
    public OffBeatMode offMode = OffBeatMode.Doubles;


    [OnValueChanged("ApplyBPM")] [MinValue(30),MaxValue(240)] [SerializeField]
    private float bpm = default; // (beat/min)
    [ReadOnly] [SerializeField]
    private float timeBetweenBeats = default; // (s/beat)


    public delegate void OnBeat();
    public static event OnBeat onBeat;
    public delegate void OffBeat();
    public static event OffBeat offBeat;
    public delegate void OnBeat1();
    public static event OnBeat1 onBeat1;


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
        ApplyBPM();
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
        InvokeRepeating("Beat", pauseOffset, timeBetweenBeats / ((int)offMode+1));
        if (showTiming) Debug.Log("Enabling Metronome\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
    }

    void OnDisable()
    {
        pauseOffset = (timeBetweenBeats/((int)offMode+1)) - (Time.time-lastBeat);
        CancelInvoke();
        if (showTiming) Debug.Log("Disabling Metronome\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
    }

    protected int lastMainIndex = int.MaxValue-1;
    protected int lastSubIndex = int.MaxValue-1;

    protected void Beat ()
    {
        lastBeat = Time.time;

        if (++lastSubIndex > (int)offMode)
        {
            lastSubIndex = 0;
            if (++lastMainIndex > (int)mode)
                lastMainIndex = 0;
        }

        TriggerBeat((BeatType)((lastMainIndex * 5) + lastSubIndex));
    }

    void TriggerBeat (BeatType bt)
    {
        if (showTiming) Debug.LogFormat(bt + "\n" + lastMainIndex + "-" + lastSubIndex + ":\t" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
        if (visualizeBeat && beatGraphic) if (beatGraphic.localScale == Vector3.one) beatGraphic.localScale *= 2; else beatGraphic.localScale = Vector3.one;


        if (bt >= BeatType._______)
        {
            Debug.LogWarning("Generic BeatTypes not supported!");
            return;
        }
    }

    

    /*protected void Beat()
    {
        lastBeat = Time.time;

        lastSubIndex++;
        if (lastSubIndex > (int)offMode) // Main Beats
        {
            lastSubIndex = 0;
            lastMainIndex++;

            onBeat?.Invoke();

            if (lastMainIndex > (int)mode) // New Bar
            {
                lastMainIndex = 0;

                onBeat1?.Invoke();
                Debug.Log("Beat1!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
            }
            else
            {
                switch (lastMainIndex)
                {
                    case 1:
                        //onBeat2?.Invoke();
                        Debug.Log("Beat2!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                        break;
                    case 2:
                        //onBeat3?.Invoke();
                        Debug.Log("Beat3!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                        break;
                    case 3:
                        //onBeat4?.Invoke();
                        Debug.Log("Beat4!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                        break;
                    case 4:
                        //onBeat5?.Invoke();
                        Debug.Log("Beat5!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                        break;
                    case 5:
                        //onBeat6?.Invoke();
                        Debug.Log("Beat6!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                        break;
                    case 6:
                        //onBeat7?.Invoke();
                        Debug.Log("Beat7!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                        break;
                    case 7:
                        //onBeat8?.Invoke();
                        Debug.Log("Beat8!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                        break;
                }
            }
        }
        else // Sub Beats
        {
            offBeat?.Invoke();

            //TODO Invert nested switch statements for blanket lettering event calls

            switch (lastMainIndex)
            {
                case 0:
                    switch (lastSubIndex)
                    {
                        case 1:
                            //offBeat1?.Invoke();
                            Debug.Log("OffBeat1!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                            break;
                        case 2:
                            //offBeat1b?.Invoke();
                            Debug.Log("OffBeat1b!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                            break;
                        case 3:
                            //offBeat1c?.Invoke();
                            Debug.Log("OffBeat1c!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                            break;
                        case 4:
                            //offBeat1d?.Invoke();
                            Debug.Log("OffBeat1d!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                            break;
                        default:
                            Debug.LogWarning("Overload not supported!" + lastSubIndex);
                            break;
                    }
                    break;
                case 1:
                    switch (lastSubIndex)
                    {
                        case 1:
                            //offBeat2?.Invoke();
                            Debug.Log("OffBeat2!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                            break;
                        case 2:
                            //offBeat2b?.Invoke();
                            Debug.Log("OffBeat2b!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                            break;
                        case 3:
                            //offBeat2c?.Invoke();
                            Debug.Log("OffBeat2c!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                            break;
                        case 4:
                            //offBeat2d?.Invoke();
                            Debug.Log("OffBeat2d!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                            break;
                    }
                    break;
                case 2:
                    switch (lastSubIndex)
                    {
                        case 1:
                            //offBeat3?.Invoke();
                            Debug.Log("OffBeat3!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                            break;
                        case 2:
                            //offBeat3b?.Invoke();
                            Debug.Log("OffBeat3b!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                            break;
                        case 3:
                            //offBeat3c?.Invoke();
                            Debug.Log("OffBeat3c!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                            break;
                        case 4:
                            //offBeat3d?.Invoke();
                            Debug.Log("OffBeat3d!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                            break;
                    }
                    break;
                case 3:
                    switch (lastSubIndex)
                    {
                        case 1:
                            //offBeat4?.Invoke();
                            Debug.Log("OffBeat4!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                            break;
                        case 2:
                            //offBeat4b?.Invoke();
                            Debug.Log("OffBeat4b!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                            break;
                        case 3:
                            //offBeat4c?.Invoke();
                            Debug.Log("OffBeat4c!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                            break;
                        case 4:
                            //offBeat4d?.Invoke();
                            Debug.Log("OffBeat4d!\n" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond);
                            break;
                    }
                    break;
                default:
                    Debug.LogWarning("Overload not supported!" + lastMainIndex);
                    break;
            }
        }
    }*/




        

    public bool SetBPM(float newBPM)
    {
        if (newBPM < 30 || newBPM > 240)
            return false;

        bpm = newBPM;
        ApplyBPM();
        return true;
    }

    protected void ApplyBPM()
    {
        timeBetweenBeats = (60 / bpm);
    }

    public bool SetBeatMode (BeatMode bm)
    {
        if (bm == mode)
            return false;

        mode = bm;
        return true;
    }

    public bool SetOffBeatMode(OffBeatMode obm)
    {
        if (obm == offMode)
            return false;

        offMode = obm;
        return true;
    }
}

public enum BeatMode
{
    One = 0,
    Two = 1,
    Three = 2,
    Four = 3,
    Five = 4,
    Six = 5,
    Seven = 6,
    Eight = 7
}

public enum OffBeatMode
{
    Singles = 0,
    Doubles = 1,
    Triplets = 2,
    Quadruplets = 3,
    Quintuplets = 4
}

public enum BeatType
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
}


/*public enum BeatType
{
    OnBeat = 0,
    OnBeat1 = 1,
    OnBeat2 = 2,
    OnBeat3 = 3,
    OnBeat4 = 4,
    OnBeat5 = 5,
    OnBeat6 = 6,
    OnBeat7= 7,
    OnBeat8 = 8,
    OnBeatLast = 9,
    OffBeat = 10,
    OffBeat1 = 11,
    OffBeat1b = 12,
    OffBeat1c = 13,
    OffBeat1d = 14,
    OffBeat2 = 15,
    OffBeat2b = 16,
    OffBeat2c = 17,
    OffBeat2d = 18,
    OffBeat3 = 19,
    OffBeat3b = 20,
    OffBeat3c = 21,
    OffBeat3d = 22,
    OffBeat4 = 23,
    OffBeat4b = 24,
    OffBeat4c = 25,
    OffBeat4d = 26,
    OffBeat5 = 27,
    OffBeat5b = 28,
    OffBeat5c = 29,
    OffBeat5d = 30,
    OffBeat6 = 31,
    OffBeat6b = 32,
    OffBeat6c = 33,
    OffBeat6d = 34,
    OffBeat7 = 35,
    OffBeat7b = 36,
    OffBeat7c = 37,
    OffBeat7d = 38,
    OffBeat8 = 39,
    OffBeat84b = 40,
    OffBeat8c = 41,
    OffBeat8d = 42,
    OffBeata = 43,
    OffBeatb = 44,
    OffBeatc = 45,
    OffBeatd = 46,
    OffBeatLast = 47
}*/