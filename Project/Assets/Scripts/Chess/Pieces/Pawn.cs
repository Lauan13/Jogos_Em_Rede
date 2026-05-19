using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(
        ChessPiece[,] board,
        int boardSizeX,
        int boardSizeY)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        int direction = team == TeamColor.White ? 1 : -1;

        Vector2Int forward = new Vector2Int(
            boardPosition.x,
            boardPosition.y + direction
        );

        // andar para frente
        if (IsInsideBoard(forward, boardSizeX, boardSizeY) &&
            board[forward.x, forward.y] == null)
        {
            moves.Add(forward);

            // primeiro movimento
            Vector2Int doubleForward = new Vector2Int(
                boardPosition.x,
                boardPosition.y + direction * 2
            );

            if (!hasMoved &&
                board[doubleForward.x, doubleForward.y] == null)
            {
                moves.Add(doubleForward);
            }
        }

        // capturas diagonais
        Vector2Int[] diagonals =
        {
            new Vector2Int(boardPosition.x - 1, boardPosition.y + direction),
            new Vector2Int(boardPosition.x + 1, boardPosition.y + direction)
        };

        foreach (var diag in diagonals)
        {
            if (!IsInsideBoard(diag, boardSizeX, boardSizeY))
                continue;

            ChessPiece target = board[diag.x, diag.y];

            if (target != null && target.team != team)
            {
                moves.Add(diag);
            }
        }

        return moves;
    }
}