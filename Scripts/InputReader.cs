using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, InputControls.IPCActions
{
    private InputControls _controls;
    public event System.Action OnGoLeftEvent;
    public event System.Action OnGoRightEvent;
    public event System.Action OnRollEvent;
    public event System.Action OnJumpEvent;
    private void Awake()
    {
        _controls = new InputControls();
        _controls.PC.SetCallbacks(this);
        _controls.PC.Enable();
    }
    private void OnEnable()
    {
        GameManager.Instance.OnGameEnd += HandleOnGameEnd;
    }
    private void OnDisable()
    {
        _controls.PC.Disable();
    }
    private void HandleOnGameEnd()
    {
        _controls.PC.Disable();
    }
    public void OnGoLeft(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            OnGoLeftEvent?.Invoke();
        }
    }

    public void OnGoRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnGoRightEvent?.Invoke();
        }
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnRollEvent?.Invoke();
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!GameManager.Instance.IsGameStarted)
        {
            GameManager.Instance.StartIntro();
            return;
        }
        if (context.performed)
        {
            OnJumpEvent?.Invoke();
        }
    }
}
