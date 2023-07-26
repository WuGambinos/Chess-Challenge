using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    readonly int[] piece_weights = { 100, 320, 330, 500, 900, 20000 };
    int maxDepth = 3;
    int largeNum = 99999999;
    Move bestMove;
    int mobilityWeight = 10;

    private static readonly ulong[,] packedScores =
    {
        {0x31CDE1EBFFEBCE00, 0x31D7D7F5FFF5D800, 0x31E1D7F5FFF5E200, 0x31EBCDFAFFF5E200},
        {0x31E1E1F604F5D80A, 0x13EBD80009FFEC0A, 0x13F5D8000A000014, 0x13FFCE000A00001E},
        {0x31E1E1F5FAF5E232, 0x13F5D80000000032, 0x0013D80500050A32, 0x001DCE05000A0F32},
        {0x31E1E1FAFAF5E205, 0x13F5D80000050505, 0x001DD80500050F0A, 0xEC27CE05000A1419},
        {0x31E1EBFFFAF5E200, 0x13F5E20000000000, 0x001DE205000A0F00, 0xEC27D805000A1414},
        {0x31E1F5F5FAF5E205, 0x13F5EC05000A04FB, 0x0013EC05000A09F6, 0x001DEC05000A0F00},
        {0x31E213F5FAF5D805, 0x13E214000004EC0A, 0x140000050000000A, 0x14000000000004EC},
        {0x31CE13EBFFEBCE00, 0x31E21DF5FFF5D800, 0x31E209F5FFF5E200, 0x31E1FFFB04F5E200},
    };

    //enumeration to keep track externally of
    //which byte is for which scores
    private enum ScoreType { Pawn, Knight, Bishop, Rook, Queen, King, KingEndgame, KingHunt };

    //Assuming you put your packed data table into a table called packedScores.
    private int GetPieceBonusScore(ScoreType type, bool isWhite, int rank, int file)
    {
        //Because the arrays are 8x4, we need to mirror across the files.
        if (file > 3) file = 7 - file;
        //Additionally, if we're checking black pieces, we need to flip the board vertically.
        if (!isWhite) rank = 7 - rank;
        int unpackedData = 0;
        ulong bytemask = 0xFF;
        //first we shift the mask to select the correct byte              ↓
        //We then bitwise-and it with PackedScores            ↓
        //We finally have to "un-shift" the resulting data to properly convert back       ↓
        //We convert the result to an sbyte, then to an int, to ensure it converts properly.
        unpackedData = (int)(sbyte)((packedScores[rank, file] & (bytemask << (int)type)) >> (int)type);
        //inverting eval scores for black pieces
        if (!isWhite) unpackedData *= -1;
        return unpackedData;
    }

    public Move Think(Board board, Timer timer)
    {
        /*
        for (int i = 0; i < maxDepth; i++)
        {
            NegamaxAlphaBeta(board, -largeNum, largeNum, maxDepth);
        }
        */
        NegamaxAlphaBeta(board, -largeNum, largeNum, maxDepth);
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
        int whiteScore = 0;
        int blackScore = 0;
        int whiteMobility = 0;
        int blackMobility = 0;

        Span<Move> currentMoves = stackalloc Move[256];
        board.GetLegalMovesNonAlloc(ref currentMoves, false);
        Span<Move> enemyMoves = GetEnemyMoves(board);

        var values = Enum.GetValues(typeof(PieceType));
        bool isWhiteMove = board.IsWhiteToMove;

        // Iterate over pieces
        foreach (var value in values)
        {
            ScoreType st = (ScoreType)value;
            if (st == ScoreType.Pawn)
            {
                ulong bb = board.GetPieceBitboard(PieceType.Pawn, isWhiteMove);

                // Popcnt
                int count = 0;
                while (bb > 0)
                {
                    count++;
                    ulong bit = bb & 1;
                    if (isWhiteMove)
                    {
                        Console.WriteLine("BIT: " + bit + " SQUARE: " + count);
                    }
                    // Reset LSB
                    bb &= bb - 1;
                }

                if (isWhiteMove)
                {
                    whiteScore += count;
                }
                else
                {
                    blackScore -= count;
                }

            }

            if (st == ScoreType.Knight)
            {
                ulong bb = board.GetPieceBitboard(PieceType.Knight, isWhiteMove);
                // Popcnt
                int count = 0;
                while (bb > 0)
                {
                    count++;
                    // Reset LSB
                    bb &= bb - 1;
                }

                if (isWhiteMove)
                {
                    whiteScore += count;
                }
                else
                {
                    blackScore -= count;
                }
            }

            if (st == ScoreType.Bishop)
            {
                ulong bb = board.GetPieceBitboard(PieceType.Bishop, isWhiteMove);
                // Popcnt
                int count = 0;
                while (bb > 0)
                {
                    count++;
                    // Reset LSB
                    bb &= bb - 1;
                }

                if (isWhiteMove)
                {
                    whiteScore += count;
                }
                else
                {
                    blackScore -= count;
                }
            }

            if (st == ScoreType.Rook)
            {
                ulong bb = board.GetPieceBitboard(PieceType.Rook, isWhiteMove);
                // Popcnt
                int count = 0;
                while (bb > 0)
                {
                    count++;
                    // Reset LSB
                    bb &= bb - 1;
                }

                if (isWhiteMove)
                {
                    whiteScore += count;
                }
                else
                {
                    blackScore -= count;
                }
            }

            if (st == ScoreType.Queen)
            {
                ulong bb = board.GetPieceBitboard(PieceType.Queen, isWhiteMove);
                // Popcnt
                int count = 0;
                while (bb > 0)
                {
                    count++;
                    // Reset LSB
                    bb &= bb - 1;
                }

                if (isWhiteMove)
                {
                    whiteScore += count;
                }
                else
                {
                    blackScore -= count;
                }
            }
        }

        /*
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
        */

        int materialScore = (whiteScore + blackScore);
        int mobilityScore = mobilityWeight * (whiteMobility + blackMobility);
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
                scores[i] += piece_weights[(int)move.CapturePieceType - 1] - piece_weights[(int)move.MovePieceType - 1] / 10;
            }

            scores[i] *= -1;
        }

        Array.Sort(scores, moves.ToArray());
    }

    int NegamaxAlphaBeta(Board board, int alpha, int beta, int depth)
    {

        if (board.IsInsufficientMaterial() || board.IsRepeatedPosition() || board.FiftyMoveCounter >= 100)
            return 0;

        Span<Move> moves = stackalloc Move[256];
        board.GetLegalMovesNonAlloc(ref moves);

        OrderMoves(moves);

        if (depth == 0 || moves.Length == 0)
        {
            if (board.IsInCheckmate())
                return -9999999;

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
    /*
    int Quiesce(Board board, int alpha, int beta)
    {
        int stand_pat = Evaluate(board);
        if (stand_pat >= beta)
            return beta;

        if (alpha < stand_pat)
            alpha = stand_pat;

        Span<Move> captures = stackalloc Move[256];
        board.GetLegalMovesNonAlloc(ref captures);

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
    */
}

