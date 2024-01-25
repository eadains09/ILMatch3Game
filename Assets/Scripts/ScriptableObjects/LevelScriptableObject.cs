using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "ScriptableObjects/LevelScriptableObject", order = 1)]
public class LevelScriptableObject : ScriptableObject
{
    public int levelId;
    public int scoreToComplete;
    public GameplayConstants.BlockType[] gridLayout = new GameplayConstants.BlockType[36];
}
