using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public TeamColor currentTurn = TeamColor.White;

    private void Awake()
    {
        Instance = this;
    }

    public void NextTurn()
    {
        currentTurn = currentTurn == TeamColor.White
            ? TeamColor.Black
            : TeamColor.White;
    }

    public bool IsPlayerTurn(ChessPiece piece)
    {
        return piece.team == currentTurn;
    }
}