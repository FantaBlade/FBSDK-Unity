using System.Collections.Generic;
using UnityEngine;

public class Analytics : MonoBehaviour
{
    void Start()
    {
        var tapDbAppId = "";
        TapDB.onStart(tapDbAppId, FantaBlade.Api.GetChannel(), null);
    }

    private void OnApplicationQuit()
    {
        TapDB.onStop();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            TapDB.onStop();
        }
        else
        {
            TapDB.onResume();
        }
    }
}