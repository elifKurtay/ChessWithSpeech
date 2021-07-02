using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : Piece
{
    private Vector2Int[] directions = new Vector2Int[] { new Vector2Int(1,1), new Vector2Int(1, -1),
                                                        new Vector2Int(-1,1), new Vector2Int(-1,-1)};

    public override List<Vector2Int> SelectAvaliableSquares()
    {
        avaliableMoves.Clear();

        float range = Board.BOARD_SIZE;
        Vector2Int nextCoords;
        Piece currentPiece;
        foreach (var direction in directions)
        {
            for (int i = 1; i <= range; i++)
            {
                nextCoords = occupiedSquare + direction * i;
                if (!board.CheckIfCoordinatedAreOnBoard(nextCoords))
                    break;

                currentPiece = board.GetPieceOnSquare(nextCoords);
                if (currentPiece == null)
                    TryToAddMove(nextCoords);
                else if (!currentPiece.IsFromSameTeam(this))
                {
                    TryToAddMove(nextCoords);
                    break;
                }
                else if (currentPiece.IsFromSameTeam(this))
                    break;
            }
        }
        return avaliableMoves;
    }
}
