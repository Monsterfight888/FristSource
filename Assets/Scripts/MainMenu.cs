using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameLauncher Launcher;
    public void Start()
    {
        Pause.paused = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void JoinMatch()
    {
        Launcher.Join();
    }
    public void CreateMatch()
    {
        Launcher.Create();
    }
    public void QuitGame() 
    {
        Application.Quit();
    }
}
