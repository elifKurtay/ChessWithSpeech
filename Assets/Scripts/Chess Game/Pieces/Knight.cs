using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : Piece
{
	Vector2Int[] offsets = new Vector2Int[]
	{
		new Vector2Int(2, 1),
		new Vector2Int(2, -1),
		new Vector2Int(-2, 1),
		new Vector2Int(-2, -1),
		new Vector2Int(1, 2),
		new Vector2Int(1, -2),
		new Vector2Int(-1, 2),
		new Vector2Int(-1, -2)
	};

 	public override List<Vector2Int> SelectAvaliableSquares()
	{
		avaliableMoves.Clear();

		Vector2Int nextCoords;
		Piece currentPiece;
		for(int i = 0; i < offsets.Length; i++)
        {
			nextCoords = occupiedSquare + offsets[i];
			if (!board.CheckIfCoordinatedAreOnBoard(nextCoords))
				continue;
			
			currentPiece = board.GetPieceOnSquare(nextCoords);
			if (currentPiece == null || !currentPiece.IsFromSameTeam(this))
				TryToAddMove(nextCoords);
		}
		return avaliableMoves;
	}
}
