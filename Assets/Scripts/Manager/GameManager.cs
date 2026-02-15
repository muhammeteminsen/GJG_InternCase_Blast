using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private FeedbacksManager _feedbacksManager;
    private UIManager _uiManager;
    private int _score;
    private int _moves;
    private void Awake()
    {
        _feedbacksManager = GetComponent<FeedbacksManager>();
        _uiManager = GetComponent<UIManager>();
    }
    public void ApplyFeedback(FeedbackType feedbackType)
    {

        _feedbacksManager.PlayFeedbacks(feedbackType);
    }
    public void InitializeGame(int moves, int score, LevelDataSo levelDataSo)
    {
        _score = 0;
        _moves = levelDataSo.initialMoves; 
        _uiManager.UpdateInitial(moves, score);
    }
    public void ApplyMoves()
    {
        _uiManager.UpdateMoves(--_moves);s
    }
    public void ApplyUpdateScore(int score)
    {
        _score += score;
        _uiManager.UpdateScore(_score);
    }
    private void ApplyUpdateFinishScore()
    {
        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);
        if (_score > currentHighScore)
        {
            currentHighScore = _score;
            PlayerPrefs.SetInt("HighScore", _score);
            PlayerPrefs.Save();
        }
        _uiManager.UpdateFinishScores(_score, currentHighScore);
    }
    public void ApplyFinishGame(Action<bool> onFinishGame)
    {
        bool isFinishGame = _moves <= 0;
        if (isFinishGame)
        {
            ApplyRestartGame();
            ApplyUpdateFinishScore();
            _uiManager.FinishGame(onFinishGame);
        }
            
        onFinishGame?.Invoke(isFinishGame);
    }
    private void ApplyRestartGame()
    {
        _uiManager.RestartGame(() => { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); });
    }
}
