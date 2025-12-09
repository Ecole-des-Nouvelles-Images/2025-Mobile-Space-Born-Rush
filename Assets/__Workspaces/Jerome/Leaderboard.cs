using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Dan.Main;

public class Leaderboard : MonoBehaviour
{
    private List<TextMeshProUGUI> _names;
    private List<TextMeshProUGUI> _scores;

    private string _publicLeaderboardKey;

    void GetLeaderboard()
    {
        LeaderboardCreator.GetLeaderboard(_publicLeaderboardKey, ((msg) =>
        {
            for (int i = 0; i < _names.Count; i++)
            {
                _names[i].text = msg[i].Username;
                _scores[i].text = msg[i].Score.ToString();
            }
        }));
    }

    public void SetLeaderboardEntry(string username, int score)
    {
        LeaderboardCreator.UploadNewEntry(_publicLeaderboardKey, username, score, ((_) =>
        {
            GetLeaderboard();
        }));
        
    }
}
