using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string firstLevelSceneName = "Level";
    public CanvasGroup fadePanel;
    public AudioClip musicClip;
    private AudioSource audioSource;

    public void Start()
    {
        audioSource = FindFirstObjectByType<Canvas>().GetComponent<AudioSource>();
        audioSource.PlayOneShot(musicClip);
    }
    public void PlayGame()
    {
        StartCoroutine(Waiting());
        
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OpenOptions()
    {
        // Enable an options panel, or load an Options scene, etc.
        // optionsPanel.SetActive(true);
    }

    private IEnumerator Waiting()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            fadePanel.alpha = (1f-t);  // fade to black
            yield return null;
        }
        SceneManager.LoadScene(firstLevelSceneName);
    }
}