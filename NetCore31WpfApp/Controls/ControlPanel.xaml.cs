using MARGO.Common;
using System.Windows;
using System.Windows.Controls;

namespace MARGO.Controls
{
    /// <summary>
    /// Interaction logic for ControlPanel.xaml
    /// </summary>
    public partial class ControlPanel : UserControl
    {
        public ControlPanel()
        {
            InitializeComponent();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb
                && rb.Tag is Step lastStepInGroup
                && this.DataContext is IHaveScript viewModel)
                viewModel.LastStep = lastStepInGroup;
        }
    }
}