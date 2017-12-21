using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltimateTicTacToe.Core.Entities;

namespace UltimateTicTacToe.Core
{
    // this project is too small to overthink the design - just have one class that does all the BL in static functions
    public static class GameMaster
    {
        public static void UpdateBoard(Game game, int gameXIndex, int gameYIndex, int pickXIndex, int pickYIndex)
        {
            if (gameXIndex < 0 || gameXIndex > 2
                || gameYIndex < 0 || gameYIndex > 2
                || pickXIndex < 0 || pickXIndex > 2
                || pickYIndex < 0 || pickYIndex > 2)
            {
                throw new ArgumentOutOfRangeException("Game or pick index out of bounds.  All must be between 0 and 2.");
            }

            if (game == null)
            {
                throw new ArgumentNullException("game cannot be null");
            }
        }

        public static bool IsPickValid(Game game, int gameXIndex, int gameYIndex, int pickXIndex, int pickYIndex)
        {
            if (gameXIndex < 0 || gameXIndex > 2
                || gameYIndex < 0 || gameYIndex > 2
                || pickXIndex < 0 || pickXIndex > 2
                || pickYIndex < 0 || pickYIndex > 2)
            {
                throw new ArgumentOutOfRangeException("Game or pick index out of bounds.  All must be between 0 and 2.");
            }
            if (game == null)
            {
                throw new ArgumentNullException("game cannot be null");
            }
            if (game.LastPlay == null)
            {
                throw new ArgumentNullException("game.LastPlay cannot be null");
            }
            if (game.Board == null)
            {
                throw new ArgumentNullException("game.Board cannot be null");
            }

            

            return game.LastPlay.Item1 == gameXIndex
                && game.LastPlay.Item2 == gameYIndex
                && game.Board[gameXIndex, gameYIndex, pickXIndex, pickYIndex] == null;
        }
    }
}
