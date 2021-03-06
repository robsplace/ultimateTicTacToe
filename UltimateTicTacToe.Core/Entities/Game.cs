﻿using System;
using System.Text;

namespace UltimateTicTacToe.Core.Entities
{
    public class Game : ICloneable
    {
        /// <summary>
        /// The player whose turn it is.
        /// </summary>
        public Players CurrentPlayer { get; set; } = Players.X;

        /// <summary>
        /// The board, where null means nobody has selected that spot.
        /// </summary>
        public Players?[,,,] Board { get; } = new Players?[3, 3, 3, 3];

        /// <summary>
        /// The last move made by the opponent.
        /// </summary>
        public Tuple<int, int, int, int> LastPlay { get; set; } = null;

        public object Clone()
        {
            var game = new Game()
            {
                CurrentPlayer = this.CurrentPlayer
            };

            if (this.LastPlay != null)
            {
                game.LastPlay = new Tuple<int, int, int, int>(this.LastPlay.Item1, this.LastPlay.Item2, this.LastPlay.Item3, this.LastPlay.Item4);
            }

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    for (int k = 0; k < 3; k++)
                        for (int l = 0; l < 3; l++)
                            game.Board[i, j, k, l] = this.Board[i, j, k, l];

            return game;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Current Player: {0}{1}", CurrentPlayer.ToString(), System.Environment.NewLine);

            // for each row of games
            for (int i = 0; i < 3; i++)
            {
                // for each board row in the current game row
                for (int k = 0; k < 3; k++)
                {
                    // for each game column in row of games
                    for (int j = 0; j < 3; j++)
                    {
                        // for each column in the board
                        for (int l = 0; l < 3; l++)
                        {
                            stringBuilder.Append(Board[i, j, k, l].HasValue ? Board[i,j,k,l].Value.ToString() : "?");
                        }
                        if (j != 2)
                        {
                            stringBuilder.Append(" | ");
                        }
                    }
                    stringBuilder.AppendLine();
                }
                if (i != 2)
                {
                    stringBuilder.AppendLine("---------------");
                }
            }

            return stringBuilder.ToString();
        }
    }
}
