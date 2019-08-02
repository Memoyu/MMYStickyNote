using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MMY.StickyNote.UI
{
    /// <summary>
    /// AboutView.xaml 的交互逻辑
    /// </summary>
    public partial class AboutView : System.Windows.Window
    {
        private static AboutView AboutViewSingleton;//静态变量用来存储类的实例
        private static readonly object locker = new object();//定义一个标识确保线程同步

        private AboutView()
        {
            InitializeComponent();

            this.CloseAboutBtn.Click += CloseAboutBtn_Click;//注册事件
            this.EmailBtn.Click += EmailBtn_Click;
            this.Closed += AboutView_Closed;
        }

       

        public static AboutView GetInstance()
        {
            if (AboutViewSingleton == null)//双重锁定只需要一句判断就可以了
            {
                lock (locker)//线程锁
                {
                    if (AboutViewSingleton == null)//判断类是否已经实例化
                    {
                        AboutViewSingleton = new AboutView();
                    }
                }
            }
            return AboutViewSingleton;
        }

        private void EmailBtn_Click(object sender, RoutedEventArgs e)
        {
            try { System.Diagnostics.Process.Start("mailto:" + EmailBtn.Content.ToString()); }
            catch { }
        }

        private void CloseAboutBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void AboutView_Closed(object sender, EventArgs e)
        {
            AboutViewSingleton = null;
        }
    }
}
