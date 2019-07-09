using System;
using System.Collections.Generic;
using System.Windows;

namespace MMY.StickyNote.UI
{
    /// <summary>
    /// NoteManager.xaml 的交互逻辑
    /// </summary>
    public partial class NoteManager : System.Windows.Window
    {
        private static NoteManager NoteManagerViewSingleton;//静态变量用来存储类的实例
        private static readonly object locker = new object();//定义一个标识确保线程同步

        private NoteManager()
        {
            InitializeComponent();

            this.NewNoteBtn.Click += NewNoteBtn_Click;//事件注册
            this.ShowSelectNoteBtn.Click += ShowSelectNoteBtn_Click;
            this.HiddeSelectNoteBtn.Click += HiddeSelectNoteBtn_Click;
            this.DeleteSelectNoteBtn.Click += DeleteSelectNoteBtn_Click;
            this.Closed += NoteManager_Closed;

            LoadAllNoteView();
        }

      

        public static NoteManager GetInstance()
        {
            if (NoteManagerViewSingleton == null)//双重锁定只需要一句判断就可以了
            {
                lock (locker)//线程锁
                {
                    if (NoteManagerViewSingleton == null)//判断类是否已经实例化
                    {
                        NoteManagerViewSingleton = new NoteManager();
                    }
                }
            }
            return NoteManagerViewSingleton;      
        }


        /// <summary>
        /// 加载所有的便签窗口到数据表中
        /// </summary>
        private void LoadAllNoteView()
        {
            List<DataModel> listViewData = new List<DataModel>();
            foreach (System.Windows.Window noteView in Application.Current.Windows)
            {
                if (noteView.GetType() != typeof(StickyNoteView)) continue;
                StickyNoteView view = noteView as StickyNoteView;
                //Data litm = new ListViewItem(view.Title);
                DataModel dataModel = new DataModel();
                //dataModel.id = view.ViewId;
                dataModel.title = view.StickyNoteTitle.Content.ToString();
                dataModel.StickyNoteView = view;

                if (view.IsVisible)
                {
                    dataModel.isVisible = "显示";
                }
                else
                {
                    dataModel.isVisible = "隐藏";
                }


                listViewData.Add(dataModel);

            }
            dataGrid.ItemsSource = listViewData;
            //foreach (var item in listViewData)
            //{
            //    Console.WriteLine(item.id);
            //    Console.WriteLine(item.title);
            //    Console.WriteLine(item.isVisible);
            //}
            //Console.WriteLine(listViewData.Count);
        }

        #region 事件相关
        /// <summary>
        /// 删除选中项事件
        /// </summary>
        private void DeleteSelectNoteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.dataGrid.SelectedItems.Count == 0) { MessageBox.Show("请选中某项再执行操作！", "提示"); return; };//判断是否有选中项，否则退出
            if (MessageBox.Show("你确定要删除选中的便签？", "警告", MessageBoxButton.YesNo) == MessageBoxResult.No) return;//弹出提示，如果确定则删除，否则退出
            var selectItems = this.dataGrid.SelectedItems;//获取选中的所有Item
            foreach (var item in selectItems)
            {
                DataModel dataModel = item as DataModel;//转换对象
                StickyNoteView noteView = dataModel.StickyNoteView;//获取窗口，进行操作
                REGISTRY.Delete(noteView.ViewId.ToString());
                Window.EmptySlots.Enqueue(noteView.ViewId);//去除集合中的项    
                noteView.Close();
            }
            LoadAllNoteView();//重新加载标签数据
        }
        /// <summary>
        /// 隐藏选中项事件
        /// </summary>
        private void HiddeSelectNoteBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcessShowOrHidden("hidden");//调用处理方法
        }
        /// <summary>
        /// 显示选中项事件
        /// </summary>
        private void ShowSelectNoteBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcessShowOrHidden("show");//调用处理方法
        }
        /// <summary>
        /// 新建项事件
        /// </summary>
        private void NewNoteBtn_Click(object sender, RoutedEventArgs e)
        {
            Window.AddNewStickyNoteView();
            LoadAllNoteView();
        }
        /// <summary>
        /// 处理显示或隐藏方法
        /// </summary>
        /// <param name="status">传入隐藏或者显示</param>
        private void ProcessShowOrHidden( string status)
        {
            if (this.dataGrid.SelectedItems.Count == 0) { MessageBox.Show("请选中某项再执行操作！", "提示"); return; }//判断是否有选中项，否则提示退出
            var selectItems = this.dataGrid.SelectedItems;//获取选中的所有Item
            foreach (var item in selectItems)
            {
                DataModel dataModel = item as DataModel; //转换对象
                StickyNoteView noteView = dataModel.StickyNoteView;//获取窗口，进行操作
                switch (status)
                {
                    case "show":
                        noteView.Show();
                        break;
                    case "hidden":
                        noteView.Hide();
                        break;
                    default:
                        break;
                }
                
                noteView.Save();
            }
            LoadAllNoteView();//重新加载标签数据
        }

        private void NoteManager_Closed(object sender, EventArgs e)
        {
            NoteManagerViewSingleton = null;
        }

        #endregion
    }

   


    /// <summary>
    /// DataGrid数据模型
    /// </summary>
    public class DataModel
    {
        //public int id { get; set; }
        public string title { get; set; }
        public string isVisible { get; set; }
        public StickyNoteView StickyNoteView { get; set; }
    }
}
