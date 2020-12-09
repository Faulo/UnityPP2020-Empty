using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputPress : IDisposable
{
    private readonly InputTestFixture input = new InputTestFixture();
    private readonly ButtonControl[] controls;
    public InputPress(params ButtonControl[] controls)
    {
        this.controls = controls;
        foreach (ButtonControl control in controls)
        {
            input.Press(control);
        }
        InputSystem.Update();
    }
    public void Dispose()
    {
        foreach (ButtonControl control in controls)
        {
            input.Release(control);
        }
        InputSystem.Update();
    }
}
