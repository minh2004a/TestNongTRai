using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TinyFarm.Items.UI
{
    public class TabButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Hover Settings")]
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float animationSpeed = 10f;

        [Header("Audio (Optional)")]
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip clickSound;

        private Vector3 normalScale;
        private Vector3 targetScale;
        private Button button;
        private Image image;

        private void Awake()
        {
            button = GetComponent<Button>();
            image = GetComponent<Image>();
            normalScale = transform.localScale;
            targetScale = normalScale;

            // Subscribe click event
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
        }

        private void Update()
        {
            // Smooth scale animation
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                targetScale,
                Time.deltaTime * animationSpeed
            );
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Hover effect
            targetScale = normalScale * hoverScale;

            // Play hover sound
            if (hoverSound != null)
            {
                AudioSource.PlayClipAtPoint(hoverSound, Camera.main.transform.position, 0.5f);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Reset scale
            targetScale = normalScale;
        }

        private void OnButtonClicked()
        {
            // Click animation (punch scale)
            StartCoroutine(PunchScale());

            // Play click sound
            if (clickSound != null)
            {
                AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position, 0.7f);
            }
        }

        private System.Collections.IEnumerator PunchScale()
        {
            float punchAmount = 0.9f;
            float duration = 0.1f;

            Vector3 originalScale = targetScale;
            Vector3 punchScale = originalScale * punchAmount;

            // Scale down
            float elapsed = 0f;
            while (elapsed < duration)
            {
                transform.localScale = Vector3.Lerp(originalScale, punchScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Scale back up
            elapsed = 0f;
            while (elapsed < duration)
            {
                transform.localScale = Vector3.Lerp(punchScale, originalScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localScale = originalScale;
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClicked);
            }
        }
    }
}

