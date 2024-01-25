using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameplayUI : MonoBehaviour
{
    public VisualTreeAsset circleBlockTreeAsset;
    public VisualTreeAsset diamondBlockTreeAsset;
    public VisualTreeAsset heartBlockTreeAsset;
    public VisualTreeAsset squareBlockTreeAsset;
    public VisualTreeAsset starBlockTreeAsset;
    public VisualTreeAsset triangleBlockTreeAsset;
    public VisualTreeAsset baseBlockTreeAsset;

    public VisualTreeAsset attributionPopupTreeAsset;
    public VisualTreeAsset instructionsPopupTreeAsset;

    public static event Action<MouseDownEvent, GameplayConstants.BlockType, int> OnMouseDownEvent;
    public static event Action<MouseUpEvent, GameplayConstants.BlockType, int> OnMouseUpEvent;
    public static event Action OnRestartGameButtonPressed;

    VisualElement root;
    Label timerLabel;
    Label levelLabel;
    ProgressBar pointsProgressBar;
    List<VisualElement> blockContainers;

    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        timerLabel = root.Q<Label>("TimerLabel");
        levelLabel = root.Q<Label>("LevelLabel");
        pointsProgressBar = root.Q<ProgressBar>("PointsProgressBar");
        blockContainers = root.Query<VisualElement>("BlockContainer").ToList();

        var attributionButton = root.Q<Button>("AttributionButton");
        attributionButton.clicked += () => ShowAttributionPopup();
        var instructionsButton = root.Q<Button>("InstructionsButton");
        instructionsButton.clicked += () => ShowInstructionsPopup();
        var restartGameButton = root.Q<Button>("RestartGameButton");
        restartGameButton.clicked += () => OnRestartGameButtonPressed?.Invoke();
    }

    void ShowAttributionPopup()
    {
        var attributionPopupInstance = attributionPopupTreeAsset.CloneTree();
        PositionPopup(attributionPopupInstance);
        SetUpCloseButton(attributionPopupInstance);
        root.Add(attributionPopupInstance);
    }

    void PositionPopup(TemplateContainer popupInstance)
    {
        popupInstance.style.position = new StyleEnum<Position>(Position.Absolute);
        popupInstance.style.top = new StyleLength(new Length(25f, LengthUnit.Percent));
        popupInstance.style.left = new StyleLength(new Length(50f, LengthUnit.Percent));
    }

    void SetUpCloseButton(TemplateContainer popupInstance)
    {
        var closeButton = popupInstance.Q<Button>("Close");
        closeButton.clicked += () => root.Remove(popupInstance);
    }

    void ShowInstructionsPopup()
    {
        var instructionsPopupInstance = instructionsPopupTreeAsset.CloneTree();
        PositionPopup(instructionsPopupInstance);
        SetUpCloseButton(instructionsPopupInstance);
        root.Add(instructionsPopupInstance);
    }

    public void UpdateNewLevel(int level, int scoreToComplete, GameplayConstants.BlockType[,] currentGameGrid)
    {
        UpdateLevelLabel(level);
        UpdateProgressBarTargetPointsAmount(scoreToComplete);
        UpdateGameGrid(currentGameGrid);
    }

    public void UpdateLevelLabel(int level)
    {
        levelLabel.text = level.ToString();
    }

    public void UpdateProgressBarTargetPointsAmount(int scoreToComplete)
    {
        pointsProgressBar.highValue = scoreToComplete;
    }

    public void UpdateGameGrid(GameplayConstants.BlockType[,] currentGameGrid)
    {
        ClearBlockContainers();

        int index = 0;
        for (int x = 0; x < 6; x++)
        {
            for (int y = 0; y < 6; y++)
            {
                var blockType = currentGameGrid[x, y];
                var blockInstance = LookupVisualTreeAssetFromBlockType(blockType).CloneTree();
                var blockContainerLocation = index;

                var blockItem = blockInstance.Q<VisualElement>("BlockItem");
                blockItem.RegisterCallback<MouseDownEvent>((evt) => OnMouseDownEvent?.Invoke(evt, blockType, blockContainerLocation));
                blockItem.RegisterCallback<MouseUpEvent>((evt) => OnMouseUpEvent?.Invoke(evt, blockType, blockContainerLocation));

                blockContainers[blockContainerLocation].Add(blockInstance);

                index++;
            }
        }
    }

    void ClearBlockContainers()
    {
        for (int i = 0; i < blockContainers.Count; i++)
        {
            blockContainers[i].Clear();
        }
    }

    // A shortcut around Unity not being able to serialize Dictionaries
    private VisualTreeAsset LookupVisualTreeAssetFromBlockType(GameplayConstants.BlockType blockType)
    {
        switch (blockType)
        {
            case GameplayConstants.BlockType.Circle:
                return circleBlockTreeAsset;
            case GameplayConstants.BlockType.Diamond:
                return diamondBlockTreeAsset;
            case GameplayConstants.BlockType.Heart:
                return heartBlockTreeAsset;
            case GameplayConstants.BlockType.Square:
                return squareBlockTreeAsset;
            case GameplayConstants.BlockType.Star:
                return starBlockTreeAsset;
            case GameplayConstants.BlockType.Triangle:
                return triangleBlockTreeAsset;

            default:
                return baseBlockTreeAsset;
        }

    }

    public void UpdatePointsProgressBar(int currentScore)
    {
        pointsProgressBar.value = currentScore;
    }

    public void UpdateTimerLabel(int timer)
    {
        timerLabel.text = timer.ToString();
    }
}
