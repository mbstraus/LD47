using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MusicManager : MonoBehaviour
{
    public int Beat;
    public float TimeBetweenBeats = 0.1f;
    private float elapsedTimeBetweenBeats = 0f;
    public UnityEvent BeatListeners;
    [Tooltip("Time between user input and a beat to allow for the action to happen.  The closer to 0 this is, the harder the game will be.")]
    public float ActionBuffer = 0.1f;
    public bool SendingBeatEvents = true;
    public bool IgnoreBeatMove = false;

    public static MusicManager Instance;

    private void Awake() {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(BeatCounter());
    }

    private void Update() {
        elapsedTimeBetweenBeats += Time.deltaTime;
    }

    public void RegisterBeatEvent(UnityAction unityAction) {
        BeatListeners.AddListener(unityAction);
    }

    public void UnregisterBeatEvent(UnityAction unityAction) {
        BeatListeners.RemoveListener(unityAction);
    }

    public float TimeToClosestBeat() {
        float compareValue = Mathf.Min(elapsedTimeBetweenBeats, TimeBetweenBeats - elapsedTimeBetweenBeats);
        return compareValue;
    }

    public bool IsInputAllowed() {
        if (IgnoreBeatMove) {
            return true;
        }
        float compareValue = TimeToClosestBeat();
        if (TimeToClosestBeat() > ActionBuffer) {
            Debug.Log("IsInputAllowed - Check came back to disallow movement - " + compareValue);
            return false;
        }
        return true;
    }

    IEnumerator BeatCounter() {
        while (true) {
            yield return new WaitForSeconds(TimeBetweenBeats);
            Beat++;
            if (SendingBeatEvents) {
                BeatListeners.Invoke();
            }
            elapsedTimeBetweenBeats = 0f;
        }
    }
}
