using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltimateTicTacToe.Core;
using UltimateTicTacToe.Core.Entities;
using UltimateTicTacToe.Core.Interfaces;

namespace UltimateTicTacToe.Ai
{
    public abstract class AbstractMinimaxMandy : IGameAi
    {
        public abstract int Depth { get; }

        private static Random _random = new Random();
        private const int MAXIMUM_UTILITY = 100;
        private const int MINIMUM_UTILITY = -100;

        public void MakePick(Game game, out int boardX, out int boardY, out int pickX, out int pickY)
        {
            boardX = boardY = pickX = pickY = 0;
            int bestScore = Int32.MinValue;
            List<Tuple<int, int, int, int>> bestResults = new List<Tuple<int, int, int, int>>();
            var children = new Dictionary<Game, Tuple<int, int, int, int>>();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        for (int l = 0; l < 3; l++)
                        {
                            if (GameMaster.IsPickValid(game, i, j, k, l))
                            {
                                var newGame = game.Clone() as Game;
                                GameMaster.UpdateBoard(newGame, i, j, k, l);
                                children.Add(newGame, new Tuple<int, int, int, int>(i, j, k, l));
                            }
                        }
                    }
                }
            }

            foreach (var child in children.Keys)
            {
                var bestCaseScore = Minimax(child, game.CurrentPlayer);

                if (bestCaseScore > bestScore)
                {
                    bestResults.Clear();
                    bestResults.Add(children[child]);
                    bestScore = bestCaseScore;
                }
                else if (bestCaseScore == bestScore)
                {
                    bestResults.Add(children[child]);
                }
            }

            var winner = bestResults[_random.Next(bestResults.Count)];
            boardX = winner.Item1;
            boardY = winner.Item2;
            pickX = winner.Item3;
            pickY = winner.Item4;
        }

        private int Minimax(Game game, Players player, int? depth = null, int min = MINIMUM_UTILITY, int max = MAXIMUM_UTILITY)
        {
            if (depth == null)
            {
                depth = Depth;
            }

            // if leaf node or reached maximum depth, return Utility value of node
            if (GameMaster.GetGameStatus(game) != null || depth == 0)
            {
                return Utility(game, player);
            }

            // derive all valid children
            var children = new List<Game>();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        for (int l = 0; l < 3; l++)
                        {
                            if (GameMaster.IsPickValid(game, i, j, k, l))
                            {
                                var newGame = game.Clone() as Game;
                                GameMaster.UpdateBoard(newGame, i, j, k, l);
                                children.Add(newGame);
                            }
                        }
                    }
                }
            }

            // if it's a max node
            if (game.CurrentPlayer == player)
            {
                var v = min;

                foreach (var child in children)
                {
                    var vPrime = Minimax(child, player, depth - 1, v, max);
                    if (vPrime > v)
                    {
                        v = vPrime;
                    }
                    if (v > max)
                    {
                        return max;
                    }
                }

                return v;
            }
            // if it's a min node
            else
            {
                var v = max;

                foreach (var child in children)
                {
                    var vPrime = Minimax(child, player, depth - 1, min, v);
                    if (vPrime < v)
                    {
                        v = vPrime;
                    }
                    if (v < min)
                    {
                        return min;
                    }
                }

                return v;
            }
        }

        private int Utility(Game game, Players player)
        {
            var gameStatus = GameMaster.GetGameStatus(game);

            var result = 0;

            // if this is a leaf node, return (100 - totalNumMoves) * (lostGame ? -1 : 1)
            if (gameStatus.HasValue)
            {
                result = 100;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            for (int l = 0; l < 3; l++)
                            {
                                if (game.Board[i, j, k, l].HasValue)
                                {
                                    result--;
                                }
                            }
                        }
                    }
                }
                if ((gameStatus == GameStatuses.OWon && player == Players.X) || (gameStatus == GameStatuses.XWon && player == Players.O))
                {
                    result *= -1;
                }
            }
            // if this isn't a leaf node, return the difference between boards won and lost
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var boardStatus = GameMaster.GetBoardStatus(game, i, j);
                        if (boardStatus.HasValue && boardStatus.Value != GameStatuses.Tie)
                        {
                            if ((boardStatus == GameStatuses.OWon && player == Players.O) || (boardStatus == GameStatuses.XWon && player == Players.X))
                            {
                                result++;
                            }
                            else
                            {
                                result--;
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
