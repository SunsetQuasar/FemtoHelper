using Celeste.Mod.Entities;
using Celeste.Mod.FemtoHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities
{
    [CustomEntity("FemtoHelper/SimpleText")]
    public class SimpleText : Entity
    {
        public string str;
        public Color color1;
        public Color color2;
        public bool shadow;
        public int spacing;
        public PlutoniumText text;
        public float parallax;
        public SimpleText(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            str = Dialog.Clean(data.Attr("dialogID", "FemtoHelper_PlutoniumText_Example"));
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
        }

        public override void Render()
        {
            base.Render();

            Vector2 position = (base.Scene as Level).Camera.Position;
            Vector2 vector = position + new Vector2(160f, 90f);
            Vector2 position2 = (Position - position + (Position - vector) * (parallax - 1)) + position;

            text.PrintCentered(position2, str, shadow, spacing, color1, color2);

        }
    }
}
