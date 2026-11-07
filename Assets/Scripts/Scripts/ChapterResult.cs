using UnityEngine;

// Tiny cross-scene holder for the star rating earned at the end of a chapter.
// Static fields persist across scene loads for the lifetime of the play session.
public static class ChapterResult
{
    public static int Stars = 3;

    // Per-chapter best star rating (index 1..5), persisted via PlayerPrefs.
    private static readonly int[] ChapterStars = new int[6];
    private static bool _loaded;

    private static void LoadIfNeeded()
    {
        if (_loaded) return;
        for (int i = 1; i <= 5; i++)
            ChapterStars[i] = PlayerPrefs.GetInt($"ChapterStars_{i}", 0);
        _loaded = true;
    }

    public static int GetChapterStars(int chapter)
    {
        LoadIfNeeded();
        return (chapter >= 1 && chapter <= 5) ? ChapterStars[chapter] : 0;
    }

    // Records the star rating for a chapter, keeping the best score on retries.
    public static void RecordChapterStars(int chapter, int stars)
    {
        if (chapter < 1 || chapter > 5) return;
        LoadIfNeeded();
        stars = System.Math.Clamp(stars, 0, 3);
        if (stars > ChapterStars[chapter])
        {
            ChapterStars[chapter] = stars;
            PlayerPrefs.SetInt($"ChapterStars_{chapter}", stars);
            PlayerPrefs.Save();
        }
    }

    public static string StarString(int stars)
    {
        stars = System.Math.Clamp(stars, 0, 3);
        return new string('★', stars) + new string('☆', 3 - stars);
    }

    public static string Comment(int stars, bool failedFromOvercooking = false)
    {
        switch (stars)
        {
            case 3:  return "Perpekto! Eksakto ang bawat hakbang ng paghahanda mo.";
            case 2:  return "Magaling! Maayos ang gamot, pero may dapat pang pagbutihin sa susunod.";
            default: return failedFromOvercooking ? "Sobra sa luto, apo." : "Kulang pa sa luto, apo.";
        }
    }
}
