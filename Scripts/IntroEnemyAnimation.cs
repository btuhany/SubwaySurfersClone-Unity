using System;
using System.Collections;
using UnityEngine;

public class IntroEnemyAnimation : MonoBehaviour
{
    [SerializeField] private Animator _guardAnimator;
    [SerializeField] private Animator _dogAnimator;

    private readonly int _animIntroHash = Animator.StringToHash("intro");
    private void OnEnable()
    {
        GameManager.Instance.OnIntroStarted += HandleOnIntroStarted;
        GameManager.Instance.OnGameStarted += HandleOnGameStarted;
    }


    private void Start()
    {
        this.gameObject.SetActive(false);
    }
    private void HandleOnGameStarted()
    {
        gameObject.SetActive(false);
    }
    private void HandleOnIntroStarted()
    {
        gameObject.SetActive(true);
        _guardAnimator.CrossFadeInFixedTime(_animIntroHash, 0.05f);
        _dogAnimator.CrossFadeInFixedTime(_animIntroHash, 0.05f);
    }
    
}
