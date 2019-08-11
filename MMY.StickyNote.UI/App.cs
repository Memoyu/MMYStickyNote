using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace MMY.StickyNote.UI
{
    public static class Window
    {

        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(IntPtr hWnd);//窗口显示置顶

        public static List<Theme> Themes;//主题数据集合
        public static List<Style> Styles;//风格数据集合
        public static int stickyNoteViewId = 1;//便签的Id
        public static Queue<int> EmptySlots;//用于排序窗口Id
        public static int ViewId = 1;


        static  System.Windows.Forms.NotifyIcon _notifyIcon;//托盘
        static List<System.Windows.Forms.MenuItem> menuItems = new List<System.Windows.Forms.MenuItem>();//托盘右键菜单选项

        private static WeatherInfo weatherInfo;
        private static Timer readDataTimer = null;

        [STAThread]
        static void Main()

        {
            // 定义Application对象作为整个应用程序入口  
            Application app = new Application();

            weatherInfo = new WeatherInfo();//开始获取天气信息
            weatherInfo.GetLocationEvent(SetWeatherControl);


            InitialTary();
            EmptySlots = new Queue<int>();
            Themes = new List<Theme>();
            Styles = new List<Style>();
            LoadThemes();//加载默认主题数据
            LoadStyles();//加载默认风格数据
            LoadAllStickyNoteView();//加载创建的标签，从注册表中读取，空则新建

         
            //NoteManager noteManager = new NoteManager();
            //AboutView aboutView = new AboutView();
            app.Run();
            if (REGISTRY.FirstRun) REGISTRY.StartWithWindows = true;//*******************************注册表启动项***************************

        }
        //获取天气信息
        private static void SetWeatherControl(string weatherInfo)
        {
            //赋值便签页面的天气控件
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                AssignmentWeatherControl(weatherInfo);
            }));
            if (readDataTimer == null)
            {
                readDataTimer = new Timer();
                readDataTimer.Interval = 7200000;//两个小时定时器调用一次
                readDataTimer.Elapsed += TimeCycle;
                readDataTimer.Enabled = true;
            }

        }
        /// <summary>
        /// 定时器调事件
        /// </summary>
        public static void TimeCycle(object sender, EventArgs e)
        {
            weatherInfo = new WeatherInfo();
            weatherInfo.GetLocationEvent(SetWeatherControl);
            //
            //.WriteLine("定时调用");
        }
        /// <summary>
        /// 加载所有的便签页面
        /// </summary>
        private static void LoadAllStickyNoteView()
        {
            List<ViewSettingData> viewSettingDataList = new List<ViewSettingData>();
            foreach (string id in REGISTRY.OPENED_NOTES)
            {
                object val = REGISTRY.GetData(id);
                if (val != null) viewSettingDataList.Add((ViewSettingData)val);//将数据加入集合中
            }
            ViewId = 1;
            foreach (ViewSettingData dat in viewSettingDataList) AddNewStickyNoteView(dat);
            if (ViewId == 1)
            {
                AddNewStickyNoteView();
            }
            REGISTRY.DeleteAll();
            SaveAllNotes();

            
        }
        public static void SaveAllNotes()
        {
            foreach (System.Windows.Window view in Application.Current.Windows)
            {
                if (view.GetType() == typeof(StickyNoteView))
                {
                    ((StickyNoteView)view).Save();
                }
            }
        }

        /// <summary>
        /// 加载主题
        /// </summary>
        public static void LoadThemes()
        {
            //将文件中主题的信息读取出来
            List<List<string>> data =Newtonsoft.Json.JsonConvert.DeserializeObject<List<List<string>>>(MMY.StickyNote.UI.Properties.Resources.Themes);

            foreach (List<string> ThData in data)
            {
                Theme th = new Theme();
                th.Name = ThData[0];
                //赋值主题对象
                th.TextColor = ThData[1];
                th.BackColor = ThData[2];
                th.TopBarColor = ThData[3];

                Themes.Add(th);
            }
        }
        /// <summary>
        /// 加载风格
        /// </summary>
        public static void LoadStyles()
        {
            //将文件中主题的信息读取出来
            List<List<string>> data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<List<string>>>(MMY.StickyNote.UI.Properties.Resources.Styles);

            foreach (List<string> stData in data)
            {

                Style st = new Style();
                st.Name = stData[0];
                //赋值风格数据时
                st.FontFamily = stData[1];
                st.FontSize = stData[2];
                //st.FStyle = (System.Drawing.FontStyle)int.Parse(stData[3]);

                Styles.Add(st);
            }
           // Console.WriteLine(Styles.Count);
        }

        /// <summary>
        /// 创建系统托盘
        /// </summary>
        private static void InitialTary()
        {
            //设置托盘的各个属性
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.BalloonTipText = "StickyNote";//托盘气泡显示内容
            _notifyIcon.Text = "StickyNote";
            _notifyIcon.Visible = true;//托盘按钮是否可见
            _notifyIcon.Icon = new System.Drawing.Icon(@"StickyNoteIcon.ico");//托盘中显示的图标
            _notifyIcon.ShowBalloonTip(2000);//托盘气泡显示时间
            _notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
            _notifyIcon.MouseClick += NotifyIcon_MouseClick;

            //新建便签
            CreateMenuItem("新建便签", NewNote_Click);
            ////显示全部便签
            CreateMenuItem("显示全部", ShowAllNote_Click);
            ////隐藏全部便签
            CreateMenuItem("隐藏全部", HideAllNote_Click);
            ////便签管理
            CreateMenuItem("便签管理", NoteManager_Click);
            ////刷新天气
            CreateMenuItem("刷新天气", RefeshWeather_Click);
            ////关于
            CreateMenuItem("关于", About_Click);
            ////退出菜单项
            CreateMenuItem("退出", Exit_Click);

            //关联托盘控件
            _notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(menuItems.ToArray());
        }

        /// <summary>
        /// 右键创建菜单栏
        /// </summary>
        /// <param name="title">选项名称</param>
        /// <param name="clickEvent">点击事件</param>
        /// <returns>创建的Item项</returns>
        private static void CreateMenuItem(string title, EventHandler clickEvent)
        {
            //退出菜单项
            System.Windows.Forms.MenuItem item = new System.Windows.Forms.MenuItem(title);
            item.Click += clickEvent;

            menuItems.Add(item);
        }

        #region 右键菜单栏选项事件
        private static void Exit_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();//关闭程序
        }

        private static void About_Click(object sender, EventArgs e)
        {
            AboutView aboutView = AboutView.GetInstance();
            aboutView.Focus();
            aboutView.Show();
        }

        private static void NoteManager_Click(object sender, EventArgs e)
        {
            NoteManager noteManager =NoteManager.GetInstance();
            noteManager.Focus();
            noteManager.Show();
        }

        private static void HideAllNote_Click(object sender, EventArgs e)
        {
            foreach (System.Windows.Window window in Application.Current.Windows)//遍历全部已创建的标签
            {
                if (window.GetType() != typeof(StickyNoteView)) continue;//判断标签类型，是否一致，否则结束当前循环
                window.Hide();
            }
        }
        /// <summary>
        /// 显示全部标签操作
        /// </summary>
        private static void ShowAllNote_Click(object sender, EventArgs e)
        {
            foreach (System.Windows.Window window in Application.Current.Windows)//遍历全部已创建的标签
            {
                if (window.GetType() != typeof(StickyNoteView)) continue;//判断标签类型，是否一致，否则结束当前循环
                window.Show();
            }
        }
        /// <summary>
        /// 新建便签
        /// </summary>
        public static void NewNote_Click(object sender, EventArgs e)
        {
            AddNewStickyNoteView();
        }
        /// <summary>
        /// 刷新显示天气
        /// </summary>
        private static void RefeshWeather_Click(object sender, EventArgs e)
        {
            TimeCycle(null, null);
        }
        #endregion


        public static void AddNewStickyNoteView(ViewSettingData viewSettingData = null )
        {
            int id = ViewId;
            if (EmptySlots.Count == 0) ++ViewId;
            else id = EmptySlots.Dequeue();
            new StickyNoteView(id, viewSettingData);//新建个便签窗口
            //stickyNoteView.Show();
        }

        /// <summary>
        /// 鼠标单击事件
        /// </summary>
        private static void NotifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Console.WriteLine("左按下");
                foreach (System.Windows.Window window in Application.Current.Windows)//遍历全部已创建的标签
                {
                    if (window.GetType() != typeof(StickyNoteView)) continue;//判断标签类型，是否一致，否则结束当前循环
                    HwndSource source = (HwndSource)PresentationSource.FromVisual(window);
                    IntPtr handle = source.Handle;
                    if (window.IsVisible) SetForegroundWindow(handle);//便签窗口置顶显示
                }
            }
        }
        /// <summary>
        /// 鼠标双击事件
        /// </summary>
        private static void NotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            NoteManager noteManager = NoteManager.GetInstance();
            noteManager.Focus();
            noteManager.Show();
        }
        /// <summary>
        /// 赋值所有便签窗口天气信息
        /// </summary>
        /// <param name="weatherInfo">天气信息字符串</param>
        private static void AssignmentWeatherControl(string weatherInfo)
        {
            foreach (System.Windows.Window window in Application.Current.Windows)//遍历全部已创建的标签
            {
                if (window.GetType() != typeof(StickyNoteView)) continue;//判断标签类型，是否一致，否则结束当前循环
                StickyNoteView noteView = window as StickyNoteView;
                noteView.Dispatcher.BeginInvoke(new Action(() =>
                {
                    noteView.contentWeather = weatherInfo;
                }));

            }
        }
    }
}
