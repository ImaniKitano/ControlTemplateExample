using Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace ImageViewer
{
    public class FileSelectorDialogViewModel : BindableBase
    {
        #region コンストラクタ
        public FileSelectorDialogViewModel()
        {
            // ファイルソート方法の選択肢を設定する。
            _sortKeyChoices = new List<SortKeyChoice> {
                    new SortKeyChoice("Sort by name",
                            () =>
                            {
                                _fileList.Sort((a, b) => String.Compare(a.Name, b.Name));
                                FileList = new ReadOnlyCollection<FileInfo>(_fileList);
                            }
                        ),
                    new SortKeyChoice("Sort by date",
                        () =>
                            {
                                _fileList.Sort((a, b) => DateTime.Compare(a.LastWriteTime, b.LastWriteTime));
                                FileList = new ReadOnlyCollection<FileInfo>(_fileList);
                            }
                           )
                };

            // ファイルソート方法の初期値を設定する。
            _selectedSortKey = SortKeyChoices[0];

            // OKボタンの動作を設定する。
            OkCommand = new DelegateCommand(() => CloseWindow?.Invoke((SelectedIndex >= 0)? FileList[SelectedIndex] : null));

            // Cancelボタンの動作を設定する。
            CancelCommand = new DelegateCommand(() => CloseWindow?.Invoke(null));

            // ドライブとフォルダのリストにロジカルドライブの一覧を設定する。
            foreach (var drive in Directory.GetLogicalDrives().ToList<string>())
            {
                var driveInfo = new DriveInfo(drive);
                driveInfo.Selected += SelectedFolderChanged;
                Drives.Add(driveInfo);
            }
        }
        #endregion

        #region フォルダツリー関係
        //! 選択されているフォルダが変わったときにファイル一覧をセットする。
        private void SelectedFolderChanged(FolderInfo selectedFolder)
        {
            // 選択されたフォルダの名前を表示用プロパティにコピーする。
            SelectedFolder = selectedFolder.FullPath;

            // 選択されたフォルダにある画像ファイルの一覧を取得する。
            var picFiles = Directory.GetFiles(SelectedFolder, "*.jpg").ToList<string>();
            picFiles.AddRange(Directory.GetFiles(SelectedFolder, "*.png").ToList<string>());
            picFiles.AddRange(Directory.GetFiles(SelectedFolder, "*.bmp").ToList<string>());

            // 見つかった画像ファイルの一覧を作る。。
            _fileList.Clear();
            foreach (var fn in picFiles)
            {
                _fileList.Add(new FileInfo(Path.Combine(SelectedFolder, fn)));
            }

            // 指定された方法でソートする。表示用プロパティFileListは、この中で更新される。
            SelectedSortKey.Execute();

        }

        public List<DriveInfo> Drives { get; } = new();

        //! 選択中のフォルダに含まれる画像ファイルの一覧。
        private List<FileInfo> _fileList = new List<FileInfo>();

        //! 選択中のフォルダに含まれる画像ファイルの一覧。Xamからの参照用。
        public ReadOnlyCollection<FileInfo> FileList { get => __fileList; set => SetProperty(ref __fileList, value); }
        private ReadOnlyCollection<FileInfo> __fileList = new ReadOnlyCollection<FileInfo>(new List<FileInfo>());
        //!< FileListの実体。

        //! 選択中のフォルダ名を保持するプロパティ。
        public string SelectedFolder { get => _selectedFolder; set => SetProperty(ref _selectedFolder, value); }
        private string _selectedFolder = string.Empty;  //!< SelectedFoldeプロパティrの実体。
        #endregion

        #region //! @name ファイルリスト関連
        //! ファイルソート方法を保持するクラス
        public class SortKeyChoice
        {
            //! コンストラクタ
            /*! @param[in]  sortMethodName  ソート方法の名前
                @param[in]  execute         ソートを実行するためのActionデリゲート
            */
            public SortKeyChoice(string sortMethodName, Action execute)
            {
                SortMethodName = sortMethodName;
                Execute = execute;
            }

            //! コンボボックスの選択肢として表示する文字列を返す。
            public override string ToString() => SortMethodName;

            //! コンストラクタに渡されたソート方法の名前を保持する。
            public string SortMethodName { get; }

            //! コンストラクタに渡されたソート実行方法のActionデリゲートを保持する。
            public Action Execute { get; }
        }

        //! 画像ファイル一覧のソート方法のリスト。
        public List<SortKeyChoice> SortKeyChoices { get => _sortKeyChoices; set => SetProperty(ref _sortKeyChoices, value); }
        private List<SortKeyChoice> _sortKeyChoices;

        //! 選択されているソート方法
        public SortKeyChoice SelectedSortKey
        {
            get => _selectedSortKey;
            set
            {
                if (SetProperty(ref _selectedSortKey, value))
                {
                    value.Execute();
                }
            }
        }
        private SortKeyChoice _selectedSortKey; //!< SelectedSortKeyの実体。

        //! 選択されているアイテム（ファイル）の番号
        public int SelectedIndex { get => _selectedIndex; set => SetProperty(ref _selectedIndex, value); }
        private int _selectedIndex = -1;
        #endregion

        #region コマンドとクローズ処理
        //! OKボタンのコマンド。
        public DelegateCommand OkCommand { get; init; }

        //! Cancelボタンのコマンド。
        public DelegateCommand CancelCommand { get; init; }

        //! ウィンドウクローズをビューに依頼する。
        public Action<FileInfo?>? CloseWindow = null;
        #endregion
    }

    //! ドライブ情報保持クラス
    public class DriveInfo : FolderInfo
    {
        public DriveInfo(string name) : base(name) {
            Name = name;
        }
    }

    //! ファイル情報保持クラス
    public class FolderInfo : BindableBase
    {
        #region コンストラクタ
        //! コンストラクタ。
        public FolderInfo(string path)
        {
            // フォルダのフルパスを覚える。
            FullPath = path;

            // フォルダ選択コントロール用の表示名を作る
            _name = Path.GetFileName(FullPath);

            // アイテムを展開させない。
            IsExpanded = false;

            // サブフォルダがあれば、サブフォルダのリストを得るためにかかる時間を節約するために、
            // 展開用のボタンを表示させるための適当な子アイテムを付けておく。
            if (Directory.GetDirectories(FullPath, "*.*", SearchOption.TopDirectoryOnly).Length > 0)
            {
                SubFolders.Add(FoldedItem);
            }
        }

        //! ダミーを作るためのコンストラクタ。
        private FolderInfo()
        {
            FullPath = "*";
            _name = "*";
            IsExpanded = false;
        }
        #endregion

        #region フォルダ情報追加・削除
        private void LoadSubFolders() {
            // 展開ボタン表示用の子アイテムを削除する。
            SubFolders.Clear();

            // サブフォルダを子アイテムとして追加する。
            foreach (var subfolder in Directory.GetDirectories(FullPath, "*.*", SearchOption.TopDirectoryOnly))
            {
                var info = new FileInfo(subfolder);
                if ((info.Attributes & System.IO.FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    try
                    {
                        var subFolderInfo = new FolderInfo(subfolder);
                        subFolderInfo.Selected += ChildFolderSelectedHandler;
                        SubFolders.Add(subFolderInfo);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // 中身を調べられなかったフォルダを登録しない。
                    }
                }
            }
        }

        private void PargeSubFolders() { 
            foreach(var subFolderInfo in SubFolders)
            {
                subFolderInfo.Selected -= ChildFolderSelectedHandler;
            }
            SubFolders.Clear();

            SubFolders.Add(FoldedItem);
        }
        #endregion

        #region 公開用プロパティ
        //! 選択されたフォルダのフルパス。
        public string FullPath { get; init; }

        //! 選択されたフォルダのファイル名部分。
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        private string _name;

        //! サブフォルダの情報を保持しているか否か。
        public bool IsExpanded { 
            get => _isExpanded;
            set
            {
                if(SetProperty(ref _isExpanded, value))
                {
                    if (_isExpanded)
                    {
                        LoadSubFolders();
                    }
                    else
                    {
                        PargeSubFolders();
                    }
                }
            }
        }
        private bool _isExpanded;

        //! 選択されているか否か。
        public bool IsSelected
        {
            get => _isSelected;
            set {
                if(SetProperty(ref _isSelected, value))
                {
                    if (_isSelected)
                    {
                        Selected?.Invoke(this);
                    }
                }
            }
        }
        private bool _isSelected;

        //! サブフォルダから選択された旨を知らせてもらうためのハンドラ。
        private void ChildFolderSelectedHandler(FolderInfo subfolder) => Selected?.Invoke(subfolder);

        //! サブフォルダの情報。なければ空リスト。閉じているときにはダミーの情報がセットされる。
        public ObservableCollection<FolderInfo> SubFolders { get => _subFolders; }
        private readonly ObservableCollection<FolderInfo> _subFolders = new();

        //! このフォルダが選択されたときに呼び出すイベント。
        public event Action<FolderInfo>? Selected;
        #endregion

        #region 内部データ
        //! ダミーのフォルダ情報。
        private static FolderInfo FoldedItem = new FolderInfo();
        #endregion
    }
}
