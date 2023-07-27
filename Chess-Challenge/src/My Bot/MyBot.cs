using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    readonly int[] piece_weights = { 100, 320, 330, 500, 900, 20000 };
    int maxDepth = 3;
    int largeNum = 99999999;
    Move bestMove;
    int mobilityWeight = 10;
    private const sbyte EXACT = 0, LOWERBOUND = -1, UPPERBOUND = 1, INVALID = -2;


    public struct Transposition 
    {
        public ulong zobristHash = 0;
        public int evaluation = 0;
        public byte depth= 0;
        public sbyte flag = INVALID;
        public Transposition(ulong zHash, int eval, byte d) {
            zobristHash = zHash;
            evaluation = eval;
            depth = d;
            flag = INVALID;
        }
    };

    private const ulong k_TpMask = 0x8FFFFF;
    private Transposition[] m_TPTable = new Transposition[k_TpMask + 1];

    Transposition Lookup(ulong zHash) {
        return m_TPTable[zHash & k_TpMask];
    }

    // MVV_VLA[victim][attacker]
    int[,] MVV_LVA  = 
    {
        {0, 0, 0, 0, 0, 0, 0},       // victim None, attacker K, Q, R, B, N, P, None
        {10, 11, 12, 13, 14, 15, 0}, // victim P, attacker K, Q, R, B, N, P, None
        {20, 21, 22, 23, 24, 25, 0}, // victim N, attacker K, Q, R, B, N, P, None
        {30, 31, 32, 33, 34, 35, 0}, // victim B, attacker K, Q, R, B, N, P, None
        {40, 41, 42, 43, 44, 45, 0}, // victim R, attacker K, Q, R, B, N, P, None
        {50, 51, 52, 53, 54, 55, 0}, // victim Q, attacker K, Q, R, B, N, P, None
        {0, 0, 0, 0, 0, 0, 0},       // victim K, attacker K, Q, R, B, N, P, None
    };

    /*
    int iterate_board(Board board, PieceType p, bool isWhiteMove) {
        int score = 0;
            ulong bb = board.GetPieceBitboard(p, isWhiteMove);

            int i = 0;
            while (bb > 0)
            {
                ulong bit = bb & 1;
                if (bit == 1) {
                    if (isWhiteMove)
                    {
                        int square = i;
                        int file = square % 8;
                        int rank = square / 8;
                        ScoreType st = (ScoreType)((int)p - 1);
                        score += GetPieceBonusScore(st, isWhiteMove, rank, file);
                    }
                    else {
                        int square = i ^ 56;
                        int file = square % 8;
                        int rank = square / 8;
                        ScoreType st = (ScoreType)((int)p - 1);
                        score += GetPieceBonusScore(st,isWhiteMove, rank, file);
                    }
                }
                // Reset LSB
               bb &= bb - 1;
               i += 1;
            }
        return score;
    }
    */

    public Move Think(Board board, Timer timer)
    {
        for (int i = 0; i < maxDepth; i++)
        {
            NegamaxAlphaBeta(board, -largeNum, largeNum, maxDepth);
        }
        return bestMove;
    }

    public Span<Move> GetEnemyMoves(Board board)
    {
        board.MakeMove(Move.NullMove);
        Move[] enemyMoves = board.GetLegalMoves();
        board.UndoMove(Move.NullMove);
        return enemyMoves;
    }

    // NEEDS TO BE TWEAKED
    int Evaluate(Board board)
    {
        PieceList[] pl = board.GetAllPieceLists();
        int whiteMobility = 0;
        int blackMobility = 0;

        Span<Move> currentMoves = stackalloc Move[256];
        board.GetLegalMovesNonAlloc(ref currentMoves, false);
        Span<Move> enemyMoves = GetEnemyMoves(board);

        var values = Enum.GetValues(typeof(PieceType));
        bool isWhiteMove = board.IsWhiteToMove;
        if (board.IsWhiteToMove)
        {
            whiteMobility += currentMoves.Length;
            blackMobility -= enemyMoves.Length;
        }
        else
        {
            blackMobility -= currentMoves.Length;
            whiteMobility += enemyMoves.Length;
        }

        int whiteScore = 0;
        int blackScore = 0;
        for (int i = 0; i < piece_weights.Length * 2; i++)
        {
            if (i < 6)
            {
                whiteScore += (piece_weights[i] * pl[i].Count);
            }
            else
            {
                blackScore -= (piece_weights[i - 6] * pl[i].Count);

            }
        }

        int mobilityScore = mobilityWeight * (whiteMobility + blackMobility);
        int materialScore = (whiteScore + blackScore);
        int sideToMove = board.IsWhiteToMove ? 1 : -1;
        return (materialScore + mobilityScore) * sideToMove;
    }

    public void OrderMoves(Span<Move> moves)
    {
        int[] scores = new int[moves.Length];

        for (int i = 0; i < scores.Length; i++)
        {
            Move move = moves[i];
            if (move.IsCapture)
            {
                int value = MVV_LVA[(int)move.CapturePieceType, (int)move.MovePieceType];
                scores[i] += value;
            }

            scores[i] *= -1;
        }

        Array.Sort(scores, moves.ToArray());
    }

    int NegamaxAlphaBeta(Board board, int alpha, int beta, int depth)
    {

        if (board.IsInsufficientMaterial() || board.IsRepeatedPosition() || board.FiftyMoveCounter >= 100)
            return -20;

        Span<Move> moves = stackalloc Move[256];
        board.GetLegalMovesNonAlloc(ref moves);

        OrderMoves(moves);

        if (depth == 0 || moves.Length == 0)
        {
            if (board.IsInCheckmate())
                return -9999999;

            //return Quiesce(board, alpha, beta);
            return Evaluate(board);
        }

        int maxEval = -largeNum;
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int eval = -NegamaxAlphaBeta(board, -beta, -alpha, depth - 1);
            board.UndoMove(move);

            if (eval > maxEval)
            {
                maxEval = eval;
                if (depth == maxDepth) bestMove = move;
            }


            alpha = Math.Max(alpha, maxEval);
            if (alpha >= beta) break;

        }
        return maxEval;
    }

    int Quiesce(Board board, int alpha, int beta)
    {
        int stand_pat = Evaluate(board);
        if (stand_pat >= beta)
            return beta;

        if (alpha < stand_pat)
            alpha = stand_pat;

        Span<Move> captures = stackalloc Move[256];
        board.GetLegalMovesNonAlloc(ref captures, true);
        OrderMoves(captures);

        foreach (Move capture in captures)
        {
            board.MakeMove(capture);
            int score = -Quiesce(board, -beta, -alpha);
            board.UndoMove(capture);

            if (score >= beta)
                return beta;

            if (score > alpha)
                alpha = score;
        }
        return alpha;
    }
}

