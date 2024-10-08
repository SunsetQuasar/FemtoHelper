﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Monocle;
using System.Collections;
using System.Reflection;
using Celeste.Mod.FemtoHelper.Utils;

namespace Celeste.Mod.FemtoHelper.Entities
{
    [Tracked]
    [CustomEntity("FemtoHelper/SMWFish")]
    public class TrollFish : Entity
    {
        public Vector2 Speed;
        public float gravity;
        public string flag;

        public Collider bonkbox;

        public bool blurp;

        public bool big;

        public bool neededflagplus => (Scene as Level).FancyCheckFlag(flag);

        public bool dead;

        public float deadspin;

        public MTexture texture;
        public MTexture textureslice;
        public float texturetimer;
        public int textureframes;

        public string audioPath;

        public bool faceRight;

        public TrollFish(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = data.Int("depth", -120000);
            Speed = new Vector2(data.Float("initialSpeedX", 0), data.Float("initialSpeedY", 0));
            gravity = data.Float("gravity", 260);
            flag = data.Attr("activationFlag", "fish_flag");
            blurp = data.Bool("blurp", false);
            big = data.Bool("big", false);
            faceRight = data.Bool("faceRight", false);

            texture = blurp ? GFX.Game[data.Attr("path", "objects/FemtoHelper/SMWFish/normal/") + "blurp"] : GFX.Game[data.Attr("path", "objects/FemtoHelper/SMWFish/normal/") + "cheep"];

            textureslice = texture.GetSubtexture(0, 0, texture.Height, texture.Height);

            textureframes = texture.Width / texture.Height;

            Collider = new Circle(big ? 16 : 8, 0, 0);
            if (!blurp) bonkbox = new Hitbox(big ? 32 : 16, (big ? 8 : 4), (big ? -16 : -8), (big ? -16 : -8));

            audioPath = data.Attr("audioPath", "event:/FemtoHelper/");

            if (!blurp) Add(new PlayerCollider(bonk, bonkbox));
            if (!data.Bool("harmless", false)) Add(new PlayerCollider(someoneGotTrolled, Collider));
        }

        private void someoneGotTrolled(Player player)
        {
            player.Die(Vector2.Normalize(player.Position - Position), false, true);
            if (SaveData.Instance.Assists.Invincible)
            {
                dead = true;
                Speed = (Vector2.Normalize(Position - player.Position) * 63.24f);
                gravity = 100;
                Collidable = false;
                Audio.Play(audioPath + "enemykill");
            }

        }

        private void bonk(Player player)
        {
            Audio.Play(audioPath + "enemykill");
            Celeste.Freeze(0.1f);
            player.Bounce(base.Top + 2f);
            dead = true;
            Speed = (Vector2.Normalize(Position - player.Position) * 63.24f);
            gravity = 180;
            Collidable = false;
        }

        public override void Update()
        {
            base.Update();

            if (X > (Scene as Level).Bounds.Right + 128 || X < (Scene as Level).Bounds.Left - 128 || Y > (Scene as Level).Bounds.Bottom + 128 || Y < (Scene as Level).Bounds.Top - 128)
            {
                RemoveSelf();
            }

            if (!neededflagplus)
            {
                Collidable = Visible = false;
            }
            else
            {

                if (!Collidable && !dead) Collidable = true;
                if (!Visible) Visible = true;
                Position += Speed * Engine.DeltaTime;
                Speed.Y += gravity * Engine.DeltaTime;

                if (dead)
                {
                    deadspin += Engine.DeltaTime * 16;
                }
                else
                {
                    texturetimer += Engine.DeltaTime * 7;
                }
            }
        }

        public override void Render()
        {
            base.Render();
            textureslice = texture.GetSubtexture(((int)Math.Floor(texturetimer) % textureframes) * texture.Height, 0, texture.Height, texture.Height);

            Vector2 size = faceRight ? new Vector2(-1, 1) : Vector2.One;
            if (big) size *= 2;

            textureslice.DrawCentered(Position, dead ? Calc.HexToColor("CCCCCC") : Color.White, size, deadspin);
        }
    }
}
