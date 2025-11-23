using System;
using System.Windows.Forms;
using Newtonsoft.Json;
using SolidWorks.Interop.sldworks;

namespace AISW
{
    public class AiForm : Form
    {
        private TextBox _promptBox;
        private Button _runButton;
        private TextBox _logBox;

        // 由 SwAddin 在 ConnectToSW 里赋值：
        // _aiForm = new AiForm(); _aiForm.SwApp = _swApp;
        public ISldWorks SwApp { get; set; }

        public AiForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "AI 建模助手（原型）";
            this.Width = 500;
            this.Height = 400;

            // 上面的自然语言输入框
            _promptBox = new TextBox();
            _promptBox.Multiline = true;
            _promptBox.Dock = DockStyle.Top;
            _promptBox.Height = 80;
            _promptBox.Text = "在这里用自然语言描述模型，比如：在前视基准面画一个100×50×20的长方体。";

            // 中间的按钮
            _runButton = new Button();
            _runButton.Text = "调用 AI 生成建模计划";
            _runButton.Dock = DockStyle.Top;
            _runButton.Height = 35;
            _runButton.Click += RunButton_Click;

            // 底下的日志输出框
            _logBox = new TextBox();
            _logBox.Multiline = true;
            _logBox.Dock = DockStyle.Fill;
            _logBox.ReadOnly = true;
            _logBox.ScrollBars = ScrollBars.Vertical;

            this.Controls.Add(_logBox);
            this.Controls.Add(_runButton);
            this.Controls.Add(_promptBox);
        }

        private async void RunButton_Click(object sender, EventArgs e)
        {
            string prompt = _promptBox.Text;
            if (string.IsNullOrWhiteSpace(prompt))
            {
                MessageBox.Show("先输入一句描述，比如：在前视基准面画一个宽100高50厚20的长方体。");
                return;
            }

            try
            {
                _runButton.Enabled = false;
                _logBox.AppendText("调用 AI 生成建模计划...\r\n");

                // 正常情况下走这一行：调用 AI
                string json = await AiPlanner.CreateBlockPlanAsync(prompt);

                // 如果老是 429，可以暂时改成本地假数据测试建模：
                // string json = "{\"operation\":\"create_block\",\"plane\":\"Front\",\"width\":100,\"height\":50,\"thickness\":20,\"unit\":\"mm\"}";

                _logBox.AppendText("AI 返回的 JSON：\r\n");
                _logBox.AppendText(json + "\r\n\r\n");

                BlockPlan plan = JsonConvert.DeserializeObject<BlockPlan>(json);
                if (plan == null)
                {
                    _logBox.AppendText("解析失败：反序列化结果为 null\r\n");
                    return;
                }

                _logBox.AppendText(
                    string.Format(
                        "解析成功：Operation={0}, Plane={1}, W={2}mm, H={3}mm, T={4}mm\r\n",
                        plan.Operation, plan.Plane, plan.Width, plan.Height, plan.Thickness
                    )
                );

                // 调 SolidWorks 建模
                if (SwApp == null)
                {
                    _logBox.AppendText("SwApp 为空，无法调用 SolidWorks 建模。\r\n");
                }
                else
                {
                    _logBox.AppendText("开始在 SolidWorks 中创建长方体...\r\n");
                    SwModeler.CreateBlockFromPlan(SwApp, plan);
                    _logBox.AppendText("SolidWorks 建模调用完成。\r\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("调用 AI 失败：\r\n" + ex.Message);
            }
            finally
            {
                _runButton.Enabled = true;
            }
        }
    }
}
