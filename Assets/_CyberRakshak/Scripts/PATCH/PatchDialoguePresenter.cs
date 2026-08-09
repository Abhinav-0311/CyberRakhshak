using System.Collections;
using TMPro;
using UnityEngine;

namespace CyberRakshak.PATCH
{
    public sealed class PatchDialoguePresenter : MonoBehaviour
    {
        [SerializeField] private CanvasGroup panel;
        [SerializeField] private TMP_Text speakerLabel;
        [SerializeField] private TMP_Text bodyLabel;
        [SerializeField] private float fadeSeconds = 0.12f;

        private Coroutine activeRoutine;

        public void Show(PatchDialogueLine line)
        {
            if (line == null)
            {
                return;
            }

            Show(line.Speaker, line.Text, line.DisplaySeconds);
        }

        public void Show(string speaker, string text, float displaySeconds = 4f)
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
            }

            activeRoutine = StartCoroutine(ShowRoutine(speaker, text, displaySeconds));
        }

        private IEnumerator ShowRoutine(string speaker, string text, float displaySeconds)
        {
            SetText(speaker, text);
            yield return FadeTo(1f);
            yield return new WaitForSeconds(displaySeconds);
            yield return FadeTo(0f);
            activeRoutine = null;
        }

        private void SetText(string speaker, string text)
        {
            if (speakerLabel != null)
            {
                speakerLabel.text = string.IsNullOrWhiteSpace(speaker) ? "PATCH" : speaker;
            }

            if (bodyLabel != null)
            {
                bodyLabel.text = text;
            }
        }

        private IEnumerator FadeTo(float targetAlpha)
        {
            if (panel == null)
            {
                yield break;
            }

            float startAlpha = panel.alpha;
            float elapsed = 0f;

            while (elapsed < fadeSeconds)
            {
                elapsed += Time.deltaTime;
                panel.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeSeconds);
                yield return null;
            }

            panel.alpha = targetAlpha;
            panel.interactable = targetAlpha > 0.01f;
            panel.blocksRaycasts = targetAlpha > 0.01f;
        }
    }
}

