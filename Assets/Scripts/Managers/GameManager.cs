using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private BubbleGuy _bubbleGuy;

    private void Awake()
    {
        instance = this;
    }

    public void CheckWinCondition()
    {
        int ballsInLastRow = DefaultBubbleSpawner.instance.GetBallsInLastRow();
        int totalBallsInLastRow = DefaultBubbleSpawner.instance.GetTotalBallsInLastRow();

        if (totalBallsInLastRow > 0 && (float)ballsInLastRow / totalBallsInLastRow <= 0.6f)
        {
            DefaultBubbleSpawner.instance.MakeRemainingBallsFall();

            Invoke(nameof(ShowWinPanel), 1f); 
        }
    }

    private void ShowWinPanel()
    {
        _bubbleGuy.SetWinStatus(true);
    }

    public void GameOver()
    {
        _bubbleGuy.SetWinStatus(false);

    }

}
