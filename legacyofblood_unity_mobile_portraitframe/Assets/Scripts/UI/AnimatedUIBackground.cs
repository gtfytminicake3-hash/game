using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegendOfBlood.UI
{
    [RequireComponent(typeof(Image))]
    public class AnimatedUIBackground : MonoBehaviour
    {
        public string resourceFolderPath = "UI/RecruitmentBG";
        public float fps = 24f;
        public bool loop = true;

        private Image targetImage;
        private List<Sprite> frames = new List<Sprite>();
        private int currentFrame = 0;
        private float timer = 0f;

        private void Awake()
        {
            targetImage = GetComponent<Image>();
            LoadFrames();
        }

        private void LoadFrames()
        {
            frames.Clear();
            Sprite[] sprites = Resources.LoadAll<Sprite>(resourceFolderPath);
            if (sprites.Length > 0)
            {
                frames.AddRange(sprites);
                frames.Sort((a, b) => a.name.CompareTo(b.name));
            }
            else
            {
                Texture2D[] textures = Resources.LoadAll<Texture2D>(resourceFolderPath);
                System.Array.Sort(textures, (a, b) => a.name.CompareTo(b.name));
                foreach (var tex in textures)
                {
                    Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
                    frames.Add(sprite);
                }
            }

            if (frames.Count > 0 && targetImage != null)
            {
                targetImage.sprite = frames[0];
            }
            else
            {
                Debug.LogWarning($"AnimatedUIBackground: No frames found in Resources/{resourceFolderPath}");
            }
        }

        private void Update()
        {
            if (frames.Count == 0 || targetImage == null) return;

            timer += Time.deltaTime;
            float frameInterval = 1f / fps;

            while (timer >= frameInterval)
            {
                timer -= frameInterval;
                currentFrame++;

                if (currentFrame >= frames.Count)
                {
                    if (loop)
                    {
                        currentFrame = 0;
                    }
                    else
                    {
                        currentFrame = frames.Count - 1;
                        break;
                    }
                }

                targetImage.sprite = frames[currentFrame];
            }
        }
    }
}
