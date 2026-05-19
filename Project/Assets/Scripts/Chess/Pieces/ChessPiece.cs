using System.Collections.Generic;
using Chess.Pieces;
using UnityEngine;

public abstract class ChessPiece : MonoBehaviour
{
    public ChessPieceType pieceType;
    public TeamColor team;

    public Vector2Int boardPosition;

    public bool hasMoved = false;

    public abstract List<Vector2Int> GetAvailableMoves(
        ChessPiece[,] board,
        int boardSizeX,
        int boardSizeY
    );

    protected bool IsInsideBoard(Vector2Int pos, int sizeX, int sizeY)
    {
        return pos.x >= 0 &&
               pos.y >= 0 &&
               pos.x < sizeX &&
               pos.y < sizeY;
    }
}