using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{

    bool _menu;
    [SerializeField] GameObject _canvas;
    [SerializeField] Slider _slider;

    public void TogglePause() {
        _menu = !_menu;
        _canvas.SetActive(_menu);

        if (_menu) {
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
            Level.PauseTimer();
        } else {
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1f;
            Level.ResumeTimer();
        }
    }

    public void Quit() {
        Application.Quit();
    }

    public void SetVolume() {
        AudioListener.volume = _slider.value;
    }

    public void Restart() {
        Level.PlayAgain();
    }

}
