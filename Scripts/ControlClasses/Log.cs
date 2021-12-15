using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Log
{
    public static void AddEntry(string entry)
    {
        MainManager.Instance.EventsLog.AddEntry(entry);
    }
}
