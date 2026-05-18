using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public ChessPiece[,] chessPieces = new ChessPiece[8,8];

    
    [SerializeField]
    private GameObject whiteQueenPrefab;

    [SerializeField]
    private GameObject blackQueenPrefab;
    
    private void Awake()
    {
        
        Instance = this;
    }

    public bool IsInsideBoard(Vector2Int pos)
    {
        return pos.x >= 0 &&
               pos.x < 8 &&
               pos.y >= 0 &&
               pos.y < 8;
    }

    public ChessPiece GetPiece(Vector2Int pos)
    {
        return chessPieces[pos.x, pos.y];
    }

    public void MovePiece(
        ChessPiece piece,
        Vector2Int targetPos
    )
    {
        chessPieces[
            piece.boardPosition.x,
            piece.boardPosition.y
        ] = null;

        ChessPiece target =
            chessPieces[targetPos.x, targetPos.y];

        if (target != null)
        {
            Destroy(target.gameObject);
        }

        piece.Move(targetPos);

        chessPieces[
            targetPos.x,
            targetPos.y
        ] = piece;

        CheckPromotion(piece);

        TeamColor enemyTeam =
            piece.team == TeamColor.White
                ? TeamColor.Black
                : TeamColor.White;

        if (CheckForCheckmate(enemyTeam))
        {
            Debug.Log("XEQUE-MATE");
        }
        else if (IsKingInCheck(enemyTeam))
        {
            Debug.Log("XEQUE");
        }

        TurnManager.Instance.EndTurn();
    }
    public bool IsKingInCheck(TeamColor kingTeam)
    {
        ChessPiece king = null;

        foreach (ChessPiece piece in chessPieces)
        {
            if (piece != null &&
                piece.team == kingTeam &&
                piece is King)
            {
                king = piece;
                break;
            }
        }

        if (king == null)
            return false;

        foreach (ChessPiece piece in chessPieces)
        {
            if (piece != null &&
                piece.team != kingTeam)
            {
                List<Vector2Int> moves =
                    piece.GetAvailableMoves(
                        chessPieces,
                        8,
                        8
                    );

                if (moves.Contains(king.boardPosition))
                {
                    return true;
                }
            }
        }

        return false;
    }
    public bool SimulateMoveForSinglePiece(
        ChessPiece piece,
        Vector2Int targetPos
    )
    {
        Vector2Int originalPos =
            piece.boardPosition;

        ChessPiece targetPiece =
            chessPieces[targetPos.x, targetPos.y];

        chessPieces[
            originalPos.x,
            originalPos.y
        ] = null;

        piece.boardPosition = targetPos;

        chessPieces[
            targetPos.x,
            targetPos.y
        ] = piece;

        bool kingInCheck =
            IsKingInCheck(piece.team);

        piece.boardPosition = originalPos;

        chessPieces[
            originalPos.x,
            originalPos.y
        ] = piece;

        chessPieces[
            targetPos.x,
            targetPos.y
        ] = targetPiece;

        return !kingInCheck;
    }
    public bool CheckForCheckmate(TeamColor team)
    {
        if (!IsKingInCheck(team))
            return false;

        foreach (ChessPiece piece in chessPieces)
        {
            if (piece == null ||
                piece.team != team)
                continue;

            List<Vector2Int> moves =
                piece.GetAvailableMoves(
                    chessPieces,
                    8,
                    8
                );

            foreach (Vector2Int move in moves)
            {
                if (SimulateMoveForSinglePiece(
                        piece,
                        move))
                {
                    return false;
                }
            }
        }

        return true;
    }
    public void CheckPromotion(ChessPiece piece)
    {
        if (!(piece is Pawn))
            return;

        if (piece.team == TeamColor.White &&
            piece.boardPosition.y == 7)
        {
            PromotePawn(piece);
        }

        if (piece.team == TeamColor.Black &&
            piece.boardPosition.y == 0)
        {
            PromotePawn(piece);
        }
    }
    void PromotePawn(ChessPiece pawn)
    {
        Vector2Int pos =
            pawn.boardPosition;

        GameObject prefab =
            pawn.team == TeamColor.White
                ? whiteQueenPrefab
                : blackQueenPrefab;

        Destroy(pawn.gameObject);

        GameObject newQueen =
            Instantiate(
                prefab,
                new Vector3(pos.x, 0, pos.y),
                Quaternion.identity
            );

        ChessPiece queen =
            newQueen.GetComponent<ChessPiece>();

        queen.team = pawn.team;
        queen.boardPosition = pos;

        chessPieces[pos.x, pos.y] = queen;
    }
}