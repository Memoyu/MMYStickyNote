using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace MMY.StickyNote.UI
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    ///         

    public partial class App : System.Windows.Application
    {
        NotifyIcon _notifyIcon;//托盘
        List<System.Windows.Forms.MenuItem> menuItems = new List<System.Windows.Forms.MenuItem>();
        //InitialTary();

        /// <summary>
        /// 创建系统托盘
        /// </summary>
        private void InitialTary()
        {
            //设置托盘的各个属性
            _notifyIcon = new NotifyIcon();
            _notifyIcon.BalloonTipText = "StickyNote";//托盘气泡显示内容
            _notifyIcon.Text = "StickyNote";
            _notifyIcon.Visible = true;//托盘按钮是否可见
            _notifyIcon.Icon = new Icon(@"StickyNoteIcon.ico");//托盘中显示的图标
            _notifyIcon.ShowBalloonTip(2000);//托盘气泡显示时间
            _notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
            _notifyIcon.MouseClick += NotifyIcon_MouseClick;

            //新建便签
            CreateMenuItem("新建", NewNote_Click);
            ////显示全部便签
            CreateMenuItem("显示全部", ShowAllNote_Click);
            ////隐藏全部便签
            CreateMenuItem("隐藏全部", HideAllNote_Click);
            ////便签管理
            CreateMenuItem("便签管理", NoteManager_Click);
            ////关于
            CreateMenuItem("关于", About_Click);
            ////退出菜单项
            CreateMenuItem("退出", Exit_Click);

            //关联托盘控件
            _notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(menuItems.ToArray());
        }
    }
   
}
