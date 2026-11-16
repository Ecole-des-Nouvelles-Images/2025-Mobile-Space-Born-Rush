using UnityEngine;

/// <summary>
/// Blood pool left on the floor where an enemy dies. A flat transparent quad (no URP decal renderer
/// feature needed, cheap on mobile) that spreads quickly, like a pool forming, then stays on the floor.
/// It is parented to the corridor module, so it scrolls with the floor and is destroyed with it.
/// </summary>
public class BloodDecal : MonoBehaviour
{
    [SerializeField] private Vector2 _sizeRange = new Vector2(1.1f, 1.6f);
    [SerializeField] private float _spreadDuration = 0.45f;
    [Tooltip("Height above the floor, avoids z-fighting.")]
    [SerializeField] private float _floorOffset = 0.015f;

    private float _targetSize;
    private float _time;

    public void Place(Vector3 point, Vector3 normal)
    {
        transform.SetPositionAndRotation(
            point + normal * _floorOffset,
            // Quad faces -Z: lay it on the floor, random spin so pools never look copy-pasted.
            Quaternion.LookRotation(-normal) * Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));

        _targetSize = Random.Range(_sizeRange.x, _sizeRange.y);
        _time = 0f;
        SetSize(_targetSize * 0.25f);
    }

    private void Update()
    {
        if (_time >= _spreadDuration) return;
        _time += Time.deltaTime;
        float t = Mathf.Clamp01(_time / _spreadDuration);
        // Ease-out: fast splash, slow end.
        float eased = 1f - (1f - t) * (1f - t) * (1f - t);
        SetSize(Mathf.Lerp(_targetSize * 0.25f, _targetSize, eased));
    }

    private void SetSize(float size)
    {
        // Compensate the parent's scale so the pool keeps its size in meters.
        Vector3 parentScale = transform.parent ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = new Vector3(size / parentScale.x, size / parentScale.y, size / parentScale.z);
    }
}
