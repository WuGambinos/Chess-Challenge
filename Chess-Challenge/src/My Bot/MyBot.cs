using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    readonly int[] weights = { 100, 320, 330, 500, 900, 20000 };
    int maxDepth = 3;
    int largeNum = 99999999;
    Move bestMove;

    public Move Think(Board board, Timer timer)
    {
        NegamaxAlphaBeta(board, -largeNum, largeNum, maxDepth);
        return bestMove;
    }

    // NEEDS TO BE TWEAKED
    int Evaluate(Board board)
    {
        PieceList[] pl = board.GetAllPieceLists();
        int whiteScore = 0;
        int blackScore = 0;


        if (board.IsInCheckmate())
            return board.IsWhiteToMove ? -largeNum : largeNum;

        for (int i = 0; i < weights.Length * 2; i++)
        {
            if (i < 6)
            {
                whiteScore += (weights[i] * pl[i].Count);
            }
            else
            {
                blackScore -= (weights[i - 6] * pl[i].Count);

            }
        }

        int materialScore = (whiteScore + blackScore);
        int sideToMove = board.IsWhiteToMove ? 1 : -1;
        return materialScore * sideToMove;
    }

    int NegamaxAlphaBeta(Board board, int alpha, int beta, int depth)
    {

        if(board.IsDraw())
            return 0;

        Move[] moves = board.GetLegalMoves();
        if (depth == 0 || moves.Length == 0)
        {
            return Evaluate(board);
        }

        int maxEval = -largeNum;
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int eval = -NegamaxAlphaBeta(board, -beta, -alpha, depth - 1);
            board.UndoMove(move);

            if(eval > maxEval){
                maxEval = eval;
                if (depth == maxDepth) bestMove = move;
            }


            alpha = Math.Max(alpha, maxEval);
            if (alpha >= beta) break;

        }
        return maxEval;
    }
}

