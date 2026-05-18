using System.Collections.Generic;
using UnityEngine;

public abstract class ChessPiece : MonoBehaviour
{
    public TeamColor team;

    public Vector2Int boardPosition;

    public bool hasMoved = false;

    public abstract List<Vector2Int> GetAvailableMoves(
        ChessPiece[,] board,
        int tileCountX,
        int tileCountY
    );

    public virtual void Move(Vector2Int newPos)
    {
        boardPosition = newPos;

        transform.position = new Vector3(
            newPos.x,
            0,
            newPos.y
        );

        hasMoved = true;
    }

    protected bool IsInsideBoard(Vector2Int pos)
    {
        return pos.x >= 0 &&
               pos.x < 8 &&
               pos.y >= 0 &&
               pos.y < 8;
    }

    protected void AddMoveIfValid(
        ChessPiece[,] board,
        List<Vector2Int> moves,
        Vector2Int target
    )
    {
        if (!IsInsideBoard(target))
            return;

        ChessPiece piece =
            board[target.x, target.y];

        if (piece == null)
        {
            moves.Add(target);
        }
        else if (piece.team != team)
        {
            moves.Add(target);
        }
    }
}