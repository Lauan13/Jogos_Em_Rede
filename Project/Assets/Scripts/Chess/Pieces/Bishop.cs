using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(
        ChessPiece[,] board,
        int tileCountX,
        int tileCountY
    )
    {
        List<Vector2Int> moves =
            new List<Vector2Int>();

        Vector2Int[] dirs =
        {
            new Vector2Int(1,1),
            new Vector2Int(-1,1),
            new Vector2Int(1,-1),
            new Vector2Int(-1,-1)
        };

        foreach (Vector2Int dir in dirs)
        {
            for (int i = 1; i < 8; i++)
            {
                Vector2Int target =
                    boardPosition + dir * i;

                if (!IsInsideBoard(target))
                    break;

                ChessPiece piece =
                    board[target.x, target.y];

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