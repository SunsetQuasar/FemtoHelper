// Celeste.CS03_Memo
using System;
using System.Collections;
using System.IO;
using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

public class HotelBreakoutMemo(Player player, int totind, float txtoffset, float txtsize)
	: CutsceneEntity
{
	private class MemoPage : Entity
	{
		public float TextScale = 0.75f;

		public float PaperScale = 1.5f;

		public float TextOffsetOnPaper = 0;

		private readonly Atlas atlas;

		private readonly MTexture paper;

		private readonly MTexture title;

		private VirtualRenderTarget target;

		private readonly FancyText.Text text;

		private readonly float textDownscale = 1f;

		private float alpha = 1f;

		private readonly float scale = 1f;

		private float rotation;

		private float timer;

		private bool easingOut;

		public int TotalIds;

		public MemoPage()
		{
			Tag = Tags.HUD;
			atlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Memo"), Atlas.AtlasDataFormat.Packer);
			paper = GFX.Gui["alicequasar/nyansword/memo"];
			if (atlas.Has("title_" + Settings.Instance.Language))
			{
				title = GFX.Gui["alicequasar/nyansword/title_" + Settings.Instance.Language];
			}
			else
			{
				title = GFX.Gui["alicequasar/nyansword/title_english"];
			}
			float num = paper.Width * 1.5f - 120f;
			text = FancyText.Parse(Dialog.Get("alicequasar_nyansword_memo"), (int)(num / TextScale), -1, 1f, Color.Black * 0.6f);
			float num2 = text.WidestLine() * TextScale;
			if (num2 > num)
			{
				textDownscale = num / num2;
			}
			Add(new BeforeRenderHook(BeforeRender));
		}

		public IEnumerator EaseIn()
		{
			Audio.Play("event:/game/03_resort/memo_in");
			Vector2 from = new Vector2(Engine.Width / 2, Engine.Height + 100);
			Vector2 to = new Vector2(Engine.Width / 2, Engine.Height / 2 - 150);
			float rFrom = -1f;
			float rTo = 0.05f;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime)
			{
				Position = from + (to - from) * Ease.CubeOut(p);
				alpha = Ease.CubeOut(p);
				rotation = rFrom + (rTo - rFrom) * Ease.CubeOut(p);
				yield return null;
			}
		}

		public IEnumerator Wait()
		{
			float start = Position.Y;
			int index = 0;
			while (!Input.MenuCancel.Pressed)
			{
				float num = start - index * (1200 / TotalIds);
				Position.Y += (num - Position.Y) * (1f - (float)Math.Pow(0.0099999997764825821, Engine.DeltaTime));
				if (Input.MenuUp.Pressed && index > 0)
				{
					index--;
				}
				else if (index < TotalIds - 1)
				{
					if ((Input.MenuDown.Pressed && !Input.MenuDown.Repeating) || Input.MenuConfirm.Pressed)
					{
						index++;
					}
				}
				else if (Input.MenuConfirm.Pressed)
				{
					break;
				}
				yield return null;
			}
			Audio.Play("event:/ui/main/button_lowkey");
		}

		public IEnumerator EaseOut()
		{
			Audio.Play("event:/game/03_resort/memo_out");
			easingOut = true;
			Vector2 from = Position;
			Vector2 to = new Vector2(Engine.Width / 2, -target.Height);
			float rFrom = rotation;
			float rTo = rotation + 0.5f;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime * 1.5f)
			{
				Position = from + (to - from) * Ease.CubeIn(p);
				alpha = 1f - Ease.CubeIn(p);
				rotation = rFrom + (rTo - rFrom) * Ease.CubeIn(p);
				yield return null;
			}
			RemoveSelf();
		}

		public void BeforeRender()
		{
			if (target == null)
			{
				target = VirtualContent.CreateRenderTarget("oshiro-memo", (int)(paper.Width * 1.5f), (int)(paper.Height * 1.5f));
			}
			Engine.Graphics.GraphicsDevice.SetRenderTarget(target);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
			paper.Draw(Vector2.Zero, Vector2.Zero, Color.White, 1.5f);
			title.Draw(Vector2.Zero, Vector2.Zero, Color.White, 1.5f);
			text.Draw(new Vector2(paper.Width * 1.5f / 2f, 210f + TextOffsetOnPaper), new Vector2(0.5f, 0f), Vector2.One * TextScale * textDownscale, 1f);
			Draw.SpriteBatch.End();
		}

		public override void Removed(Scene scene)
		{
			if (target != null)
			{
				target.Dispose();
			}
			target = null;
			atlas.Dispose();
			base.Removed(scene);
		}

		public override void SceneEnd(Scene scene)
		{
			if (target != null)
			{
				target.Dispose();
			}
			target = null;
			atlas.Dispose();
			base.SceneEnd(scene);
		}

		public override void Update()
		{
			timer += Engine.DeltaTime;
			base.Update();
		}

		public override void Render()
		{
			Level level = Scene as Level;
			if ((level == null || (!level.FrozenOrPaused && level.RetryPlayerCorpse == null && !level.SkippingCutscene)) && target != null)
			{
				Draw.SpriteBatch.Draw((RenderTarget2D)target, Position, target.Bounds, Color.White * alpha, rotation, new Vector2(target.Width, 0f) / 2f, scale, SpriteEffects.None, 0f);
				if (!easingOut)
				{
					GFX.Gui["textboxbutton"].DrawCentered(Position + new Vector2(target.Width / 2 + 40, target.Height + (timer % 1f < 0.25f ? 6 : 0)));
				}
			}
		}
	}

	private const string ReadOnceFlag = "memo_read";

	private MemoPage memo;

	public override void OnBegin(Level level)
	{
		Add(new Coroutine(Routine()));
	}

	private IEnumerator Routine()
	{
		player.StateMachine.State = 11;
		player.StateMachine.Locked = true;
		if (!Level.Session.GetFlag("memo_read"))
		{
			yield return Textbox.Say("alicequasar_nyansword_memo_opening");
			yield return 0.1f;
		}
		memo = new MemoPage();
		memo.TotalIds = totind;
		memo.TextScale = txtsize;
		memo.TextOffsetOnPaper = txtoffset;
		if (totind == 0)
        {
			Scene.Add(new MiniTextbox("FEMTOHELPER_ERRORHANDLER_INDEX_ZERO"));
		} 
		else
		{
			Scene.Add(memo);
			yield return memo.EaseIn();
			yield return memo.Wait();
			yield return memo.EaseOut();
			memo = null;
		}
		if (!Level.Session.GetFlag("memo_read"))
		{
			yield return Textbox.Say("alicequasar_nyansword_memo_ending");
			yield return 0.1f;
		}
		EndCutscene(Level);
	}

	public override void OnEnd(Level level)
	{
		player.StateMachine.Locked = false;
		player.StateMachine.State = 0;
		level.Session.SetFlag("memo_read");
		if (memo != null)
		{
			memo.RemoveSelf();
		}
	}
}
