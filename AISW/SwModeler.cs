using System;
using System.Windows.Forms;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace AISW
{
    public static class SwModeler
    {
        public static void CreateBlockFromPlan(ISldWorks swApp, BlockPlan plan)
        {
            if (swApp == null)
            {
                MessageBox.Show("SwApp 为 null，Addin 还没有正确连接到 SolidWorks。");
                return;
            }

            if (plan == null)
            {
                MessageBox.Show("建模计划为空，无法建模。");
                return;
            }

            // 1. 确保有一个零件文档
            ModelDoc2 model = swApp.ActiveDoc as ModelDoc2;
            if (model == null || model.GetType() != (int)swDocumentTypes_e.swDocPART)
            {
                model = swApp.NewPart() as ModelDoc2;
                if (model == null)
                {
                    MessageBox.Show("无法创建新的零件文档，请检查模板设置。");
                    return;
                }
            }

            // 2. 选择基准面
            string planeName;
            switch (plan.Plane.ToLower())
            {
                case "front":
                    planeName = "Front Plane";   // 中文版可能叫“前视基准面”
                    break;
                case "top":
                    planeName = "Top Plane";     // “上视基准面”
                    break;
                case "right":
                    planeName = "Right Plane";   // “右视基准面”
                    break;
                default:
                    planeName = "Front Plane";
                    break;
            }

            bool selPlane = model.Extension.SelectByID2(
                planeName,
                "PLANE",
                0, 0, 0,
                false,
                0,
                null,
                0);

            if (!selPlane)
            {
                MessageBox.Show("无法选中基准面：" + planeName +
                                "\n如果你是中文版 SolidWorks，把代码里的名字改成 “前视基准面 / 上视基准面 / 右视基准面” 再试。");
                return;
            }

            SketchManager sketchMgr = model.SketchManager;
            FeatureManager featMgr = model.FeatureManager;

            // 3. 进入草图，在原点画中心矩形
            sketchMgr.InsertSketch(true);

            // SolidWorks 内部单位是米，plan 里是 mm
            double width = plan.Width / 1000.0;
            double height = plan.Height / 1000.0;
            double thickness = plan.Thickness / 1000.0;

            // 以原点为中心画一个中心矩形
            sketchMgr.CreateCenterRectangle(
                0, 0, 0,
                width / 2.0, height / 2.0, 0);

            // 退出草图
            sketchMgr.InsertSketch(true);

            // 4. 直接用当前草图做拉伸（草图刚结束时一般是选中的）
            // 如果保险一点，也可以按名字选草图，这里先简单处理
            // model.ClearSelection2(true);
            // model.Extension.SelectByID2("Sketch1", "SKETCH", 0,0,0,false,0,null,0);

            Feature feat = featMgr.FeatureExtrusion2(
                true,   // 方向1 草图轮廓
                false,  // 方向2（不用）
                false,  // 薄壁特征?
                (int)swEndConditions_e.swEndCondBlind, // 方向1 终止条件：Blind
                0,      // 方向2 终止条件
                thickness, // 深度1 (米)
                0.01,   // 深度2（不会用到，随便一个小值）
                false, false, false, false,
                0.0, 0.0,
                false, false,
                false, false,
                true,   // 合并实体
                true, true,
                (int)swStartConditions_e.swStartSketchPlane,
                0.0,
                false);

            if (feat == null)
            {
                MessageBox.Show("拉伸特征创建失败。");
                return;
            }

            model.ViewZoomtofit2();
        }
    }
}
