using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/*
 * This singleton manages the whole chess game
 *  - board data (see BoardState class)
 *  - piece models instantiation
 *  - player interactions (piece grab, drag and release)
 *  - AI update calls (see UpdateAITurn and ChessAI class)
 */

public partial class ChessGameManager : MonoBehaviour
{

    #region Singleton
    static ChessGameManager instance = null;
    public static ChessGameManager Instance {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<ChessGameManager>();
            return instance;
        }
    }
    #endregion

    [SerializeField]
    private bool isAIEnabled = true;

    private ChessAI chessAI = null;
    private Transform boardTransform = null;
    private static int BOARD_SIZE = 8;
    private int pieceLayerMask;
    private int boardLayerMask;
    
    [SerializeField] private Material whitesMaterial;
    [SerializeField] private Material blacksMaterial;
    [SerializeField] private ColorPicker colorPicker;
    [SerializeField] private ChatManager chatManager;
    private Client client;
    private bool isPlayingBlacks = false;

    #region Enums
    public enum EPieceType : uint
    {
        Pawn = 0,
        King,
        Queen,
        Rook,
        Knight,
        Bishop,
        NbPieces,
        None
    }

    public enum EChessTeam
    {
        White = 0,
        Black,
        None
    }

    public enum ETeamFlag : uint
    {
        None = 1 << 0,
        Friend = 1 << 1,
        Enemy = 1 << 2
    }
    #endregion

    #region Structs & Classes
    public struct BoardSquare
    {
        public EPieceType piece;
        public EChessTeam team;

        public BoardSquare(EPieceType p, EChessTeam t)
        {
            piece = p;
            team = t;
        }

        static public BoardSquare Empty()
        {
            BoardSquare res;
            res.piece = EPieceType.None;
            res.team = EChessTeam.None;
            return res;
        }
    }

    public struct Move
    {
        public int from;
        public int to;

        public Move(int _from, int _to)
        {
            from = _from;
            to   = _to;
        }

        public Move(int[] fromTo)
        {
            if (fromTo.Length != 2)
            {
                from = -1;
                to   = -1;
            }
            else
            {
                from = fromTo[0];
                to   = fromTo[1];
            }
        }

        public Move Mirror()
        {
            int fromRow = 7 - from / 8;
            int fromCol = 7 - from % 8;
            int toRow   = 7 - to   / 8;
            int toCol   = 7 - to   % 8;
            
            Move mirror = new Move(
                fromRow * 8 + fromCol,
                toRow   * 8 + toCol
            );
            return mirror;
        }

        public override bool Equals(object o)
        {
            try
            {
                return (bool)(this == (Move)o);
            }
            catch
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return from + to;
        }

        public static bool operator==(Move move1, Move move2)
        {
            return move1.from == move2.from && move1.to == move2.to;
        }

        public static bool operator!=(Move move1, Move move2)
        {
            return move1.from != move2.from || move1.to != move2.to;
        }
    }

    #endregion

    #region Chess Game Methods

    BoardState boardState = null;
    public BoardState GetBoardState() { return boardState; }

    EChessTeam teamTurn;

    List<uint> scores;

    public delegate void PlayerTurnEvent(bool isWhiteMove);
    public event PlayerTurnEvent OnPlayerTurn = null;

    public delegate void ScoreUpdateEvent(uint whiteScore, uint blackScore);
    public event ScoreUpdateEvent OnScoreUpdated = null;

    public void PrepareGame(bool resetScore = true)
    {
        chessAI = ChessAI.Instance;

        // Begin game
        boardState.Reset(isPlayingBlacks);

        teamTurn = isPlayingBlacks ? EChessTeam.Black : EChessTeam.White;
        if (scores == null)
        {
            scores = new List<uint>();
            scores.Add(0);
            scores.Add(0);
        }
        if (resetScore)
        {
            scores.Clear();
            scores.Add(0);
            scores.Add(0);
        }
    }

    public void PlayTurn(Move move, bool isPlayer = true)
    {
        if (boardState.IsValidMove(teamTurn, move))
        {
            BoardState.EMoveResult result = boardState.PlayUnsafeMove(move);
            if (isPlayer) {
                string message = move.from + ":" + move.to;
                Packet packet  = new Packet(Packet.Type.Move, message);
                client.Send(packet);
            }

            if (result == BoardState.EMoveResult.Promotion)
            {
                // instantiate promoted queen gameobject
                AddQueenAtPos(move.to);
            }

            EChessTeam otherTeam = (teamTurn == EChessTeam.White) ? EChessTeam.Black : EChessTeam.White;
            if (boardState.DoesTeamLose(otherTeam))
            {
                // increase score and reset board
                scores[(int)teamTurn]++;
                if (OnScoreUpdated != null)
                    OnScoreUpdated(scores[0], scores[1]);

                PrepareGame(false);
                // remove extra piece instances if pawn promotions occured
                teamPiecesArray[0].ClearPromotedPieces();
                teamPiecesArray[1].ClearPromotedPieces();
            }
            else
            {
                teamTurn = otherTeam;
            }
            // raise event
            OnPlayerTurn?.Invoke(teamTurn == EChessTeam.White);
        }
    }

    // used to instantiate newly promoted queen
    private void AddQueenAtPos(int pos)
    {
        teamPiecesArray[(int)teamTurn].AddPiece(EPieceType.Queen);
        GameObject[] crtTeamPrefabs = (teamTurn == EChessTeam.White) ? whitePiecesPrefab : blackPiecesPrefab;
        GameObject crtPiece = Instantiate(crtTeamPrefabs[(uint)EPieceType.Queen]);
        teamPiecesArray[(int)teamTurn].StorePiece(crtPiece, EPieceType.Queen);
        crtPiece.transform.position = GetWorldPos(pos);
    }

    public bool IsPlayerTurn()
    {
        return teamTurn == EChessTeam.White;
    }

    public BoardSquare GetSquare(int pos)
    {
        return boardState.squares[pos];
    }

    public uint GetScore(EChessTeam team)
    {
        return scores[(int)team];
    }

    private void UpdateBoardPiece(Transform pieceTransform, int destPos)
    {
        pieceTransform.position = GetWorldPos(destPos);
    }

    private Vector3 GetWorldPos(int pos)
    {
        Vector3 piecePos = boardTransform.position;
        piecePos.y += zOffset;
        piecePos.x = -widthOffset + pos % BOARD_SIZE;
        piecePos.z = -widthOffset + pos / BOARD_SIZE;

        return piecePos;
    }

    private int GetBoardPos(Vector3 worldPos)
    {
        int xPos = Mathf.FloorToInt(worldPos.x + widthOffset) % BOARD_SIZE;
        int zPos = Mathf.FloorToInt(worldPos.z + widthOffset);

        return xPos + zPos * BOARD_SIZE;
    }

    #endregion

    #region MonoBehaviour

    private TeamPieces[] teamPiecesArray = new TeamPieces[2];
    private float zOffset = 0.5f;
    private float widthOffset = 3.5f;

    void Start()
    {
        client = FindObjectOfType<Client>();
        client.receiveCallback = p =>
        {
            switch (p.type)
            {
                case Packet.Type.Command:
                {
                    Debug.Log(p.DataAsString());
                    break;
                }
                case Packet.Type.Message:
                {
                    string message = p.DataAsString();
                    chatManager.ReceiveMessage(message);
                    break;
                }
                case Packet.Type.Move:
                {
                    string[] splitMove = p.DataAsString().Split(':');
                    Move move = new Move(Array.ConvertAll(splitMove, int.Parse)).Mirror();
                    PlayTurn(move, false);
                    UpdatePieces();
                    break;
                }
                case Packet.Type.Castle:
                {
                    string[] splitMove = p.DataAsString().Split(':');
                    Move move = new Move(Array.ConvertAll(splitMove.Take(2).ToArray(), int.Parse)).Mirror();
                    Move rook = new Move(-1, int.Parse(splitMove[2])).Mirror();
                    boardState.TryExecuteCastling(move, rook.to, false);
                    UpdatePieces();
                    break;
                }
                case Packet.Type.Color:
                {
                    Color color = p.DataAsColor();
                    blacksMaterial.color = color;
                    break;
                }
            }
        };
        
        isPlayingBlacks = FindObjectsOfType<Server>().Length <= 0;
        whitesMaterial.color = isPlayingBlacks ? Color.black : Color.white;
        blacksMaterial.color = isPlayingBlacks ? Color.white : Color.black;
        colorPicker.color    = whitesMaterial.color;

        pieceLayerMask = 1 << LayerMask.NameToLayer("Piece");
        boardLayerMask = 1 << LayerMask.NameToLayer("Board");

        boardTransform = GameObject.FindGameObjectWithTag("Board").transform;

        LoadPiecesPrefab();

        boardState = new BoardState();

        PrepareGame();

        teamPiecesArray[0] = null;
        teamPiecesArray[1] = null;

        CreatePieces();

        if (OnPlayerTurn != null)
            OnPlayerTurn(teamTurn == EChessTeam.White);
        if (OnScoreUpdated != null)
            OnScoreUpdated(scores[0], scores[1]);
    }

    void Update()
    {
        // human player always plays white
        if (teamTurn == EChessTeam.White)
            UpdatePlayerTurn();
        // AI plays black
        else if (isAIEnabled)
            UpdateAITurn();
        else
            UpdatePlayerTurn();

        if (whitesMaterial.color != colorPicker.color)
        {
            whitesMaterial.color = colorPicker.color;
            client.Send(new Packet(Packet.Type.Color, whitesMaterial.color));
        }
    }
    #endregion

    #region Pieces

    GameObject[] whitePiecesPrefab = new GameObject[6];
    GameObject[] blackPiecesPrefab = new GameObject[6];

    void LoadPiecesPrefab()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhitePawn");
        whitePiecesPrefab[(uint)EPieceType.Pawn] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteKing");
        whitePiecesPrefab[(uint)EPieceType.King] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteQueen");
        whitePiecesPrefab[(uint)EPieceType.Queen] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteRook");
        whitePiecesPrefab[(uint)EPieceType.Rook] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteKnight");
        whitePiecesPrefab[(uint)EPieceType.Knight] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteBishop");
        whitePiecesPrefab[(uint)EPieceType.Bishop] = prefab;

        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackPawn");
        blackPiecesPrefab[(uint)EPieceType.Pawn] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackKing");
        blackPiecesPrefab[(uint)EPieceType.King] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackQueen");
        blackPiecesPrefab[(uint)EPieceType.Queen] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackRook");
        blackPiecesPrefab[(uint)EPieceType.Rook] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackKnight");
        blackPiecesPrefab[(uint)EPieceType.Knight] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackBishop");
        blackPiecesPrefab[(uint)EPieceType.Bishop] = prefab;
    }

    void CreatePieces()
    {
        // Instantiate all pieces according to board data
        if (teamPiecesArray[0] == null)
            teamPiecesArray[0] = new TeamPieces();
        if (teamPiecesArray[1] == null)
            teamPiecesArray[1] = new TeamPieces();

        GameObject[] crtTeamPrefabs = null;
        int crtPos = 0;
        foreach (BoardSquare square in boardState.squares)
        {
            crtTeamPrefabs = square.team == EChessTeam.White ? whitePiecesPrefab : blackPiecesPrefab;
            
            if (square.piece != EPieceType.None)
            {
                GameObject crtPiece = Instantiate(crtTeamPrefabs[(uint)square.piece]);
                teamPiecesArray[(int)square.team].StorePiece(crtPiece, square.piece);

                // set position
                Vector3 piecePos = boardTransform.position;
                piecePos.y += zOffset;
                piecePos.x = -widthOffset + crtPos % BOARD_SIZE;
                piecePos.z = -widthOffset + crtPos / BOARD_SIZE;
                crtPiece.transform.position = piecePos;
            }
            crtPos++;
        }
    }

    void UpdatePieces()
    {
        teamPiecesArray[0].Hide();
        teamPiecesArray[1].Hide();

        for (int i = 0; i < boardState.squares.Count; i++)
        {
            BoardSquare square = boardState.squares[i];
            if (square.team == EChessTeam.None)
                continue;

            int teamId = (int)square.team;
            EPieceType pieceType = square.piece;

            teamPiecesArray[teamId].SetPieceAtPos(pieceType, GetWorldPos(i));
        }
    }

    #endregion

    #region Gameplay

    Transform grabbed = null;
    float maxDistance = 100f;
    int startPos = 0;
    int destPos = 0;

    void UpdateAITurn()
    {
        // Move move = chessAI.ComputeMove();
        // PlayTurn(move);

        // UpdatePieces();
    }

    void UpdatePlayerTurn()
    {
        if (Input.GetMouseButton(0))
        {
            if (grabbed)
                ComputeDrag();
            else
                ComputeGrab();
        }
        else if (grabbed != null)
        {
            // find matching square when releasing grabbed piece
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxDistance, boardLayerMask))
            {
                grabbed.root.position = hit.transform.position + Vector3.up * zOffset;
            }

            destPos = GetBoardPos(grabbed.root.position);
            if (startPos != destPos)
            {
                Move move = new Move();
                move.from = startPos;
                move.to = destPos;

                PlayTurn(move);

                UpdatePieces();
            }
            else
            {
                grabbed.root.position = GetWorldPos(startPos);
            }
            grabbed = null;
        }
    }

    void ComputeDrag()
    {
        // drag grabbed piece on board
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance, boardLayerMask))
        {
            grabbed.root.position = hit.point;
        }
    }

    void ComputeGrab()
    {
        // grab a new chess piece from board
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, maxDistance, pieceLayerMask))
        {
            grabbed = hit.transform;
            startPos = GetBoardPos(grabbed.root.position);
        }
    }

    #endregion
}
