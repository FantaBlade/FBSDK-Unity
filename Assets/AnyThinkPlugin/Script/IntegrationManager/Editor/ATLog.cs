using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ATLog {
    public static void log(string msg) 
    {
        // string msg = 
        Debug.Log(msg);
    }

    public static void log(string tag, string msg)
    {
        Debug.Log(tag + ": " + msg);
    }

    public static void logFormat(string msg, object[] args)
    {   
        Debug.LogFormat(msg, args);
    }

    public static void logError(string msg)
    {
        Debug.LogError(msg);
    }
}