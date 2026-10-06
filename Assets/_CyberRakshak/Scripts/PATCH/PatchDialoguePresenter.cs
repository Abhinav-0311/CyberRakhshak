using System.Collections;
using System.Collections.Generic;
using CyberRakshak.Platformer;
using CyberRakshak.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CyberRakshak.PATCH
{
    public sealed class PatchDialoguePresenter : MonoBehaviour
    {
        [System.Serializable]
        public struct DialogueMessage
        {
            [SerializeField] private string speaker;
            [SerializeField, TextArea(2, 5)] private string text;
            [SerializeField] private float displaySeconds;

            public DialogueMessage(string speaker, string text, float displaySeconds)
            {
                this.speaker = speaker;
                this.text = text;
                this.displaySeconds = displaySeconds;
            }

            public string Speaker => speaker;
            public string Text => text;
            public float DisplaySeconds => displaySeconds;
        }

        [SerializeField] private CanvasGroup panel;
        [SerializeField] private TMP_Text speakerLabel;
        [SerializeField] private TMP_Text bodyLabel;
        [SerializeField] private TMP_Text continueLabel;
        [SerializeField] private Image characterImage;
        [SerializeField] private RectTransform bubbleRect;
        [SerializeField] private Image bubbleTail;
        [SerializeField] private Button continueButton;
        [SerializeField] private Sprite bubbleSprite;
        [SerializeField] private GameObject narrativeStage;
        [SerializeField] private Image jayPortrait;
        [SerializeField] private Image adiPortrait;
        [SerializeField] private float fadeSeconds = 0.2f;
        [SerializeField, Min(1f)] private float charactersPerSecond = 45f;

        private Coroutine activeRoutine;
        private bool conversation;
        private bool officeConversation;
        private bool advanceRequested;
        private GameplayPauseController pauseController;
        private AudioSource dialogueVoice;
        private readonly HashSet<string> shownHints = new HashSet<string>();

        public bool IsShowing => activeRoutine != null;
        public bool IsBlocking => conversation && IsShowing;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterTutorialGuidance()
        {
            SceneManager.sceneLoaded -= ShowTutorialGuidance;
            SceneManager.sceneLoaded += ShowTutorialGuidance;
        }

        public static PatchDialoguePresenter Ensure()
        {
            PatchDialoguePresenter presenter = FindFirstObjectByType<PatchDialoguePresenter>();
            if (presenter != null)
            {
                return presenter;
            }

            var prefab = Resources.Load<GameObject>("UI/PatchDialoguePanel");
            return prefab != null ? Instantiate(prefab).GetComponent<PatchDialoguePresenter>() :
                new GameObject("PATCH_DialogueUI", typeof(RectTransform)).AddComponent<PatchDialoguePresenter>();
        }

        private static void ShowTutorialGuidance(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Game_Tutorial")
            {
                var onboarding = Resources.Load<PatchDialogueSequence>("Dialogue/TutorialOnboarding");
                if (onboarding != null) Ensure().ShowSequence(onboarding);
                else Debug.LogError("Tutorial onboarding dialogue asset is missing.");
            }
            else if (scene.name == "Game_Level01")
            {
                Ensure().Show("PATCH", "Clear the patrol: jump onto the spacemen from above. Side contact damages your integrity. Then use the launch pad to reach the water control.", 9f);
            }
            else if (scene.name == "Game_Level02")
            {
                var intro = Resources.Load<PatchDialogueLine>("Dialogue/PhishingMazeIntro");
                if (intro != null) Ensure().Show(intro);
                else Debug.LogError("Phishing Maze introduction dialogue asset is missing.");
            }
        }

        private void Awake()
        {
            dialogueVoice = gameObject.AddComponent<AudioSource>();
            dialogueVoice.playOnAwake = false;
            dialogueVoice.spatialBlend = 0f;
            if (panel == null || speakerLabel == null || bodyLabel == null || continueButton == null)
            {
                if (panel != null) Destroy(panel.gameObject);
                var template = Resources.Load<GameObject>("UI/PatchDialoguePanel");
                if (bubbleSprite == null && template != null)
                    bubbleSprite = template.GetComponent<PatchDialoguePresenter>().bubbleSprite;
                BuildDefaultPanel();
            }
            if (narrativeStage == null) BuildNarrativeStage();
            narrativeStage.SetActive(false);
            if (continueButton != null) continueButton.onClick.AddListener(Advance);
            panel.alpha = 0f;
            panel.blocksRaycasts = false;
        }

        private void Update()
        {
            if (IsShowing && Input.GetKeyDown(KeyCode.Return)) Advance();
        }

        public void Advance()
        {
            if (!IsShowing || (pauseController != null && pauseController.IsPaused)) return;
            if (bodyLabel.maxVisibleCharacters < bodyLabel.textInfo.characterCount)
            {
                bodyLabel.maxVisibleCharacters = bodyLabel.textInfo.characterCount;
                dialogueVoice.Stop();
            }
            else
                advanceRequested = true;
        }

        public void Hide()
        {
            if (dialogueVoice != null) dialogueVoice.Stop();
            if (activeRoutine != null) StopCoroutine(activeRoutine);
            activeRoutine = null;
            if (conversation)
            {
                if (pauseController != null && pauseController.isActiveAndEnabled)
                    pauseController.SetDialogueBlocking(false);
                else Time.timeScale = 1f;
            }
            conversation = false;
            officeConversation = false;
            if (narrativeStage != null) narrativeStage.SetActive(false);
            if (panel == null) return;
            panel.alpha = 0f;
            panel.interactable = panel.blocksRaycasts = false;
        }

        private void OnDisable() => Hide();

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
            // Gameplay hints must not interrupt an input-blocking story conversation.
            if (IsBlocking) return;
            Hide();
            pauseController = FindFirstObjectByType<GameplayPauseController>();
            activeRoutine = StartCoroutine(ShowRoutine(speaker, text, displaySeconds));
        }

        public bool ShowOnce(string key, string speaker, string text, float displaySeconds = 4f)
        {
            if (IsBlocking || !shownHints.Add(key)) return false;
            Show(speaker, text, displaySeconds);
            return true;
        }

        public void ShowSequence(params DialogueMessage[] messages) => StartSequence(messages, false);

        public void ShowSequence(PatchDialogueSequence sequence)
        {
            if (sequence != null) StartSequence(sequence.Messages, sequence.UseOfficeBackdrop);
        }

        private void StartSequence(DialogueMessage[] messages, bool office)
        {
            if (messages == null || messages.Length == 0)
            {
                return;
            }

            Hide();
            conversation = true;
            officeConversation = office;
            if (narrativeStage != null) narrativeStage.SetActive(office);
            pauseController = FindFirstObjectByType<GameplayPauseController>();
            if (pauseController != null) pauseController.SetDialogueBlocking(true);
            activeRoutine = StartCoroutine(ShowSequenceRoutine(messages));
        }

        private IEnumerator ShowRoutine(string speaker, string text, float displaySeconds)
        {
            SetText(speaker, text);
            yield return FadeTo(1f);
            yield return RevealText();
            yield return WaitForReading(text, displaySeconds);
            yield return FadeTo(0f);
            activeRoutine = null;
        }

        private IEnumerator ShowSequenceRoutine(DialogueMessage[] messages)
        {
            foreach (DialogueMessage message in messages)
            {
                SetText(message.Speaker, message.Text);
                yield return FadeTo(1f);
                yield return RevealText();
                yield return WaitForReading(message.Text, message.DisplaySeconds);
                yield return FadeTo(0f);
            }
            Hide();
        }

        private IEnumerator RevealText()
        {
            float revealed = 0f;
            float nextSyllableTime = 0f;
            int length = bodyLabel.textInfo.characterCount;
            while (bodyLabel.maxVisibleCharacters < length)
            {
                if (pauseController == null || !pauseController.IsPaused)
                {
                    int previous = bodyLabel.maxVisibleCharacters;
                    revealed += Time.unscaledDeltaTime * charactersPerSecond;
                    bodyLabel.maxVisibleCharacters = Mathf.Min(length, Mathf.FloorToInt(revealed));
                    if (speakerLabel.text == "PATCH" && bodyLabel.maxVisibleCharacters > previous &&
                        Time.unscaledTime >= nextSyllableTime)
                    {
                        char character = bodyLabel.textInfo.characterInfo[bodyLabel.maxVisibleCharacters - 1].character;
                        if (!char.IsWhiteSpace(character) && !char.IsPunctuation(character))
                        {
                            PlatformerSfx.PlayPatchChatter(dialogueVoice, character);
                            nextSyllableTime = Time.unscaledTime + .085f;
                        }
                    }
                }
                else dialogueVoice.Stop();
                yield return null;
            }
            dialogueVoice.Stop();
            if (continueLabel != null)
                continueLabel.text = conversation ? "CLICK / ENTER TO CONTINUE" : "ENTER TO DISMISS";
        }

        private IEnumerator WaitForReading(string text, float minimumSeconds)
        {
            float duration = Mathf.Max(minimumSeconds, 1f + (text ?? string.Empty).Split(' ').Length / 3f);
            for (float elapsed = 0f; conversation || elapsed < duration; elapsed += Time.deltaTime)
            {
                if (advanceRequested) break;
                yield return null;
            }
        }

        private void SetText(string speaker, string text)
        {
            if (panel != null)
            {
                bool right = speaker == "ADI";
                RectTransform rect = panel.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(officeConversation ? .05f : right ? .12f : .04f, .04f);
                rect.anchorMax = new Vector2(officeConversation ? .95f : right ? .96f : .88f, officeConversation ? .26f : .33f);
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                bool patch = string.IsNullOrWhiteSpace(speaker) || speaker == "PATCH";
                if (characterImage != null) characterImage.gameObject.SetActive(patch);
                if (bubbleTail != null) bubbleTail.gameObject.SetActive(patch);
                if (bubbleRect != null)
                {
                    bubbleRect.anchorMin = new Vector2(patch ? .24f : .05f, .08f);
                    bubbleRect.anchorMax = new Vector2(.99f, .88f);
                }
                if (officeConversation)
                {
                    jayPortrait.color = speaker == "JAY" ? Color.white : new Color(.6f, .65f, .72f, 1f);
                    adiPortrait.color = right ? Color.white : new Color(.6f, .65f, .72f, 1f);
                }
            }
            if (speakerLabel != null)
            {
                speakerLabel.text = string.IsNullOrWhiteSpace(speaker) ? "PATCH" : speaker;
                var badge = (RectTransform)speakerLabel.transform.parent;
                badge.anchorMin = new Vector2(speaker == "ADI" ? .71f : .045f, .88f);
                badge.anchorMax = new Vector2(speaker == "ADI" ? .955f : .29f, 1.12f);
            }

            if (bodyLabel != null)
            {
                bodyLabel.text = text ?? string.Empty;
                bodyLabel.maxVisibleCharacters = int.MaxValue;
                bodyLabel.ForceMeshUpdate();
                bodyLabel.maxVisibleCharacters = 0;
            }
            advanceRequested = false;
            if (continueLabel != null) continueLabel.text = "ENTER TO REVEAL";
        }

        private IEnumerator FadeTo(float targetAlpha)
        {
            if (panel == null)
            {
                yield break;
            }

            float startAlpha = panel.alpha;
            float elapsed = 0f;
            RectTransform rect = panel.GetComponent<RectTransform>();
            float direction = speakerLabel.text == "ADI" ? 1f : -1f;

            while (elapsed < fadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / Mathf.Max(.01f, fadeSeconds));
                panel.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress * (2f - progress));
                rect.anchoredPosition = Vector2.right * direction * (1f - panel.alpha) * 80f;
                yield return null;
            }

            panel.alpha = targetAlpha;
            rect.anchoredPosition = Vector2.right * direction * (1f - targetAlpha) * 80f;
            panel.interactable = targetAlpha > 0f;
            panel.blocksRaycasts = targetAlpha > 0f && conversation;
        }

        private void BuildDefaultPanel()
        {
            Canvas canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                gameObject.AddComponent<CanvasScaler>();
                gameObject.AddComponent<GraphicRaycaster>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;
            CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>() ?? gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = .5f;

            BuildNarrativeStage();

            GameObject panelObject = new GameObject("Panel", typeof(RectTransform), typeof(CanvasGroup));
            panelObject.transform.SetParent(transform, false);
            panel = panelObject.GetComponent<CanvasGroup>();
            RectTransform panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = new Vector2(.04f, .04f);
            panelRect.anchorMax = new Vector2(.88f, .33f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var portrait = new GameObject("CharacterPortrait", typeof(RectTransform), typeof(Image));
            portrait.transform.SetParent(panel.transform, false);
            characterImage = portrait.GetComponent<Image>();
            characterImage.sprite = Resources.Load<Sprite>("UI/PatchPortrait");
            characterImage.preserveAspect = true;
            characterImage.raycastTarget = false;
            characterImage.rectTransform.anchorMin = new Vector2(0f, -.02f);
            characterImage.rectTransform.anchorMax = new Vector2(.25f, 1.25f);
            characterImage.rectTransform.offsetMin = characterImage.rectTransform.offsetMax = Vector2.zero;

            var tail = new GameObject("BubbleTail", typeof(RectTransform), typeof(Image), typeof(Outline));
            tail.transform.SetParent(panel.transform, false);
            bubbleTail = tail.GetComponent<Image>();
            bubbleTail.color = new Color(.97f, .985f, 1f, 1f);
            bubbleTail.raycastTarget = false;
            bubbleTail.rectTransform.anchorMin = bubbleTail.rectTransform.anchorMax = new Vector2(.24f, .45f);
            bubbleTail.rectTransform.sizeDelta = new Vector2(42f, 42f);
            bubbleTail.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            tail.GetComponent<Outline>().effectColor = new Color(.05f, .80f, 1f, 1f);
            tail.GetComponent<Outline>().effectDistance = new Vector2(3f, -3f);

            var bubble = new GameObject("SpeechBubble", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Button));
            bubble.transform.SetParent(panel.transform, false);
            var background = bubble.GetComponent<Image>();
            background.sprite = bubbleSprite;
            background.type = Image.Type.Sliced;
            background.pixelsPerUnitMultiplier = .25f;
            background.color = new Color(.97f, .985f, 1f, 1f);
            bubble.GetComponent<Outline>().effectColor = new Color(.05f, .80f, 1f, 1f);
            bubble.GetComponent<Outline>().effectDistance = new Vector2(4f, -4f);
            bubbleRect = background.rectTransform;
            bubbleRect.anchorMin = new Vector2(.24f, .08f);
            bubbleRect.anchorMax = new Vector2(.99f, .88f);
            bubbleRect.offsetMin = bubbleRect.offsetMax = Vector2.zero;
            continueButton = bubble.GetComponent<Button>();
            continueButton.targetGraphic = background;
            continueButton.transition = Selectable.Transition.None;
            continueButton.navigation = new Navigation { mode = Navigation.Mode.None };

            var badge = new GameObject("SpeakerBadge", typeof(RectTransform), typeof(Image), typeof(Outline));
            badge.transform.SetParent(bubble.transform, false);
            var badgeImage = badge.GetComponent<Image>();
            badgeImage.sprite = bubbleSprite;
            badgeImage.type = Image.Type.Sliced;
            badgeImage.pixelsPerUnitMultiplier = .5f;
            badgeImage.color = new Color(.025f, .055f, .16f, 1f);
            badgeImage.raycastTarget = false;
            badgeImage.rectTransform.anchorMin = new Vector2(.045f, .88f);
            badgeImage.rectTransform.anchorMax = new Vector2(.29f, 1.12f);
            badgeImage.rectTransform.offsetMin = badgeImage.rectTransform.offsetMax = Vector2.zero;
            badge.GetComponent<Outline>().effectColor = new Color(.05f, .80f, 1f, 1f);
            badge.GetComponent<Outline>().effectDistance = new Vector2(3f, -3f);
            speakerLabel = CreateLabel("Speaker", badge.transform, Vector2.zero, Vector2.one, 30f, new Color(.10f, .85f, 1f, 1f));
            speakerLabel.alignment = TextAlignmentOptions.Center;
            speakerLabel.fontStyle = FontStyles.Bold;
            bodyLabel = CreateLabel("Message", bubble.transform, new Vector2(.045f, .25f), new Vector2(.955f, .86f), 34f, new Color(.025f, .045f, .10f, 1f));
            bodyLabel.textWrappingMode = TextWrappingModes.Normal;
            bodyLabel.enableAutoSizing = true;
            bodyLabel.fontSizeMin = 28f;
            bodyLabel.fontSizeMax = 34f;
            continueLabel = CreateLabel("ContinueHint", bubble.transform, new Vector2(.045f, .04f), new Vector2(.955f, .18f), 19f, new Color(.25f, .35f, .48f, 1f));
            continueLabel.text = "CLICK / ENTER TO CONTINUE";
            continueLabel.alignment = TextAlignmentOptions.BottomRight;
            panel.alpha = 0f;
            panel.interactable = false;
            panel.blocksRaycasts = false;
        }

        private void BuildNarrativeStage()
        {
            narrativeStage = new GameObject("NarrativeStage", typeof(RectTransform));
            narrativeStage.transform.SetParent(transform, false);
            narrativeStage.transform.SetAsFirstSibling();
            var rect = (RectTransform)narrativeStage.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var backdrop = CreateNarrativeImage("OfficeBackdrop", "UI/OfficeBackdrop", Vector2.zero, Vector2.one);
            backdrop.preserveAspect = false;
            backdrop.raycastTarget = true;
            jayPortrait = CreateNarrativeImage("JayPortrait", "UI/JayPortrait", new Vector2(.07f, .15f), new Vector2(.40f, .97f));
            adiPortrait = CreateNarrativeImage("AdiPortrait", "UI/AdiPortrait", new Vector2(.61f, .15f), new Vector2(.94f, .97f));
            narrativeStage.SetActive(false);
        }

        private Image CreateNarrativeImage(string name, string resource, Vector2 minimum, Vector2 maximum)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(narrativeStage.transform, false);
            var image = obj.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>(resource);
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.rectTransform.anchorMin = minimum;
            image.rectTransform.anchorMax = maximum;
            image.rectTransform.offsetMin = image.rectTransform.offsetMax = Vector2.zero;
            return image;
        }

        private TextMeshProUGUI CreateLabel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, float fontSize, Color color)
        {
            GameObject label = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            label.transform.SetParent(parent, false);
            RectTransform rect = label.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            TextMeshProUGUI text = label.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;
            return text;
        }
    }
}

