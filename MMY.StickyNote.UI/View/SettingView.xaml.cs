using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MMY.StickyNote.UI
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class SettingView : System.Windows.Window
    {
        public StickyNoteView stickyNoteView = null;//获取标签窗口
        private static SettingView SettingViewSingleton;//静态变量用来存储类的实例
        private static readonly object locker = new object();//定义一个标识确保线程同步

        private SettingView()
        {
            InitializeComponent();

            this.FontSizeShow.Click += FontSizeShow_Click;//注册按钮事件
            this.TopColorShow.Click += TopColorShow_Click;
            this.BackgroundColorShow.Click += BackgroundColorShow_Click;
            this.FontColorShow.Click += FontColorShow_Click;
            this.ConfirmButton.Click += ConfirmButton_Click;

            this.StyleComboBox.SelectionChanged += StyleComboBox_SelectionChanged;//combBox选项改变事件
            this.ThemeComboBox.SelectionChanged += ThemeComboBox_SelectionChanged;

            this.FontSizeValue.PreviewKeyDown += FontSizeValue_PreviewKeyDown;//输入字体大小与输入透明度值时回车事件
            this.OpacityValue.PreviewKeyDown += OpacityValue_PreviewKeyDown;

            this.OpacitySlider.ValueChanged += OpacitySlider_ValueChanged;

            this.Closed += SettingView_Closed;
            
        }

    

        private SettingView(StickyNoteView stickyNote) : this()
        {
            stickyNoteView = stickyNote;//赋值全局变量
            LoadTheme();
            LoadStyle();
            LoadCurrentViewSetting();

        }
        /// <summary>
        /// 单例创建
        /// </summary>
        /// <param name="stickyNote">传入便签窗口</param>
        /// <returns>返回创建的设置窗口</returns>
        public static SettingView GetInstance(StickyNoteView stickyNote)
        {
            if (SettingViewSingleton == null)//双重锁定只需要一句判断就可以了
            {
                lock (locker)//线程锁
                {
                    if (SettingViewSingleton == null)//判断类是否已经实例化
                    {
                        SettingViewSingleton = new SettingView( stickyNote);
                    }
                }
            }
            return SettingViewSingleton;
        }


        #region 依赖属性
        public static DependencyProperty topBackgroundProperty;//标题颜色展示
        public static DependencyProperty backBackgroundProperty;//背景颜色展示
        public static DependencyProperty fontBackgroundProperty;//字体颜色展示
        public static DependencyProperty fontFamilyProperty;//字体展示
        public static DependencyProperty fontSizeProperty;//字体尺寸展示
        public string topBackground //依赖属性封装
        {
            get { return (string)GetValue(topBackgroundProperty); }
            set { SetValue(topBackgroundProperty, value); }
        }
        public string backBackground //依赖属性封装
        {
            get { return (string)GetValue(backBackgroundProperty); }
            set { SetValue(backBackgroundProperty, value); }
        }
        public string fontBackground //依赖属性封装
        {
            get { return (string)GetValue(fontBackgroundProperty); }
            set { SetValue(fontBackgroundProperty, value); }
        }
        public string fontFamily //依赖属性封装
        {
            get { return (string)GetValue(fontFamilyProperty); }
            set { SetValue(fontFamilyProperty, value); }
        }
        public string fontSize //依赖属性封装
        {
            get { return (string)GetValue(fontSizeProperty); }
            set { SetValue(fontSizeProperty, value); }
        }

        //静态构造函数 , 注册依赖属性，仅在第一次创建实例时调用，且只调用一次
        static SettingView()
        {
            topBackgroundProperty = DependencyProperty.Register("topBackground", typeof(string), typeof(SettingView), new FrameworkPropertyMetadata("#FFAEB9"));
            backBackgroundProperty = DependencyProperty.Register("backBackground", typeof(string), typeof(SettingView), new FrameworkPropertyMetadata("#FFAEB9"));
            fontBackgroundProperty = DependencyProperty.Register("fontBackground", typeof(string), typeof(SettingView), new FrameworkPropertyMetadata("#FFAEB9"));
            fontFamilyProperty = DependencyProperty.Register("fontFamily", typeof(string), typeof(SettingView), new FrameworkPropertyMetadata("微软雅黑"));
            fontSizeProperty = DependencyProperty.Register("fontSize", typeof(string), typeof(SettingView), new FrameworkPropertyMetadata("16"));
        }
        #endregion
        /// <summary>
        /// 加载预置主题信息
        /// </summary>
        private void LoadTheme()
        {
            List<string> items = new List<string>();
            for (int i = 0; i < Window.Themes.Count; i++)
            {
                items.Add(string.Format("{0} : {1}", i + 1, Window.Themes[i].Name));//将组合的字符串赋值Item
            }
            ThemeComboBox.Items.Clear();
            ThemeComboBox.ItemsSource = items;
        }
        /// <summary>
        /// 加载预置文本样式信息
        /// </summary>
        private void LoadStyle()
        {
            List<string> items = new List<string>();
            for (int i = 0; i < Window.Styles.Count; i++)
            {
                items.Add(string.Format("{0} : {1}", i + 1, Window.Styles[i].Name));//将组合的字符串赋值Item
            }
            StyleComboBox.Items.Clear();
            StyleComboBox.ItemsSource = items;
        }
        /// <summary>
        /// 加载当前便签窗口设置的属性信息
        /// </summary>
        private void LoadCurrentViewSetting()
        {
            SetShowStyle();
            SetShowTheme();
            this.OpacityValue.Text = stickyNoteView.Opacity.ToString();
            this.OpacitySlider.Value = (double.Parse(stickyNoteView.backgroundOpacity)) * 10;
            ThemeComboBox.SelectedIndex = stickyNoteView.CurrentTheme;
            StyleComboBox.SelectedIndex = stickyNoteView.CurrentStyle;
        }

        private void SetShowStyle()
        {
            Style st = Window.Styles[stickyNoteView.CurrentStyle];
            if (st.Name == "用户") st = stickyNoteView.CustomStyle;

            this.fontFamily = st.FontFamily;
            this.fontSize = st.FontSize;
            // Console.WriteLine(st.FontFamily);
        }

        private void SetShowTheme()
        {
            Theme th = Window.Themes[stickyNoteView.CurrentTheme];
            if (th.Name == "用户") th = stickyNoteView.CustomTheme;
            this.topBackground = th.TopBarColor;
            this.backBackground = th.BackColor;
            this.fontBackground = th.TextColor;
        }



        #region 事件相关

        private void FontSizeValue_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Enter))
            {
                try
                {
                    Style th = Window.Styles[stickyNoteView.CurrentStyle];
                    if (th.Name == "Custom") th = stickyNoteView.CustomStyle;
                    if (th.FontSize == this.FontSizeValue.Text) return;

                    stickyNoteView.CustomStyle.FontSize = this.FontSizeValue.Text;
                    if (StyleComboBox.SelectedIndex != 0)
                    {
                        StyleComboBox.SelectedIndex = 0;
                    }
                    else
                    {
                        stickyNoteView.ReloadStyle();
                        stickyNoteView.Save();
                        SetShowStyle();
                    }
                }
                catch
                { MessageBox.Show("提示", "请输入数字"); }
            }
        }

        /// <summary>
        /// 键盘事件，输入完成后回车即可应用数据
        /// </summary>
        private void OpacityValue_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Enter))
            {
                try
                {
                    stickyNoteView.backgroundOpacity = this.OpacityValue.Text;
                    stickyNoteView.navBarOpacity = (double.Parse(this.OpacityValue.Text) + stickyNoteView.navOpacityIncrease).ToString();
                    Console.WriteLine(stickyNoteView.navBarOpacity);
                }
                catch
                { MessageBox.Show("提示", "请输入数字"); }
            }

        }
        /// <summary>
        /// 透明度滑条值发生改变事件
        /// </summary>  
        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Console.WriteLine(this.OpacitySlider.Value);   slider的值为0-10  ，我们只需0-1
            double value = (double)this.OpacitySlider.Value / 10;//获取值
            this.OpacityValue.Text = String.Format("{0:F}", value);//将获取到的滑条值赋值输入框，显示后两位小数
            if (value <= 0.3) { value = 0.3; } stickyNoteView.backgroundOpacity = value.ToString();//赋值窗口透明度
            stickyNoteView.navBarOpacity = (value + stickyNoteView.navOpacityIncrease).ToString();
        }
        /// <summary>
        /// 主题CombBox主题选项的改变
        /// </summary>
        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //设置当前主题号
            try
            {
                stickyNoteView.CurrentTheme = (int)ThemeComboBox.SelectedIndex;
                SetShowTheme();
            }
            catch { }
        }

        /// <summary>
        /// 风格CombBox选项的改变
        /// </summary>
        private void StyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //设置当前风格号
            try
            {
                stickyNoteView.CurrentStyle = (int)StyleComboBox.SelectedIndex;
                SetShowStyle();
            }
            catch { }
        }
        /// <summary>
        /// 确认按钮事件
        /// </summary>
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            stickyNoteView.Save();
            this.Close();
        }
        /// <summary>
        /// 字体颜色、背景颜色、标题栏颜色的按钮事件
        /// </summary>
        private void FontColorShow_Click(object sender, RoutedEventArgs e)
        {
            CreateColorDialog("text", this.fontBackground);
        }

        private void BackgroundColorShow_Click(object sender, RoutedEventArgs e)
        {
            CreateColorDialog("back", this.backBackground);
        }

        private void TopColorShow_Click(object sender, RoutedEventArgs e)
        {
            CreateColorDialog("top", this.topBackground);
        }
        /// <summary>
        /// 字体按钮选择事件
        /// </summary>
        private void FontSizeShow_Click(object sender, RoutedEventArgs e)
        {

            System.Windows.Forms.FontDialog fod = new System.Windows.Forms.FontDialog();

            fod.Font = stickyNoteView.CustomStyle.GetFont();
            if (fod.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                stickyNoteView.CustomStyle.FontSize = fod.Font.Size.ToString();
                stickyNoteView.CustomStyle.FontFamily = fod.Font.FontFamily.Name;
                if (StyleComboBox.SelectedIndex != 0)
                {
                    StyleComboBox.SelectedIndex = 0;
                    Console.WriteLine("打印");
                }
                else
                {
                    stickyNoteView.ReloadStyle();
                    stickyNoteView.Save();
                    SetShowStyle();
                }
            }
        }

        /// <summary>
        /// 颜色三按钮调用方法，根据传入名称对不同的的控件进行颜色更改
        /// </summary>
        /// <param name="setType">控件名称</param>
        /// <param name="colorProperty">该控件的颜色字符串16进制</param>
        void CreateColorDialog(string setType, string colorProperty)
        {

            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            Console.WriteLine(colorProperty);

            cd.Color = System.Drawing.ColorTranslator.FromHtml(colorProperty);
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                switch (setType)
                {
                    case "back":
                        stickyNoteView.CustomTheme.BackColor = System.Drawing.ColorTranslator.ToHtml(cd.Color);
                        break;
                    case "top":
                        stickyNoteView.CustomTheme.TopBarColor = System.Drawing.ColorTranslator.ToHtml(cd.Color);
                        break;
                    case "text":
                        stickyNoteView.CustomTheme.TextColor = System.Drawing.ColorTranslator.ToHtml(cd.Color);
                        break;
                    default:
                        break;
                }

                if (ThemeComboBox.SelectedIndex != 0)
                {
                    ThemeComboBox.SelectedIndex = 0;
                }
                else
                {
                    stickyNoteView.ReloadTheme();
                    stickyNoteView.Save();
                    SetShowTheme();
                }
            }
        }

        private void SettingView_Closed(object sender, EventArgs e)
        {
            SettingViewSingleton = null;
        }
        #endregion
    }
}
