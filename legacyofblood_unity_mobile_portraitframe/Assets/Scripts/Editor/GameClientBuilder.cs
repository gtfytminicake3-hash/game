#if UNITY_EDITOR
namespace LegendOfBlood.Editor
{
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine.SceneManagement;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using TMPro;
    using System.Collections.Generic;
    using LegendOfBlood;

    public class GameClientBuilder : EditorWindow
    {
        [MenuItem("Tools/Legend Of Blood/Build Single GameClient Scene")]
        public static void BuildScene()
        {
            if (!EditorUtility.DisplayDialog("Build Single Scene Clean", 
                "Hệ thống sẽ BÓC TÁCH từng thành phần UI riêng biệt từ các Scene cũ và ráp lại thành 1 kiến trúc SIÊU SẠCH (Industry Standard). Chạy ngay?", "Quất!", "Hủy"))
            {
                return;
            }

            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            newScene.name = "GameClient";

            // 1. TẠO CORE_SYSTEMS (Tự động kéo Prefab vào)
            GameObject corePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scenes/CORE_SYSTEMS.prefab");
            GameObject coreSystems = null;
            if (corePrefab != null) {
                coreSystems = (GameObject)PrefabUtility.InstantiatePrefab(corePrefab);
            } else {
                coreSystems = new GameObject("CORE_SYSTEMS"); // Fallback nếu không có Prefab
            }
            SceneManager.MoveGameObjectToScene(coreSystems, newScene);

            // 2. MAIN CAMERA & EVENTSYSTEM (Copy từ Boot_Scene để giữ cấu hình Input chuẩn)
            Scene tempBoot = EditorSceneManager.OpenScene("Assets/Scenes/Boot_Scene.unity", OpenSceneMode.Additive);
            foreach (var go in tempBoot.GetRootGameObjects())
            {
                if (go.GetComponent<EventSystem>() != null) {
                    GameObject esClone = Instantiate(go);
                    esClone.name = "EventSystem";
                    SceneManager.MoveGameObjectToScene(esClone, newScene);
                }
                if (go.GetComponent<Camera>() != null) {
                    GameObject camClone = Instantiate(go);
                    camClone.name = "Main Camera";
                    SceneManager.MoveGameObjectToScene(camClone, newScene);
                }
            }
            EditorSceneManager.CloseScene(tempBoot, true);

            // 3. MAIN CANVAS (Tự tay thiết lập Canvas tiêu chuẩn vàng)
            GameObject canvasObj = new GameObject("MainCanvas");
            Canvas mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            UIManager uiManager = canvasObj.AddComponent<UIManager>();
            SceneManager.MoveGameObjectToScene(canvasObj, newScene);

            // 4. DANH SÁCH BÓC TÁCH PHẪU THUẬT CÁC UI GROUP
            Dictionary<string, UIPanelType> panelMappings = new Dictionary<string, UIPanelType>() {
                {"Village_View", UIPanelType.MainScreen},
                {"WorldMap_Panel", UIPanelType.WorldMap},
                {"HeroInfo_Panel", UIPanelType.HeroInfo},
                {"Breeding_Panel", UIPanelType.Breeding},
                {"Hospital_Panel", UIPanelType.Hospital},
                {"Arena_Panel", UIPanelType.Arena},
                {"SquadSelection_Panel", UIPanelType.SquadSelection},
                {"HeroPicker_Panel", UIPanelType.HeroPicker},
                {"SettingsPanel", UIPanelType.Settings},
                {"BuildingUpgradePanel", UIPanelType.BuildingUpgrade},
                {"TutorialPanel", UIPanelType.Tutorial},
                {"UIResourceBar_Prefab", UIPanelType.None},
                {"Notification_Container", UIPanelType.None}
            };

            List<string> scenesToExtract = new List<string> { 
                "Assets/Scenes/Boot_Scene.unity", 
                "Assets/Scenes/MainScene.unity", 
                "Assets/Scenes/WorldMap_Scene.unity" 
            };

            // Setup khung Panel_Bootloader
            GameObject panelBootloader = new GameObject("Panel_Bootloader");
            panelBootloader.transform.SetParent(canvasObj.transform, false);
            RectTransform bootRect = panelBootloader.AddComponent<RectTransform>();
            bootRect.anchorMin = Vector2.zero; bootRect.anchorMax = Vector2.one;
            bootRect.offsetMin = Vector2.zero; bootRect.offsetMax = Vector2.zero;
            
            UIPanel bootUIPan = panelBootloader.AddComponent<UIPanel>();
            bootUIPan.PanelType = UIPanelType.Bootloader;
            Bootloader bootloaderScript = panelBootloader.AddComponent<Bootloader>();

            GameObject tapToStartClone = null;
            GameObject loadingScreenClone = null;
            HashSet<string> extractedPanels = new HashSet<string>(); // Cờ tránh việc hút trùng lặp

            try
            {
                foreach(string scenePath in scenesToExtract)
                {
                    Scene oldScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                    foreach (var rootGo in oldScene.GetRootGameObjects())
                    {
                        if (rootGo.GetComponent<Canvas>() != null)
                        {
                            // Tiến vào trong Canvas cũ, bóc tách từng Child
                            foreach (Transform child in rootGo.transform)
                            {
                                if (child.name.Contains("TapToStart_Group") && tapToStartClone == null)
                                {
                                    tapToStartClone = Instantiate(child.gameObject, panelBootloader.transform, false);
                                    tapToStartClone.name = "TapToStart_Group";
                                }
                                else if (child.name.Contains("LoadingScreen_Group") && loadingScreenClone == null)
                                {
                                    loadingScreenClone = Instantiate(child.gameObject, panelBootloader.transform, false);
                                    loadingScreenClone.name = "LoadingScreen_Group";
                                }
                                else
                                {
                                    // Bóc các Panel khác chuẩn xác dựa theo tên khai báo
                                    foreach(var kvp in panelMappings)
                                    {
                                        if (child.name.Contains(kvp.Key) && !extractedPanels.Contains(kvp.Key))
                                        {
                                            GameObject clonedPanel = Instantiate(child.gameObject, canvasObj.transform, false);
                                            clonedPanel.name = "EXTRACTED_" + kvp.Key; // Đổi tên để nhận diện dễ
                                            
                                            // Ngoại tuyến Panel_MainScreen để wrap Village View lại
                                            if (kvp.Key == "Village_View") {
                                                GameObject wrapper = new GameObject("Panel_MainScreen");
                                                wrapper.transform.SetParent(canvasObj.transform, false);
                                                
                                                RectTransform wrapperRect = wrapper.AddComponent<RectTransform>();
                                                wrapperRect.anchorMin = Vector2.zero; wrapperRect.anchorMax = Vector2.one;
                                                wrapperRect.offsetMin = Vector2.zero; wrapperRect.offsetMax = Vector2.zero;

                                                clonedPanel.transform.SetParent(wrapper.transform, false);
                                                clonedPanel.name = "Village_View";
                                                
                                                UIPanel uiPanWrapper = wrapper.AddComponent<UIPanel>();
                                                uiPanWrapper.PanelType = UIPanelType.MainScreen;
                                                wrapper.SetActive(false);
                                            }
                                            else 
                                            {
                                                if (kvp.Value != UIPanelType.None) {
                                                    UIPanel uiPan = clonedPanel.GetComponent<UIPanel>();
                                                    if (uiPan == null) uiPan = clonedPanel.AddComponent<UIPanel>();
                                                    uiPan.PanelType = kvp.Value;
                                                }
                                                // Luôn tắt các Panel lúc mới ráp, trừ Bootloader
                                                clonedPanel.SetActive(false);
                                            }
                                            
                                            extractedPanels.Add(kvp.Key);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    EditorSceneManager.CloseScene(oldScene, true);
                }

                // 5. TỰ ĐỘNG NỐI DÂY SCRIPT BOOTLOADER MÀ KHÔNG CẦN CHUỘT (Dùng SerializedObject hack inspector)
                SerializedObject so = new SerializedObject(bootloaderScript);
                so.Update();
                if (corePrefab != null) so.FindProperty("coreSystemsPrefab").objectReferenceValue = corePrefab;
                if (tapToStartClone != null) so.FindProperty("tapToStartGroup").objectReferenceValue = tapToStartClone;
                if (loadingScreenClone != null) {
                    so.FindProperty("loadingScreenGroup").objectReferenceValue = loadingScreenClone;
                    Slider slider = loadingScreenClone.GetComponentInChildren<Slider>(true);
                    if (slider != null) so.FindProperty("loadingSlider").objectReferenceValue = slider;
                    TextMeshProUGUI txt = loadingScreenClone.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (txt != null) so.FindProperty("loadingText").objectReferenceValue = txt;
                }
                so.ApplyModifiedProperties();

                // Vuốt Bootloader xuống cuối phả hệ để đè lên mọi UI lúc bật máy
                panelBootloader.transform.SetAsLastSibling();

                // Lưu GameClient Mới Toanh Thành Công!
                string savePath = "Assets/Scenes/GameClient.unity";
                EditorSceneManager.SaveScene(newScene, savePath);
                
                Debug.Log($"<color=green>BÓC TÁCH THÀNH CÔNG! Scene {savePath} đã được chế tạo từ các bộ phận sạch 100%.</color>");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Lỗi khi bóc tách Scene: " + ex.Message);
            }
        }
    }
}
#endif
