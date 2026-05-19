using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(
        ChessPiece[,] board,
        int boardSizeX,
        int boardSizeY)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        Vector2Int[] offsets =
        {
            new Vector2Int(1,2),
            new Vector2Int(2,1),
            new Vector2Int(-1,2),
            new Vector2Int(-2,1),

            new Vector2Int(1,-2),
            new Vector2Int(2,-1),
            new Vector2Int(-1,-2),
            new Vector2Int(-2,-1)
        };

        foreach (var offset in offsets)
        {
            Vector2Int target = boardPosition + offset;

            if (!IsInsideBoard(target, boardSizeX, boardSizeY))
                continue;

            ChessPiece piece = board[target.x, target.y];

            if (piece == null || piece.team != team)
            {
                moves.Add(target);
            }
        }

        return moves;
    }
}