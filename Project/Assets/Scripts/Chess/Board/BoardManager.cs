using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public int sizeX = 8;
    public int sizeY = 8;

    public ChessPiece[,] board;

    private void Awake()
    {
        Instance = this;

        board = new ChessPiece[sizeX, sizeY];
    }

    public void MovePiece(
        ChessPiece piece,
        Vector2Int targetPosition)
    {
        board[piece.boardPosition.x,
            piece.boardPosition.y] = null;

        ChessPiece targetPiece =
            board[targetPosition.x, targetPosition.y];

        // captura
        if (targetPiece != null)
        {
            Destroy(targetPiece.gameObject);
        }

        piece.boardPosition = targetPosition;

        board[targetPosition.x,
            targetPosition.y] = piece;

        piece.transform.position =
            new Vector3(targetPosition.x, targetPosition.y);

        piece.hasMoved = true;

        TurnManager.Instance.NextTurn();
    }
}