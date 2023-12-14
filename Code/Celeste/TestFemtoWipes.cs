// Celeste.TestWipes
using System.Collections;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.FemtoHelper.Wipes
{

	public class TestFemtoWipes : Scene
	{
		private Coroutine coroutine;

		private Color lastColor = Color.White;

		public TestFemtoWipes()
		{
			coroutine = new Coroutine(routine());
		}

		private IEnumerator routine()
		{
			float dur = 3f;
			yield return 1f;
			while (true)
			{
				ScreenWipe.WipeColor = Color.Black;
				new CirclerWipe(this, wipeIn: false).Duration = dur;
				yield return dur;
				lastColor = Color.White;

				ScreenWipe.WipeColor = Calc.HexToColor("ff0034");
				new SquareWipe(this, wipeIn: false).Duration = dur;
				yield return dur;
				lastColor = ScreenWipe.WipeColor;

				ScreenWipe.WipeColor = Calc.HexToColor("0b0960");
				new CliffhangerWipe(this, wipeIn: false).Duration = dur;
				yield return dur;
				lastColor = ScreenWipe.WipeColor;

				ScreenWipe.WipeColor = Calc.HexToColor("39bf00");
				new SineWipe(this, wipeIn: false).Duration = dur;
				yield return dur;
				lastColor = ScreenWipe.WipeColor;
			}
		}

		public override void Update()
		{
			base.Update();
			coroutine.Update();
		}

		public override void Render()
		{
			Draw.SpriteBatch.Begin();
			Draw.Rect(-1f, -1f, 1920f, 1080f, lastColor);
			Draw.SpriteBatch.End();
			base.Render();
		}
	}
}
