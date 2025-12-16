using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SplashScreenManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject panel1;
    [SerializeField] private CanvasGroup panel1CanvasGroup; // CanvasGroup sur Panel 1
    [SerializeField] private GameObject panel2;

    [Header("VFX")]
    [SerializeField] private ParticleSystem loadingPS;

    [Header("Timing")]
    [SerializeField] private float panel1Duration = 5f;
    [SerializeField] private float fadeOutDuration = 0.75f;
    [SerializeField] private float afterLoadedDelay = 3f;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Game";

    private void Awake()
    {
        if (panel1 != null) panel1.SetActive(true);
        if (panel2 != null) panel2.SetActive(false);

        if (panel1CanvasGroup != null)
        {
            panel1CanvasGroup.alpha = 1f;
            panel1CanvasGroup.interactable = false;
            panel1CanvasGroup.blocksRaycasts = false;
        }

        if (loadingPS != null)
        {
            loadingPS.gameObject.SetActive(false);
            loadingPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void Start()
    {
        StartCoroutine(RunSplashFlow());
    }

    private IEnumerator RunSplashFlow()
    {
        yield return new WaitForSeconds(panel1Duration);
        
        if (panel1CanvasGroup != null && fadeOutDuration > 0f)
        {
            float t = 0f;
            float startA = panel1CanvasGroup.alpha;

            while (t < fadeOutDuration)
            {
                t += Time.deltaTime;
                panel1CanvasGroup.alpha = Mathf.Lerp(startA, 0f, t / fadeOutDuration);
                yield return null;
            }
            panel1CanvasGroup.alpha = 0f;
        }

        if (panel1 != null) panel1.SetActive(false);
        
        if (panel2 != null) panel2.SetActive(true);

        if (loadingPS != null)
        {
            loadingPS.gameObject.SetActive(true);
            loadingPS.Play(true);
        }
        
        AsyncOperation op = SceneManager.LoadSceneAsync(gameSceneName);
        op.allowSceneActivation = false;
        
        while (op.progress < 0.9f)
            yield return null;
        
        yield return new WaitForSeconds(afterLoadedDelay);

        op.allowSceneActivation = true;
    }
}
