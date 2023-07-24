using ChessChallenge.API;

using System;

namespace ChessChallenge.Example
{


    public class RaineBot : IChessBot
    {

        int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

        int maximum_depth = 4;

        public Move Think(Board board, Timer time)
        {
            bool current_player = board.IsWhiteToMove;

            (Move?, int) result = search(board, null, int.MinValue, int.MaxValue, maximum_depth, current_player, true);

            return result.Item1.Value;
        }

        (Move?, int) search(Board board, Move? move, int alpha, int beta, int depth, bool is_white, bool positive)
        {

            if (move.HasValue)
            {
                board.MakeMove(move.Value);
                (Move?, int) val;
                if (depth == 0)
                {
                    if (board.IsInCheckmate()) val = (move.Value, 1000000);                            // Checkmate
                    else if (board.IsDraw()) val = (move.Value, 0);                                  // Draw
                    else val = (move.Value, EvaluatePosition(board, is_white));  // "The horison" nodes
                    board.UndoMove(move.Value);
                    return val;
                }
            }

            if (board.GetLegalMoves().Length == 0)
            {
                if (move.HasValue) board.UndoMove(move.Value);
                return (null, 0);                     // Catching weird edge cases.
            }

            //. Otherwise:
            //  - Find all possible moves.
            Move? best_move = null;
            int best_eval = int.MinValue;
            int eval;


            foreach (Move possible_move in board.GetLegalMoves())
            {
                //  - Loop through them.
                //    - Run search() to find evaluation.

                eval = -search(board, possible_move, alpha, beta, depth - 1, !is_white, !positive).Item2;

                if (eval > best_eval)
                {
                    best_eval = eval;
                    best_move = possible_move;
                }

                if (positive) alpha = Math.Max(alpha, best_eval);
                else beta = Math.Min(beta, -best_eval);

                if (beta <= alpha) break;
            }

            if (move.HasValue) board.UndoMove(move.Value);

            return (best_move, best_eval);
        }

        int EvaluatePosition(Board board, bool is_white)
        {
            ulong pieces, op_pieces;
            int val = 0;

            if (is_white)
            {
                pieces = board.WhitePiecesBitboard;
                op_pieces = board.BlackPiecesBitboard;
            }
            else
            {
                pieces = board.BlackPiecesBitboard;
                op_pieces = board.WhitePiecesBitboard;
            }

            while (pieces > 0)
            {
                Piece piece = board.GetPiece(new Square(BitboardHelper.ClearAndGetIndexOfLSB(ref pieces)));
                val += pieceValues[(int)piece.PieceType];
            }
            while (op_pieces > 0)
            {
                Piece piece = board.GetPiece(new Square(BitboardHelper.ClearAndGetIndexOfLSB(ref op_pieces)));
                val -= pieceValues[(int)piece.PieceType] * 2;
            }

            if (board.IsInCheck()) val *= 2;
            return val;
        }
    }
}
