using UltimateTicTacToe.Core;
using UltimateTicTacToe.Core.Entities;

namespace UltimateTicTacToe.Ai
{
    public sealed class MinimaxMandy : Minimax
    {
        public override int MinDifficulty => 1;
        public override int MaxDifficulty => 6;
        protected override int MAXIMUM_UTILITY => 100;
        protected override int MINIMUM_UTILITY => -100;

        protected override int Utility(Game game, Players player)
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
