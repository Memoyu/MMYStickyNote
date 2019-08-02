using Microsoft.Win32;
using Newtonsoft.Json;
using System.Reflection;

namespace MMY.StickyNote.UI
{
    /// <summary>
    /// 注册表相关类，将便签数据写入注册表和开机自起
    /// </summary>
    public static class REGISTRY
    {
        /// <summary>
        /// 开机启动项打开节点赋值
        /// </summary>
        public static RegistryKey START_KEY = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        /// <summary>
        /// 创建该软件注册表目录，用存储相关数据，并且是注册表对象
        /// </summary>
        public static RegistryKey REG_PATH = Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("MMY StickyNote").CreateSubKey("StickyNote");

        /// <summary>
        /// 打开该注册表，获取其注册表中的所有项的Id
        /// </summary>
        public static string[] OPENED_NOTES
        {
            get
            {
                return REG_PATH.GetValueNames();
            }
        }
        /// <summary>
        /// 将数据写入注册表
        /// </summary>
        /// <param name="id">便签窗口Id</param>
        /// <param name="data">此便签相关的数据对象</param>
        public static void SetData(string id, ViewSettingData data)
        {
            string dat = JsonConvert.SerializeObject(data);
            REG_PATH.SetValue(id, dat);//写入注册表
        }
        /// <summary>
        /// 根据便签窗口Id获取对应的注册表信息
        /// </summary>
        /// <param name="id">传入窗口Id</param>
        /// <returns></returns>
        public static ViewSettingData GetData(string id)
        {
            object dat = REG_PATH.GetValue(id, null);
            if (dat == null) return null;
            return JsonConvert.DeserializeObject<ViewSettingData>((string)dat);
        }
        /// <summary>
        /// 删除指定窗口Id注册表数据
        /// </summary>
        /// <param name="id">指定Id</param>
        public static void Delete(string id)
        {
            try { REG_PATH.DeleteValue(id); }
            catch { }
        }
        /// <summary>
        /// 删除所有该软件注册表目录下的所有窗口数据
        /// </summary>
        public static void DeleteAll()
        {
            foreach (string val in OPENED_NOTES)
            {
                REG_PATH.DeleteValue(val);
            }
        }
        /// <summary>
        /// 注册表开机自项创建及修改
        /// </summary>
        public static bool StartWithWindows
        {
            get
            {
                object obj = START_KEY.GetValue("MMY StickyNote", null);//获取自启注册表项，
                if (obj == null) return false;
                return true;
            }
            set
            {
                if (value) { START_KEY.SetValue("MMY StickyNote", Assembly.GetExecutingAssembly().Location); }//根据checkBox传入的值来决定。设置自启注册表项，获取可执行文件目录，赋值注册表
                else
                {
                    try { START_KEY.DeleteValue("MMY StickyNote", false); }//删除该注册表自启项
                    catch { }
                }
            }
        }
        /// <summary>
        /// 首次运行注册表检测
        /// </summary>
        public static bool FirstRun
        {
            get
            {
                object obj = START_KEY.GetValue("First Run", null);//获取注册中First Run的值
                if (obj == null) return true; //为空直接返回true
                START_KEY.SetValue("First Run", 0);//否则设置值为0
                return false;
            }
        }
    }
}
