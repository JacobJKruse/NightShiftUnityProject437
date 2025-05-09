using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public GameObject PlayerOneToken;
    public GameObject PlayerTwoToken;
    public GameObject[] spwanLoc;
    public GameObject exitLockUI;
    public GameObject portal;
    public DifficultyLevel difficulty = DifficultyLevel.Medium;

    private GameObject ghostToken;
    private int currentColumn = 0;
    private bool playerTurn = true;
    private bool isProcessingMove = false;
    private bool gameOver = false;
    private bool exitReached = false;
    private bool minigameActive = false;

    private const int height = 6;
    private const int length = 7;
    private int[,] boardIndex;
    public Image redScreen;
    public Image greenScreen;

    public float fadeDuration = 2f;
    private bool fadingInProgress = false;

    public enum DifficultyLevel { Easy = 2, Medium = 4, Hard = 6 }

    void Start()
    {
        boardIndex = new int[length, height];
        SpawnHoverToken();
        exitLockUI.SetActive(false);
        if (portal != null) portal.SetActive(false);

        string difficultySetting = PlayerPrefs.GetString("Difficulty", "Easy");
        switch (difficultySetting)
        {
            case "Easy": difficulty = DifficultyLevel.Easy; break;
            case "Medium": difficulty = DifficultyLevel.Medium; break;
            case "Hard": difficulty = DifficultyLevel.Hard; break;
            default: difficulty = DifficultyLevel.Medium; break;
        }

        if (redScreen != null)
        {
            Color screenColor = redScreen.color;
            screenColor.a = 0f;
            redScreen.color = screenColor;
        }
        if (greenScreen != null)
        {
            Color screenColor = greenScreen.color;
            screenColor.a = 0f;
            greenScreen.color = screenColor;
        }
    }

    void Update()
    {
        if (!playerTurn && !gameOver && !isProcessingMove && !minigameActive)
        {
            StartCoroutine(CPUTurn());
        }

        // Trigger fade if CPU wins and hasn't already started fading
        if (!playerTurn && !fadingInProgress && CheckWinForPlayer(2))
        {
            fadingInProgress = true;
            StartCoroutine(FadeToRedAndGameOver());
        }
        else if (playerTurn && !fadingInProgress && CheckWinForPlayer(1)) {
            fadingInProgress = true;
            StartCoroutine(FadeToGreenAndGameWin());
        }
    }

    private IEnumerator FadeToRedAndGameOver()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            redScreen.color = new Color(redScreen.color.r, redScreen.color.g, redScreen.color.b, alpha);
            yield return null;
        }
        SceneManager.LoadScene("GameOver");
        Debug.Log("I am DEAD");
    }

    private IEnumerator FadeToGreenAndGameWin()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            redScreen.color = new Color(greenScreen.color.r, greenScreen.color.g, greenScreen.color.b, alpha);
            yield return null;
        }
        SceneManager.LoadScene("Win");
        Debug.Log("I am DEAD");
    }

    IEnumerator CPUTurn()
    {
        isProcessingMove = true;
        yield return new WaitForSeconds(1f);

        int column = GetBestMove();
        TakeTurn(column);
    }

    int GetBestMove()
    {
        // First check for immediate win or block (this stays the same for all difficulties)
        int bestMove = CheckForImmediateWinOrBlock();
        if (bestMove != -1) return bestMove;

        // For Easy difficulty, add some randomness
        if (difficulty == DifficultyLevel.Easy)
        {
            // 40% chance to make a random valid move
            if (UnityEngine.Random.Range(0, 100) < 40)
            {
                return GetRandomValidMove();
            }
        }
        // For Medium difficulty, reduce search depth occasionally
        else if (difficulty == DifficultyLevel.Medium)
        {
            // 30% chance to use reduced depth
            if (UnityEngine.Random.Range(0, 100) < 30)
            {
                return GetMoveWithReducedDepth(2); // Reduce depth by 2
            }
        }

        else if (difficulty == DifficultyLevel.Medium)
        {
            // 15% chance to use reduced depth
            if (UnityEngine.Random.Range(0, 100) < 15)
            {
                return GetMoveWithReducedDepth(2); // Reduce depth by 2
            }

        }
            // Proceed with normal Minimax for the chosen difficulty
            return GetMinimaxMove();
    }

    int CheckForImmediateWinOrBlock()
    {
        // Check for immediate win (same as before)
        for (int col = 0; col < length; col++)
        {
            if (IsValidMove(col))
            {
                int row = GetLowestEmptyRow(col);
                boardIndex[col, row] = 2;
                if (CheckWinForPlayer(2))
                {
                    boardIndex[col, row] = 0;
                    return col;
                }
                boardIndex[col, row] = 0;
            }
        }

        // Block player if they can win next move
        for (int col = 0; col < length; col++)
        {
            if (IsValidMove(col))
            {
                int row = GetLowestEmptyRow(col);
                boardIndex[col, row] = 1;
                if (CheckWinForPlayer(1))
                {
                    boardIndex[col, row] = 0;
                    return col;
                }
                boardIndex[col, row] = 0;
            }
        }

        return -1; // No immediate win or block found
    }

    int GetRandomValidMove()
    {
        // Get all valid columns
        System.Collections.Generic.List<int> validColumns = new System.Collections.Generic.List<int>();
        for (int col = 0; col < length; col++)
        {
            if (IsValidMove(col)) validColumns.Add(col);
        }

        // Return a random valid column (prefer center if available)
        if (validColumns.Contains(3)) return 3;
        return validColumns[UnityEngine.Random.Range(0, validColumns.Count)];
    }

    int GetMoveWithReducedDepth(int depthReduction)
    {
        int bestScore = int.MinValue;
        int bestMove = 3;
        int currentDepth = (int)difficulty - depthReduction;
        currentDepth = Mathf.Max(currentDepth, 1); // Ensure at least depth 1

        for (int col = 0; col < length; col++)
        {
            if (IsValidMove(col))
            {
                int row = GetLowestEmptyRow(col);
                boardIndex[col, row] = 2;
                int score = Minimax(boardIndex, currentDepth, false, int.MinValue, int.MaxValue);
                boardIndex[col, row] = 0;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = col;
                }
            }
        }

        return bestMove;
    }

    int GetMinimaxMove()
    {
        // Original Minimax implementation
        int bestScore = int.MinValue;
        int bestMove = 3;

        for (int col = 0; col < length; col++)
        {
            if (IsValidMove(col))
            {
                int row = GetLowestEmptyRow(col);
                boardIndex[col, row] = 2;
                int score = Minimax(boardIndex, (int)difficulty, false, int.MinValue, int.MaxValue);
                boardIndex[col, row] = 0;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = col;
                }
            }
        }

        return bestMove;
    }

    int Minimax(int[,] board, int depth, bool isMaximizing, int alpha, int beta)
    {
        if (CheckWinForPlayer(2)) return 1000 + depth;
        if (CheckWinForPlayer(1)) return -1000 - depth;
        if (CheckDraw() || depth == 0) return EvaluateBoard();

        if (isMaximizing)
        {
            int bestScore = int.MinValue;
            for (int col = 0; col < length; col++)
            {
                if (IsValidMove(col))
                {
                    int row = GetLowestEmptyRow(col);
                    board[col, row] = 2;
                    int score = Minimax(board, depth - 1, false, alpha, beta);
                    board[col, row] = 0;
                    bestScore = Mathf.Max(score, bestScore);
                    alpha = Mathf.Max(alpha, score);
                    if (beta <= alpha) break;
                }
            }
            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;
            for (int col = 0; col < length; col++)
            {
                if (IsValidMove(col))
                {
                    int row = GetLowestEmptyRow(col);
                    board[col, row] = 1;
                    int score = Minimax(board, depth - 1, true, alpha, beta);
                    board[col, row] = 0;
                    bestScore = Mathf.Min(score, bestScore);
                    beta = Mathf.Min(beta, score);
                    if (beta <= alpha) break;
                }
            }
            return bestScore;
        }
    }

    int EvaluateBoard()
    {
        int score = 0;

        // Center column preference
        for (int row = 0; row < height; row++)
        {
            if (boardIndex[3, row] == 2) score += 3;
        }

        // Evaluate horizontal sequences
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < length - 3; col++)
            {
                score += EvaluateWindow(new int[] {
                    boardIndex[col, row],
                    boardIndex[col + 1, row],
                    boardIndex[col + 2, row],
                    boardIndex[col + 3, row]
                });
            }
        }

        // Evaluate vertical sequences
        for (int col = 0; col < length; col++)
        {
            for (int row = 0; row < height - 3; row++)
            {
                score += EvaluateWindow(new int[] {
                    boardIndex[col, row],
                    boardIndex[col, row + 1],
                    boardIndex[col, row + 2],
                    boardIndex[col, row + 3]
                });
            }
        }

        // Evaluate diagonal (positive slope)
        for (int col = 0; col < length - 3; col++)
        {
            for (int row = 0; row < height - 3; row++)
            {
                score += EvaluateWindow(new int[] {
                    boardIndex[col, row],
                    boardIndex[col + 1, row + 1],
                    boardIndex[col + 2, row + 2],
                    boardIndex[col + 3, row + 3]
                });
            }
        }

        // Evaluate diagonal (negative slope)
        for (int col = 0; col < length - 3; col++)
        {
            for (int row = 3; row < height; row++)
            {
                score += EvaluateWindow(new int[] {
                    boardIndex[col, row],
                    boardIndex[col + 1, row - 1],
                    boardIndex[col + 2, row - 2],
                    boardIndex[col + 3, row - 3]
                });
            }
        }

        return score;
    }

    int EvaluateWindow(int[] window)
    {
        int cpuCount = 0;
        int playerCount = 0;
        int emptyCount = 0;

        foreach (int cell in window)
        {
            if (cell == 2) cpuCount++;
            else if (cell == 1) playerCount++;
            else emptyCount++;
        }

        if (cpuCount == 4) return 100;
        else if (cpuCount == 3 && emptyCount == 1) return 5;
        else if (cpuCount == 2 && emptyCount == 2) return 2;
        else if (playerCount == 3 && emptyCount == 1) return -4;
        else if (playerCount == 4) return -100;

        return 0;
    }

    bool IsValidMove(int column)
    {
        return boardIndex[column, height - 1] == 0;
    }

    int GetLowestEmptyRow(int column)
    {
        for (int row = 0; row < height; row++)
        {
            if (boardIndex[column, row] == 0)
            {
                return row;
            }
        }
        return -1;
    }

    bool CheckWinForPlayer(int player)
    {
        // Horizontal
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < length - 3; col++)
            {
                if (boardIndex[col, row] == player &&
                    boardIndex[col + 1, row] == player &&
                    boardIndex[col + 2, row] == player &&
                    boardIndex[col + 3, row] == player)
                {
                    return true;
                }
            }
        }

        // Vertical
        for (int col = 0; col < length; col++)
        {
            for (int row = 0; row < height - 3; row++)
            {
                if (boardIndex[col, row] == player &&
                    boardIndex[col, row + 1] == player &&
                    boardIndex[col, row + 2] == player &&
                    boardIndex[col, row + 3] == player)
                {
                    return true;
                }
            }
        }

        // Diagonal (positive slope)
        for (int col = 0; col < length - 3; col++)
        {
            for (int row = 0; row < height - 3; row++)
            {
                if (boardIndex[col, row] == player &&
                    boardIndex[col + 1, row + 1] == player &&
                    boardIndex[col + 2, row + 2] == player &&
                    boardIndex[col + 3, row + 3] == player)
                {
                    return true;
                }
            }
        }

        // Diagonal (negative slope)
        for (int col = 0; col < length - 3; col++)
        {
            for (int row = 3; row < height; row++)
            {
                if (boardIndex[col, row] == player &&
                    boardIndex[col + 1, row - 1] == player &&
                    boardIndex[col + 2, row - 2] == player &&
                    boardIndex[col + 3, row - 3] == player)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void SelectColum(int colum)
    {
        if (isProcessingMove || gameOver || !playerTurn || minigameActive) return;
        TakeTurn(colum);
    }

    public void SetHoverColumn(int colum)
    {
        if (gameOver || minigameActive) return;
        currentColumn = colum;
        MoveHoverToken();
    }

    private void TakeTurn(int colum)
    {
        if (!UpdateBoard(colum)) return;

        isProcessingMove = true;

        if (ghostToken != null)
        {
            Destroy(ghostToken);
            ghostToken = null;
        }

        GameObject tokenPrefab = playerTurn ? PlayerOneToken : PlayerTwoToken;
        GameObject fallingToken = Instantiate(tokenPrefab, spwanLoc[colum].transform.position, Quaternion.identity);

        StartCoroutine(WaitForTokenToLand(fallingToken));
    }

    private IEnumerator WaitForTokenToLand(GameObject token)
    {
        Rigidbody rb = token.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Wait until it's asleep
            while (!rb.IsSleeping())
            {
                yield return null;
            }

            // Disable physics and interaction to simulate permanent sleep
            rb.isKinematic = true;
            rb.detectCollisions = true;
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (CheckWin())
        {
            Debug.Log("Player " + (playerTurn ? "1" : "2") + " wins!");
            gameOver = true;
            if (ghostToken != null) Destroy(ghostToken);

            if (playerTurn) // Player 1 won
            {
                exitReached = true;
                StartCoroutine(ActivateExitLock());
            }
            else
            {
                FadeToRedAndGameOver();
            }

                yield break;
        }

        if (CheckDraw())
        {
            Debug.Log("Game ended in a draw!");
            gameOver = true;
            if (ghostToken != null) Destroy(ghostToken);
            yield break;
        }

        playerTurn = !playerTurn;
        isProcessingMove = false;
        UpdateHoverToken();
    }



    IEnumerator ActivateExitLock()
    {
        yield return new WaitForSeconds(2f);
        minigameActive = true;
        exitLockUI.SetActive(true);
        Debug.Log("Exit lock activated! Difficulty: " + difficulty);
    }

    public void SetDifficulty(int level)
    {
        difficulty = (DifficultyLevel)level;
        Debug.Log("Difficulty set to: " + difficulty);
    }

    public void OnMinigameComplete(bool success)
    {
        minigameActive = false;
        exitLockUI.SetActive(false);

        if (success)
        {
            Debug.Log("Lock opened! Portal activated!");
            if (portal != null) portal.SetActive(true);
        }
        else
        {
            FadeToRedAndGameOver();
        }
    }

    bool UpdateBoard(int colum)
    {
        for (int i = 0; i < height; i++)
        {
            if (boardIndex[colum, i] == 0)
            {
                boardIndex[colum, i] = playerTurn ? 1 : 2;
                return true;
            }
        }
        Debug.Log("Column " + colum + " is full!");
        return false;
    }

    bool CheckWin()
    {
        int player = playerTurn ? 1 : 2;
        return CheckWinForPlayer(player);
    }

    bool CheckDraw()
    {
        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (boardIndex[x, y] == 0)
                {
                    return false;
                }
            }
        }
        return true;
    }

    void SpawnHoverToken()
    {
        if (isProcessingMove || gameOver || minigameActive) return;

        if (ghostToken != null)
        {
            Destroy(ghostToken);
        }

        GameObject tokenPrefab = playerTurn ? PlayerOneToken : PlayerTwoToken;
        ghostToken = Instantiate(tokenPrefab, GetHoverPosition(currentColumn), Quaternion.identity);

        Collider col = ghostToken.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = ghostToken.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    void UpdateHoverToken()
    {
        if (playerTurn) // Only spawn hover token during player's turn
        {
            SpawnHoverToken();
        }
        else if (ghostToken != null) // Destroy ghost token if it's CPU's turn
        {
            Destroy(ghostToken);
            ghostToken = null;
        }
    }

    void MoveHoverToken()
    {
        if (ghostToken != null)
        {
            ghostToken.transform.position = GetHoverPosition(currentColumn);
        }
        else
        {
            SpawnHoverToken();
        }
    }

    Vector3 GetHoverPosition(int colum)
    {
        Vector3 basePos = spwanLoc[colum].transform.position;
        return new Vector3(basePos.x, basePos.y, basePos.z);
    }

    

}