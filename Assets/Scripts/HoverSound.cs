using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonSound : MonoBehaviour, IPointerEnterHandler
{
    public AudioClip hoverClip;
    public AudioClip clickClip;

    private AudioSource audioSource;
    private Button button;

    private void Awake()
    {
        audioSource = FindFirstObjectByType<Canvas>().GetComponent<AudioSource>();
        button = GetComponent<Button>();

        // Register click event
        if (button != null)
            button.onClick.AddListener(PlayClick);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverClip != null && audioSource != null)
            audioSource.PlayOneShot(hoverClip);
    }

    private void PlayClick()
    {
        if (clickClip != null && audioSource != null)
            audioSource.PlayOneShot(clickClip);
    }
}