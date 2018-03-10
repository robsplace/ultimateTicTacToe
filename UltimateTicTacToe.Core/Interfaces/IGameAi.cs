using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltimateTicTacToe.Core.Entities;

namespace UltimateTicTacToe.Core.Interfaces
{
    public interface IGameAi
    {
        void MakePick(Game game, out int boardX, out int boardY, out int pickX, out int pickY);
        int MinDifficulty { get; }
        int MaxDifficulty { get; }
        int Difficulty { get; set; }
        void Cancel();
    }
}
