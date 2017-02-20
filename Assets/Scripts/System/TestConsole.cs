using UnityEngine;
using System.Collections;

public class TestConsole : MonoBehaviour
{
    private bool consoleishidden;
    private string output;
    private string stack;
    public GUISkin consoleskin;
    private Vector2 scroll;

    void Start()
    {

    }
    void Update(){
        Application.RegisterLogCallback(HandleLog);
        if (Input.GetKeyDown("`") && Input.GetKey("left ctrl")){
            ShowHideConsole();
        }
    }
    void OnGUI()
    {
        GUI.skin = consoleskin;
        GUI.depth = -10000;
        if (consoleishidden)
        {
            ShowConsole();
        }
    }
    public void ShowHideConsole()
    {
        if (consoleishidden)
        {
            consoleishidden = false;
        }
        else
        {
            consoleishidden = true;
        }
    }
    void ShowConsole()
    {
        GUILayout.BeginArea(new Rect(0, 5, Screen.width, Screen.height / 2));
        scroll = GUILayout.BeginScrollView(scroll);
        GUILayout.Label(output);
        //GUILayout.Label(stack);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    void OnEnable()
    {
        Application.RegisterLogCallback(HandleLog);
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if ( stackTrace!= "")
            output += type + ": " + logString + "\n" + stackTrace + "\n";
        else
            output += type + ": " + logString + "\n";
        //stack += stackTrace;
        scroll.y = 10000000000;
    }
}