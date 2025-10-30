using Microsoft.Win32;
using System.Windows;

namespace Path.Services
{
    /// <summary>
    /// WPF 对话框服务实现
    /// </summary>
    public class WpfDialogService : IDialogService
    {
     public string? ShowOpenFileDialog(string filter, string defaultExt = "xml")
        {
            var dialog = new OpenFileDialog
    {
      Filter = filter,
     DefaultExt = defaultExt
       };

    return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

public string? ShowSaveFileDialog(string filter, string defaultExt = "xml", string? defaultFileName = null)
 {
            var dialog = new SaveFileDialog
         {
                Filter = filter,
         DefaultExt = defaultExt,
          FileName = defaultFileName ?? string.Empty
   };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

        public void ShowMessage(string message, string title = "信息")
        {
     MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

 public void ShowError(string message, string title = "错误")
{
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public bool ShowConfirmation(string message, string title = "确认")
   {
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}
