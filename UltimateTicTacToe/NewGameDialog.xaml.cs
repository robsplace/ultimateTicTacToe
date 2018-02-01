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
        public Type PlayerXType { get; private set; }
        public Type PlayerOType { get; private set; }

        private List<Type> _pluginTypes = new List<Type>();

        public NewGameDialog(List<Type> pluginTypes, string playerXName, string playerOName, Type playerXType, Type playerOType)
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

            tbPlayerXName.Text = playerXName;
            tbPlayerOName.Text = playerOName;
        }

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerXName = tbPlayerXName.Text;
            PlayerOName = tbPlayerOName.Text;

            PlayerXType = cbPlayerXType.SelectedValue as Type;
            PlayerOType = cbPlayerOType.SelectedValue as Type;

            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
