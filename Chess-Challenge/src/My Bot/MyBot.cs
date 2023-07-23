using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    readonly int[] weights = { 100, 320, 330, 500, 900, 20000 };
    int mobilityWeight = 10;

    public Move Think(Board board, Timer timer)
    {
        Move move = EvilFindBestMove(board);
        return move;
    }

    // NEEDS TO BE TWEAKED
    int Evaluate(Board board)
    {
        PieceList[] pl = board.GetAllPieceLists();
        int whiteScore = 0;
        int blackScore = 0;
        int whiteMobility = 0;
        int blackMobility = 0;

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
        int mobilityScore = (whiteMobility + blackMobility) * 10;
        int sideToMove = board.IsWhiteToMove ? 1 : -1;

        return (materialScore + mobilityScore) * sideToMove;
    }

    Move FindBestMove(Board board)
    {
        int bestVal = int.MinValue;
        Random rng = new();
        Move[] moves = OrderMoves(board, board.GetLegalMoves());
        Move bestMove = moves[rng.Next(moves.Length)];
        //PrintMoves(board, moves);

        for (int i = 0; i < moves.Length; i++)
        {
            Move move = moves[i];
            board.MakeMove(move);
            int moveVal = AlphaBeta(board, int.MinValue, int.MaxValue, 3);
            board.UndoMove(move);

            if (moveVal > bestVal)
            {
                bestMove = move;
                bestVal = moveVal;
            }
        }
        return moves[0];
    }

    int MoveToInt(Board board, Move move)
    {
        if (MoveIsCheckmate(board, move)) {
            return -100000;
        }
        if (move.IsCapture)
        {
            return weights[(int)move.CapturePieceType-1] * -1;
        }

        else if (move.IsCastles)
        {
            return 1;
        }

        else
        {
            return 2;
        }

    }


    Move[] OrderMoves(Board board, Move[] moves)
    {
        Move[] newMoves = (Move[])moves.Clone();

        bool swapped;
        for (int i = 0; i < newMoves.Length - 1; i++)
        {
            swapped = false;
            for (int j = 0; j < newMoves.Length - i - 1; j++)
            {
                int currMove = MoveToInt(board, newMoves[j]);
                int nextMove = MoveToInt(board, newMoves[j + 1]);
                if (currMove > nextMove)
                {
                    Move temp = newMoves[j + 1];
                    newMoves[j + 1] = newMoves[j];
                    newMoves[j] = temp;
                    swapped = true;
                }
            }

            if (swapped == false)
            {
                break;
            }

        }
        return newMoves;
    }

    Move EvilFindBestMove(Board board)
    {
        int bestVal = int.MinValue;
        Random rng = new();
        Move[] moves = board.GetLegalMoves();
        Move bestMove = moves[rng.Next(moves.Length)];
        int highestValueCapure = 0;

        for (int i = 0; i < moves.Length; i++)
        {
            Move move = moves[i];
            if (MoveIsCheckmate(board, move))
            {
                bestMove = move;
                break;
            }
            /*
            board.MakeMove(move);
            int moveVal = AlphaBeta(board, int.MinValue, int.MaxValue, 1);
            board.UndoMove(move);

            if (moveVal > bestVal)
            {
                bestMove = move;
                bestVal = moveVal;
            }
            */
            Piece capturedPiece = board.GetPiece(move.TargetSquare);
            int capturedPieceValue = weights[(int)capturedPiece.PieceType];

            if (capturedPieceValue > highestValueCapure)
            {
                bestMove = move;
                highestValueCapure = capturedPieceValue;
            }
        }
        return bestMove;
    }

    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    int AlphaBeta(Board board, int alpha, int beta, int depth)
    {
        if (depth == 0)
        {
            //return Quiesce(board, alpha, beta);
            return Evaluate(board);
        }

        Move[] moves = board.GetLegalMoves();
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int score = -AlphaBeta(board, -beta, -alpha, depth - 1);
            board.UndoMove(move);

            if (score >= beta)
            {
                return beta;

            }
            alpha = Math.Max(alpha, score);

        }
        return alpha;
    }

    int Quiesce(Board board, int alpha, int beta)
    {
        int stand_pat = Evaluate(board);

        if (stand_pat >= beta)
        {
            return beta;
        }

        if (stand_pat > alpha)
        {
            alpha = stand_pat;
        }

        Move[] captures = board.GetLegalMoves(true);

        for (int i = 0; i < captures.Length; i++)
        {
            board.MakeMove(captures[i]);
            int score = -Quiesce(board, -beta, -alpha);
            board.UndoMove(captures[i]);

            if (score >= beta)
            {
                return beta;
            }

            if (score > alpha)
            {
                alpha = score;
            }

        }
        return alpha;
    }

    void PrintMoves(Board board, Move[] moves)
    {
        foreach (Move move in moves)
        {
            Console.WriteLine("MOVE " + move + " VAL: " + MoveToInt(board, move));
        }
        Console.WriteLine();
        Console.WriteLine();

    }
}

