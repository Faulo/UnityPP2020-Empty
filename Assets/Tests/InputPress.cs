using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputPress : IDisposable {
    readonly InputTestFixture input;
    readonly ButtonControl[] controls;
    public InputPress(InputTestFixture input, params ButtonControl[] controls) {
        this.input = input;
        this.controls = controls;
        foreach (var control in controls) {
            input.Press(control);
        }
        InputSystem.Update();
    }
    public void Dispose() {
        foreach (var control in controls) {
            input.Release(control);
        }
        InputSystem.Update();
    }
}
