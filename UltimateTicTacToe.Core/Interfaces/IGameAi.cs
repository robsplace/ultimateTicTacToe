using UltimateTicTacToe.Core.Entities;

namespace UltimateTicTacToe.Core.Interfaces
{
    /// <summary>
    /// Ai interface for Ultimate Tic Tac Toe AI's.
    /// </summary>
    public interface IGameAi
    {
        /// <summary>
        /// Based on the input game, select a LEGAL board and pick on that board.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="boardX"></param>
        /// <param name="boardY"></param>
        /// <param name="pickX"></param>
        /// <param name="pickY"></param>
        void MakePick(Game game, out int boardX, out int boardY, out int pickX, out int pickY);

        /// <summary>
        /// The minimum difficulty level of the AI.
        /// </summary>
        int MinDifficulty { get; }

        /// <summary>
        /// The maximum difficulty level of the AI.
        /// </summary>
        int MaxDifficulty { get; }

        /// <summary>
        /// The set difficulty level of the AI.
        /// </summary>
        int Difficulty { get; set; }

        /// <summary>
        /// Cancel() is called when the AI takes too long.  This function should clean up all unmanaged resources (threads for example) to prevent memory/processing leaks.
        /// </summary>
        void Cancel();
    }
}
