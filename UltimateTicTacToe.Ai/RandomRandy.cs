﻿using System;
using UltimateTicTacToe.Core;
using UltimateTicTacToe.Core.Entities;
using UltimateTicTacToe.Core.Interfaces;

namespace UltimateTicTacToe.Ai
{
    public sealed class RandomRandy : IGameAi
    {
        public int MinDifficulty => 1;

        public int MaxDifficulty => 1;

        public int Difficulty { get => 1; set { } }

        private Random Random;

        public RandomRandy()
        {
            Random = new Random();
        }

        public void MakePick(Game game, out int boardX, out int boardY, out int pickX, out int pickY)
        {
            // this could be improved to not blindly pick _any_ spot until it finds a valid one, but that doesn't seem like Random Randy's way
            do
            {
                boardX = Random.Next(3);
                boardY = Random.Next(3);
                pickX = Random.Next(3);
                pickY = Random.Next(3);
            } while (GameMaster.IsPickValid(game, boardX, boardY, pickX, pickY));
        }

        public void Cancel()
        {
        }
    }
}
