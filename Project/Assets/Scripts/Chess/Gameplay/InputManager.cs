using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    private ChessPiece selectedPiece;

    private List<Vector2Int> availableMoves;

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(
                Input.mousePosition
            );

            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector2Int clickedPos = new Vector2Int(
                    Mathf.RoundToInt(hit.point.x),
                    Mathf.RoundToInt(hit.point.z)
                );

                HandleClick(clickedPos);
            }
        }
    }

    void HandleClick(Vector2Int pos)
    {
        ChessPiece piece =
            BoardManager.Instance.GetPiece(pos);

        // Selecionar peça
        if(selectedPiece == null)
        {
            if(piece != null &&
               TurnManager.Instance.IsPlayerTurn(piece.team))
            {
                selectedPiece = piece;

                availableMoves =
                    piece.GetAvailableMoves(
                        BoardManager.Instance.chessPieces,
                        8,
                        8
                    );

                Debug.Log("Peça selecionada");
            }
        }
        else
        {
            // Mover
            if(availableMoves.Contains(pos))
            {
                BoardManager.Instance.MovePiece(
                    selectedPiece,
                    pos
                );
            }

            selectedPiece = null;
        }
    }
}