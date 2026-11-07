using UnityEngine;

public class JournalManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject bookPanel;
    public GameObject page1;
    public GameObject page2;
    public GameObject page3; // optional: Achievements page (chapter star ratings)
    public GameObject page4; // optional: Plant Gallery page (unlocked chapter plants)

    [Header("Controls")]
    public KeyCode toggleKey = KeyCode.G;

    [Header("Proximity Gate")]
    [Tooltip("If true, G only works when the player is inside the JournalZone trigger (until unlocked).")]
    public bool requireProximity = true;

    [Tooltip("Once the player opens the journal once, it becomes accessible from anywhere (for this session).")]
    public bool unlockAnywhereOnFirstOpen = true;

    [Tooltip("Optional: small 'Press G' prompt shown while in range (only while still locked).")]
    public GameObject inRangePrompt;

    [Header("External Lock")]
    [Tooltip("When true, G does nothing even if in range (used by flow until cutscene/dialogue are finished).")]
    public bool externallyLocked = false;

    [Header("Player Controls")]
    public SimpleFPSController fpsController; // drag your player here

    private bool isOpen = false;
    private int currentPage = 1;
    private bool isPlayerInRange = false; // set by JournalZone

    bool ProximityRequired =>
        requireProximity && !(unlockAnywhereOnFirstOpen && JournalAccessService.Unlocked);

    void Start()
    {
        Close(); // hide + reset at start
        if (inRangePrompt) inRangePrompt.SetActive(false);
    }

    void Update()
    {
        if (externallyLocked) return;

        if (ProximityRequired && !isPlayerInRange) return;

        if (Input.GetKeyDown(toggleKey))
        {
            if (isOpen) Close();
            else Open();
        }
    }

    // Called by JournalZone
    public void SetInRange(bool inRange)
    {
        isPlayerInRange = inRange;
        if (inRangePrompt)
            inRangePrompt.SetActive(inRange && !isOpen && !externallyLocked && ProximityRequired);
    }

    public void SetExternalLock(bool locked)
    {
        externallyLocked = locked;
        if (inRangePrompt)
            inRangePrompt.SetActive(isPlayerInRange && !isOpen && !externallyLocked && ProximityRequired);
    }

    public bool IsOpen => isOpen;

    public void ToggleFromUI()
    {
        if (externallyLocked) return;
        if (ProximityRequired && !isPlayerInRange) return;
        if (isOpen) Close(); else Open();
    }

    int MaxPage()
    {
        if (page4) return 4;
        if (page3) return 3;
        return 2;
    }

    public void NextPage()
    {
        if (!isOpen) return;
        currentPage++;
        if (currentPage > MaxPage()) currentPage = 1;
        ShowPage(currentPage);
    }

    public void PrevPage()
    {
        if (!isOpen) return;
        currentPage--;
        if (currentPage < 1) currentPage = MaxPage();
        ShowPage(currentPage);
    }

    void Open()
    {
        if (bookPanel) bookPanel.SetActive(true);
        ShowPage(1);
        isOpen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (fpsController != null) fpsController.controlsEnabled = false;

        if (inRangePrompt) inRangePrompt.SetActive(false);

        // 🔓 Permanently unlock after first open
        if (unlockAnywhereOnFirstOpen && !JournalAccessService.Unlocked)
            JournalAccessService.Unlock();
    }

    void Close()
    {
        if (bookPanel) bookPanel.SetActive(false);
        if (page1) page1.SetActive(false);
        if (page2) page2.SetActive(false);
        if (page3) page3.SetActive(false);
        if (page4) page4.SetActive(false);
        isOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (fpsController != null) fpsController.controlsEnabled = true;

        if (inRangePrompt)
            inRangePrompt.SetActive(isPlayerInRange && !externallyLocked && ProximityRequired);
    }

    void ShowPage(int page)
    {
        if (page1) page1.SetActive(page == 1);
        if (page2) page2.SetActive(page == 2);
        if (page3) page3.SetActive(page == 3);
        if (page4) page4.SetActive(page == 4);
    }
}
