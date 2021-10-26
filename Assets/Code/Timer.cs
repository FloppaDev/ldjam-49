using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{

    [SerializeField] Text _txt;

    void Start() {
        var minutes = Level.s_time / 60;
        var seconds = Level.s_time % 60;
        _txt.text = $"You finished in {minutes}:{seconds}";
    }
}
