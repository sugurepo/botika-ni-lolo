using UnityEngine;
using TMPro;
using System.Text;

public class JournalAchievementsPage : MonoBehaviour
{
    public TMP_Text achievementsText;

    void OnEnable() => Refresh();

    public void Refresh()
    {
        if (!achievementsText) return;

        var sb = new StringBuilder();
        for (int ch = 1; ch <= 5; ch++)
            sb.AppendLine($"Kabanata {ch}:  {ChapterResult.StarString(ChapterResult.GetChapterStars(ch))}");

        achievementsText.text = sb.ToString();
    }
}
