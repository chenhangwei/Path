using Path.ViewModels;
using System.Windows;

namespace Path
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainViewModel();
            DataContext = _vm;
        }

        // 打开文件的事件处理程序
        private void OnOpen(object sender, RoutedEventArgs e)
        {
            // Delegate to ViewModel ImportXml command
            try
            {
                if (_vm != null && _vm.ImportXmlCommand != null && _vm.ImportXmlCommand.CanExecute(null))
                {
                    _vm.ImportXmlCommand.Execute(null);
                    // after import, if PathEditor control exists, load steps to draw
                    try { var pe = FindName("PathEditorControl") as Path.Views.PathEditor; if (pe != null) pe.LoadFromSteps(_vm.Steps); } catch { }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("打开文件失败: " + ex.Message);
            }
        }

        // 保存文件的事件处理程序
        private void OnSave(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_vm != null && _vm.ExportXmlCommand != null && _vm.ExportXmlCommand.CanExecute(null))
                {
                    _vm.ExportXmlCommand.Execute(null);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("保存文件失败: " + ex.Message);
            }
        }

        // 导出XML的事件处理程序
        private void OnExportXml(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_vm != null && _vm.ExportXmlCommand != null && _vm.ExportXmlCommand.CanExecute(null))
                {
                    _vm.ExportXmlCommand.Execute(null);
                    try { var pe2 = FindName("PathEditorControl") as Path.Views.PathEditor; if (pe2 != null) pe2.LoadFromSteps(_vm.Steps); } catch { }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("导出 XML 失败: " + ex.Message);
            }
        }

        // TreeView选中项更改的事件处理程序
        private void OnStepSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is Step step)
            {
                _vm.SelectedStep = step;
                try { var pe3 = FindName("PathEditorControl") as Path.Views.PathEditor; if (pe3 != null) pe3.LoadFromSteps(_vm.Steps); } catch { }
            }
        }
    }
}