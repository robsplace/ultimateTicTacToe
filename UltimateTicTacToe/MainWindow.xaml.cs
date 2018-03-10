using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UltimateTicTacToe.Core;
using UltimateTicTacToe.Core.Entities;
using UltimateTicTacToe.Core.Interfaces;
using UltimateTicTacToe.Properties;

namespace UltimateTicTacToe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Game _game = null;
        private Button[,,,] _gameButtons = new Button[3, 3, 3, 3];
        private Grid[,] _boardGridViews = new Grid[3, 3];
        private TextBlock[,] _boardResults = new TextBlock[3, 3];
        private List<Type> _pluginTypes = new List<Type>();
        private Dictionary<Players, IGameAi> _playerAis = new Dictionary<Players, IGameAi>() { { Players.X, null }, { Players.O, null } };
        private uint? NumSimulations = null;

        public MainWindow()
        {
            InitializeComponent();
            RefreshPlugins();
        }

        private async void btnNewGame_Click(object sender, RoutedEventArgs e)
        {
            NewGameDialog ngd = new NewGameDialog(_pluginTypes, txtPlayerX.Text, txtPlayerO.Text, _playerAis[Players.X]?.GetType(), _playerAis[Players.O]?.GetType(), _playerAis[Players.X]?.Difficulty ?? 1, _playerAis[Players.O]?.Difficulty ?? 1, NumSimulations);
            if (ngd.ShowDialog() == true) // this looks stupid but easy way to check for truthy since ShowDialog returns bool?
            {
                RestartGameButton.IsEnabled = true;
                _game = new Game();
                txtPlayerX.Text = ngd.PlayerXName;
                txtPlayerO.Text = ngd.PlayerOName;
                tbSimPlayerX.Text = ngd.PlayerXName;
                tbSimPlayerO.Text = ngd.PlayerOName;
                _playerAis[Players.X] = ngd.PlayerX;
                _playerAis[Players.O] = ngd.PlayerO;
                NumSimulations = ngd.NumSimulations;

                txtGameStatus.Text = string.Empty;

                if (NumSimulations.HasValue)
                {
                    gridSimulation.Visibility = Visibility.Visible;
                    gridVisibleGame.Visibility = Visibility.Collapsed;

                    await SimulateGames();
                }
                else
                {
                    gridSimulation.Visibility = Visibility.Collapsed;
                    gridVisibleGame.Visibility = Visibility.Visible;
                    await UpdateBoard();
                }
            }
        }

        private async Task SimulateGames()
        {
            IsEnabled = false;
            txtGameStatus.Visibility = Visibility.Collapsed;
            pbTimer.Visibility = Visibility.Visible;
            pbTimer.Maximum = Settings.Default.MaxAiTime / 1000;
            tbTotalGames.Text = NumSimulations.ToString();
            tbSimPlayerXType.Text = string.Format("{0} ({1})", _playerAis[Players.X].GetType().Name, _playerAis[Players.X].GetType().Namespace);
            tbSimPlayerOType.Text = string.Format("{0} ({1})", _playerAis[Players.O].GetType().Name, _playerAis[Players.O].GetType().Namespace);
            int xGames = 0, oGames = 0, ties = 0;

            tbGamesPlayed.Text = "0";
            for (uint i = 0; i < NumSimulations.Value; i++)
            {
                _game = new Game();

                while (!GameMaster.GetGameStatus(_game).HasValue)
                {
                    pbTimer.Value = pbTimer.Maximum;
                    using (var timer = new System.Windows.Forms.Timer())
                    {
                        var startTime = DateTime.Now;
                        timer.Interval = 1000;
                        timer.Tick += new EventHandler((sender, e) => { pbTimer.Value = pbTimer.Maximum - (DateTime.Now - startTime).TotalSeconds; });
                        timer.Start();
                        try
                        {
                            int boardX = 0, boardY = 0, pickX = 0, pickY = 0;

                            await Task.Run(() =>
                            {
                                var thread = new Thread(() =>
                                {
                                    do
                                    {
                                        _playerAis[_game.CurrentPlayer].MakePick(_game, out boardX, out boardY, out pickX, out pickY);
                                    } while (!GameMaster.IsPickValid(_game, boardX, boardY, pickX, pickY));
                                });

                                thread.Start();

                                if (!thread.Join(TimeSpan.FromMilliseconds(Settings.Default.MaxAiTime)))
                                {
                                    thread.Abort();
                                    _playerAis[_game.CurrentPlayer].Cancel();
                                    throw new OperationCanceledException();
                                }
                            });

                            GameMaster.UpdateBoard(_game, boardX, boardY, pickX, pickY);
                        }
                        catch (OperationCanceledException)
                        {
                            if (_game.CurrentPlayer == Players.O)
                            {
                                xGames++;
                            }
                            else
                            {
                                oGames++;
                            }
                            continue;
                        }
                        timer.Stop();
                    }
                }

                var gameStatus = GameMaster.GetGameStatus(_game);
                if (gameStatus == GameStatuses.OWon)
                {
                    oGames++;
                }
                else if (gameStatus == GameStatuses.XWon)
                {
                    xGames++;
                }
                else
                {
                    ties++;
                }

                tbSimPlayerOWins.Text = oGames.ToString();
                tbSimPlayerXWins.Text = xGames.ToString();

                tbSimTies.Text = ties.ToString();
                tbGamesPlayed.Text = (i + 1).ToString();
            }

            txtGameStatus.Visibility = Visibility.Visible;
            pbTimer.Visibility = Visibility.Collapsed;

            IsEnabled = true;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private async Task UpdateBoard()
        {
            do
            {
                txtPlayerO.FontWeight = FontWeights.Normal;
                txtPlayerX.FontWeight = FontWeights.Normal;

                if (_game == null)
                {
                    foreach (var gameButton in _gameButtons)
                    {
                        gameButton.IsEnabled = true;
                        ((gameButton.Content as Viewbox).Child as TextBlock).Text = string.Empty;
                    }

                    foreach (var grid in _boardGridViews)
                    {
                        grid.IsEnabled = false;
                    }

                    return;
                }

                txtGameStatus.Text = string.Empty;

                switch (_game.CurrentPlayer)
                {
                    case Players.X:
                        txtPlayerX.FontWeight = FontWeights.ExtraBold;
                        break;
                    case Players.O:
                        txtPlayerO.FontWeight = FontWeights.ExtraBold;
                        break;
                }

                Tuple<int, int> forcedBoard = null;

                // if the next play is in a forced block
                if (_game.LastPlay != null
                    && _game.LastPlay.Item3 >= 0 && _game.LastPlay.Item3 < 3
                    && _game.LastPlay.Item4 >= 0 && _game.LastPlay.Item4 < 3
                    && !GameMaster.GetBoardStatus(_game, _game.LastPlay.Item3, _game.LastPlay.Item4).HasValue)
                {
                    forcedBoard = new Tuple<int, int>(_game.LastPlay.Item3, _game.LastPlay.Item4);
                }

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var gameStatus = GameMaster.GetBoardStatus(_game, i, j);
                        if (gameStatus.HasValue)
                        {
                            switch (gameStatus)
                            {
                                case GameStatuses.Tie:
                                    _boardResults[i, j].Text = "?";
                                    break;
                                case GameStatuses.XWon:
                                    _boardResults[i, j].Text = "X";
                                    break;
                                case GameStatuses.OWon:
                                    _boardResults[i, j].Text = "O";
                                    break;
                            }

                            if (_game.LastPlay.Item1 == i && _game.LastPlay.Item2 == j)
                            {
                                _boardResults[i, j].Foreground = Brushes.BlueViolet;
                            }
                            else
                            {
                                _boardResults[i, j].Foreground = Brushes.Black;
                            }

                            _boardResults[i, j].Visibility = Visibility.Visible;
                            _boardGridViews[i, j].Opacity = 0.15;
                        }
                        else
                        {
                            _boardResults[i, j].Text = string.Empty;
                            _boardResults[i, j].Visibility = Visibility.Collapsed;
                            _boardGridViews[i, j].Opacity = 1;
                        }
                        _boardGridViews[i, j].IsEnabled = forcedBoard != null ? forcedBoard.Item1 == i && forcedBoard.Item2 == j : !gameStatus.HasValue;
                        for (int k = 0; k < 3; k++)
                        {
                            for (int l = 0; l < 3; l++)
                            {
                                ((_gameButtons[i, j, k, l].Content as Viewbox).Child as TextBlock).Text = _game.Board[i, j, k, l]?.ToString() ?? string.Empty;
                                _gameButtons[i, j, k, l].IsEnabled = !_game.Board[i, j, k, l].HasValue;

                                if (_game.LastPlay?.Item1 == i && _game.LastPlay?.Item2 == j && _game.LastPlay?.Item3 == k && _game.LastPlay?.Item4 == l)
                                {
                                    ((_gameButtons[i, j, k, l].Content as Viewbox).Child as TextBlock).Foreground = Brushes.BlueViolet;
                                }
                                else
                                {
                                    ((_gameButtons[i, j, k, l].Content as Viewbox).Child as TextBlock).Foreground = Brushes.Black;
                                }
                            }
                        }
                    }
                }

                var gameStatusX = GameMaster.GetGameStatus(_game);
                if (gameStatusX.HasValue)
                {
                    foreach (var grid in _boardGridViews)
                    {
                        grid.IsEnabled = false;
                    }

                    switch (gameStatusX)
                    {
                        case GameStatuses.Tie:
                            txtGameStatus.Text = "Game tied!";
                            break;
                        case GameStatuses.OWon:
                            txtGameStatus.Text = string.Format("{0} has won!!", txtPlayerO.Text);
                            break;
                        case GameStatuses.XWon:
                            txtGameStatus.Text = string.Format("{0} has won!!", txtPlayerX.Text);
                            break;
                    }

                    return;
                }
            } while (await ProcessNextPlayerAiTurn());
        }

        private async Task<bool> ProcessNextPlayerAiTurn()
        {
            if (_playerAis[_game.CurrentPlayer] == null)
            {
                return false;
            }

            IsEnabled = false;
            txtGameStatus.Visibility = Visibility.Collapsed;
            pbTimer.Visibility = Visibility.Visible;
            pbTimer.Maximum = Settings.Default.MaxAiTime / 1000;

            try
            {
                int boardX = 0, boardY = 0, pickX = 0, pickY = 0;
                pbTimer.Value = pbTimer.Maximum;
                using (var timer = new System.Windows.Forms.Timer())
                {
                    timer.Interval = 1000;
                    var startTime = DateTime.Now;
                    timer.Tick += new EventHandler((sender, e) => { pbTimer.Value = pbTimer.Maximum - (DateTime.Now - startTime).TotalSeconds; });
                    timer.Start();
                    Task makePick = Task.Run(() =>
                    {
                        var thread = new Thread(() =>
                        {
                            do
                            {
                                _playerAis[_game.CurrentPlayer].MakePick(_game, out boardX, out boardY, out pickX, out pickY);
                            } while (!GameMaster.IsPickValid(_game, boardX, boardY, pickX, pickY));
                        });

                        thread.Start();

                        if (!thread.Join(TimeSpan.FromMilliseconds(Settings.Default.MaxAiTime)))
                        {
                            thread.Abort();
                            _playerAis[_game.CurrentPlayer].Cancel();
                            throw new OperationCanceledException();
                        }
                    });

                    await Task.WhenAll(makePick, Task.Delay(Settings.Default.MinAiTime));
                    timer.Stop();
                }
                GameMaster.UpdateBoard(_game, boardX, boardY, pickX, pickY);
            }
            catch (OperationCanceledException)
            {
                // ai timed out.  what do?
                foreach (var grid in _boardGridViews)
                {
                    grid.IsEnabled = false;
                }

                txtGameStatus.Text = string.Format("{0} took longer than {1} milliseconds and is disqualified", _game.CurrentPlayer, Settings.Default.MaxAiTime);
                return false;
            }
            finally
            {
                txtGameStatus.Visibility = Visibility.Visible;
                pbTimer.Visibility = Visibility.Collapsed;
                IsEnabled = true;
            }

            return true;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // i'm sure there's a better way to do this but...meh
            _gameButtons[0, 0, 0, 0] = btn00;
            _gameButtons[0, 0, 0, 1] = btn01;
            _gameButtons[0, 0, 0, 2] = btn02;
            _gameButtons[0, 0, 1, 0] = btn10;
            _gameButtons[0, 0, 1, 1] = btn11;
            _gameButtons[0, 0, 1, 2] = btn12;
            _gameButtons[0, 0, 2, 0] = btn20;
            _gameButtons[0, 0, 2, 1] = btn21;
            _gameButtons[0, 0, 2, 2] = btn22;

            _gameButtons[0, 1, 0, 0] = btn03;
            _gameButtons[0, 1, 0, 1] = btn04;
            _gameButtons[0, 1, 0, 2] = btn05;
            _gameButtons[0, 1, 1, 0] = btn13;
            _gameButtons[0, 1, 1, 1] = btn14;
            _gameButtons[0, 1, 1, 2] = btn15;
            _gameButtons[0, 1, 2, 0] = btn23;
            _gameButtons[0, 1, 2, 1] = btn24;
            _gameButtons[0, 1, 2, 2] = btn25;

            _gameButtons[0, 2, 0, 0] = btn06;
            _gameButtons[0, 2, 0, 1] = btn07;
            _gameButtons[0, 2, 0, 2] = btn08;
            _gameButtons[0, 2, 1, 0] = btn16;
            _gameButtons[0, 2, 1, 1] = btn17;
            _gameButtons[0, 2, 1, 2] = btn18;
            _gameButtons[0, 2, 2, 0] = btn26;
            _gameButtons[0, 2, 2, 1] = btn27;
            _gameButtons[0, 2, 2, 2] = btn28;

            _gameButtons[1, 0, 0, 0] = btn30;
            _gameButtons[1, 0, 0, 1] = btn31;
            _gameButtons[1, 0, 0, 2] = btn32;
            _gameButtons[1, 0, 1, 0] = btn40;
            _gameButtons[1, 0, 1, 1] = btn41;
            _gameButtons[1, 0, 1, 2] = btn42;
            _gameButtons[1, 0, 2, 0] = btn50;
            _gameButtons[1, 0, 2, 1] = btn51;
            _gameButtons[1, 0, 2, 2] = btn52;

            _gameButtons[1, 1, 0, 0] = btn33;
            _gameButtons[1, 1, 0, 1] = btn34;
            _gameButtons[1, 1, 0, 2] = btn35;
            _gameButtons[1, 1, 1, 0] = btn43;
            _gameButtons[1, 1, 1, 1] = btn44;
            _gameButtons[1, 1, 1, 2] = btn45;
            _gameButtons[1, 1, 2, 0] = btn53;
            _gameButtons[1, 1, 2, 1] = btn54;
            _gameButtons[1, 1, 2, 2] = btn55;

            _gameButtons[1, 2, 0, 0] = btn36;
            _gameButtons[1, 2, 0, 1] = btn37;
            _gameButtons[1, 2, 0, 2] = btn38;
            _gameButtons[1, 2, 1, 0] = btn46;
            _gameButtons[1, 2, 1, 1] = btn47;
            _gameButtons[1, 2, 1, 2] = btn48;
            _gameButtons[1, 2, 2, 0] = btn56;
            _gameButtons[1, 2, 2, 1] = btn57;
            _gameButtons[1, 2, 2, 2] = btn58;

            _gameButtons[2, 0, 0, 0] = btn60;
            _gameButtons[2, 0, 0, 1] = btn61;
            _gameButtons[2, 0, 0, 2] = btn62;
            _gameButtons[2, 0, 1, 0] = btn70;
            _gameButtons[2, 0, 1, 1] = btn71;
            _gameButtons[2, 0, 1, 2] = btn72;
            _gameButtons[2, 0, 2, 0] = btn80;
            _gameButtons[2, 0, 2, 1] = btn81;
            _gameButtons[2, 0, 2, 2] = btn82;

            _gameButtons[2, 1, 0, 0] = btn63;
            _gameButtons[2, 1, 0, 1] = btn64;
            _gameButtons[2, 1, 0, 2] = btn65;
            _gameButtons[2, 1, 1, 0] = btn73;
            _gameButtons[2, 1, 1, 1] = btn74;
            _gameButtons[2, 1, 1, 2] = btn75;
            _gameButtons[2, 1, 2, 0] = btn83;
            _gameButtons[2, 1, 2, 1] = btn84;
            _gameButtons[2, 1, 2, 2] = btn85;

            _gameButtons[2, 2, 0, 0] = btn66;
            _gameButtons[2, 2, 0, 1] = btn67;
            _gameButtons[2, 2, 0, 2] = btn68;
            _gameButtons[2, 2, 1, 0] = btn76;
            _gameButtons[2, 2, 1, 1] = btn77;
            _gameButtons[2, 2, 1, 2] = btn78;
            _gameButtons[2, 2, 2, 0] = btn86;
            _gameButtons[2, 2, 2, 1] = btn87;
            _gameButtons[2, 2, 2, 2] = btn88;

            _boardGridViews[0, 0] = game00;
            _boardGridViews[0, 1] = game01;
            _boardGridViews[0, 2] = game02;
            _boardGridViews[1, 0] = game10;
            _boardGridViews[1, 1] = game11;
            _boardGridViews[1, 2] = game12;
            _boardGridViews[2, 0] = game20;
            _boardGridViews[2, 1] = game21;
            _boardGridViews[2, 2] = game22;

            _boardResults[0, 0] = txtBoardStatus00;
            _boardResults[0, 1] = txtBoardStatus01;
            _boardResults[0, 2] = txtBoardStatus02;
            _boardResults[1, 0] = txtBoardStatus10;
            _boardResults[1, 1] = txtBoardStatus11;
            _boardResults[1, 2] = txtBoardStatus12;
            _boardResults[2, 0] = txtBoardStatus20;
            _boardResults[2, 1] = txtBoardStatus21;
            _boardResults[2, 2] = txtBoardStatus22;

            await UpdateBoard();
        }

        private async void btn_Click(object sender, RoutedEventArgs e)
        {
            var row = Int32.Parse(((Button)sender).Name.Substring(3, 1));
            var col = Int32.Parse(((Button)sender).Name.Substring(4, 1));
            GameMaster.UpdateBoard(_game, row / 3, col / 3, row % 3, col % 3);
            await UpdateBoard();
        }

        private void btn_MouseLeave(object sender, MouseEventArgs e)
        {
            var btn = sender as Button;
            if (btn.IsEnabled)
            {
                ((btn.Content as Viewbox).Child as TextBlock).Text = string.Empty;
            }
        }

        private void btn_MouseEnter(object sender, MouseEventArgs e)
        {
            var btn = sender as Button;
            if (btn.IsEnabled
                && _game != null)
            {
                ((btn.Content as Viewbox).Child as TextBlock).Text = _game.CurrentPlayer == Players.X ? "X" : "O";
            }
        }

        private async void RestartGame(object sender, RoutedEventArgs e)
        {
            _game = new Game();
            txtGameStatus.Text = string.Empty;
            await UpdateBoard();
        }

        private void ModifyOptions(object sender, RoutedEventArgs e)
        {
            // ShowDialog is nullable so comparing to true is easy way to test for truth
            if (new OptionsDialog().ShowDialog() == true)
            {
                RefreshPlugins();
            }
        }

        private void RefreshPlugins()
        {
            var path = Settings.Default.PluginsFolder ?? string.Empty;

            if (!Directory.Exists(path))
            {
                return;
            }

            var dllFileNames = Directory.GetFiles(path, "*.dll");

            if (dllFileNames.Length == 0)
            {
                return;
            }

            var assemblies = new List<Assembly>(dllFileNames.Length);
            foreach (var dllFile in dllFileNames)
            {
                AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                Assembly assembly = Assembly.Load(an);
                assemblies.Add(assembly);
            }

            var pluginType = typeof(IGameAi);
            _pluginTypes = new List<Type>();
            foreach (var assembly in assemblies)
            {
                if (assembly != null)
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.IsInterface || type.IsAbstract)
                        {
                            continue;
                        }
                        else
                        {
                            if (type.GetInterface(pluginType.FullName) != null)
                            {
                                _pluginTypes.Add(type);
                            }
                        }
                    }
                }
            }
        }
    }
}
