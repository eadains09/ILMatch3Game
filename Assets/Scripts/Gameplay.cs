using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Gameplay : MonoBehaviour
{
    public GameplayUI ui;

    LevelScriptableObject level;
    PlayerData player = new PlayerData();

    GameplayConstants.BlockType[,] currentGameGridState;

    bool isActivePlay = false;
    int currentLevelNumber = 1;

    GameplayConstants.BlockType currentMouseDownBlockType;
    int currentMouseDownGridLocation;

    const int GRID_SIZE = 6;

    void OnEnable()
    {
        GameplayUI.OnMouseDownEvent += OnMouseDownEvent;
        GameplayUI.OnMouseUpEvent += OnMouseUpEvent;
        GameplayUI.OnRestartGameButtonPressed += OnRestartGameEvent;
    }

    void OnDisable()
    {
        GameplayUI.OnMouseDownEvent -= OnMouseDownEvent;
        GameplayUI.OnMouseUpEvent -= OnMouseUpEvent;
        GameplayUI.OnRestartGameButtonPressed -= OnRestartGameEvent;
    }

    // Start is called before the first frame update
    void Start()
    {
        // load level blocks
        LoadCurrentLevelData();
        CreateCurrentGameGridState();
        ui.UpdateNewLevel(currentLevelNumber, level.scoreToComplete, currentGameGridState);

        // enable gameplay
        isActivePlay = true;

        // start timer
        ui.UpdateLevelLabel(level.levelId);
        ui.UpdatePointsProgressBar(player.currentScore);
    }

    void LoadCurrentLevelData()
    {
        level = Resources.Load<LevelScriptableObject>($"Levels/Level{currentLevelNumber}");
    }

    void CreateCurrentGameGridState()
    {
        int index = 0;
        currentGameGridState = new GameplayConstants.BlockType[GRID_SIZE, GRID_SIZE];

        for (int x = 0; x < GRID_SIZE; x++)
        {
            for (int y = 0; y < GRID_SIZE; y++)
            {
                currentGameGridState[x, y] = level.gridLayout[index];
                index++;
            }
        }
    }

    void Update()
    {
        if (isActivePlay && IsGameOver())
        {
            //  show gameOver popup
            isActivePlay = false;
        }
        else
        {
            //  update countdown on screen
            ui.UpdateTimerLabel(0);
        }
    }

    bool IsGameOver()
    {
        // get current time
        // return (current time - countdownStartTime) >= GameplayConstants.TIME_LIMIT

        return false;
    }

    void HandlePlayerMatchAttempt(GameplayConstants.BlockType mouseUpBlockType, int mouseUpGridLocation)
    {
        if (IsSwappingBlocksLegal(currentMouseDownGridLocation, mouseUpGridLocation))
        {
            SwapBlocks(currentMouseDownGridLocation, mouseUpGridLocation);
            var matchLocations = GetMatchLocations(currentMouseDownGridLocation, mouseUpGridLocation);

            if (matchLocations.Count >= 3)
            {
                HandleSuccessfulMatchAttempt(matchLocations);
            }
            else
            {
                // Unswap the blocks on the game board
                SwapBlocks(currentMouseDownGridLocation, mouseUpGridLocation);
                ui.UpdateGameGrid(currentGameGridState);
                Debug.Log("Not enough matches.");
            }
        }
        else
        {
            Debug.Log("Not eligible to swap.");
        }

        currentMouseDownBlockType = GameplayConstants.BlockType.None;
        currentMouseDownGridLocation = -1;
    }

    bool IsSwappingBlocksLegal(int swap1GridLocation, int swap2GridLocation)
    {
        var (swap1Row, swap1Col) = Convert1DLocationTo2DLocation(swap1GridLocation);
        var (swap2Row, swap2Col) = Convert1DLocationTo2DLocation(swap2GridLocation);

        if (BothBlocksAreInBounds(swap1Row, swap1Col, swap2Row, swap2Col) &&
            BlocksAreAdjacent(swap1Row, swap1Col, swap2Row, swap2Col) &&
            BlocksAreLegalSwitchingTypes(swap1Row, swap1Col, swap2Row, swap2Col))
        {
            return true;
        }

        return false;
    }

    (int, int) Convert1DLocationTo2DLocation(int oneDimensionalGridLocation)
    {
        int twoDimensionalGridRow = oneDimensionalGridLocation / GRID_SIZE;
        int twoDimensionalGridCol = oneDimensionalGridLocation % GRID_SIZE;

        return (twoDimensionalGridRow, twoDimensionalGridCol);
    }

    bool BothBlocksAreInBounds(int swap1Row, int swap1Col, int swap2Row, int swap2Col)
    {
        if (swap1Row < 0 || swap1Row >= GRID_SIZE ||
            swap1Col < 0 || swap1Col >= GRID_SIZE ||
            swap2Row < 0 || swap2Row >= GRID_SIZE ||
            swap2Col < 0 || swap2Col >= GRID_SIZE)
        {
            return false;
        }

        return true;
    }

    bool BlocksAreAdjacent(int swap1Row, int swap1Col, int swap2Row, int swap2Col)
    {
        if (swap1Row == swap2Row && (swap1Col - 1 == swap2Col || swap1Col + 1 == swap2Col))
        {
            return true;
        }

        if (swap1Col == swap2Col && (swap1Row - 1 == swap2Row || swap1Row + 1 == swap2Row))
        {
            return true;
        }

        return false;
    }

    bool BlocksAreLegalSwitchingTypes(int swap1Row, int swap1Col, int swap2Row, int swap2Col)
    {
        if (currentGameGridState[swap1Row, swap1Col] == GameplayConstants.BlockType.None ||
            currentGameGridState[swap2Row, swap2Col] == GameplayConstants.BlockType.None)
        {
            return false;
        }

        return true;
    }

    void SwapBlocks(int swap1GridLocation, int swap2GridLocation)
    {
        var (swap1Row, swap1Col) = Convert1DLocationTo2DLocation(swap1GridLocation);
        var (swap2Row, swap2Col) = Convert1DLocationTo2DLocation(swap2GridLocation);

        var tempBlockType = currentGameGridState[swap1Row, swap1Col];
        currentGameGridState[swap1Row, swap1Col] = currentGameGridState[swap2Row, swap2Col];
        currentGameGridState[swap2Row, swap2Col] = tempBlockType;

        ui.UpdateGameGrid(currentGameGridState);
    }

    List<int> GetMatchLocations(int mouseDownGridLocation, int mouseUpGridLocation)
    {
        List<int> matchMouseDownLocations = new List<int>();
        int[,] searchedLocations = new int[GRID_SIZE, GRID_SIZE];

        // Search for match around mouseDownBlock
        var (mouseDownRow, mouseDownCol) = Convert1DLocationTo2DLocation(mouseDownGridLocation);

        searchedLocations[mouseDownRow, mouseDownCol] = 1;
        var targetBlockType = currentGameGridState[mouseDownRow, mouseDownCol];

        SearchCurrentLocationNeighborsForMatch(mouseDownRow, mouseDownCol, targetBlockType, searchedLocations, matchMouseDownLocations);

        // If searching neighbors found at least one block that matches mouseDownBlockType, 
        // then the mouseDownGridLocation should also be added as a matchLocation.
        if (matchMouseDownLocations.Count >= 1)
        {
            matchMouseDownLocations.Add(mouseDownGridLocation);
        }

        if (matchMouseDownLocations.Count < 3)
        {
            // If not enough matches were found around mouseDownLocation,
            // the matchMouseDownLocations array should be cleared so that we don't
            // end up accidentally removing a pair of matched blocks.
            matchMouseDownLocations.Clear();
        }

        // Search for match around mouseUpBlock
        List<int> matchMouseUpLocations = new List<int>();

        // clear searchedLocations
        searchedLocations = new int[GRID_SIZE, GRID_SIZE];
        var (mouseUpRow, mouseUpCol) = Convert1DLocationTo2DLocation(mouseUpGridLocation);

        searchedLocations[mouseUpRow, mouseUpCol] = 1;
        targetBlockType = currentGameGridState[mouseUpRow, mouseUpCol];

        SearchCurrentLocationNeighborsForMatch(mouseUpRow, mouseUpCol, targetBlockType, searchedLocations, matchMouseUpLocations);

        // If searching neighbors found at least one block that matches mouseUpBlockType, 
        // then the mouseUpGridLocation should also be added as a matchLocation.
        if (matchMouseUpLocations.Count >= 1)
        {
            matchMouseUpLocations.Add(mouseUpGridLocation);
        }

        if (matchMouseUpLocations.Count < 3)
        {
            // If not enough matches were found around mouseUpLocation,
            // the matchMouseUpLocations array should be cleared so that we don't
            // end up accidentally removing a pair of matched blocks.
            matchMouseUpLocations.Clear();
        }

        // Combine match lists
        List<int> allMatchLocations = new List<int>();
        matchMouseDownLocations.ForEach(matchLocation => allMatchLocations.Add(matchLocation));
        matchMouseUpLocations.ForEach(matchLocation => allMatchLocations.Add(matchLocation));

        return allMatchLocations;
    }

    void SearchCurrentLocationNeighborsForMatch(int currentRow, int currentCol, GameplayConstants.BlockType targetBlockType, int[,] searchedLocations, List<int> matchLocations)
    {
        // Search Up
        int searchRow = currentRow - 1;
        int searchCol = currentCol;

        if (searchRow >= 0 && searchRow < GRID_SIZE &&
            searchCol >= 0 && searchCol < GRID_SIZE &&
            searchedLocations[searchRow, searchCol] != 1)
        {
            SearchLocationRecursively(searchRow, searchCol, targetBlockType, searchedLocations, matchLocations);
        }

        // Search Down
        searchRow = currentRow + 1;
        searchCol = currentCol;

        if (searchRow >= 0 && searchRow < GRID_SIZE &&
            searchCol >= 0 && searchCol < GRID_SIZE &&
            searchedLocations[searchRow, searchCol] != 1)
        {
            SearchLocationRecursively(searchRow, searchCol, targetBlockType, searchedLocations, matchLocations);
        }

        // Search Left
        searchRow = currentRow;
        searchCol = currentCol - 1;

        if (searchRow >= 0 && searchRow < GRID_SIZE &&
            searchCol >= 0 && searchCol < GRID_SIZE &&
            searchedLocations[searchRow, searchCol] != 1)
        {
            SearchLocationRecursively(searchRow, searchCol, targetBlockType, searchedLocations, matchLocations);
        }

        // Search Right
        searchRow = currentRow;
        searchCol = currentCol + 1;

        if (searchRow >= 0 && searchRow < GRID_SIZE &&
            searchCol >= 0 && searchCol < GRID_SIZE &&
            searchedLocations[searchRow, searchCol] != 1)
        {
            SearchLocationRecursively(searchRow, searchCol, targetBlockType, searchedLocations, matchLocations);
        }
    }

    void SearchLocationRecursively(int searchRow, int searchCol, GameplayConstants.BlockType targetBlockType, int[,] searchedLocations, List<int> matchLocations)
    {
        searchedLocations[searchRow, searchCol] = 1;

        if (currentGameGridState[searchRow, searchCol] == targetBlockType)
        {
            matchLocations.Add(Convert2DLocationTo1DLocation(searchRow, searchCol));
            SearchCurrentLocationNeighborsForMatch(searchRow, searchCol, targetBlockType, searchedLocations, matchLocations);
        }
    }

    int Convert2DLocationTo1DLocation(int row, int col)
    {
        return row * GRID_SIZE + col;
    }

    void HandleSuccessfulMatchAttempt(List<int> matchLocations)
    {
        double multiplier = GetMatchMultiplier(matchLocations.Count);
        UpdateScore(matchLocations.Count, multiplier);
        RemoveMatchedBlocks(matchLocations);
        if (IsLevelComplete())
        {
            Debug.Log("YOU WIN!!");
            // Pause Timer
            // Show Level End Screen
        }
    }

    double GetMatchMultiplier(int matchCount)
    {
        switch (matchCount)
        {
            case >= 4 and <= 5:
                return GameplayConstants.SMALL_MULTIPLIER;

            case >= 6 and <= 7:
                return GameplayConstants.MEDIUM_MULTIPLIER;

            case >= 8:
                return GameplayConstants.LARGE_MULTIPLIER;

            default:
                return GameplayConstants.NO_MULTIPLIER;
        }
    }

    void UpdateScore(int matchCount, double multiplier)
    {
        int score = (int)(matchCount * GameplayConstants.POINTS_PER_BLOCK * multiplier);
        player.currentScore += score;
        ui.UpdatePointsProgressBar(player.currentScore);

        Debug.Log($"Player earned {score} points!");
    }

    void RemoveMatchedBlocks(List<int> matchLocations)
    {
        foreach (var matchLocation in matchLocations)
        {
            var (row, col) = Convert1DLocationTo2DLocation(matchLocation);
            currentGameGridState[row, col] = GameplayConstants.BlockType.None;
        }

        ui.UpdateGameGrid(currentGameGridState);
    }

    bool IsLevelComplete()
    {
        return player.currentScore >= level.scoreToComplete;
    }

    public void OnMouseDownEvent(MouseDownEvent evt, GameplayConstants.BlockType mouseDownBlockType, int mouseDownGridLocation)
    {
        currentMouseDownBlockType = mouseDownBlockType;
        currentMouseDownGridLocation = mouseDownGridLocation;
    }

    public void OnMouseUpEvent(MouseUpEvent evt, GameplayConstants.BlockType mouseUpBlockType, int mouseUpGridLocation)
    {
        HandlePlayerMatchAttempt(mouseUpBlockType, mouseUpGridLocation);
    }

    public void OnRestartGameEvent()
    {
        CreateCurrentGameGridState();
        ui.UpdateGameGrid(currentGameGridState);

        player.currentScore = 0;
        ui.UpdatePointsProgressBar(player.currentScore);
    }
}
