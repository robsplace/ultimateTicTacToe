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
        public static void UpdateBoard(Game game, int boardXIndex, int boardYIndex, int pickXIndex, int pickYIndex)
        {
            if (boardXIndex < 0 || boardXIndex > 2
                || boardYIndex < 0 || boardYIndex > 2
                || pickXIndex < 0 || pickXIndex > 2
                || pickYIndex < 0 || pickYIndex > 2)
            {
                throw new ArgumentOutOfRangeException("Board or pick index out of bounds.  All must be between 0 and 2.");
            }

            if (game == null)
            {
                throw new ArgumentNullException("game cannot be null");
            }

            if (!IsPickValid(game, boardXIndex, boardYIndex, pickXIndex, pickYIndex))
            {
                throw new InvalidOperationException("Invalid pick.");
            }

            game.Board[boardXIndex, boardYIndex, pickXIndex, pickYIndex] = game.CurrentPlayer;
            game.LastPlay = new Tuple<int, int>(pickXIndex, pickYIndex);
            game.CurrentPlayer = game.CurrentPlayer == Players.X ? Players.O : Players.X;
        }

        public static bool IsPickValid(Game game, int boardXIndex, int boardYIndex, int pickXIndex, int pickYIndex)
        {
            if (boardXIndex < 0 || boardXIndex > 2
                || boardYIndex < 0 || boardYIndex > 2
                || pickXIndex < 0 || pickXIndex > 2
                || pickYIndex < 0 || pickYIndex > 2)
            {
                throw new ArgumentOutOfRangeException("Board or pick indexes out of bounds.  All must be between 0 and 2.");
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
                    || (game.LastPlay.Item1 == boardXIndex && game.LastPlay.Item2 == boardYIndex)
                    || GetBoardStatus(game, game.LastPlay.Item1, game.LastPlay.Item2).HasValue)
                && !GetBoardStatus(game, boardXIndex, boardYIndex).HasValue
                && game.Board[boardXIndex, boardYIndex, pickXIndex, pickYIndex] == null;
        }

        /// <summary>
        /// Return the game status of a particular board in a game.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="boardXIndex"></param>
        /// <param name="boardYIndex"></param>
        /// <returns>Game status of requested game, null if game is incomplete</returns>
        public static GameStatuses? GetBoardStatus(Game game, int boardXIndex, int boardYIndex)
        {
            if (boardXIndex < 0 || boardXIndex > 2
                   || boardYIndex < 0 || boardYIndex > 2)
            {
                throw new ArgumentOutOfRangeException("Game indexes out of bounds.  All must be between 0 and 2.");
            }
            if (game == null)
            {
                throw new ArgumentNullException("game cannot be null");
            }

            if (game.Board[boardXIndex, boardYIndex, 0, 0].HasValue
                && ((game.Board[boardXIndex, boardYIndex, 0, 0] == game.Board[boardXIndex, boardYIndex, 0, 1]
                        && game.Board[boardXIndex, boardYIndex, 0, 1] == game.Board[boardXIndex, boardYIndex, 0, 2])
                    || (game.Board[boardXIndex, boardYIndex, 0, 0] == game.Board[boardXIndex, boardYIndex, 1, 1]
                        && game.Board[boardXIndex, boardYIndex, 1, 1] == game.Board[boardXIndex, boardYIndex, 2, 2])
                    || (game.Board[boardXIndex, boardYIndex, 0, 0] == game.Board[boardXIndex, boardYIndex, 1, 0]
                        && game.Board[boardXIndex, boardYIndex, 1, 0] == game.Board[boardXIndex, boardYIndex, 2, 0])))
            {
                return (GameStatuses)game.Board[boardXIndex, boardYIndex, 0, 0];
            }
            else if (game.Board[boardXIndex, boardYIndex, 1, 0].HasValue
                && game.Board[boardXIndex, boardYIndex, 1, 0] == game.Board[boardXIndex, boardYIndex, 1, 1]
                && game.Board[boardXIndex, boardYIndex, 1, 1] == game.Board[boardXIndex, boardYIndex, 1, 2])
            {
                return (GameStatuses)game.Board[boardXIndex, boardYIndex, 1, 0];
            }
            else if (game.Board[boardXIndex, boardYIndex, 2, 0].HasValue
                && game.Board[boardXIndex, boardYIndex, 2, 0] == game.Board[boardXIndex, boardYIndex, 2, 1]
                && game.Board[boardXIndex, boardYIndex, 2, 1] == game.Board[boardXIndex, boardYIndex, 2, 2])
            {
                return (GameStatuses)game.Board[boardXIndex, boardYIndex, 2, 0];
            }
            else if (game.Board[boardXIndex, boardYIndex, 0, 1].HasValue
                && game.Board[boardXIndex, boardYIndex, 0, 1] == game.Board[boardXIndex, boardYIndex, 1, 1]
                && game.Board[boardXIndex, boardYIndex, 1, 1] == game.Board[boardXIndex, boardYIndex, 2, 1])
            {
                return (GameStatuses)game.Board[boardXIndex, boardYIndex, 0, 1];
            }
            else if (game.Board[boardXIndex, boardYIndex, 0, 2].HasValue
                && ((game.Board[boardXIndex, boardYIndex, 0, 2] == game.Board[boardXIndex, boardYIndex, 1, 2]
                        && game.Board[boardXIndex, boardYIndex, 1, 2] == game.Board[boardXIndex, boardYIndex, 2, 2])
                    || (game.Board[boardXIndex, boardYIndex, 0, 2] == game.Board[boardXIndex, boardYIndex, 1, 1]
                        && game.Board[boardXIndex, boardYIndex, 1, 1] == game.Board[boardXIndex, boardYIndex, 2, 0])))
            {
                return (GameStatuses)game.Board[boardXIndex, boardYIndex, 0, 2];
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (!game.Board[boardXIndex, boardYIndex, i, j].HasValue)
                    {
                        return null;
                    }
                }
            }

            return GameStatuses.Tie;
        }

        public static GameStatuses? GetGameStatus(Game game)
        {
            var boardStatuses = new GameStatuses?[3, 3];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    boardStatuses[i, j] = GetBoardStatus(game, i, j);
                }
            }


            throw new NotImplementedException();
        }
    }
}
