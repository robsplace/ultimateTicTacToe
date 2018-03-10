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
    public class PredictablePriya : IGameAi
    {
        public int MinDifficulty => 1;

        public int MaxDifficulty => 1;

        public int Difficulty { get => 1; set { } }

        public void MakePick(Game game, out int boardX, out int boardY, out int pickX, out int pickY)
        {
            boardX = 0;
            boardY = 0;
            pickX = 0;
            pickY = 0;

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    for (int k = 0; k < 3; k++)
                        for (int l = 0; l < 3; l++)
                            if (GameMaster.IsPickValid(game, i, j, k, l))
                            {
                                boardX = i;
                                boardY = j;
                                pickX = k;
                                pickY = l;
                                return;
                            }
        }
    }
}
