using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private TextAsset _levelFile;

    void Start()
    {
        LoadLevel();
    }

    public void LoadLevel()
    {
        if (_levelFile == null)
        {
            return;
        }

        string[] lines = _levelFile.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        List<LevelRow> rows = new List<LevelRow>();

        for (int i = 0; i < lines.Length; i++)
        {
            string[] tokens = lines[i].Split(',');
            List<LevelBall> balls = new List<LevelBall>();

            foreach (string token in tokens)
            {
                if (string.IsNullOrEmpty(token))
                {
                    balls.Add(new LevelBall { isEmpty = true, color = Color.clear });
                }
                else
                {
                    Color color = GetColorFromString(token.Trim());
                    balls.Add(new LevelBall { isEmpty = false, color = color });
                }
            }

            rows.Add(new LevelRow { balls = balls.ToArray() });
        }

        LevelData levelData = new LevelData { rows = rows.ToArray() };
        DefaultBubbleSpawner.instance.SetLevelData(levelData);
    }

    private Color GetColorFromString(string colorStr)
    {
        return colorStr.ToUpper() switch
        {
            "R" => new Color(1f, 0.5f, 0.5f),  
            "G" => new Color(0.5f, 1f, 0.5f),  
            "B" => new Color(0.5f, 0.5f, 1f),  
            "Y" => new Color(1f, 1f, 0.5f),    
            "P" => new Color(0.75f, 0.5f, 0.75f), 
            _ => new Color(0.9f, 0.9f, 0.9f), 
        };
    }

}
