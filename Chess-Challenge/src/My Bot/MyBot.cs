using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    readonly int[] piece_weights = { 100, 320, 330, 500, 900, 20000 };
    int maxDepth = 4;
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

        int materialScore = (whiteScore + blackScore);
        int sideToMove = board.IsWhiteToMove ? 1 : -1;
        return materialScore * sideToMove;
    }

    public void OrderMoves(Move[] moves) {
        int[] scores = new int[moves.Length];

        for(int i = 0; i < scores.Length; i++){
            Move move = moves[i];
            if (move.IsCapture) {
                scores[i] += piece_weights[(int)move.CapturePieceType - 1] - piece_weights[(int)move.MovePieceType - 1] / 10;
            }

            scores[i] *= -1;
        }

        Array.Sort(scores, moves);
    }

    int NegamaxAlphaBeta(Board board, int alpha, int beta, int depth)
    {


        Move[] moves = board.GetLegalMoves();
        if (board.IsInCheckmate())
            return board.IsWhiteToMove ? -largeNum : largeNum;

        if (depth == 0 || moves.Length == 0)
        {
            return Evaluate(board);
        }

        //OrderMoves(moves);

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

