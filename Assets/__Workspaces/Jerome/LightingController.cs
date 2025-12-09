using System;
using UnityEngine;

public class LightingController : MonoBehaviour
{

    // As the players health changes we can change the lighting and play with it to give some nice life.
    // First we get a reference to the players' health.
    private PlayerStatController _playerStats;
    // Then we get a reference to the light we want to play with. Set in the inspector.
    [SerializeField] private Material _lightMaterial;
    // We can keep a reference to the original colour to come back to.
    private Color _originalLightColor;

    private void Start()
    {
        _originalLightColor =  _lightMaterial.color;
    }

    private void Update()
    {
        switch (_playerStats.CurrentHealth)
        {
            case >= 50:
                LerpLighting(_originalLightColor);
                break;
            case <= 50:
                LerpLighting(Color.red);
                break;
        }
    }

    void LerpLighting(Color color)
    {
        //Here we lerp between the current colour and the one passed
        _lightMaterial.color = Color.Lerp(_lightMaterial.color, color, 0.2f);
    }
}
