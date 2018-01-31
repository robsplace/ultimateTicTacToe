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

namespace UltimateTicTacToe
{
    /// <summary>
    /// Interaction logic for NewGameDialog.xaml
    /// </summary>
    public partial class NewGameDialog : Window
    {
        public string PlayerXName { get; private set; }
        public string PlayerOName { get; private set; }

        public NewGameDialog()
        {
            InitializeComponent();

            cbPlayerXType.SelectedValuePath = "Key";
            cbPlayerXType.DisplayMemberPath = "Value";
            cbPlayerXType.Items.Add(new KeyValuePair<Type, string>(null, "Human"));
            cbPlayerXType.SelectedIndex = 0;

            cbPlayerOType.SelectedValuePath = "Key";
            cbPlayerOType.DisplayMemberPath = "Value";
            cbPlayerOType.Items.Add(new KeyValuePair<Type, string>(null, "Human"));
            cbPlayerOType.SelectedIndex = 0;
        }

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerXName = tbPlayerXName.Text;
            PlayerOName = tbPlayerOName.Text;
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
