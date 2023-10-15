using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldCurveManager : MonoBehaviour
{
    [SerializeField] private float _xMaxBend = 0.009f;
    [SerializeField] private float _xMinBend = 0.004f;
    [SerializeField] private float _yMaxBend = 0.0f;
    [SerializeField] private float _yMinBend = -0.009f;
    [SerializeField] private float _changeBendMaxTime = 10f;
    [SerializeField] private float _changeBendMinTime = 5f;
    [SerializeField] private Material[] _curvedMaterials;
    private float _timeCounter = 0f;
    private float _changeBendTime;
    private bool _isBendToRight;
    private bool _isInTransition = false;
    [ContextMenu("ResetCurve")]
    private void ResetCurve()
    {
        foreach (Material mat in _curvedMaterials)
        {
            mat.SetFloat(Shader.PropertyToID("_X_Bend"), 0f);
            mat.SetFloat(Shader.PropertyToID("_Y_Bend"), 0f);
        }
    }
    [ContextMenu("SetStartCurve")]
    private void SetStartCurve()
    {
        foreach (Material mat in _curvedMaterials)
        {
            mat.SetFloat(Shader.PropertyToID("_X_Bend"), 0.001f);
            mat.SetFloat(Shader.PropertyToID("_Y_Bend"), -0.0008f);
        }
    }
    private void Start()
    {
        _isBendToRight = true;
        _changeBendTime = Random.Range(_changeBendMinTime, _changeBendMaxTime);
    }
    private void Update()
    {
        if (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameEnded) return;
        if (_isInTransition) return;
        _timeCounter += Time.deltaTime;
        if (_timeCounter > _changeBendTime)
        {
            float newX = Random.Range(_xMinBend, _xMaxBend);
            float newY = Random.Range(_yMinBend, _yMaxBend);
            if(Random.Range(0,2) == 1)
            {
                newX = -newX;
            }
            StartCoroutine(BendLerp(newX, newY, 0.18f));
            _isBendToRight = !_isBendToRight;
            _changeBendTime = Random.Range(_changeBendMinTime, _changeBendMaxTime);
            _timeCounter = 0;
        }
    }

    private void SetCurveValues(float bendX, float bendY)
    {
        foreach (Material mat in _curvedMaterials)
        {
            mat.SetFloat(Shader.PropertyToID("_X_Bend"), bendX);
            mat.SetFloat(Shader.PropertyToID("_Y_Bend"), bendY);
        }
    }
    private IEnumerator BendLerp(float bendX, float bendY, float lerpSpeed)
    {
        _isInTransition = true;
        float xPrewBend = _curvedMaterials[0].GetFloat(Shader.PropertyToID("_X_Bend"));
        float yPrewBend = _curvedMaterials[0].GetFloat(Shader.PropertyToID("_Y_Bend"));
        float howFar = 0f;
        do
        {
            howFar += Time.deltaTime * lerpSpeed;
            if (howFar > 1f)
                howFar = 1f;
            float xNewBend = Mathf.Lerp(xPrewBend, bendX, howFar);
            float yNewBend = Mathf.Lerp(yPrewBend, bendY, howFar);
            SetCurveValues(xNewBend, yNewBend);
            if (GameManager.Instance.IsGameEnded)
                yield break;
            yield return null;
        }
        while (howFar != 1f);
        _isInTransition = false;
    }
}
