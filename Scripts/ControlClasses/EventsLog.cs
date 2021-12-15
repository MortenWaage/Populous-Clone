using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventsLog : MonoBehaviour
{
    private int _entryAmount;

    private List<string> _logEntries;

    void Awake()
    {
        _logEntries = new List<string>();
        StartCoroutine("AwaitEndOfFrame");
    }

    IEnumerator AwaitEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        MainManager.Instance.EventsLog = this;
        Debug.Log($"{Globe.RunOrder} - EventsLog is set");
    }
    public void AddEntry(string entry)
    {
        _entryAmount++;
        _logEntries.Add($"ENTRY::{_entryAmount} - {entry}");
    }
}
