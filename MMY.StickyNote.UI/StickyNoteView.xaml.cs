using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MMY.StickyNote.UI
{
    /// <summary>
    /// StickyNoteView.xaml 的交互逻辑
    /// </summary>

    public partial class StickyNoteView : System.Windows.Window
    {
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();//标题栏隐藏定时器
        public double navOpacityIncrease = 0.3;//默认标题栏透明度比背景透明度高0.3
        private WeatherInfo weatherInfo;
        public Theme CustomTheme { get; set; }
        public Style CustomStyle { get; set; }
        public int ViewId;
        public string ViewTitle;

        private static string weather = null;
        private static DispatcherTimer readDataTimer = null;


        #region 依赖属性
        //声明依赖属性    
        public static DependencyProperty viewBackgroundProperty;//窗口背景颜色
        public static DependencyProperty navBarBackgroundProperty;//标题栏背景颜色
        public static DependencyProperty navBarOpacityProperty;//标题栏背景透明度
        public static DependencyProperty backgroundOpacityProperty;//窗口背景透明度
        public static DependencyProperty contentTxtProperty;//文本内容
        public static DependencyProperty contentFontFamilyProperty;//文本内容字体
        public static DependencyProperty contentFontSizeProperty;//文本内容字体大小
        public static DependencyProperty contentFontColorProperty;//文本内容字体颜色
        public static DependencyProperty contentWeatherProperty;//天气文本
        //依赖属性封装
        public string viewBackground
        {
            get { return (string)GetValue(viewBackgroundProperty); }
            set { SetValue(viewBackgroundProperty , value); }
        }
        public string navBarBackground
        {
            get { return (string)GetValue(navBarBackgroundProperty); }
            set { SetValue(navBarBackgroundProperty, value); }
        }
        public string navBarOpacity
        {
            get { return (string)GetValue(navBarOpacityProperty); }
            set { SetValue(navBarOpacityProperty, value); }
        }
        public string backgroundOpacity
        {
            get { return (string)GetValue(backgroundOpacityProperty); }
            set { SetValue(backgroundOpacityProperty, value); }
        }
        public string contentTxt
        {
            get { return (string)GetValue(contentTxtProperty); }
            set { SetValue(contentTxtProperty, value); }
        }
        public string contentFontFamily
        {
            get { return (string)GetValue(contentFontFamilyProperty); }
            set { SetValue(contentFontFamilyProperty, value); }
        }
        public string contentFontSize
        {
            get { return (string)GetValue(contentFontSizeProperty); }
            set { SetValue(contentFontSizeProperty, value); }
        }
        public string contentFontColor
        {
            get { return (string)GetValue(contentFontColorProperty); }
            set { SetValue(contentFontColorProperty, value); }
        }
        public string contentWeather
        {
            get { return (string)GetValue(contentWeatherProperty); }
            set { SetValue(contentWeatherProperty, value); }
        }


        //静态构造函数 , 注册依赖属性，仅在第一次创建实例时调用，且只调用一次
        static StickyNoteView()
        {
            viewBackgroundProperty = DependencyProperty.Register("viewBackground" , typeof(string) , typeof(StickyNoteView) ,new FrameworkPropertyMetadata("#FFAEB9"));
            navBarBackgroundProperty = DependencyProperty.Register("navBarBackground", typeof(string), typeof(StickyNoteView), new FrameworkPropertyMetadata("#FFAEB9"));
            backgroundOpacityProperty = DependencyProperty.Register("backgroundOpacity", typeof(string), typeof(StickyNoteView), new FrameworkPropertyMetadata("0.9"));
            navBarOpacityProperty = DependencyProperty.Register("navBarOpacity", typeof(string), typeof(StickyNoteView), new FrameworkPropertyMetadata("1"));
            contentTxtProperty = DependencyProperty.Register("contentTxt", typeof(string), typeof(StickyNoteView), new FrameworkPropertyMetadata("在这里写入你的便签内容！"));
            contentFontFamilyProperty = DependencyProperty.Register("contentFontFamily", typeof(string), typeof(StickyNoteView), new FrameworkPropertyMetadata("微软雅黑"));
            contentFontSizeProperty = DependencyProperty.Register("contentFontSize", typeof(string), typeof(StickyNoteView), new FrameworkPropertyMetadata("16"));
            contentFontColorProperty = DependencyProperty.Register("contentFontColor", typeof(string), typeof(StickyNoteView), new FrameworkPropertyMetadata("white"));
            contentWeatherProperty = DependencyProperty.Register("contentWeather", typeof(string), typeof(StickyNoteView), new FrameworkPropertyMetadata("我是天气"));
        }
        #endregion

        public int stickyNoteViewId { get; set; }//便签Id
        public StickyNoteView()
        {
            InitializeComponent();

            this.MouseEnter += StickyNoteView_MouseEnter;//鼠标进入窗口
            this.MouseLeave += StickyNoteView_MouseLeave;//鼠标离开窗口

            this.LocationChanged += StickyNoteView_PropertyChanged;//窗口位置发生调用数据保存事件
            this.SizeChanged += StickyNoteView_SizeChanged;//窗口大小改变事件
            this.AddStickyNote.Click += AddStickyNote_Click;//添加便签按钮事件
            this.editTitle.PreviewKeyDown += EditTitle_PreviewKeyDown;
            this.EditCompleted.Click += EditCompleted_Click;//点击完成编辑按钮
            this.ContentTextBox.TextChanged += StickyNoteView_PropertyChanged;//文本改变调用保存事件
            this.HideStickyNote.Click += HideStickyNote_Click;//隐藏便签
            this.Setting.Click += Setting_Click;
            this.ShowInTaskbar = false;//不再任务栏显示图标
            this.WindowStartupLocation = WindowStartupLocation.Manual;
        }

    

        public StickyNoteView(int viewId, ViewSettingData viewSettingData = null) :this()
        {
            ViewId = viewId;//赋值全局Id
            CustomTheme = new Theme();//创建主题
            CustomStyle = new Style();//创建样式
            weatherInfo = new WeatherInfo();
            weatherInfo.GetLocationEvent(GetAddreaaCode);
            LoadData(viewSettingData);
            StickyNoteView_MouseLeave(null , null);//调用鼠标离开事件，实现软件开启，标题栏自动隐藏
            if (readDataTimer == null)
            {
                readDataTimer = new DispatcherTimer();
                readDataTimer.Tick += new EventHandler(timeCycle);
                readDataTimer.Interval = new TimeSpan(0, 2, 0, 0);
                readDataTimer.Start();
            }
        }
        //获取天气信息
        private void GetAddreaaCode(AddressComponent address)
        {
            if (address == null) return;
            //根据获取到的省市获取本地相应的天气编码
            string cityCode = weatherInfo.GetCityCode(address);
            //获取天气信息
            if (weather == null)
            {
               weather = weatherInfo.RequestWeatherWebAnalysisData(cityCode);
            }
            //Console.WriteLine(weather);
            //这里开始赋值窗口天气
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.contentWeather = weather;
            }));
        }
        /// <summary>
        /// 定时器调事件
        /// </summary>
        public void timeCycle(object sender, EventArgs e)
        {
            weatherInfo = new WeatherInfo();
            weatherInfo.GetLocationEvent(GetAddreaaCode);
            weather = null;
        }
        /// <summary>
        /// 加载便签数据
        /// </summary>
        /// <param name="viewSettingData">便签配置数据</param>
        private void LoadData(ViewSettingData viewSettingData = null)
        {
            if (string.IsNullOrEmpty(this.StickyNoteTitle.Content.ToString()))
            {
                this.StickyNoteTitle.Content = string.Format("Note {0:##}", ViewId);
                //Console.WriteLine(ViewId);
            }
            if (Window.Themes.Count > 1)//如果获取到主题默认数据，随机赋值主题
            {
                Random rand = new Random();
                CurrentTheme = rand.Next(Window.Themes.Count - 1) + 1;
            }
            if (Window.Styles.Count > 1)
            { CurrentStyle = 1;
            }
            if (viewSettingData == null)//判断是否有传入窗口显示 相关数据，空则直接显示，否则加载数据
            {
                this.Show();
                return;
            }

            //this.SuspendLayout();
            //如果有数据，即传入的viewSettingData不为null，则进行数据赋值到对应的便签窗口
            this.Left = viewSettingData.WinLeft;
            this.Top = viewSettingData.WinTop;
            this.Width = viewSettingData.WinWidth;
            this.Height = viewSettingData.WinHeigth;

            this.StickyNoteTitle.Content = viewSettingData.Title;
            this.contentTxt = viewSettingData.Data;
            //Console.WriteLine(this.contentTxt);
            Console.WriteLine(this.Opacity);
            this.backgroundOpacity = viewSettingData.Opacity.ToString();

            double tempValue = viewSettingData.Opacity + navOpacityIncrease;
            if (tempValue > 1){this.navBarOpacity = "1";}
            this.navBarOpacity = tempValue.ToString();

            this.ViewTitle = viewSettingData.Title;
            if (viewSettingData.CustomTheme != null) { this.CustomTheme = viewSettingData.CustomTheme; }
            this.CurrentTheme = viewSettingData.Theme;
            if (viewSettingData.CustomStyle != null) { this.CustomStyle = viewSettingData.CustomStyle; }
            this.CurrentStyle = viewSettingData.Style;
            if (viewSettingData.CreationTime > new DateTime(2019, 5, 23)) { this.CreationTime = viewSettingData.CreationTime; }
            //this.ResumeLayout(true);

            this.Show();
            if (viewSettingData.Hidden) this.Hide();
        }

        

        #region 显示风格相关
        /// <summary>
        /// 时间属性
        /// </summary>
        public DateTime CreationTime { get; set; }
        private int _theme = 0;
        private int _style = 0;
        public int CurrentTheme//当前主题
        {
            get { return _theme; }
            set
            {
                if (value >= Window.Themes.Count)
                    value = Window.Themes.Count - 1;
                if (value < 0) value = 0;

                if (_theme != value)
                {
                    _theme = value;
                    Save();
                }
                ReloadTheme();
            }
        }
        public int CurrentStyle
        {
            get { return _style; }
            set
            {
                if (value >= Window.Styles.Count)
                    value = Window.Styles.Count - 1;
                if (value < 0) value = 0;

                if (_style != value)
                {
                    _style = value;
                    Save();
                }
                ReloadStyle();
            }
        }
        /// <summary>
        /// 保存数据到注册表
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            try
            {
                REGISTRY.SetData(ViewId.ToString(), new ViewSettingData(this));
                return true;
            }
            catch { return false; }
        }
        /// <summary>
        /// 重新载入主题
        /// </summary>
        public void ReloadTheme()
        {
            Theme th = Window.Themes[_theme];
            if (th.Name == "用户") th = CustomTheme;
           

            this.viewBackground =th.BackColor;
            this.contentFontColor= th.TextColor;
            this.navBarBackground = th.TopBarColor;
        }
        /// <summary>
        /// 重新载入风格
        /// </summary>
        public void ReloadStyle()
        {
            Style st = Window.Styles[_style];
            if (st.Name == "用户") st = CustomStyle;
            this.contentFontFamily = st.FontFamily;
            this.contentFontSize = st.FontSize;
        }
        #endregion


        #region 事件相关
        /// <summary>
        /// 窗口位置发生改变
        /// </summary>
        private void StickyNoteView_PropertyChanged(object sender, EventArgs e)
        {
            Save();//进行数据保存
            //Console.WriteLine("改变*");
        }
        /// <summary>
        /// 鼠标离开窗口事件
        /// </summary>
        private void StickyNoteView_MouseLeave(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("鼠标离开");
            dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 3);
            dispatcherTimer.Start();
        }
        /// <summary>
        /// 鼠标进入窗口事件
        /// </summary>
        private void StickyNoteView_MouseEnter(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("鼠标进入");
            this.NavBar.Visibility = Visibility.Visible;//显示标题栏
            this.WeatherShow.Visibility = Visibility.Hidden;//隐藏天气
            dispatcherTimer.Stop();
        }
        /// <summary>
        /// 定时器调用
        /// </summary>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.NavBar.Visibility = Visibility.Hidden;//隐藏标题栏
            this.WeatherShow.Visibility = Visibility.Visible;//显示天气
            dispatcherTimer.Stop();
        }
        /// <summary>
        /// 隐藏标签事件
        /// </summary>
        private void HideStickyNote_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();

        }
        /// <summary>
        /// 设置按钮事件
        /// </summary>
        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            SettingView settingView = SettingView.GetInstance(this);
            settingView.Focus();
            settingView.Show();
        }
        /// <summary>
        /// 添加新标签事件
        /// </summary>
        private void AddStickyNote_Click(object sender, RoutedEventArgs e)
        {
            Window.NewNote_Click(null, EventArgs.Empty);//调用App中的添加新便签事件
        }
        /// <summary>
        /// 标题编辑栏键盘按下事件
        /// </summary>
        private void EditTitle_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EditCompleted_Click(null, new RoutedEventArgs());
            }
        }
        /// <summary>
        /// 完成编辑事件
        /// </summary>
        private void EditCompleted_Click(object sender, RoutedEventArgs e)
        {
            StickyNoteTitle.Content = editTitle.Text;//赋值标题栏
            editTitle.Visibility = Visibility.Hidden;//隐藏输入框和编辑完成按钮
            EditCompleted.Visibility = Visibility.Hidden;
            Save();//保存数据
        }
        /// <summary>
        /// 窗口大小改变事件
        /// </summary>
        private void StickyNoteView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.StickyNoteTitle.MaxWidth = this.Width - (this.AddStickyNote.Width + this.HideStickyNote.Width + this.Setting.Width + this.EditCompleted.Width + 15);//动态调整输入框宽度
            this.StickyNoteTitle.Width = this.StickyNoteTitle.MaxWidth;
            this.editTitle.Width = this.StickyNoteTitle.MaxWidth;
            this.ContentTextBox.Width = this.Width;
            this.ContentTextBox.Height = this.Height - NavBar.Height;
            this.WeatherShow.Width = this.Width;
            Save();//保存数据
        }
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        private void StickyNote_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //注册UI线程事件，异步处理
                Dispatcher.BeginInvoke(new Action(() =>
                {

                    try
                    {
                        this.DragMove();//实现

                    }
                    catch
                    {
                    }
                }));
            }
        }
        /// <summary>
        /// 标题栏双击事件
        /// </summary>
        private void StickyNoteTitle_MouseDoubleClick(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.EditCompleted.Visibility = Visibility.Visible;//显示控件
            this.editTitle.Text = this.StickyNoteTitle.Content.ToString();
            this.editTitle.Visibility = Visibility.Visible;
        }

    } 
    #endregion
}
