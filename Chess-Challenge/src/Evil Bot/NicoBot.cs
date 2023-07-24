using System;
using System.Collections.Generic;
using ChessChallenge.API;

namespace ChessChallenge.Example
{
    public class NicoBot : IChessBot
    {
        private readonly int MAX_DEPTH = 1;
        private readonly int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
        private bool? isWhite = null;
        private Random rng = new Random();

        public Move Think(Board board, Timer timer)
        {
            // On first move determine what color we're playing as
            if (isWhite == null) isWhite = board.IsWhiteToMove;

            return FindBestMove(board);
        }

        private Move FindBestMove(Board board)
        {
            Move[] moves = board.GetLegalMoves();
            List<Move> bestMoves = new List<Move>();
            int bestEval = int.MinValue;
            foreach (Move aMove in moves)
            {
                int eval = Minmax(board, aMove, false, MAX_DEPTH);
                if (eval > bestEval)
                {
                    bestMoves = new List<Move> { aMove };
                    bestEval = eval;
                }
                else if (eval == bestEval)
                {
                    bestMoves.Add(aMove);
                }
            }

            return bestMoves[rng.Next(bestMoves.Count)];
        }

        private int Minmax(Board board, Move move, bool isMax, int depth)
        {
            board.MakeMove(move);
            int value = 0;

            // evaluate leaf nodes
            if (depth <= 0) value = Evaluate(board);

            // maximising player
            Move[] moves = board.GetLegalMoves();
            if (depth > 0 && isMax)
            {
                int maxValue = int.MinValue;
                foreach (Move aMove in moves)
                {
                    maxValue = Math.Max(maxValue, Minmax(board, aMove, !isMax, depth - 1));
                }

                value = maxValue;
            }
            else if (depth > 0 && !isMax)
            {
                // minimising player
                int minValue = int.MaxValue;
                foreach (Move aMove in moves)
                {
                    minValue = Math.Min(minValue, Minmax(board, aMove, !isMax, depth - 1));
                }

                value = minValue;
            }

            board.UndoMove(move);
            return value;
        }

        private int Evaluate(Board board)
        {
            int boardValue = 0;

            if (board.IsDraw()) return -10000;
            if (board.IsInCheck()) return 10000;
            if (board.IsInCheckmate()) return int.MaxValue;

            foreach (PieceList aPieceList in board.GetAllPieceLists())
            {
                int materialValue = pieceValues[(int)aPieceList.TypeOfPieceInList] * aPieceList.Count;
                if (aPieceList.IsWhitePieceList == isWhite) boardValue += materialValue;
                else boardValue -= materialValue;
            }

            return boardValue;
        }
    }
}
