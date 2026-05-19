using System.Collections.Generic;
using UnityEngine;

public class Rook : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(
        ChessPiece[,] board,
        int boardSizeX,
        int boardSizeY)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var dir in directions)
        {
            for (int i = 1; i < 8; i++)
            {
                Vector2Int target = boardPosition + dir * i;

                if (!IsInsideBoard(target, boardSizeX, boardSizeY))
                    break;

                ChessPiece piece = board[target.x, target.y];

                if (piece == null)
                {
                    moves.Add(target);
                }
                else
                {
                    if (piece.team != team)
                        moves.Add(target);

                    break;
                }
            }
        }

        return moves;
    }
}