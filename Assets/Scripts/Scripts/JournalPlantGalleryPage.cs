using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Scrollable (left/right) gallery of chapter plants. Only shows the plant
// art/name for chapters the player has completed at least once; locked
// chapters show a "???" placeholder.
public class JournalPlantGalleryPage : MonoBehaviour
{
    [System.Serializable]
    public class PlantEntry
    {
        [Tooltip("1-5. Used to check whether this chapter has been completed.")]
        public int chapterNumber = 1;
        public string plantName;
        public Sprite plantImage;
    }

    public PlantEntry[] plants;

    [Header("UI")]
    public Image plantImage;
    public TMP_Text plantNameText;

    [Tooltip("Optional: shown instead of the plant name/image while the chapter is still locked.")]
    public string lockedLabel = "???";

    [Tooltip("Which chapter's plant this scene's journal should open on (1-5). Set per scene.")]
    public int defaultChapter = 1;

    private int index = 0;

    void OnEnable()
    {
        index = 0;
        if (plants != null)
        {
            for (int i = 0; i < plants.Length; i++)
            {
                if (plants[i].chapterNumber == defaultChapter) { index = i; break; }
            }
        }
        Refresh();
    }

    public void Next()
    {
        if (plants == null || plants.Length == 0) return;
        index = (index + 1) % plants.Length;
        Refresh();
    }

    public void Prev()
    {
        if (plants == null || plants.Length == 0) return;
        index = (index - 1 + plants.Length) % plants.Length;
        Refresh();
    }

    void Refresh()
    {
        if (plants == null || plants.Length == 0) return;

        var entry = plants[index];
        bool unlocked = ChapterResult.GetChapterStars(entry.chapterNumber) > 0;

        if (plantImage)
        {
            plantImage.sprite = unlocked ? entry.plantImage : null;
            plantImage.enabled = unlocked && entry.plantImage != null;
        }

        if (plantNameText)
            plantNameText.text = unlocked ? entry.plantName : lockedLabel;
    }
}
