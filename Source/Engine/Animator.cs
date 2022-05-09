using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using TiledCS;
using System.Diagnostics;
namespace Engine
{
    public class Animator : Behaviour
    {
        // paths
        private string loadFromContentPath;
        private string textureAssetName = null;
        // contents
        private Dictionary<string, Animation> animes;
        private string currentAnimeKey = null;
        private string entryAnimeKey = null;
        private float passedTime = 0f;
        private SpriteRenderer renderer;

        public Animation CurrentAnime
        {
            get
            {
                // TODO handle
                if (currentAnimeKey == null) {
                    return null;
                }
                return animes[currentAnimeKey];
            }
        }
        protected override void OnEnable()
        {
            animes = new Dictionary<string, Animation>();
            this.LoadFromContentPath();

        }

        protected override void Update()
        {
            base.Update();
            this.passedTime += Time.DeltaTime;
            if(passedTime >= CurrentAnime.CurrentFrame.Duration)
            {
                passedTime = 0f;
                CurrentAnime.NextFrame();
                SyncRect();
            }
        }
        public void Entry()
        {
            NextAnime(this.entryAnimeKey);
        }

        public void NextAnime(string animName)
        {
            if (this.CurrentAnime != null)
            {
                this.CurrentAnime.Reset();
            }

            if (this.animes.ContainsKey(animName))
            {
                this.currentAnimeKey = animName;
            }
            else
            {
                throw new ArgumentException("Invalid AnimName!");
            }
        }

        private void SyncRect()
        {
            // TODO handle exceptions
            renderer.SourceRect = CurrentAnime.CurrentFrame.SourceRect;

        }

        public void LoadFromContent(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }
            this.loadFromContentPath = path;
            this.LoadFromContentPath();
        }

        public void LoadFromContent(string path, string texture)
        {
            this.textureAssetName = texture;
            LoadFromContent(path);
        }

        private void ClearContent()
        {
            // clear existing content
            animes = new Dictionary<string, Animation>();
            currentAnimeKey = null;
            entryAnimeKey = null;
            passedTime = 0f;
            if (renderer != null)
            {
                renderer.Destroy();
                renderer = null;
            }
        }

        private void LoadFromContentPath()
        {
            if (this.loadFromContentPath == null || !this.Owner.Scene.IsLoaded || !this.IsActiveInHierarchy)
            {
                return;
            }

            ClearContent();

            // load .tsx file
            //FIXME hard code path
            var tiledS = new TiledTileset($"Content/tiles/{loadFromContentPath}.tsx");
            if (tiledS == null)
            {
                throw new NullReferenceException("Load tiledS Failed");
            }

            // TODO load texture in SpriteRenderer;
            // TODO better Tiled project structure and accurate paths
            if (textureAssetName == null)
            {
                textureAssetName = Path.GetFileNameWithoutExtension(tiledS.Image.source);
            }
            var texture = GameEngine.Instance.Content.Load<Texture2D>(textureAssetName);
            this.renderer =  this.Owner.AddComponent<SpriteRenderer>();
            this.renderer.LoadFromContent(textureAssetName);

            // foreach tile, check if it is anime
            foreach (TiledTile tile in tiledS.Tiles)
            {
                var newAnim = ParseTiledAnimation(tile, tiledS);
                if (newAnim!=null)
                {
                    animes.Add(newAnim.Name, newAnim);
                }
            }
            if (this.entryAnimeKey == null)
            {
                throw new ArgumentNullException("No entry animation defined!");
            }
            this.currentAnimeKey = this.entryAnimeKey;
            SyncRect();
        }

        private void LoadSingleAnimationFromTile(TiledTile tile, TiledTileset tiledS)
        {
            if (textureAssetName == null)
            {
                textureAssetName = Path.GetFileNameWithoutExtension(tiledS.Image.source);
            }
            this.renderer = this.Owner.AddComponent<SpriteRenderer>();
            this.renderer.LoadFromContent(textureAssetName);
            var newAnim = ParseTiledAnimation(tile, tiledS);
            if (newAnim != null)
            {
                animes.Add(newAnim.Name, newAnim);
            }
            this.entryAnimeKey = newAnim.Name;
            this.currentAnimeKey = newAnim.Name;
            SyncRect();
        }

            private Animation ParseTiledAnimation(TiledTile tile, TiledTileset tiledS)
        {
            if (tile.animation != null)
            {
                // found a animation
                // foreach anime calculate list of sourceRect <- (tileId, tile-w&h, texture-w)
                // and note frame-length for each frame
                int textureWidth = this.renderer.Texture.Width;
                List<Frame> frameList = new List<Frame>();
                foreach (TiledTileAnimation tiledFrame in tile.animation)
                {
                    // Calculate source rectangle of the frame
                    int tileId = tiledFrame.tileid;
                    Rectangle sourceRectangle = new Rectangle(
                        tileId * tiledS.TileWidth % textureWidth,
                        tileId * tiledS.TileWidth / textureWidth * tiledS.TileHeight,
                        tiledS.TileWidth,
                        tiledS.TileHeight);
                    // Calculate duration in seconds
                    float duration = tiledFrame.duration / 1000f;
                    // Create new Frame
                    frameList.Add(new Frame(sourceRectangle, duration));
                }
                var newAnim = new Animation(frameList.ToArray());

                var animIsEntry = false;
                foreach (TiledProperty p in tile.properties)
                {
                    // TODO hard code
                    switch (p.name)
                    {
                        case "AnimName":
                            newAnim.Name = p.value;
                            break;
                        case "AnimIsLoop":
                            // TODO not used
                            newAnim.IsLoop = bool.Parse(p.value);
                            break;
                        case "AnimIsEntry":
                            animIsEntry = bool.Parse(p.value);
                            break;
                    }
                }
                if (newAnim.Name == null)
                {
                    throw new ArgumentNullException(nameof(newAnim.Name));
                }
                if (animIsEntry)
                {
                    if (this.entryAnimeKey == null)
                    {
                        this.entryAnimeKey = newAnim.Name;
                    }
                    else
                    {
                        throw new ArgumentException("Multiple animation entries!");
                    }
                }
                return newAnim;
            }
            return null;
        }
    }

    public class Animation
    {
        // TODO regulate code style
        public Frame[] Frames { get; set; }
        public int FrameItr;
        public Frame CurrentFrame
        {
            get
            {
                return Frames[FrameItr];
            }
        }
        public bool IsLoop;
        public bool IsAtEnd;
        public string Name;

        public Animation(Frame[] frameArray)
        {
            Frames = frameArray;
            FrameItr = 0;
            IsLoop = true;
            IsAtEnd = false;
            Name = null;
        }

        public void NextFrame()
        {
            if(FrameItr < Frames.Length - 1)
            {
                FrameItr += 1;
            }
            else if(IsLoop)
            {
                FrameItr = 0;
            }
            else if(!IsAtEnd)
            {
                IsAtEnd = true;
            }
        }

        public void Reset()
        {
            IsAtEnd = false;
            FrameItr = 0;
        }

    }

    public struct Frame
    {
        public Rectangle SourceRect;
        public float Duration;

        public Frame(Rectangle sourceRectangle, float duration)
        {
            SourceRect = sourceRectangle;
            Duration = duration;
        }
    }
}
