using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltimateTicTacToe.Core.Entities
{
    public class Game
    {
        public Players CurrentPlayer { get; set; }

        public Players?[,,,] Board { get; } = new Players?[3, 3, 3, 3];

        public Tuple<int, int> LastPlay { get; set; } = new Tuple<int, int>(-1, -1);
    }
}
