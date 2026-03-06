using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LegendOfBlood.DebugTools
{
    /// <summary>
    /// Script hỗ trợ tự động click tất cả các nút trong Scene để stress test UI.
    /// Cách dùng: Gắn Script này vào một GameObject rỗng trong Scene "gameclient" và bấm Play.
    /// Có thể bấm phím "T" trên bàn phím để kích hoạt Auto Test bất cứ lúc nào.
    /// </summary>
    public class AutoUITester : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Bật để script tự chạy ngay khi Scene Load xong")]
        public bool runOnStart = false;
        
        [Tooltip("Thời gian chờ giữa 2 lần bấm nút (giây)")]
        public float delayBetweenClicks = 0.5f;
        
        [Tooltip("Tự động tìm phím mới sinh ra qua mỗi lần click?")]
        public bool findNewButtonsDynamically = true;

        private bool _isTesting = false;
        private HashSet<Button> _clickedButtons = new HashSet<Button>();
        private Coroutine _testRoutine;

        private void Start()
        {
            if (runOnStart)
            {
                StartAutoTest();
            }
        }

        [ContextMenu("▶ Start Auto Test Dữ Dội")]
        public void StartAutoTest()
        {
            if (_isTesting) return;
            
            Debug.Log("<color=yellow>=== BẮT ĐẦU CHẠY AUTO UI TESTER ===</color>\nChương trình sẽ tự quét và bấm lần lượt các nút.");
            _isTesting = true;
            _clickedButtons.Clear();
            _testRoutine = StartCoroutine(TestRoutine());
        }

        [ContextMenu("⏹ Stop Auto Test")]
        public void StopAutoTest()
        {
            if (!_isTesting) return;
            
            Debug.Log("<color=red>=== DỪNG AUTO UI TESTER ===</color>");
            _isTesting = false;
            if (_testRoutine != null) StopCoroutine(_testRoutine);
        }

        private IEnumerator TestRoutine()
        {
            while (_isTesting)
            {
                // Quét toàn bộ Scene tìm nút có thể bấm được (bỏ qua nút đã ẩn hoặc không interactable)
                Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                
                List<Button> validButtons = new List<Button>();
                foreach (var btn in allButtons)
                {
                    if (btn.interactable && btn.gameObject.activeInHierarchy && !_clickedButtons.Contains(btn))
                    {
                        validButtons.Add(btn);
                    }
                }

                if (validButtons.Count == 0)
                {
                    Debug.Log("<color=green>=== Auto UI Tester: ĐÃ BẤM HẾT MỌI NÚT TRONG TẦM MẮT! ===</color>");
                    
                    if (!findNewButtonsDynamically)
                    {
                        StopAutoTest();
                        break;
                    }
                    
                    // Nếu vẫn muốn rình nút mới, chờ 1 tí rồi quét lại
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                // Chọn nút đầu tiên trong danh sách chưa bấm
                Button targetButton = validButtons[0];
                _clickedButtons.Add(targetButton); // Đánh dấu đã bấm để tránh lặp vô hạn

                // Log Tên nút và Đường dẫn của nó trong Hierarchy để dễ dò lỗi
                string path = GetGameObjectPath(targetButton.gameObject);
                Debug.Log($"<color=cyan>[AutoUITester]</color> Đang bấm chọn: <b>{targetButton.gameObject.name}</b>\n<color=grey>Path: {path}</color>");

                // Lám lóe màu nút để người xem biết nút nào đang bị bấm
                ColorBlock originalColors = targetButton.colors;
                ColorBlock highlightColors = targetButton.colors;
                highlightColors.normalColor = Color.yellow;
                targetButton.colors = highlightColors;
                
                yield return new WaitForSeconds(0.1f); // Dừng lại 0.1s cho nút sáng lên
                
                // Trả màu lại
                targetButton.colors = originalColors;

                // THỰC HIỆN CÚ CLICK CHÁY MÁY
                try
                {
                    // Giả lập Click hệ thống Event (chuẩn xác nhất)
                    PointerEventData pointerData = new PointerEventData(EventSystem.current);
                    ExecuteEvents.Execute(targetButton.gameObject, pointerData, ExecuteEvents.submitHandler);
                    ExecuteEvents.Execute(targetButton.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"<color=red>[AutoUITester]</color> Kẹt lỗi Exception khi bấm nút <b>{targetButton.name}</b>!\nChi tiết: {e.Message}");
                }

                // Chờ một quãng để Panel mở lên kịp/Animation chạy xong
                yield return new WaitForSeconds(delayBetweenClicks);
            }
        }

        /// <summary>
        /// Hàm nội bộ để lấy đường dẫn GameObject trong Hierarchy. Dùng để in Log cho dễ fix.
        /// </summary>
        private string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform current = obj.transform;
            while (current.parent != null)
            {
                current = current.parent;
                path = current.name + "/" + path;
            }
            return path;
        }
    }
}
