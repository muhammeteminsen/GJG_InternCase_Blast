using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI scoreValueText;
    [SerializeField] private TextMeshProUGUI movesValueText;
    [SerializeField] private TextMeshProUGUI finishScoreValueText;
    [SerializeField] private TextMeshProUGUI finishHighScoreValueText;
    [Header("Panels")]
    [SerializeField] private GameObject finishGamePanel;
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private GameObject movesPanel;
    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    public void UpdateScore(int score)
    {
        scoreValueText.text = score.ToString();
    }
    public void UpdateMoves(int moves)
    {
        movesValueText.text = moves.ToString();
    }
    public void UpdateFinishScores(int score, int highScore)
    {
        finishScoreValueText.text = score.ToString();
        finishHighScoreValueText.text = highScore.ToString();
    }
    public void UpdateInitial(int moves, int score )
    {
        movesValueText.text = moves.ToString();
        scoreValueText.text = score.ToString();
        scorePanel.transform.localScale = Vector3.zero;
        movesPanel.transform.localScale = Vector3.zero;
        scorePanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        movesPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        finishGamePanel.SetActive(false);
    }
    public void RestartGame(Action restartCallback)
    {
        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(() => { restartCallback?.Invoke(); });
    }
    public void FinishGame(Action<bool> clickableCallback)
    {
        finishGamePanel.transform.localScale = Vector3.zero;
        finishGamePanel.SetActive(true);
        finishGamePanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        scorePanel.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutBack);
        movesPanel.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutBack);
        clickableCallback?.Invoke(false);
    }
}
