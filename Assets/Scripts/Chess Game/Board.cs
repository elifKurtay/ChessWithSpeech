using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SquareSelectorCreator))]
public class Board : MonoBehaviour
{
    public const int BOARD_SIZE = 8;

    [SerializeField] private Transform bottomLeftSquareTransform;
    [SerializeField] private float squareSize;

    private Piece[,] grid;
    private Piece selectedPiece;
    private ChessGameController chessGameController;

    private void Awake()
    {
        grid = new Piece[BOARD_SIZE, BOARD_SIZE];
    }

    public void SetDependencies(ChessGameController chessGameController)
    {
        this.chessGameController = chessGameController;
    }

    public Vector3 CalculatePositionFromCoords(Vector2Int coords)
    {
        return bottomLeftSquareTransform.position + new Vector3(coords.x * squareSize, 0f, coords.y * squareSize);
    }

    public void OnSquareSelected(Vector3 inputPosition)
    {
        Vector2Int coords = CalculateCoordsFromPosition( inputPosition);
        Piece piece = GetPieceOnSquare(coords); 
        if( selectedPiece)
        {
            if (piece != null && selectedPiece == piece)
                DeselectPiece();
            else if (piece != null && selectedPiece != piece && chessGameController.IsTeamTurnActive(piece.team))
                SelectPiece(piece);
            else if (selectedPiece.CanMoveTo(coords))
                OnSelectedPieceMoved(coords, selectedPiece);
        }
        else
        {
            if (piece != null && chessGameController.IsTeamTurnActive(piece.team))
                SelectPiece(piece);
        }
    }

    private void SelectPiece(Piece piece)
    {
        selectedPiece = piece;
    }

    private void DeselectPiece()
    {
        selectedPiece = null;
    }

    private void OnSelectedPieceMoved( Vector2Int coords, Piece piece)
    {
        UpdateBoardOnPieceMove(coords, piece.occupiedSquare, piece, null);
        selectedPiece.MovePiece(coords);
        DeselectPiece();
        EndTurn();
    }

    private void EndTurn()
    {
        chessGameController.EndTurn();
    }

    private void UpdateBoardOnPieceMove(Vector2Int newCoords, Vector2Int oldCoords, Piece newPiece, Piece oldPiece)
    {
        grid[oldCoords.x, oldCoords.y] = oldPiece;
        grid[newCoords.x, newCoords.y] = newPiece;
    }
    public bool HasPiece(Piece piece)
    {
        for(int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (grid[i, j] == piece)
                    return true;
            }
        }
        return false;
    }

    private Piece GetPieceOnSquare(Vector2Int coords)
    {
        if (CheckIfCoordinatedAreOnBoard(coords))
            return grid[coords.x, coords.y];
        return null;
    }

    private bool CheckIfCoordinatedAreOnBoard(Vector2Int coords)
    {
        if (coords.x < 0 || coords.y < 0 || coords.x >= BOARD_SIZE || coords.y >= BOARD_SIZE)
            return false;
        return true;
    }

    private Vector2Int CalculateCoordsFromPosition(Vector3 inputPosition)
    {
        int x = Mathf.FloorToInt(inputPosition.x / squareSize) + BOARD_SIZE / 2;
        int y = Mathf.FloorToInt(inputPosition.z / squareSize) + BOARD_SIZE / 2;
        return new Vector2Int(x, y);
    }
}
