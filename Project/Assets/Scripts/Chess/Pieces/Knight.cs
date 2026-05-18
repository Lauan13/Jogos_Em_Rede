using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(
        ChessPiece[,] board,
        int tileCountX,
        int tileCountY
    )
    {
        List<Vector2Int> moves =
            new List<Vector2Int>();

        Vector2Int[] positions =
        {
            new Vector2Int(1,2),
            new Vector2Int(-1,2),
            new Vector2Int(1,-2),
            new Vector2Int(-1,-2),

            new Vector2Int(2,1),
            new Vector2Int(-2,1),
            new Vector2Int(2,-1),
            new Vector2Int(-2,-1)
        };

        foreach (Vector2Int pos in positions)
        {
            AddMoveIfValid(
                board,
                moves,
                boardPosition + pos
            );
        }

        return moves;
    }
}