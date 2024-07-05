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
            ScreenWipe.WipeColor = Color.Black;
        }

        private IEnumerator routine()
		{
			float dur = 1f;
			yield return 1f;
			while (true)
			{

				new CirclerWipe(this, wipeIn: false).Duration = dur;
				yield return dur;
                new CirclerWipe(this, wipeIn: true).Duration = dur;
                yield return dur;

                new SquareWipe(this, wipeIn: false).Duration = dur;
				yield return dur;
                new SquareWipe(this, wipeIn: true).Duration = dur;
                yield return dur;

                new CliffhangerWipe(this, wipeIn: false).Duration = dur;
                yield return dur;
                new CliffhangerWipe(this, wipeIn: true).Duration = dur;
                yield return dur;

                new SineWipe(this, wipeIn: false).Duration = dur;
                yield return dur;
                new SineWipe(this, wipeIn: true).Duration = dur;
                yield return dur;

                new DiamondWipe(this, wipeIn: false).Duration = dur;
                yield return dur;
                new DiamondWipe(this, wipeIn: true).Duration = dur;
                yield return dur;

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
