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
        private static readonly Tuple<int[], int[], int[]>[] _winningGameConfigurations = new Tuple<int[], int[], int[]>[]
        {
            new Tuple<int[], int[], int[]>(new int[] {0, 0}, new int[] {0, 1}, new int[] {0, 2}),
            new Tuple<int[], int[], int[]>(new int[] {1, 0}, new int[] {1, 1}, new int[] {1, 2}),
            new Tuple<int[], int[], int[]>(new int[] {2, 0}, new int[] {2, 1}, new int[] {2, 2}),
            new Tuple<int[], int[], int[]>(new int[] {0, 0}, new int[] {1, 0}, new int[] {2, 0}),
            new Tuple<int[], int[], int[]>(new int[] {0, 1}, new int[] {1, 1}, new int[] {2, 1}),
            new Tuple<int[], int[], int[]>(new int[] {0, 2}, new int[] {1, 2}, new int[] {2, 2}),
            new Tuple<int[], int[], int[]>(new int[] {0, 0}, new int[] {1, 1}, new int[] {2, 2}),
            new Tuple<int[], int[], int[]>(new int[] {0, 2}, new int[] {1, 1}, new int[] {2, 0}),
        };

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
            game.LastPlay = new Tuple<int, int, int, int>(boardXIndex, boardYIndex, pickXIndex, pickYIndex);
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
                    || (game.LastPlay.Item3 == boardXIndex && game.LastPlay.Item4 == boardYIndex)
                    || GetBoardStatus(game, game.LastPlay.Item3, game.LastPlay.Item4).HasValue)
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

            foreach (var winningGameConfig in _winningGameConfigurations)
            {
                var result = GetGameStatus(game.Board[boardXIndex, boardYIndex, winningGameConfig.Item1[0], winningGameConfig.Item1[1]], game.Board[boardXIndex, boardYIndex, winningGameConfig.Item2[0], winningGameConfig.Item2[1]], game.Board[boardXIndex, boardYIndex, winningGameConfig.Item3[0], winningGameConfig.Item3[1]]);
                if (result.HasValue)
                {
                    return result;
                }
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

            GameStatuses? result = null, previousResult = null;
            foreach (var winningGameConfig in _winningGameConfigurations)
            {
                result = GetGameStatus(boardStatuses[winningGameConfig.Item1[0], winningGameConfig.Item1[1]], boardStatuses[winningGameConfig.Item2[0], winningGameConfig.Item2[1]], boardStatuses[winningGameConfig.Item3[0], winningGameConfig.Item3[1]]);

                if (result.HasValue)
                {
                    if (result == GameStatuses.Tie
                        || (previousResult.HasValue && previousResult != result))
                    {
                        return GameStatuses.Tie;
                    }

                    previousResult = result;
                }
            }

            // null for previousResult means there is no game winner...better make sure the board isn't full
            if (previousResult == null)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (!GetBoardStatus(game, i, j).HasValue)
                        {
                            return null;
                        }
                    }
                }

                // if the board is full, it's a tie
                return GameStatuses.Tie;
            }

            return previousResult;
        }

        private static GameStatuses? GetGameStatus(Players? player0, Players? player1, Players? player2)
        {
            GameStatuses? gameStatus0 = null, gameStatus1 = null, gameStatus2 = null;
            if (player0.HasValue)
            {
                switch (player0)
                {
                    case Players.X:
                        gameStatus0 = GameStatuses.XWon;
                        break;
                    case Players.O:
                        gameStatus0 = GameStatuses.OWon;
                        break;
                }
            }
            if (player1.HasValue)
            {
                switch (player1)
                {
                    case Players.X:
                        gameStatus1 = GameStatuses.XWon;
                        break;
                    case Players.O:
                        gameStatus1 = GameStatuses.OWon;
                        break;
                }
            }
            if (player2.HasValue)
            {
                switch (player2)
                {
                    case Players.X:
                        gameStatus2 = GameStatuses.XWon;
                        break;
                    case Players.O:
                        gameStatus2 = GameStatuses.OWon;
                        break;
                }
            }

            return GetGameStatus(gameStatus0, gameStatus1, gameStatus2);
        }

        private static GameStatuses? GetGameStatus(GameStatuses? status0, GameStatuses? status1, GameStatuses? status2)
        {
            // make sure each cell has a value
            if (!status0.HasValue
                || !status1.HasValue
                || !status2.HasValue)
            {
                return null;
            }

            //  ? | _ | _
            if (status0 == GameStatuses.Tie)
            {
                //  ? | ? | ?
                //  ? | ? | i
                //  ? | i | i
                if (status1 == GameStatuses.Tie
                    || status1 == status2)
                {
                    return status2;
                }
                //  ? | i | ?
                else if (status2 == GameStatuses.Tie)
                {
                    return status1;
                }
            }
            //  i | ? | _
            else if (status1 == GameStatuses.Tie)
            {
                //  i | ? | ?
                //  i | ? | i
                if (status2 == GameStatuses.Tie
                    || status2 == status0)
                {
                    return status0;
                }
            }
            //  i | i | ?
            //  i | i | i
            else if (status0 == status1 && (status2 == GameStatuses.Tie || status1 == status2))
            {
                return status0;
            }

            return null;
        }
    }
}
