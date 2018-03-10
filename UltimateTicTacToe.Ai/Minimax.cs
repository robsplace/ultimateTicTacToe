using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UltimateTicTacToe.Core;
using UltimateTicTacToe.Core.Entities;
using UltimateTicTacToe.Core.Interfaces;

namespace UltimateTicTacToe.Ai
{
    abstract public class Minimax : IGameAi
    {
        public int MinDifficulty => 1;

        public int MaxDifficulty => 5;

        public int Difficulty { get; set; }

        private static Random _random = new Random();
        abstract protected int MAXIMUM_UTILITY { get; }
        abstract protected int MINIMUM_UTILITY { get; }

        public void MakePick(Game game, out int boardX, out int boardY, out int pickX, out int pickY)
        {
            boardX = boardY = pickX = pickY = 0;
            int bestScore = Int32.MinValue;
            List<Tuple<int, int, int, int>> bestResults = new List<Tuple<int, int, int, int>>();
            var children = new Dictionary<Game, Tuple<int, int, int, int>>();
            var threads = new List<Thread>();
            var results = new List<Tuple<int, int, int, int, int>>();

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
                var thread = new Thread(() =>
                {
                    results.Add(new Tuple<int, int, int, int, int>(children[child].Item1, children[child].Item2, children[child].Item3, children[child].Item4, GetMinimax(child, game.CurrentPlayer)));
                });
                thread.Start();
                threads.Add(thread);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            foreach (var result in results)
            {
                // idk ytf result is null sometimes but i'm too past the ballmer peak to implement anything but a workaround and null reference exceptions aren't cool
                if (result == null)
                {
                    continue;
                }

                var bestCaseScore = result.Item5;

                if (bestCaseScore > bestScore)
                {
                    bestResults.Clear();
                    bestResults.Add(new Tuple<int, int, int, int>(result.Item1, result.Item2, result.Item3, result.Item4));
                    bestScore = bestCaseScore;
                }
                else if (bestCaseScore == bestScore)
                {
                    bestResults.Add(new Tuple<int, int, int, int>(result.Item1, result.Item2, result.Item3, result.Item4));
                }
            }

            var winner = bestResults[_random.Next(bestResults.Count)];
            boardX = winner.Item1;
            boardY = winner.Item2;
            pickX = winner.Item3;
            pickY = winner.Item4;
        }

        private int GetMinimax(Game game, Players player, int? depth = null, int? min = null, int? max = null)
        {
            depth = depth ?? Difficulty;
            min = min ?? MINIMUM_UTILITY;
            max = max ?? MAXIMUM_UTILITY;

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
                    var vPrime = GetMinimax(child, player, depth - 1, v, max);
                    if (vPrime > v)
                    {
                        v = vPrime;
                    }
                    if (v > max)
                    {
                        return max.Value;
                    }
                }

                return v.Value;
            }
            // if it's a min node
            else
            {
                var v = max;

                foreach (var child in children)
                {
                    var vPrime = GetMinimax(child, player, depth - 1, min, v);
                    if (vPrime < v)
                    {
                        v = vPrime;
                    }
                    if (v < min)
                    {
                        return min.Value;
                    }
                }

                return v.Value;
            }
        }

        abstract protected int Utility(Game game, Players player);
    }
}
