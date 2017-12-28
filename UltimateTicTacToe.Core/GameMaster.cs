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
            if (!IsPickValid(game, gameXIndex, gameYIndex, pickXIndex, pickYIndex))
            {
                throw new InvalidOperationException("Invalid pick.");
            }
            game.Board[gameXIndex, gameYIndex, pickXIndex, pickYIndex] = game.CurrentPlayer;
            game.LastPlay = new Tuple<int, int>(pickXIndex, pickYIndex);
            game.CurrentPlayer = game.CurrentPlayer == Players.X ? Players.O : Players.X;
        }

        public static bool IsPickValid(Game game, int gameXIndex, int gameYIndex, int pickXIndex, int pickYIndex)
        {
            if (gameXIndex < 0 || gameXIndex > 2
                || gameYIndex < 0 || gameYIndex > 2
                || pickXIndex < 0 || pickXIndex > 2
                || pickYIndex < 0 || pickYIndex > 2)
            {
                throw new ArgumentOutOfRangeException("Game or pick indexes out of bounds.  All must be between 0 and 2.");
            }
            if (game == null)
            {
                throw new ArgumentNullException("game cannot be null");
            }
            if (game.Board == null)
            {
                throw new ArgumentNullException("game.Board cannot be null");
            }

            return (game.LastPlay == null
                    || (game.LastPlay.Item1 == gameXIndex && game.LastPlay.Item2 == gameYIndex)
                    || GetGameStatus(game, game.LastPlay.Item1, game.LastPlay.Item2).HasValue)
                && !GetGameStatus(game, gameXIndex, gameYIndex).HasValue
                && game.Board[gameXIndex, gameYIndex, pickXIndex, pickYIndex] == null;
        }

        /// <summary>
        /// Return the game status of a particular subgame in a game.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="gameXIndex"></param>
        /// <param name="gameYIndex"></param>
        /// <returns>Game status of requested game, null if game is incomplete</returns>
        public static GameStatuses? GetGameStatus(Game game, int gameXIndex, int gameYIndex)
        {
            if (gameXIndex < 0 || gameXIndex > 2
                   || gameYIndex < 0 || gameYIndex > 2)
            {
                throw new ArgumentOutOfRangeException("Game indexes out of bounds.  All must be between 0 and 2.");
            }
            if (game == null)
            {
                throw new ArgumentNullException("game cannot be null");
            }

            if (game.Board[gameXIndex, gameYIndex, 0, 0].HasValue
                && ((game.Board[gameXIndex, gameYIndex, 0, 0] == game.Board[gameXIndex, gameYIndex, 0, 1]
                        && game.Board[gameXIndex, gameYIndex, 0, 1] == game.Board[gameXIndex, gameYIndex, 0, 2])
                    || (game.Board[gameXIndex, gameYIndex, 0, 0] == game.Board[gameXIndex, gameYIndex, 1, 1]
                        && game.Board[gameXIndex, gameYIndex, 1, 1] == game.Board[gameXIndex, gameYIndex, 2, 2])
                    || (game.Board[gameXIndex, gameYIndex, 0, 0] == game.Board[gameXIndex, gameYIndex, 1, 0]
                        && game.Board[gameXIndex, gameYIndex, 1, 0] == game.Board[gameXIndex, gameYIndex, 2, 0])))
            {
                return (GameStatuses)game.Board[gameXIndex, gameYIndex, 0, 0];
            }
            else if (game.Board[gameXIndex, gameYIndex, 1, 0].HasValue
                && game.Board[gameXIndex, gameYIndex, 1, 0] == game.Board[gameXIndex, gameYIndex, 1, 1]
                && game.Board[gameXIndex, gameYIndex, 1, 1] == game.Board[gameXIndex, gameYIndex, 1, 2])
            {
                return (GameStatuses)game.Board[gameXIndex, gameYIndex, 1, 0];
            }
            else if (game.Board[gameXIndex, gameYIndex, 2, 0].HasValue
                && game.Board[gameXIndex, gameYIndex, 2, 0] == game.Board[gameXIndex, gameYIndex, 2, 1]
                && game.Board[gameXIndex, gameYIndex, 2, 1] == game.Board[gameXIndex, gameYIndex, 2, 2])
            {
                return (GameStatuses)game.Board[gameXIndex, gameYIndex, 2, 0];
            }
            else if (game.Board[gameXIndex, gameYIndex, 0, 1].HasValue
                && game.Board[gameXIndex, gameYIndex, 0, 1] == game.Board[gameXIndex, gameYIndex, 1, 1]
                && game.Board[gameXIndex, gameYIndex, 1, 1] == game.Board[gameXIndex, gameYIndex, 2, 1])
            {
                return (GameStatuses)game.Board[gameXIndex, gameYIndex, 0, 1];
            }
            else if (game.Board[gameXIndex, gameYIndex, 0, 2].HasValue
                && ((game.Board[gameXIndex, gameYIndex, 0, 2] == game.Board[gameXIndex, gameYIndex, 1, 2]
                        && game.Board[gameXIndex, gameYIndex, 1, 2] == game.Board[gameXIndex, gameYIndex, 2, 2])
                    || (game.Board[gameXIndex, gameYIndex, 0, 2] == game.Board[gameXIndex, gameYIndex, 1, 1]
                        && game.Board[gameXIndex, gameYIndex, 1, 1] == game.Board[gameXIndex, gameYIndex, 2, 0])))
            {
                return (GameStatuses)game.Board[gameXIndex, gameYIndex, 0, 2];
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (!game.Board[gameXIndex, gameYIndex, i, j].HasValue)
                    {
                        return null;
                    }
                }
            }

            return GameStatuses.Tie;
        }
    }
}
