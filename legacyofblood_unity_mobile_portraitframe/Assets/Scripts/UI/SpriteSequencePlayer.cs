using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LegendOfBlood
{
    /// <summary>
    /// Phát một chuỗi Sprite từ thư mục Resources.
    /// Hỗ trợ cả Image và RawImage để tránh xung đột component.
    /// </summary>
    public class SpriteSequencePlayer : MonoBehaviour
    {
        [Tooltip("Đường dẫn đến thư mục chứa frames trong Resources (ví dụ: UI/Panel/panel_dong_frames)")]
        public string resourcePath;
        
        [Tooltip("Số khung hình trên giây")]
        public float frameRate = 12f;
        
        public bool loop = true;
        public bool playOnAwake = true;

        private Graphic _targetGraphic;
        private Sprite[] _frames;
        private bool _isPlaying = false;
        private int _currentIndex = 0;

        void Awake()
        {
            // Tìm Image hoặc RawImage trên đối tượng
            _targetGraphic = GetComponent<Image>();
            if (_targetGraphic == null) _targetGraphic = GetComponent<RawImage>();
        }

        void Start()
        {
            if (!string.IsNullOrEmpty(resourcePath))
            {
                LoadFrames();
            }

            if (playOnAwake && _frames != null && _frames.Length > 0)
            {
                Play();
            }
        }

        void OnEnable()
        {
            // Reset trạng thái và chạy lại nếu playOnAwake được bật
            if (playOnAwake && _frames != null && _frames.Length > 0)
            {
                _isPlaying = false; // Reset flag để Play() có thể chạy lại Coroutine
                Play();
            }
        }

        void OnDisable()
        {
            _isPlaying = false;
        }

        public void LoadFrames()
        {
            _frames = Resources.LoadAll<Sprite>(resourcePath)
                               .OrderBy(s => s.name)
                               .ToArray();

            if (_frames == null || _frames.Length == 0)
            {
                // Fallback thử tìm Texture2D nếu không thấy Sprite
                var textures = Resources.LoadAll<Texture2D>(resourcePath)
                                       .OrderBy(t => t.name)
                                       .ToArray();
                
                if (textures.Length > 0)
                {
                    _frames = textures.Select(t => Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f))).ToArray();
                }
            }

            if (_frames == null || _frames.Length == 0)
            {
                Debug.LogWarning($"[SpriteSequencePlayer] Không tìm thấy ảnh nào tại: {resourcePath}. Hãy chắc chắn bạn đã đổi Texture Type thành Sprite.");
            }
        }

        public void Play()
        {
            if (_isPlaying) return;
            if (_frames == null || _frames.Length == 0) LoadFrames();

            if (_frames != null && _frames.Length > 0)
            {
                _isPlaying = true;
                StartCoroutine(AnimateSequence());
            }
        }

        private IEnumerator AnimateSequence()
        {
            while (_isPlaying)
            {
                if (_targetGraphic != null && _frames[_currentIndex] != null)
                {
                    if (_targetGraphic is Image img) img.sprite = _frames[_currentIndex];
                    else if (_targetGraphic is RawImage raw) raw.texture = _frames[_currentIndex].texture;
                }

                _currentIndex = (_currentIndex + 1) % _frames.Length;
                if (!loop && _currentIndex == 0)
                {
                    _isPlaying = false;
                    yield break;
                }

                yield return new WaitForSeconds(1f / frameRate);
            }
        }
    }
}
