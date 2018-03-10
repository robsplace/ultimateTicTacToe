using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UltimateTicTacToe.Core.Interfaces;

namespace UltimateTicTacToe
{
    /// <summary>
    /// Interaction logic for NewGameDialog.xaml
    /// </summary>
    public partial class NewGameDialog : Window
    {
        public string PlayerXName { get; private set; }
        public string PlayerOName { get; private set; }
        public IGameAi PlayerX { get; private set; } = null;
        public IGameAi PlayerO { get; private set; } = null;
        public uint? NumSimulations { get; private set; }

        private List<Type> _pluginTypes = new List<Type>();

        public NewGameDialog(List<Type> pluginTypes, string playerXName, string playerOName, Type playerXType, Type playerOType, int playerXDifficulty, int playerODifficulty, uint? numSimulations = null)
        {
            InitializeComponent();

            cbPlayerXType.SelectedValuePath = "Key";
            cbPlayerOType.SelectedValuePath = "Key";

            cbPlayerXType.DisplayMemberPath = "Value";
            cbPlayerOType.DisplayMemberPath = "Value";

            cbPlayerXType.Items.Add(new KeyValuePair<Type, string>(null, "Human"));
            cbPlayerOType.Items.Add(new KeyValuePair<Type, string>(null, "Human"));

            foreach (var type in pluginTypes)
            {
                cbPlayerXType.Items.Add(new KeyValuePair<Type, string>(type, string.Format("{0} ({1})", type.Name, type.Namespace)));
                cbPlayerOType.Items.Add(new KeyValuePair<Type, string>(type, string.Format("{0} ({1})", type.Name, type.Namespace)));
            }

            cbPlayerXType.SelectedValue = playerXType;
            cbPlayerOType.SelectedValue = playerOType;

            cbPlayerXType_SelectionChanged(null, null);
            cbPlayerOType_SelectionChanged(null, null);

            slPlayerXDifficulty.Value = playerXDifficulty;
            slPlayerODifficulty.Value = playerODifficulty;

            tbPlayerXName.Text = playerXName;
            tbPlayerOName.Text = playerOName;

            chkSimulate.IsChecked = numSimulations.HasValue;
        }

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerXName = tbPlayerXName.Text;
            PlayerOName = tbPlayerOName.Text;

            if (string.IsNullOrWhiteSpace(PlayerXName))
            {
                MessageBox.Show("Please give Player X a name.");
                return;
            }

            if (string.IsNullOrWhiteSpace(PlayerOName))
            {
                MessageBox.Show("Please give Player O a name.");
                return;
            }

            if (PlayerX != null)
                PlayerX.Difficulty = (int)slPlayerXDifficulty.Value;

            if (PlayerO != null)
                PlayerO.Difficulty = (int)slPlayerODifficulty.Value;

            if (chkSimulate.IsChecked == true)
            {
                if (PlayerX == null || PlayerO == null)
                {
                    MessageBox.Show("Cannot use human players in simulations.");
                    return;
                }

                if (uint.TryParse(tbGamesToSimulate.Text.Trim(), out uint numSimulations))
                {
                    NumSimulations = numSimulations;
                }
                else
                {
                    MessageBox.Show(string.Format("Number of simulations must be a value between 1 and {0}", uint.MaxValue));
                    return;
                }
            }
            else
            {
                NumSimulations = null;
            }

            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextBoxSelectAll(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        private void chkSimulate_Checked(object sender, RoutedEventArgs e)
        {
            gbSimOptions.IsEnabled = true;
        }

        private void chkSimulate_Unchecked(object sender, RoutedEventArgs e)
        {
            gbSimOptions.IsEnabled = false;
        }

        private void cbPlayerXType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var playerType = cbPlayerXType.SelectedValue as Type;
            PlayerX = playerType == null ? null : (IGameAi)Activator.CreateInstance(playerType);
            slPlayerXDifficulty.Minimum = PlayerX?.MinDifficulty ?? 1;
            slPlayerXDifficulty.Maximum = PlayerX?.MaxDifficulty ?? 1;
        }

        private void cbPlayerOType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var playerType = cbPlayerOType.SelectedValue as Type;
            PlayerO = playerType == null ? null : (IGameAi)Activator.CreateInstance(playerType);
            slPlayerODifficulty.Minimum = PlayerO?.MinDifficulty ?? 1;
            slPlayerODifficulty.Maximum = PlayerO?.MaxDifficulty ?? 1;
        }
    }
}
