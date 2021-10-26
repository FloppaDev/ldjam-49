using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics;

public static class Level
{
    
    static List<GameObject> s_disables = new List<GameObject>();
    static Stopwatch s_stopwatch = new Stopwatch();
    public static int s_time;


    public static void Disable(GameObject go) {
        go.SetActive(false);        
        s_disables.Add(go);
    }

    public static void Restart() {
        foreach (var go in s_disables) go.SetActive(true);
        s_disables.Clear();

        Player.Restart();
    }

    public static void Next() {
        s_disables.Clear();
        var i = SceneManager.GetActiveScene().buildIndex+1;
        if (i == 1) s_stopwatch.Start();
        else if (i == 5) s_stopwatch.Stop();
        s_time = (int)s_stopwatch.Elapsed.TotalSeconds;
        
        SceneManager.LoadScene(i);
        
    }

    public static void PlayAgain() {
        SceneManager.LoadScene(1);
    }

    public static void PauseTimer() {
        s_stopwatch.Stop();
    }

    public static void ResumeTimer() {
        s_stopwatch.Start();
    }

}
