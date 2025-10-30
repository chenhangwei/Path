namespace Path.Services
{
    /// <summary>
    /// 对话框服务接口
    /// </summary>
    public interface IDialogService
    {
   /// <summary>
    /// 显示打开文件对话框
     /// </summary>
        string? ShowOpenFileDialog(string filter, string defaultExt = "xml");

        /// <summary>
     /// 显示保存文件对话框
   /// </summary>
  string? ShowSaveFileDialog(string filter, string defaultExt = "xml", string? defaultFileName = null);

        /// <summary>
        /// 显示信息消息框
      /// </summary>
  void ShowMessage(string message, string title = "信息");

   /// <summary>
        /// 显示错误消息框
        /// </summary>
        void ShowError(string message, string title = "错误");

     /// <summary>
     /// 显示确认对话框
 /// </summary>
     bool ShowConfirmation(string message, string title = "确认");
    }
}
