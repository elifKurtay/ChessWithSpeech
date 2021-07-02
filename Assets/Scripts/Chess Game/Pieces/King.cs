using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
    private Vector2Int[] directions = new Vector2Int[] { new Vector2Int(1,1), new Vector2Int(1, -1),
                                                        new Vector2Int(-1,1), new Vector2Int(-1,-1),
                                                        Vector2Int.left, Vector2Int.right,
                                                        Vector2Int.up, Vector2Int.down};

    public override List<Vector2Int> SelectAvaliableSquares()
    {
        avaliableMoves.Clear();
        
        Vector2Int nextCoords;
        Piece currentPiece;
        foreach (var direction in directions)
        {
            nextCoords = occupiedSquare + direction;
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
        return avaliableMoves;
    }
}
