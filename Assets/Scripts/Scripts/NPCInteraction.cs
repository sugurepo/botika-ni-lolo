using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class NPCInteraction : MonoBehaviour
{
    [Header("References (drag OR auto-wire by Tag/Name)")]
    public HerbalCureManager cureManager;

    [Header("Journal")]
    [Tooltip("Chapter number (1-5) used to record this chapter's star rating in the journal's Achievements page.")]
    public int chapterNumber = 1;

    [Tooltip("If your pre-cutscene small-talk uses DialogueManager, drag it here so we can stop it when showing result dialogue.")]
    public DialogueManager preTalkDialogue;   // hard-stop before result

    public GameObject talkPrompt;        // Tag: TalkPrompt
    public GameObject givePrompt;        // Tag: GivePrompt

    public GameObject dialoguePanel;     // Tag: DialoguePanel
    public TextMeshProUGUI nameText;     // Tag: NameText
    public TextMeshProUGUI dialogueText; // Tag: DialogueText

    [Header("(Optional) Prompt containers")]
    public Canvas promptsCanvas;
    public Canvas dialogueCanvas;
    public CanvasGroup promptsGroup;
    public CanvasGroup dialogueGroup;

    [Header("Speaker Names")]
    [Tooltip("Leave blank to use NpcIdentity.displayName or SpeakerBinder override.")]
    public string npcDisplayName = "";
    public string playerDisplayName = "You";

    [Header("Speaker Override (optional)")]
    [Tooltip("If assigned, this will override the NPC display name for result dialogue too.")]
    public SpeakerBinder speakerBinder;

    [Header("Dialogue Tree")]
    public ResultDialogue resultDialogue;

    [Header("Fallback Dialogue (optional)")]
    [TextArea] public string[] dialogueUndercooked;
    [TextArea] public string[] dialogueSuccess;
    [TextArea] public string[] dialogueOvercooked;

    [Header("Voice Over (optional)")]
    [Tooltip("AudioSource used to play per-line voice clips. If empty, one is auto-added to the dialoguePanel.")]
    public AudioSource voiceSource;
    [Tooltip("Per-line VO for each outcome. Indices must match the chosen dialogue lines.")]
    public AudioClip[] voicesUndercooked;
    public AudioClip[] voicesSuccess;
    public AudioClip[] voicesOvercooked;

    [Header("Debug/Hardening")]
    public bool autoWire = true;
    public bool verboseLogs = true;
    public bool forceEnableEveryFrame = true;
    public bool disableAnimatorsDuringDialogue = true;

    [Header("External Gating")]
    public bool externalPromptSuppress = false;

    [Header("Outcome Flow")]
    [Tooltip("Scene to load when result is SUCCESS (e.g., CH1_Outro). Fail will use failDecisionPanel instead.")]
    public string successOutroSceneName = "CH1_Outro";
    [Tooltip("Panel shown only if result is undercooked/overcooked.")]
    public ResultDecisionPanel failDecisionPanel;

    // ---------- CH5 (optional) ----------
    [Header("Chapter 5 (optional)")]
    [Tooltip("Hand model shown when holding the EMPTY minibowl (Chapter 5 flow).")]
    public GameObject ch5MiniBowlHandEmpty;
    [Tooltip("Hand model shown when holding the minibowl WITH pounded leaves (Chapter 5 flow).")]
    public GameObject ch5MiniBowlHandWithPounded;

    // ----- State -----
    private bool playerInside = false;
    private bool dialogueActive = false;
    private int dialogueIndex = 0;
    private string[] currentDialogue;
    private AudioClip[] currentVoices; // VO aligned to currentDialogue
    private string _resolvedNpcName = "Aling Rosa";
    private bool _didSortBoost = false;

    private readonly List<Animator> _disabledAnimators = new List<Animator>();

    public bool IsDialogueOpen()
    {
        if (dialogueActive) return true;
        return dialoguePanel && dialoguePanel.activeInHierarchy;
    }

    private void Awake()
    {
        // Base resolve from NpcIdentity or inspector
        var id = GetComponent<NpcIdentity>();
        _resolvedNpcName = string.IsNullOrWhiteSpace(npcDisplayName)
            ? (id ? id.displayName : "Aling Rosa")
            : npcDisplayName;

        // Final override from SpeakerBinder (scene-level)
        if (speakerBinder != null)
        {
            var bound = speakerBinder.ResolveName();
            if (!string.IsNullOrWhiteSpace(bound))
            {
                _resolvedNpcName = bound;
                npcDisplayName   = bound; // keep field in sync
            }
        }

        if (autoWire) TryAutoWireAll();
        EnsureVoiceSource();
    }

    private void Start()
    {
        SafeSetActive(talkPrompt, false);
        SafeSetActive(givePrompt, false);
        SafeSetActive(dialoguePanel, false);
        if (nameText) nameText.gameObject.SetActive(false);

        TryBoostSortingOrder();
        ApplyPromptVisibility();
    }

    private void Update()
    {
        if (dialogueActive)
        {
            HideOnlyPrompts();
            EnsureDialogueUIVisible();

            if (forceEnableEveryFrame)
            {
                HardEnableLabel(nameText);
                HardEnableLabel(dialogueText);
            }

            if (Input.GetKeyDown(KeyCode.Space))
                NextDialogueLine();
            return;
        }

        ApplyPromptVisibility();

        if (!playerInside) return;

        if (cureManager && cureManager.currentStep == 21 && CanShowPrompts())
        {
            if (Input.GetKeyDown(KeyCode.E))
                GiveLagundiToNPC();
        }
    }

    private void LateUpdate()
    {
        TryBoostSortingOrder();

        if (dialogueActive)
        {
            HideOnlyPrompts();
            EnsureDialogueUIVisible();

            if (forceEnableEveryFrame)
            {
                HardEnableLabel(nameText);
                HardEnableLabel(dialogueText);
            }
        }
        else
        {
            ApplyPromptVisibility();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;

        if (dialogueActive)
        {
            HideOnlyPrompts();
            EnsureDialogueUIVisible();
            return;
        }

        ApplyPromptVisibility();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;

        HideOnlyPrompts();

        SafeSetActive(dialoguePanel, false);
        if (nameText) nameText.gameObject.SetActive(false);
        dialogueActive = false;

        StopVoice();
        RestoreAnimators();
    }

    // --------- Flow ---------

    private void GiveLagundiToNPC()
    {
        // Hide cup in hands (chapters 1–4 flow)
        if (cureManager != null)
        {
            cureManager.cupHandUndercooked?.SetActive(false);
            cureManager.cupHandNormal?.SetActive(false);
            cureManager.cupHandOvercooked?.SetActive(false);
        }

        // Also hide CH5 minibowl hands if present (chapter 5 flow)
        if (ch5MiniBowlHandEmpty)       ch5MiniBowlHandEmpty.SetActive(false);
        if (ch5MiniBowlHandWithPounded) ch5MiniBowlHandWithPounded.SetActive(false);
        else
        {
            var ch5 = FindObjectOfType<Chapter5MortarFlow>();
            if (ch5)
            {
                ch5.miniBowlHandEmpty?.SetActive(false);
                ch5.miniBowlHandWithPounded?.SetActive(false);
            }
        }

        // Kill pre-talk DialogueManager so its lines can't appear
        if (preTalkDialogue != null)
            preTalkDialogue.EndDialogueImmediate();

        // Build result dialogue from ScriptableObject
        var brew = cureManager ? cureManager.LastCookResult : "success";
        if (resultDialogue != null)
            currentDialogue = resultDialogue.GetByBrew(brew);
        else
            currentDialogue = PickFallback(brew);

        // VO selection to align with currentDialogue
        currentVoices = PickVoices(brew);

        // Suppress prompts while result dialogue runs
        externalPromptSuppress = true;

        StartDialogue();
    }

    private string[] PickFallback(string result)
    {
        switch (result)
        {
            case "undercooked": return dialogueUndercooked;
            case "success":     return dialogueSuccess;
            case "overcooked":  return dialogueOvercooked;
            default:            return dialogueOvercooked;
        }
    }

    private AudioClip[] PickVoices(string result)
    {
        switch (result)
        {
            case "undercooked": return voicesUndercooked;
            case "success":     return voicesSuccess;
            case "overcooked":  return voicesOvercooked;
            default:            return voicesOvercooked;
        }
    }

    public void StartDialogue()
    {
        if (preTalkDialogue != null)
            preTalkDialogue.EndDialogueImmediate();

        dialogueIndex = 0;
        dialogueActive = true;

        HideOnlyPrompts();

        SafeSetActive(dialoguePanel, true);
        EnsureDialogueUIVisible(true);

        if (disableAnimatorsDuringDialogue)
            DisableAnimatorsOn(dialoguePanel);

        if (currentDialogue == null || currentDialogue.Length == 0)
        {
            ForceSpeakerAndLine(_resolvedNpcName, "Thank you.");
            StartCoroutine(FinishDialogueNextFrame());
            return;
        }

        EnsureVoiceSource();
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        HideOnlyPrompts();
        EnsureDialogueUIVisible();

        if (dialogueIndex < currentDialogue.Length)
        {
            string raw = currentDialogue[dialogueIndex] ?? string.Empty;
            ParseSpeakerLine(raw, out var speaker, out var content);
            if (string.IsNullOrWhiteSpace(speaker)) speaker = _resolvedNpcName;

            ForceSpeakerAndLine(speaker, content);
            PlayVoiceForIndex(dialogueIndex);

            if (forceEnableEveryFrame)
            {
                HardEnableLabel(nameText);
                HardEnableLabel(dialogueText);
            }
        }
        else
        {
            EndDialogue();
        }
    }

    private void NextDialogueLine()
    {
        StopVoice();
        dialogueIndex++;
        ShowCurrentLine();
    }

    private IEnumerator FinishDialogueNextFrame()
    {
        yield return null;
        EndDialogue();
    }

    private void EndDialogue()
    {
        SafeSetActive(dialoguePanel, false);
        if (nameText) nameText.gameObject.SetActive(false);
        dialogueActive = false;

        RestoreAnimators();

        externalPromptSuppress = false;
        ApplyPromptVisibility();

        if (cureManager) cureManager.currentStep = 22;

        // Star rating: 3 = both minigames went well, 2 = only one did, 1 = both failed.
        int stars = cureManager ? cureManager.ComputeStars() : 3;
        ChapterResult.Stars = stars;
        ChapterResult.RecordChapterStars(chapterNumber, stars);

        // 1 star -> must repeat the chapter (fail decision panel).
        // 2-3 stars -> proceed to outro/next chapter.
        if (stars >= 2)
        {
            // go straight to outro cutscene scene
            if (!string.IsNullOrEmpty(successOutroSceneName))
            {
                LoadSceneSafe(successOutroSceneName);
            }
            else
            {
                Debug.LogWarning("[NPCInteraction] successOutroSceneName is empty – staying in scene.");
            }
        }
        else
        {
            // both minigames failed -> use fail decision panel in this scene
            if (failDecisionPanel != null)
            {
                bool failedOver = cureManager && cureManager.FailedFromOvercooking();
                failDecisionPanel.Show(ChapterResult.Comment(stars, failedOver), stars);
            }
            else
            {
                Debug.LogWarning("[NPCInteraction] failDecisionPanel is not assigned.");
            }
        }
    }

    private void LoadSceneSafe(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        if (TransitionManager.Instance)
            TransitionManager.Instance.LoadSceneWithFade(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

    // --------- Prompt gating ---------

    private bool IsAnyDialogueOpenLocal()
    {
        return IsDialogueOpen();
    }

    private bool CanShowPrompts()
    {
        if (!playerInside) return false;
        if (IsAnyDialogueOpenLocal()) return false;
        if (externalPromptSuppress) return false;
        return true;
    }

    private void ApplyPromptVisibility()
    {
        SafeSetActive(talkPrompt, false);
        SafeSetActive(givePrompt, false);

        if (!CanShowPrompts())
        {
            HideOnlyPrompts();
            return;
        }

        ShowPromptsGroup();

        if (!cureManager)
        {
            SafeSetActive(talkPrompt, true);
            return;
        }

        if (cureManager.currentStep < 21)
        {
            SafeSetActive(talkPrompt, true);
            SafeSetActive(givePrompt, false);
        }
        else if (cureManager.currentStep == 21)
        {
            SafeSetActive(talkPrompt, false);
            SafeSetActive(givePrompt, true);
        }
        else
        {
            SafeSetActive(talkPrompt, false);
            SafeSetActive(givePrompt, false);
        }
    }

    private void HideOnlyPrompts()
    {
        if (promptsGroup)
        {
            promptsGroup.alpha = 0f;
            promptsGroup.interactable = false;
            promptsGroup.blocksRaycasts = false;
        }
        SafeSetActive(talkPrompt, false);
        SafeSetActive(givePrompt, false);
    }

    private void ShowPromptsGroup()
    {
        if (promptsGroup)
        {
            promptsGroup.alpha = 1f;
            promptsGroup.interactable = true;
            promptsGroup.blocksRaycasts = true;
        }
    }

    // --------- UI hardening ---------

    private void TryBoostSortingOrder()
    {
        if (_didSortBoost) return;

        if (dialogueCanvas)
        {
            dialogueCanvas.overrideSorting = true;

            if (promptsCanvas)
            {
                promptsCanvas.overrideSorting = true;
                int pOrder = promptsCanvas.sortingOrder;
                if (dialogueCanvas.sortingOrder <= pOrder)
                    dialogueCanvas.sortingOrder = pOrder + 10;
            }
        }
        _didSortBoost = true;
    }

    private void EnsureDialogueUIVisible(bool force = false)
    {
        if (!dialoguePanel) return;

        if (!dialoguePanel.activeSelf || force)
            dialoguePanel.SetActive(true);

        if (dialogueCanvas)
        {
            if (!dialogueCanvas.enabled) dialogueCanvas.enabled = true;
            dialogueCanvas.overrideSorting = true;
            if (promptsCanvas)
                dialogueCanvas.sortingOrder = Mathf.Max(dialogueCanvas.sortingOrder, promptsCanvas.sortingOrder + 10);
        }

        if (!dialogueGroup && dialoguePanel)
            dialogueGroup = dialoguePanel.GetComponent<CanvasGroup>();
        if (dialogueGroup)
        {
            dialogueGroup.alpha = 1f;
            dialogueGroup.interactable = true;
            dialogueGroup.blocksRaycasts = true;
        }

        HardEnableLabel(dialogueText);
        HardEnableLabel(nameText);
    }

    private void HardEnableLabel(TMP_Text label)
    {
        if (!label) return;

        var t = label.transform;
        while (t != null)
        {
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);

            var c = t.GetComponent<Canvas>();
            if (c && !c.enabled) c.enabled = true;

            var cg = t.GetComponent<CanvasGroup>();
            if (cg)
            {
                if (cg.alpha < 1f) cg.alpha = 1f;
                if (!cg.interactable) cg.interactable = true;
                if (!cg.blocksRaycasts) cg.blocksRaycasts = true;
            }
            t = t.parent;
        }

        if (!label.enabled) label.enabled = true;
        var col = label.color;
        if (col.a < 1f) { col.a = 1f; label.color = col; }
        if (!label.gameObject.activeSelf) label.gameObject.SetActive(true);
    }

    private void RestoreAnimators()
    {
        if (_disabledAnimators.Count == 0) return;
        foreach (var a in _disabledAnimators)
        {
            if (a) a.enabled = true;
        }
        _disabledAnimators.Clear();
    }

    private void DisableAnimatorsOn(GameObject go)
    {
        if (!go || !disableAnimatorsDuringDialogue) return;
        var anims = go.GetComponentsInChildren<Animator>(true);
        foreach (var a in anims)
        {
            if (a && a.enabled)
            {
                a.enabled = false;
                _disabledAnimators.Add(a);
            }
        }
    }

    private void ForceSpeakerAndLine(string speaker, string content)
    {
        if (nameText)
        {
            HardEnableLabel(nameText);
            nameText.text = string.IsNullOrEmpty(speaker) ? _resolvedNpcName : speaker;
        }
        else
        {
            Debug.LogError("[NPCInteraction] nameText is NULL.");
        }

        if (dialogueText)
        {
            HardEnableLabel(dialogueText);

            var tw = dialogueText.GetComponent<TypewriterText>();
            if (tw != null)
                tw.Play(content ?? "");
            else
                dialogueText.text = content ?? "";
        }
        else
        {
            Debug.LogError("[NPCInteraction] dialogueText is NULL.");
        }
    }

    private void ParseSpeakerLine(string raw, out string speaker, out string content)
    {
        speaker = null;
        content = raw ?? string.Empty;
        if (string.IsNullOrWhiteSpace(raw)) return;

        string line = raw.TrimStart();

        if (line.StartsWith("NPC:", System.StringComparison.OrdinalIgnoreCase))
        {
            speaker = _resolvedNpcName;
            content = line.Substring(4).TrimStart();
            return;
        }

        if (line.StartsWith("PLAYER:", System.StringComparison.OrdinalIgnoreCase) ||
            line.StartsWith("YOU:", System.StringComparison.OrdinalIgnoreCase))
        {
            speaker = string.IsNullOrEmpty(playerDisplayName) ? "You" : playerDisplayName;
            int idx = line.IndexOf(':');
            content = (idx >= 0 && idx < line.Length - 1)
                ? line.Substring(idx + 1).TrimStart()
                : "";
            return;
        }
        // default speaker handled by caller
    }

    // --------- Auto-wiring, voice helpers ---------

    private void TryAutoWireAll()
    {
        if (!preTalkDialogue) preTalkDialogue = FindObjectOfType<DialogueManager>();

        talkPrompt    = talkPrompt    ?? FindByTagOrName("TalkPrompt",    "TalkPrompt");
        givePrompt    = givePrompt    ?? FindByTagOrName("GivePrompt",    "GivePrompt");
        dialoguePanel = dialoguePanel ?? FindByTagOrName("DialoguePanel", "DialoguePanel");

        if (!nameText)
        {
            var go = FindByTagOrName("NameText", "NameText");
            if (go) nameText = go.GetComponent<TextMeshProUGUI>();
            if (!nameText && dialoguePanel)
            {
                foreach (var t in dialoguePanel.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    if (!string.IsNullOrEmpty(t.name) &&
                        t.name.ToLower().Contains("name"))
                    {
                        nameText = t;
                        break;
                    }
                }
            }
        }
        if (!dialogueText)
        {
            var go = FindByTagOrName("DialogueText", "DialogueText");
            if (go) dialogueText = go.GetComponent<TextMeshProUGUI>();
            if (!dialogueText && dialoguePanel)
            {
                foreach (var t in dialoguePanel.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    if (t != nameText)
                    {
                        dialogueText = t;
                        break;
                    }
                }
            }
        }

        if (!promptsCanvas)
        {
            var go = FindByTagOrName("PromptsCanvas", "PromptsCanvas");
            if (go) promptsCanvas = go.GetComponent<Canvas>();
        }
        if (!dialogueCanvas && dialoguePanel)
        {
            dialogueCanvas = dialoguePanel.GetComponentInParent<Canvas>(true);
        }
        if (!promptsGroup)
        {
            var root = FindByTagOrName("PromptsRoot", "PromptsRoot");
            if (root)
            {
                promptsGroup = root.GetComponent<CanvasGroup>();
                if (!promptsGroup) promptsGroup = root.AddComponent<CanvasGroup>();
            }
        }
        if (!dialogueGroup && dialoguePanel)
        {
            dialogueGroup = dialoguePanel.GetComponent<CanvasGroup>();
        }
    }

    private GameObject FindByTagOrName(string tag, string name)
    {
        try
        {
            var byTag = GameObject.FindGameObjectWithTag(tag);
            if (byTag) return byTag;
        }
        catch { }

        return GameObject.Find(name);
    }

    private void SafeSetActive(GameObject go, bool state)
    {
        if (!go) return;
        if (go.activeSelf != state) go.SetActive(state);
    }

    private void EnsureVoiceSource()
    {
        if (voiceSource) return;
        var host = dialoguePanel ? dialoguePanel : gameObject;
        voiceSource = host.GetComponent<AudioSource>();
        if (!voiceSource) voiceSource = host.AddComponent<AudioSource>();
        voiceSource.playOnAwake = false;
        voiceSource.loop = false;
    }

    private void PlayVoiceForIndex(int i)
    {
        if (!voiceSource || currentVoices == null) return;
        if (i < 0 || i >= currentVoices.Length)
        {
            voiceSource.Stop();
            return;
        }

        var clip = currentVoices[i];
        if (!clip)
        {
            voiceSource.Stop();
            return;
        }

        voiceSource.Stop();
        voiceSource.clip = clip;
        voiceSource.Play();
    }

    private void StopVoice()
    {
        if (voiceSource && voiceSource.isPlaying)
            voiceSource.Stop();
    }
}
