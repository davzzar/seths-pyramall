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
        private Dictionary<string, Animation> animes;
        private string currentAnimeKey = null;
        private string loadFromContentPath;
        private float passedTime = 0f;

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
            this.passedTime += Time.DeltaTime * 1000 ;//TODO milliseconds
            Debug.Print(passedTime + "," + CurrentAnime.CurrentFrame.Duration);
            if(passedTime >= CurrentAnime.CurrentFrame.Duration)
            {
                passedTime = 0f;
                CurrentAnime.NextFrame();
                SyncRect();
            }
        }

        public Boolean NextAnime(String animeId)
        {
            if (this.animes.ContainsKey(animeId))
            {
                this.currentAnimeKey = animeId;
                return true;
            }
            return false;
        }

        private void SyncRect()
        {
            // TODO handle exceptions
            var renderer = this.Owner.GetComponent<SpriteRenderer>();
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

        private void LoadFromContentPath()
        {
            if (this.loadFromContentPath == null || !this.Owner.Scene.IsLoaded || !this.IsActiveInHierarchy)
            {
                return;
            }
            // load .tsx file
            //FIXME hard code path
            var tiledS = new TiledTileset($"Content/tiles/{loadFromContentPath}.tsx");
            if (tiledS == null)
            {
                throw new NullReferenceException("Load tiledS Failed");
            }

            // TODO load texture as SpriteRenderer if there is none;
            string texturePath = tiledS.Image.source;
            string textureAssetName = Path.GetFileNameWithoutExtension(tiledS.Image.source);
            Texture2D texture = GameEngine.Instance.Content.Load<Texture2D>(textureAssetName);
            var renderer = this.Owner.AddComponent<SpriteRenderer>();
            renderer.LoadFromContent(textureAssetName);

            // TODO create SpriteRender Compounent

            // foreach tile, check if it is anime
            int animeId = 0;
            foreach (TiledTile tile in tiledS.Tiles)
            {
                if (tile.animation != null)
                {
                    // found a animation
                    // foreach anime calculate list of sourceRect <- (tileId, tile-w&h, texture-w)
                    // and note frame-length for each frame
                    List<Frame> frameList = new List<Frame>();
                    foreach (TiledTileAnimation tiledFrame in tile.animation)
                    {
                        float duration = tiledFrame.duration;
                        int tileId = tiledFrame.tileid;
                        Rectangle sourceRectangle = new Rectangle(
                            tileId * tiledS.TileWidth % texture.Width,
                            tileId * tiledS.TileWidth / texture.Width * tiledS.TileHeight,
                            tiledS.TileWidth,
                            tiledS.TileHeight);
                        // create new frame
                        frameList.Add(new Frame(sourceRectangle, duration));
                    }
                    // FIXME proper animation id
                    animes.Add("anime_" + animeId++ ,new Animation(frameList.ToArray()));
                }

            }
            //FIXME proper default current anime
            this.currentAnimeKey = "anime_0";
            SyncRect();

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

        public Animation(Frame[] frameArray)
        {
            Frames = frameArray;
            FrameItr = 0;
        }

        public void NextFrame()
        {
            FrameItr += 1;
            FrameItr %= Frames.Length;
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
