using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    private ChessPiece selectedPiece;

    private List<Vector2Int> moves;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos =
                Camera.main
                .ScreenToWorldPoint(
                    Input.mousePosition
                );

            RaycastHit2D hit =
                Physics2D.Raycast(
                    mousePos,
                    Vector2.zero
                );

            if (hit.collider != null)
            {
                ChessPiece piece =
                    hit.collider
                    .GetComponent<ChessPiece>();

                Tile tile =
                    hit.collider
                    .GetComponent<Tile>();

                if (piece != null)
                {
                    SelectPiece(piece);
                }
                else if (tile != null)
                {
                    TryMove(tile.boardPosition);
                }
            }
        }
    }

    void SelectPiece(ChessPiece piece)
    {
        if (!TurnManager.Instance
            .IsPlayerTurn(piece.team))
            return;

        selectedPiece = piece;

        moves =
            piece.GetAvailableMoves(
                BoardManager.Instance.chessPieces,
                8,
                8
            );

        HighlightMoves();
    }

    void TryMove(Vector2Int target)
    {
        if (selectedPiece == null)
            return;

        if (moves.Contains(target))
        {
            BoardManager.Instance
                .MovePiece(
                    selectedPiece,
                    target
                );
        }

        ClearHighlights();

        selectedPiece = null;
    }

    void HighlightMoves()
    {
        foreach (Vector2Int move in moves)
        {
            BoardManager.Instance
                .GetTile(move)
                .Highlight();
        }
    }

    void ClearHighlights()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                BoardManager.Instance
                    .GetTile(
                        new Vector2Int(x,y)
                    )
                    .ResetColor();
            }
        }
    }
}