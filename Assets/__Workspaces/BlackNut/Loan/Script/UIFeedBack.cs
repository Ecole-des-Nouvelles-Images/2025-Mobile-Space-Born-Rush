using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIFeedBack : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
{
    private AudioSource _audioSource;

        [Header("Animation Settings")]
        [SerializeField] private bool _playTweening = true;
        [SerializeField] private float _animationTime = 0.3f;
        [SerializeField] private AnimationCurve _animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float _animationEndScale = 1.2f;
        [SerializeField] private bool _isSlimeButton;

        // private void Awake() => _audioSource = GetComponent<AudioSource>();

        private void OnEnable()
        {
            transform.localScale = Vector3.one;

            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
            {
                PlayScaleAnim(_animationEndScale);
            }
        }

        private void OnDisable()
        {
            transform.DOKill();
            transform.localScale = Vector3.one;
        }

        public void OnSelect(BaseEventData eventData) => HandleEnter();
        public void OnPointerEnter(PointerEventData eventData) 
        {
            if (eventData != null) EventSystem.current.SetSelectedGameObject(gameObject);
            HandleEnter();
        }

        private void HandleEnter()
        {
            if (_playTweening) PlayScaleAnim(_animationEndScale);
            // PlaySFX();
        }

        public void OnDeselect(BaseEventData eventData) => HandleExit();
        public void OnPointerExit(PointerEventData eventData) => HandleExit();

        private void HandleExit()
        {
            if (_playTweening) PlayScaleAnim(1f);
        }

        private void PlayScaleAnim(float targetScale)
        {
            transform.DOKill();
            transform.DOScale(targetScale, _animationTime).SetEase(_animationCurve).SetUpdate(true);
        }

        // private void PlaySFX()
        // {
        //     if (AudioStock.Instance == null) return;
        //     _audioSource.clip = _isSlimeButton ? AudioStock.Instance.SlimeButtonClip : AudioStock.Instance.ClassicButtonClip;
        //     _audioSource.Play();
        // }

        public void OnPointerClick(PointerEventData eventData) { /* Logique de clic si besoin */ }
}
