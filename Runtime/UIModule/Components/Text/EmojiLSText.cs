    /*public class LSText : TextMeshProUGUI
    {
        private ListSlice<Emoji.Range> emojis;
        private ListSlice<Emoji.Sprite> sprites;
        private Action releaseEmojiImages;
        public bool IsTextEmpty => string.IsNullOrEmpty(m_text);
        
        public override float preferredWidth
        {
            get
            {
                if(IsTextEmpty) return 0f;
                
                if (emojis is { Count: > 0 })
                {
                    string lastText = m_text;
                    ModifyText();
                    float width = base.preferredWidth;
                    m_text = lastText;
                    return width;
                }
                
                return base.preferredWidth;
            }
        }

        public override float preferredHeight
        {
            get
            {
                if(IsTextEmpty) return 0f;
                
                if (emojis is { Count: > 0 })
                {
                    string lastText = m_text;
                    ModifyText();
                    float height = base.preferredHeight;
                    m_text = lastText;
                    return height;
                }

                return base.preferredHeight;
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
            Init();
            OnPreRenderText += HandleOnPreRenderText;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ReleaseAllEmojiImages();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            releaseEmojiImages = null;
        }

        private void Init()
        {
#if UNITY_EDITOR
            EditorApplication.update -= Init;
#endif
            EmojiImage.InitPool();
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            OnPreRenderText -= HandleOnPreRenderText;
            OnPreRenderText -= HandleOnPreRenderText;
            OnPreRenderText += HandleOnPreRenderText;
            
            if (World.IsEditMode)
            {
                EditorApplication.update += Init;
            }
        }
#endif

        public override void Rebuild(CanvasUpdate update)
        {
            string t = m_text;
            
            if (update == CanvasUpdate.Prelayout)
            {
                ModifyText();
            }
            
            base.Rebuild(update);
            
            m_text = t;
        }

        public override void ForceMeshUpdate(bool ignoreActiveState = false)
        {
            string t = m_text;
            ModifyText();
            base.ForceMeshUpdate(ignoreActiveState);
            m_text = t;
        }

        public override void ClearMesh()
        {
            base.ClearMesh();
            ReleaseAllEmojiImages();
        }

        private void ModifyText()
        {
            if(IsTextEmpty) return;
            emojis = Emoji.ParseEmojis(m_text, out sprites);
            m_text = Emoji.ReplaceWithEmojiRanges(m_text, emojis, "\ue000\u200b", sprites);
        }

        private void HandleOnPreRenderText(TMP_TextInfo textInfo)
        {
            ReleaseAllEmojiImages();
            Color32 clear = new Color32(0, 0, 0, 0);
            int count = Mathf.Min(emojis.Count, sprites.Count);
            
            for (int i = 0; i < count; i++)
            {
                Emoji.Range emoji = emojis[i];
                int charIndexToHide = emoji.adjustedIndex;
                
                if (charIndexToHide < 0 || charIndexToHide >= textInfo.characterCount) return;
                
                TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndexToHide];
                
                if (!charInfo.isVisible) return;
                
                int vertexIndex = charInfo.vertexIndex;
                TMP_MeshInfo meshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];
                
                Color32[] vertexColors = meshInfo.colors32;
                
                for (int x = 0; x < 4; x++)
                {
                    vertexColors[vertexIndex + x] = clear;
                }

                CreateRawImageForChar(charInfo, i);
            }
        }
        
        private void CreateRawImageForChar(TMP_CharacterInfo charInfo, int i)
        {
            EmojiImage rawImage = EmojiImage.Get();
            rawImage.transform.SetParent(transform);
            releaseEmojiImages += () => EmojiImage.Release(rawImage);
            
            rawImage.Sprite = sprites[i];
            Vector3 bottomLeft = charInfo.bottomLeft;
            Vector3 topRight = charInfo.topRight;
            
            RectTransform r = rawImage.rectTransform;
            r.localPosition = Vector3.zero;
            r.sizeDelta = new Vector2(topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);
            
            Vector3 charPosition = (bottomLeft + topRight) / 2;
            Vector2 pivot = rectTransform.pivot;
            r.anchorMin = pivot;
            r.anchorMax = pivot;
            r.anchoredPosition = new Vector2(charPosition.x, charPosition.y);
            
            r.localScale = Vector3.one;
        }

        private void ReleaseAllEmojiImages()
        {
            releaseEmojiImages?.Invoke();
            releaseEmojiImages = null;
        }
    }*/