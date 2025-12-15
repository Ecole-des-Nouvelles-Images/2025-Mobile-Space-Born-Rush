using UnityEngine;

public interface IHighScoreService
{
    float GetHighScore();
    bool IsHighScore(float score);
    void SubmitScore(float score);
}
