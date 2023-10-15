using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;
using TMPro;
using System;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Vector3 _approachDistance;
    [SerializeField] private float _speed;
    [SerializeField] private Animator _guardAnimator;
    [SerializeField] private Animator _dogAnimator;
    [SerializeField] private Transform _playerTransform;
    private float _howFar;

    private Transform _transform;
    private readonly int _animCaughtHash = Animator.StringToHash("catch");
    private Coroutine _moveCoroutine;
    private AudioSource _audioSource;
    private Vector3 _startLocalPos;
    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _startLocalPos = _transform.localPosition;
        _audioSource = GetComponent<AudioSource>();
        _audioSource.Stop();
    }
    private void OnEnable()
    {
        GameManager.Instance.OnBump += HandleOnPlayerBumped;
        GameManager.Instance.OnLifeRegenerated += HandleOnLifeRegenerated;
        GameManager.Instance.OnLifeless += HandleOnGameEnd;
        GameManager.Instance.OnGameStarted += HandleOnGameStarted;
    }
    private void Start()
    {
        this.gameObject.SetActive(false);
    }
    private void HandleOnGameStarted()
    {
        this.gameObject.SetActive(true);
        StartCoroutine(GameStartAnim());
    }
    private void HandleOnGameEnd()
    {
        StopAllCoroutines();
        _guardAnimator.CrossFadeInFixedTime(_animCaughtHash, 0.1f);
        _dogAnimator.CrossFadeInFixedTime(_animCaughtHash, 0.1f);
        _transform.SetParent(_playerTransform);
        StartCoroutine(MoveToPlayer());
    }
    private void HandleOnPlayerBumped()
    {
        if (GameManager.Instance.IsGameEnded) return;
        StopAllCoroutines();
        _transform.localPosition = _startLocalPos;
        this.gameObject.SetActive(true);
        _audioSource.Play();
        _moveCoroutine = StartCoroutine(MoveForward());
    }
    private void HandleOnLifeRegenerated()
    {
        if (GameManager.Instance.IsGameEnded) return;
        _moveCoroutine = StartCoroutine(MoveBack());
    }
    private IEnumerator GameStartAnim()
    {
        _moveCoroutine = StartCoroutine(MoveForward());
        yield return _moveCoroutine;
        _moveCoroutine = StartCoroutine(MoveBack());
    }
    private IEnumerator MoveForward()
    {
        _howFar = 0f;
        Vector3 to = _transform.localPosition + _approachDistance;
        do
        {
            _howFar += Time.deltaTime * _speed;
            if (_howFar > 1f)
                _howFar = 1f;
            _transform.localPosition = Vector3.Lerp(_transform.localPosition, to, _howFar);
            yield return null;
        }
        while (_howFar != 1f);
    }
    private IEnumerator MoveToPlayer()
    {
        _howFar = 0f;
        do
        {
            _howFar += Time.deltaTime * 1.5f;
            if (_howFar > 1f)
                _howFar = 1f;
            _transform.localPosition = Vector3.Lerp(_transform.localPosition, new Vector3(0, 0.11f, 0.58f), _howFar);
            yield return null;
        }
        while (_howFar != 1f);
    }
    private IEnumerator MoveBack()
    {
        _howFar = 0f;
        Vector3 to = _transform.localPosition - _approachDistance;
        do
        {
            _howFar += Time.deltaTime * _speed;
            if (_howFar > 1f)
                _howFar = 1f;
            _transform.localPosition = Vector3.Lerp(_transform.localPosition, to, _howFar);
            yield return null;
        }
        while (_howFar != 1f);
        _audioSource.Stop();
        this.gameObject.SetActive(false);
    }
}
