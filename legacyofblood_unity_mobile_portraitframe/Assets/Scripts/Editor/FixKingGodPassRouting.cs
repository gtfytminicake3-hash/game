using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using LegendOfBlood.UI;

namespace LegendOfBlood.Editor
{
    public class FixKingGodPassRouting : EditorWindow
    {
        [MenuItem("UI Tools/Fix KingGodPass Routing")]
        public static void FixRouting()
        {
            bool hasChanges = false;

            // 1. Kiểm tra và gỡ bỏ KingGodPassPanel bị gắn nhầm vào MenuPanel hoặc MainScreen
            var allMenuPanels = Object.FindObjectsByType<MenuPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var mp in allMenuPanels)
            {
                var kgpassComp = mp.GetComponent<KingGodPassPanel>();
                if (kgpassComp != null)
                {
                    Debug.Log($"[FixRouting] Đã gỡ bỏ script KingGodPassPanel gắn nhầm trên GameObject {mp.gameObject.name}");
                    Undo.DestroyObjectImmediate(kgpassComp);
                    hasChanges = true;
                }
            }

            // Kiểm tra MainScreen / Canvas
            var mainControllers = Object.FindObjectsByType<UIMainController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var mc in mainControllers)
            {
                var kgpassComp = mc.GetComponent<KingGodPassPanel>();
                if (kgpassComp != null)
                {
                    Debug.Log($"[FixRouting] Đã gỡ bỏ script KingGodPassPanel gắn nhầm trên GameObject {mc.gameObject.name} (Main Controller)");
                    Undo.DestroyObjectImmediate(kgpassComp);
                    hasChanges = true;
                }
            }

            // 2. Tìm đúng KingGodPassPanel và set PanelType chính xác
            var allKgPanels = Object.FindObjectsByType<KingGodPassPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var p in allKgPanels)
            {
                if (p.PanelType != UIPanelType.KingGodPass)
                {
                    Undo.RecordObject(p, "Set KingGodPass PanelType");
                    p.PanelType = UIPanelType.KingGodPass;
                    Debug.Log($"[FixRouting] Đã set lại PanelType của {p.gameObject.name} thành KingGodPass");
                    hasChanges = true;
                }
            }

            // 3. Tìm nút KingGodPass tại màn hình chính và đảm bảo nó trỏ đúng về KingGodPass
            GameObject pPass = GameObject.Find("KingGodPass");
            if (pPass != null)
            {
                var btn = pPass.GetComponent<Button>();
                if (btn != null)
                {
                    var nav = pPass.GetComponent<UIPanelNavButton>();
                    if (nav != null && nav.targetPanel != UIPanelType.KingGodPass)
                    {
                        Undo.RecordObject(nav, "Fix Nav Button");
                        nav.targetPanel = UIPanelType.KingGodPass;
                        Debug.Log($"[FixRouting] Đã sửa nút KingGodPass để trỏ tới UIPanelType.KingGodPass (cũ: {nav.targetPanel})");
                        hasChanges = true;
                    }
                }
            }

            if (hasChanges)
            {
                Debug.Log("[FixRouting] Đã hoàn tất sửa lỗi điều hướng King God Pass.");
            }
            else
            {
                Debug.Log("[FixRouting] Không phát hiện thiết lập sai nào. Nếu vẫn bị lỗi, có thể prefab chưa được kéo vào scene hoặc UIPanelType bị set thủ công sai trong lúc Play.");
            }
        }
    }
}
