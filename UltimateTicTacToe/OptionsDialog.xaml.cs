using System.Windows;
using System.Windows.Forms;
using UltimateTicTacToe.Properties;

namespace UltimateTicTacToe
{
    /// <summary>
    /// Interaction logic for OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog : Window
    {
        public OptionsDialog()
        {
            InitializeComponent();
            tbFolder.Text = Settings.Default.PluginsFolder;
            tbMinAiTime.Text = Settings.Default.MinAiTime.ToString();
            tbMaxAiTime.Text = Settings.Default.MaxAiTime.ToString();
        }

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(tbMinAiTime.Text, out int minAiTime)
                || minAiTime < 10
                || minAiTime > 10000)
            {
                System.Windows.MessageBox.Show("Minimum AI Time must be a value between 10 and 10000");
                return;
            }
            
            if (!int.TryParse(tbMaxAiTime.Text, out int maxAiTime)
                || maxAiTime <= minAiTime
                || maxAiTime > 600000)
            {
                System.Windows.MessageBox.Show("Maximum AI Time must be a value between Minimum AI Time and 60000");
                return;
            }

            Settings.Default.PluginsFolder = tbFolder.Text;
            Settings.Default.MinAiTime = minAiTime;
            Settings.Default.MaxAiTime = maxAiTime;
            Settings.Default.Save();
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PickFolder(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select Plugins folder";
                fbd.SelectedPath = tbFolder.Text;
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tbFolder.Text = fbd.SelectedPath;
                }
            }
        }
    }
}
