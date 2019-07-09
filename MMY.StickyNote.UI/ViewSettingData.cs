using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MMY.StickyNote.UI
{
    public class ViewSettingData
    {
        /// <summary>
        /// 相对屏幕的左距离和上距离决定窗口位置
        /// </summary>
        public double WinLeft { get; set; }
        public double WinTop { get; set; }
        /// <summary>
        /// 相对于宽高决定窗口尺寸
        /// </summary>
        public double WinWidth { get; set; }
        public double WinHeigth { get; set; }
        /// <summary>
        /// 标签内容
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// 主题Id
        /// </summary>
        public int Theme { get; set; }
        /// <summary>
        /// 当前风格Id
        /// </summary>
        public int Style { get; set; }
        /// <summary>
        /// 透明度
        /// </summary>
        public double Opacity { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 隐藏与否
        /// </summary>
        public bool Hidden { get; set; }
        /// <summary>
        /// 置顶与否
        /// </summary>
        public bool Topmost { get; set; }
        /// <summary>
        /// 默认主题
        /// </summary>
        public Theme CustomTheme { get; set; }
        /// <summary>
        /// 默认风格
        /// </summary>
        public Style CustomStyle { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }

   

        public ViewSettingData() { }
        public ViewSettingData(StickyNoteView stickyNoteView)
        {

            WinLeft = stickyNoteView.Left;
            WinTop = stickyNoteView.Top;

            WinWidth = stickyNoteView.Width;
            WinHeigth = stickyNoteView.Height;

            //******放弃使用富文本，原因在于富文本回车会假象换两行，再次读取内容时正常！且读取注册表内容数据后赋值控件后，末尾多加\r\n字符，
            //TextRange textRange = new TextRange(stickyNoteView.ContentTextBox.Document.ContentStart, stickyNoteView.ContentTextBox.Document.ContentEnd);
            //Data = textRange.Text;
            Data = stickyNoteView.ContentTextBox.Text;

            Theme = stickyNoteView.CurrentTheme;
            Style = stickyNoteView.CurrentStyle;

            Opacity = double.Parse(stickyNoteView.backgroundOpacity);
            Hidden = !stickyNoteView.IsVisible;/////////由于获取IsVisible是获取窗口是否处于显示状态，是则为true，否则为false，而Hidden是关于隐藏的描述，如果隐藏则为true，否则为false ，所以需要取非

            Title = stickyNoteView.StickyNoteTitle.Content.ToString();
            Topmost = stickyNoteView.Topmost;
            CustomTheme = stickyNoteView.CustomTheme;
            //creationTime = stickyNoteView.CreationTime;
            CustomStyle = stickyNoteView.CustomStyle;
            

        }
    }
    public class Theme
    {
        public Theme() { DefaultTheme(); }
        public Theme(string name, string text, string back, string top)
        {
            Name = name;
            TextColor = text;
            BackColor = back;
            TopBarColor = top;
        }

        public string Name { get; set; }
        public string TextColor { get; set; }
        public string BackColor { get; set; }
        public string TopBarColor { get; set; }

        private void DefaultTheme()
        {
            Name = "默认";
            TextColor = "#FFFFFF";
            BackColor = "#FFAEB9";
            TopBarColor = "#FFAEB9";
        }
    }

    public class Style
    {
        public Style() { DefaultStyle(); }
        public Style(string name, string family, string size, string top)
        {
            Name = name;
            FontFamily = family;
            FontSize = size;
            //FStyle = FontStyle.;
        }

        public System.Drawing.Font GetFont()
        {
            System.Drawing.FontFamily fontFamily = new System.Drawing.FontFamily(this.FontFamily);
            System.Drawing.Font font = new System.Drawing.Font(fontFamily, float.Parse(this.FontSize));

            return font;
        }


        public string Name { get; set; }
        public string FontFamily { get; set; }
        public string FontSize { get; set; }
        //public FontStyle FStyle { get; set; }

        private void DefaultStyle()
        {
            Name = "默认";
            FontFamily = "微软雅黑";
            FontSize = "15";
        }
    }
}
