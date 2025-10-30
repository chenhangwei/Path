namespace Path.Services
{
    /// <summary>
    /// �Ի������ӿ�
    /// </summary>
    public interface IDialogService
    {
   /// <summary>
    /// ��ʾ���ļ��Ի���
     /// </summary>
        string? ShowOpenFileDialog(string filter, string defaultExt = "xml");

        /// <summary>
     /// ��ʾ�����ļ��Ի���
   /// </summary>
  string? ShowSaveFileDialog(string filter, string defaultExt = "xml", string? defaultFileName = null);

        /// <summary>
        /// ��ʾ��Ϣ��Ϣ��
      /// </summary>
  void ShowMessage(string message, string title = "��Ϣ");

   /// <summary>
        /// ��ʾ������Ϣ��
        /// </summary>
        void ShowError(string message, string title = "����");

     /// <summary>
     /// ��ʾȷ�϶Ի���
 /// </summary>
     bool ShowConfirmation(string message, string title = "ȷ��");
    }
}
