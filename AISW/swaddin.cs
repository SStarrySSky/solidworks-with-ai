using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;

namespace AISW
{
    [ComVisible(true)]
    [Guid("437FF2A8-1551-4026-9EC4-3F63119D7C6A")] // 用你之前一直在用的那串 GUID
    [ProgId("SwAiAddin.Main")]
    public class SwAddin : ISwAddin
    {
        private ISldWorks _swApp;
        private int _addinId;

        private AiForm _aiForm;

        public bool ConnectToSW(object ThisSW, int cookie)
        {
            try
            {
                _swApp = (ISldWorks)ThisSW;
                _addinId = cookie;

                _swApp.SetAddinCallbackInfo(0, this, _addinId);

                // 调试用：确保 ConnectToSW 确实被调用
                MessageBox.Show("SwAddin.ConnectToSW 被调用，准备创建 AiForm。", "AISW");

                // 创建并显示 AI 窗口（注意这里改成无参构造）
                _aiForm = new AiForm();
                _aiForm.SwApp = _swApp;  // 把 SolidWorks 对象传给窗口
                _aiForm.StartPosition = FormStartPosition.CenterScreen;
                _aiForm.TopMost = true;   // 防止被 SW 主窗体盖住
                _aiForm.Show();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ConnectToSW 中发生异常：\n\n" + ex, "AISW 错误");
                return false;
            }
        }

        public bool DisconnectFromSW()
        {
            try
            {
                if (_aiForm != null && !_aiForm.IsDisposed)
                {
                    _aiForm.Close();
                    _aiForm = null;
                }

                _swApp = null;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("DisconnectFromSW 中发生异常：\n\n" + ex, "AISW 错误");
                return false;
            }
        }

        [ComRegisterFunction]
        public static void RegisterFunction(Type t)
        {
            try
            {
                string key =
                    @"SOFTWARE\SolidWorks\Addins\" +
                    "{" + t.GUID.ToString().ToUpper() + "}";

                using (RegistryKey rk = Registry.LocalMachine.CreateSubKey(key))
                {
                    rk.SetValue(null, 1);  // 默认加载
                    rk.SetValue("Description", "AISW AI SolidWorks Addin");
                    rk.SetValue("Title", "AISW AI Addin");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("注册 Addin 失败：\n\n" + ex, "AISW 注册错误");
            }
        }

        [ComUnregisterFunction]
        public static void UnregisterFunction(Type t)
        {
            try
            {
                string key =
                    @"SOFTWARE\SolidWorks\Addins\" +
                    "{" + t.GUID.ToString().ToUpper() + "}";
                Registry.LocalMachine.DeleteSubKey(key, false);
            }
            catch
            {
                // 忽略
            }
        }
    }
}
