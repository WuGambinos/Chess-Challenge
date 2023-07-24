using ChessChallenge.API;
using System.Linq;
using System;

namespace ChessChallenge.Example
{
    public class OdinBot : IChessBot
    {
        public Move Think(Board board, Timer timer)
        {
            var legs = board.GetLegalMoves();
            var values = legs
                .Select(x =>
                    new Random().Next(3)
                    - Math.Abs(x.StartSquare.Index - board.GetKingSquare(board.IsWhiteToMove).Index) * (board.PlyCount < 60 ? 1 : -1))
                .ToList();
            foreach (Move leg in legs)
            {
                board.MakeMove(leg);
                if (board.IsInCheckmate() || leg.IsCapture) return leg;
                board.UndoMove(leg);
            }
            return legs[values.FindIndex(x => x == values.Max())];
        }
    }
}
