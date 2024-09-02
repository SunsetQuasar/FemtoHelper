using Celeste.Mod.Entities;
using Celeste.Mod.FemtoHelper;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities
{
    [CustomEntity("FemtoHelper/SimpleText")]
    public class SimpleText : Entity
    {
        public class Node
        {

        }
        public class Text : Node
        {
            public string text;
            public Text(string str) {
                text = str;
            }
        }
        public class Counter : Node
        {
            public string key;
            public Counter(string k) {
                key = k;
            }
        }
        public class Flag : Node
        {
            public string key;
            public string strIfOn;
            public string strIfOff;
            public Flag(string k, string on, string off)
            {
                key = k;
                strIfOn = on;
                strIfOff = off;
            }
        }
        public List<Node> nodes;
        public Color color1;
        public Color color2;
        public bool shadow;
        public int spacing;
        public PlutoniumText text;
        public float parallax;
        public bool hud;
        public float scale;
        public SimpleText(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            nodes = new List<Node>();

            string[] split_str = Regex.Split(Dialog.Get(data.Attr("dialogID", "FemtoHelper_PlutoniumText_Example")), "(\\s|\\{|\\})");
            string[] split_str2 = new string[split_str.Length];
            int num = 0;
            for (int i = 0; i < split_str.Length; i++)
            {
                if (!string.IsNullOrEmpty(split_str[i]))
                {
                    split_str2[num++] = split_str[i];
                }
            }

            for(int i = 0; i < split_str2.Length; i++)
            {
                if (split_str2[i] == "{")
                {
                    i++;

                    for (; i < split_str2.Length && split_str2[i] != "}"; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(split_str2[i]))
                        {
                            string[] splitOnceAgain = split_str2[i].Split(';');
                            if (splitOnceAgain.Length == 3)
                            {
                                nodes.Add(new Flag(splitOnceAgain[0], splitOnceAgain[1], splitOnceAgain[2]));
                            } 
                            else
                            {
                                nodes.Add(new Counter(split_str2[i]));
                            }
                        }
                    }
                } 
                else
                {
                    nodes.Add(new Text(split_str2[i]));
                }
            }

            color1 = Calc.HexToColorWithAlpha(data.Attr("mainColor", "ffffffff"));
            color2 = Calc.HexToColorWithAlpha(data.Attr("outlineColor", "000000ff"));
            Depth = data.Int("depth", -100);
            shadow = data.Bool("shadow", false);
            spacing = data.Int("spacing", 7);
            string path = data.Attr("fontPath", "objects/FemtoHelper/PlutoniumText/example");
            string list = data.Attr("charList", " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*()_+-=?'\".,ç");
            Vector2 size = new Vector2(data.Int("fontWidth", 7), data.Int("fontHeight", 7));
            Add(text = new PlutoniumText(path, list, size));
            parallax = data.Float("parallax", 1);
            hud = data.Bool("hud", false);
            scale = data.Float("scale", 1);
            if (hud) Tag |= TagsExt.SubHUD;
        }

        public string ConstructString()
        {
            string result = "";
            foreach (Node n in nodes)
            {
                if(n is Text t)
                {
                    result += t.text;
                }
                else if (n is Counter c)
                {
                    result += SceneAs<Level>().Session.GetCounter(c.key).ToString();
                }
                else if (n is Flag f)
                {
                    result += SceneAs<Level>().Session.GetFlag(f.key) ? f.strIfOn : f.strIfOff;
                }
            }
            return result;
        }

        public override void Render()
        {
            string str2 = ConstructString();
            base.Render();
            if (hud)
            {
                SubHudRenderer.EndRender();

                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGrade.Effect, Matrix.Identity);
            }

            Vector2 position = (Scene as Level).Camera.Position;
            Vector2 vector = position + new Vector2(160f, 90f);
            Vector2 position2 = (Position - position + (Position - vector) * (parallax - 1)) + position;

            float scale2 = scale;

            if (hud)
            {
                position2 -= position;
                position2 *= 6;
                scale2 *= 6;
            }

            text.PrintCentered(position2, str2, shadow, spacing, color1, color2, scale2);

            if (hud)
            {
                SubHudRenderer.EndRender();

                SubHudRenderer.BeginRender();
            }
        }
    }
}
