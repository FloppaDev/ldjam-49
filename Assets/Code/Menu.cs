using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;

public class Menu : MonoBehaviour
{

    bool _anyKey;

    void Start() {
        // new Input System shenans
        // https://forum.unity.com/threads/check-if-any-key-is-pressed.763751/
        InputSystem.onEvent += (eventPtr, device) => {
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
                return;
            var controls = device.allControls;
            var buttonPressPoint = InputSystem.settings.defaultButtonPressPoint;
            for (var i = 0; i < controls.Count; ++i) {
                var control = controls[i] as ButtonControl;
                if (control == null || control.synthetic || control.noisy)
                    continue;
                if (control.ReadValueFromEvent(eventPtr, out var value) && value >= buttonPressPoint) {
                    _anyKey = true;
                    break;
                }
            }
        };
    }

    void Update()
    {
        if (_anyKey) Level.Next();
    }

}
