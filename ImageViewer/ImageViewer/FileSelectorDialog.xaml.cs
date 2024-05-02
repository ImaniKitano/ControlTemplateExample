using System.IO;
using System.Windows;

namespace ImageViewer
{
    //! FileSelectorDialog.xaml の相互作用ロジック
    /*! TreeViewにドライブとフォルダの一覧を、GridViewにファイルの一覧を表示して、
        その中から一つを選ぶためのダイアログの動作を実現する。

    FileSelectorDialogは、Viewだけで構成されている。
    */
    public partial class FileSelectorDialog : Window
    {
        public FileSelectorDialog()
        {
            InitializeComponent();
        }

        //! DataContextにビューモデルが設定されたら、ウィンドウを閉じるためのデリゲートを設定する。
        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((FileSelectorDialogViewModel)DataContext).CloseWindow = (fileInfo) =>
            {
                SelectedFile = fileInfo;
                DialogResult = fileInfo != null;
                Close();
            };
        }

        //! 選択されたファイルの情報。選択されていなければnull。
        public FileInfo? SelectedFile { get; private set; } = null;
    }
}
