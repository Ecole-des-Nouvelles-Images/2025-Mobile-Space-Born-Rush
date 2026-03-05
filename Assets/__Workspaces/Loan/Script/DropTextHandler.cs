using System;
using TMPro;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.Serialization;

public class DropTextHandler : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI _textMesh;
    [FormerlySerializedAs("_isTakeDrops")] public bool IsTakeDrops;
    [SerializeField]private float _popUpTimer;
    [SerializeField]private float _popUpDuration = 3f;
    [SerializeField] private float _appearRatio = 0.3f; 
    [SerializeField]private float _maxScale = 1.5f;
    [SerializeField]private float _minScale = 0f;

    private void Update()
    {
        if (IsTakeDrops)
        {
            _popUpTimer += Time.deltaTime;

            float appearDuration = _popUpDuration * _appearRatio;
            float disappearDuration = _popUpDuration - appearDuration;

            if (_popUpTimer <= appearDuration)
            {
                // Phase de grandissement (plus rapide)
                float t = _popUpTimer / appearDuration;
                float scale = Mathf.Lerp(_minScale, _maxScale, t);
                _textMesh.transform.localScale = Vector3.one * scale;
            }
            else if (_popUpTimer <= _popUpDuration)
            {
                // Phase de rétrécissement (plus lente)
                float t = (_popUpTimer - appearDuration) / disappearDuration;
                float scale = Mathf.Lerp(_maxScale, _minScale, t);
                _textMesh.transform.localScale = Vector3.one * scale;
            }
            else
            {
                // Fin de l'animation
                _textMesh.transform.localScale = Vector3.zero;
                IsTakeDrops = false;
                _popUpTimer = 0f;
            }
        }
    }
}
