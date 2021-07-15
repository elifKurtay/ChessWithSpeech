using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PiecesCreator))]
public class ChessGameController : MonoBehaviour
{
    private enum GameState { Init, Play, Finished }

    [SerializeField] private BoardLayout startingBoardLayout;
    [SerializeField] private Board board;
    [SerializeField] private ChessUIManager UIManager;

    private PiecesCreator pieceCreator;
    private ChessPlayer whitePlayer;
    private ChessPlayer blackPlayer;
    private ChessPlayer activePlayer;
    private GameState state;

    private void Awake()
    {
        SetDependencies();
        CreatePlayer();
    }

    private void SetDependencies()
    {
        pieceCreator = GetComponent<PiecesCreator>();
    }

    private void CreatePlayer()
    {
        whitePlayer = new ChessPlayer(TeamColor.White, board);
        blackPlayer = new ChessPlayer(TeamColor.Black, board);
    }
  
    private void Start()
    {
        StartNewGame();
    }

    private void StartNewGame()
    {
        UIManager.HideUI();
        SetGameState(GameState.Init);
        board.SetDependencies(this);
        CreatePiecesFromLayout(startingBoardLayout);
        activePlayer = whitePlayer;
        GenerateAllPossiblePlayerMoves(activePlayer);
        SetGameState(GameState.Play);
    }

    private void SetGameState(GameState state)
    {
        this.state = state;
    }

    public bool IsGameInProgress()
    {
        return state == GameState.Play;
    }

    private void CreatePiecesFromLayout(BoardLayout layout)
    {
        for (int i = 0; i < layout.GetPiecesCount(); i++)
        {
            Vector2Int squareCoords = layout.GetSquareCoordsAtIndex(i);
            TeamColor team = layout.GetSquareTeamColorAtIndex(i);
            string typeName = layout.GetSquarePieceNameAtIndex(i);

            Type type = Type.GetType(typeName);
            CreatePieceAndInitialize(squareCoords, team, type);
        }
    }

    public bool IsTeamTurnActive(TeamColor team)
    {
        return activePlayer.team == team;
    }

    public void CreatePieceAndInitialize(Vector2Int squareCoords, TeamColor team, Type type)
    {
        Piece newPiece = pieceCreator.CreatePiece(type).GetComponent<Piece>();
        newPiece.SetData(squareCoords, team, board);

        Material teamMaterial = pieceCreator.GetTeamMaterial(team);
        newPiece.SetMaterial(teamMaterial);

        board.SetPieceOnBoard(squareCoords, newPiece);

        ChessPlayer currentPlayer = team == TeamColor.White ? whitePlayer : blackPlayer;
        currentPlayer.AddPiece(newPiece);
    }

    private bool GenerateAllPossiblePlayerMoves(ChessPlayer player)
    {
        return player.GenerateAllPossibleMoves();
    }

    public void EndTurn()
    {
        Debug.Log("Player turn ended."); 
        GenerateAllPossiblePlayerMoves(activePlayer);

        //checking for stalemate
        if ( !GenerateAllPossiblePlayerMoves(GetOpponentToPlayer(activePlayer)))
            EndGameWithDraw();

        if (CheckIfGameIsFinished())
            EndGame();
        else
            ChangeActiveTeam();
    }

    private bool CheckIfGameIsFinished()
    {
        Piece[] kingAttackingPieces = activePlayer.GetPiecesAttackingOppositePieceOfType<King>();
        if (kingAttackingPieces.Length > 0)
        {
            ChessPlayer oppositePlayer = GetOpponentToPlayer(activePlayer);
            Piece attackedKing = oppositePlayer.GetPiecesOfType<King>()[0];
            oppositePlayer.RemoveMovesEnablingAttackOnPiece<King>(activePlayer, attackedKing);

            int avaliableKingMoves = attackedKing.avaliableMoves.Count;
            if(avaliableKingMoves == 0)
            {
                bool canCoverKing = oppositePlayer.CanHidePieceFromAttack<King>(activePlayer);
                if (!canCoverKing)
                    return true;
            }
        }
        return false;
    }

    private void EndGameWithDraw()
    {
        Debug.Log("Game Ended With Stalemate.");
        SetGameState(GameState.Finished);
        UIManager.OnGameFinishedWithStalemate();

    }

    private void EndGame()
    {
        Debug.Log("Game Ended.");
        SetGameState(GameState.Finished);
        UIManager.OnGameFinished(activePlayer.team.ToString());
    }

    private ChessPlayer GetOpponentToPlayer(ChessPlayer player)
    {
        return player == whitePlayer ? blackPlayer : whitePlayer;
    }

    private void ChangeActiveTeam()
    {
        activePlayer = activePlayer == whitePlayer ? blackPlayer : whitePlayer;
    }

    public void RemoveMovesEnablingAttackOnPieceOfType<T>(Piece piece) where T : Piece
    {
        activePlayer.RemoveMovesEnablingAttackOnPiece<T>(GetOpponentToPlayer(activePlayer), piece);
    }

    public void OnPieceRemoved(Piece piece)
    {
        ChessPlayer pieceOwner = (piece.team == TeamColor.White) ? whitePlayer : blackPlayer;
        pieceOwner.RemovePiece(piece);
        Destroy(piece.gameObject);
    }

    public void RestartGame()
    {
        DestroyPieces();
        board.OnGameRestarted();
        whitePlayer.OnGameRestarted();
        blackPlayer.OnGameRestarted();
        StartNewGame();
    }

    private void DestroyPieces()
    {
        whitePlayer.activePieces.ForEach(p => Destroy(p.gameObject));
        blackPlayer.activePieces.ForEach(p => Destroy(p.gameObject));
    }

    public void FindAndMovePiece(string pieceType, string goal)
    {
        Piece[] avaliablePieces = null;
        Vector2Int coords = new Vector2Int();
        coords.x = (goal[0] - 'a')  ;
        coords.y = goal[1] - '0' - 1;
        Debug.LogFormat("The piece should be moved to coords ({0}, {1})", coords.x, coords.y);

        switch (pieceType)
        {
            case "king":
                avaliablePieces = activePlayer.GetPiecesOfType<King>();
                break;
            case "queen":
                avaliablePieces = activePlayer.GetPiecesOfType<Queen>();
                break;
            case "bishop":
                avaliablePieces = activePlayer.GetPiecesOfType<Bishop>();
                break;
            case "knight":
                avaliablePieces = activePlayer.GetPiecesOfType<Knight>();
                break;
            case "rook":
                avaliablePieces = activePlayer.GetPiecesOfType<Rook>();
                break;
            case "pawn":
                avaliablePieces = activePlayer.GetPiecesOfType<Pawn>();
                break;
            default: break;
        }
        
        if (avaliablePieces.Length < 1)
            return;
        Debug.Log("found avaliable pieces");
       
        foreach( var piece in avaliablePieces)
        {
            if (piece.CanMoveTo(coords))
            {
                Debug.Log("found the piece to move");
                board.SelectPiece(piece);
                board.OnSelectedPieceMoved(coords, piece);

            }
        }
        
    }
}

