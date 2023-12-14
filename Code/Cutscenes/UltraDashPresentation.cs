// Celeste.WaveDashPresentation
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Celeste;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

public class UltraDashPresentation : Entity
{
	public Vector2 ScaleInPoint = new Vector2(1920f, 1080f) / 2f;

	public readonly int ScreenWidth = 1920;

	public readonly int ScreenHeight = 1080;

	private float ease;

	private bool loading;

	private float waitingForInputTime;

	private VirtualRenderTarget screenBuffer;

	private VirtualRenderTarget prevPageBuffer;

	private VirtualRenderTarget currPageBuffer;

	private int pageIndex;

	private List<UltraDashPage> pages = new List<UltraDashPage>();

	private float pageEase;

	private bool pageTurning;

	private bool pageUpdating;

	private bool waitingForPageTurn;

	private VertexPositionColorTexture[] verts = new VertexPositionColorTexture[6];

	private EventInstance usingSfx;

	public bool Viewing
	{
		get;
		private set;
	}

	public Atlas Gfx
	{
		get;
		private set;
	}

	public bool ShowInput
	{
		get
		{
			if (!waitingForPageTurn)
			{
				if (CurrPage != null)
				{
					return CurrPage.WaitingForInput;
				}
				return false;
			}
			return true;
		}
	}

	private UltraDashPage PrevPage
	{
		get
		{
			if (pageIndex <= 0)
			{
				return null;
			}
			return pages[pageIndex - 1];
		}
	}

	private UltraDashPage CurrPage
	{
		get
		{
			if (pageIndex >= pages.Count)
			{
				return null;
			}
			return pages[pageIndex];
		}
	}

	public UltraDashPresentation(EventInstance usingSfx = null)
	{
		base.Tag = Tags.HUD;
		Viewing = true;
		loading = true;
		Add(new Coroutine(Routine()));
		this.usingSfx = usingSfx;
		RunThread.Start(LoadingThread, "Wave Dash Presentation Loading", highPriority: true);
	}

	private void LoadingThread()
	{
		Gfx = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "WaveDashing"), Atlas.AtlasDataFormat.Packer);
		loading = false;
	}

	private IEnumerator Routine()
	{
		while (loading)
		{
			yield return null;
		}
		pages.Add(new UltraDashPage00());
		pages.Add(new UltraDashPage01());
		pages.Add(new UltraDashPage02());
		pages.Add(new UltraDashPage03());
		pages.Add(new UltraDashPage04());
		pages.Add(new UltraDashPage05());
		pages.Add(new UltraDashPage06());
		pages.Add(new UltraDashPage07());
		pages.Add(new UltraDashPage08());
		pages.Add(new UltraDashPage09());
		foreach (UltraDashPage page in pages)
		{
			page.Added(this);
		}
		Add(new BeforeRenderHook(BeforeRender));
		while (ease < 1f)
		{
			ease = Calc.Approach(ease, 1f, Engine.DeltaTime * 2f);
			yield return null;
		}
		while (pageIndex < pages.Count)
		{
			pageUpdating = true;
			yield return CurrPage.Routine();
			if (!CurrPage.AutoProgress)
			{
				waitingForPageTurn = true;
				while (!Input.MenuConfirm.Pressed)
				{
					yield return null;
				}
				waitingForPageTurn = false;
				Audio.Play("event:/new_content/game/10_farewell/ppt_mouseclick");
			}
			pageUpdating = false;
			pageIndex++;
			if (pageIndex < pages.Count)
			{
				float num = 0.5f;
				if (CurrPage.Transition == UltraDashPage.Transitions.Rotate3D)
				{
					num = 1.5f;
				}
				else if (CurrPage.Transition == UltraDashPage.Transitions.Blocky)
				{
					num = 1f;
				}
				else if (CurrPage.Transition == UltraDashPage.Transitions.LinesH)
				{
					num = 1f;
				}
				else if (CurrPage.Transition == UltraDashPage.Transitions.LinesV)
				{
					num = 1f;
				}
				pageTurning = true;
				pageEase = 0f;
				Add(new Coroutine(TurnPage(num)));
				yield return num * 0.8f;
			}
		}
		if (usingSfx != null)
		{
			Audio.SetParameter(usingSfx, "end", 1f);
			usingSfx.release();
		}
		Audio.Play("event:/new_content/game/10_farewell/cafe_computer_off");
		while (ease > 0f)
		{
			ease = Calc.Approach(ease, 0f, Engine.DeltaTime * 2f);
			yield return null;
		}
		Viewing = false;
		RemoveSelf();
	}

	private IEnumerator TurnPage(float duration)
	{
		if (CurrPage.Transition != 0 && CurrPage.Transition != UltraDashPage.Transitions.FadeIn)
		{
			if (CurrPage.Transition == UltraDashPage.Transitions.Rotate3D)
			{
				Audio.Play("event:/new_content/game/10_farewell/ppt_cube_transition");
			}
			else if (CurrPage.Transition == UltraDashPage.Transitions.Blocky)
			{
				Audio.Play("event:/new_content/game/10_farewell/ppt_dissolve_transition");
			}
			else if (CurrPage.Transition == UltraDashPage.Transitions.LinesH)
			{
				Audio.Play("event:/AliceQuasar_FOOL_SPACE/ppt_linesh_transition");
			}
			else if (CurrPage.Transition == UltraDashPage.Transitions.LinesV)
			{
				Audio.Play("event:/AliceQuasar_FOOL_SPACE/ppt_linesh_transition");
			}
			else if (CurrPage.Transition == UltraDashPage.Transitions.Spiral)
			{
				Audio.Play("event:/new_content/game/10_farewell/ppt_spinning_transition");
			}
		}
		while (pageEase < 1f)
		{
			pageEase += Engine.DeltaTime / duration;
			yield return null;
		}
		pageTurning = false;
	}

	private void BeforeRender()
	{
		if (loading)
		{
			return;
		}
		if (screenBuffer == null || screenBuffer.IsDisposed)
		{
			screenBuffer = VirtualContent.CreateRenderTarget("WaveDash-Buffer", ScreenWidth, ScreenHeight, depth: true);
		}
		if (prevPageBuffer == null || prevPageBuffer.IsDisposed)
		{
			prevPageBuffer = VirtualContent.CreateRenderTarget("WaveDash-Screen1", ScreenWidth, ScreenHeight);
		}
		if (currPageBuffer == null || currPageBuffer.IsDisposed)
		{
			currPageBuffer = VirtualContent.CreateRenderTarget("WaveDash-Screen2", ScreenWidth, ScreenHeight);
		}
		if (pageTurning && PrevPage != null)
		{
			Engine.Graphics.GraphicsDevice.SetRenderTarget(prevPageBuffer);
			Engine.Graphics.GraphicsDevice.Clear(PrevPage.ClearColor);
			Draw.SpriteBatch.Begin();
			PrevPage.Render();
			Draw.SpriteBatch.End();
		}
		if (CurrPage != null)
		{
			Engine.Graphics.GraphicsDevice.SetRenderTarget(currPageBuffer);
			Engine.Graphics.GraphicsDevice.Clear(CurrPage.ClearColor);
			Draw.SpriteBatch.Begin();
			CurrPage.Render();
			Draw.SpriteBatch.End();
		}
		Engine.Graphics.GraphicsDevice.SetRenderTarget(screenBuffer);
		Engine.Graphics.GraphicsDevice.Clear(Color.Black);
		if (pageTurning)
		{
			if (CurrPage.Transition == UltraDashPage.Transitions.ScaleIn)
			{
				Draw.SpriteBatch.Begin();
				Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
				Vector2 scale = Vector2.One * pageEase;
				Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, ScaleInPoint, currPageBuffer.Bounds, Color.White, 0f, ScaleInPoint, scale, SpriteEffects.None, 0f);
				Draw.SpriteBatch.End();
			}
			else if (CurrPage.Transition == UltraDashPage.Transitions.FadeIn)
			{
				Draw.SpriteBatch.Begin();
				Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
				Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, Vector2.Zero, Color.White * pageEase);
				Draw.SpriteBatch.End();
			}
			else if (CurrPage.Transition == UltraDashPage.Transitions.Rotate3D)
			{
				float num = -(float)Math.PI / 2f * pageEase;
				RenderQuad((RenderTarget2D)prevPageBuffer, pageEase, num);
				RenderQuad((RenderTarget2D)currPageBuffer, pageEase, (float)Math.PI / 2f + num);
			}
			else if (CurrPage.Transition == UltraDashPage.Transitions.Blocky)
			{
				Draw.SpriteBatch.Begin();
				Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
				uint seed = 1u;
				int num2 = ScreenWidth / 60;
				for (int i = 0; i < ScreenWidth; i += num2)
				{
					for (int j = 0; j < ScreenHeight; j += num2)
					{
						if (PseudoRandRange(ref seed, 0f, 1f) <= pageEase)
						{
							Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, new Rectangle(i, j, num2, num2), new Rectangle(i, j, num2, num2), Color.White);
						}
					}
				}
				Draw.SpriteBatch.End();
			}
			else if (CurrPage.Transition == UltraDashPage.Transitions.LinesH)
			{
				Draw.SpriteBatch.Begin();
				Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
				uint seed = 1u;
				int num2 = ScreenWidth / 140;
				for (int i = 0; i < ScreenWidth; i += ScreenWidth)
				{
					for (int j = 0; j < ScreenHeight; j += num2)
					{
						if (PseudoRandRange(ref seed, 0f, 1f) <= pageEase)
						{
							Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, new Rectangle(i, j, ScreenWidth, num2), new Rectangle(i, j, ScreenWidth, num2), Color.White);
						}
					}
				}
				Draw.SpriteBatch.End();
			}
			else if (CurrPage.Transition == UltraDashPage.Transitions.LinesV)
			{
				Draw.SpriteBatch.Begin();
				Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
				uint seed = 1u;
				int num2 = ScreenHeight / 140;
				for (int i = 0; i < ScreenWidth; i += num2)
				{
					for (int j = 0; j < ScreenHeight; j += ScreenHeight)
					{
						if (PseudoRandRange(ref seed, 0f, 1f) <= pageEase)
						{
							Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, new Rectangle(i, j, num2, ScreenHeight), new Rectangle(i, j, num2, ScreenHeight), Color.White);
						}
					}
				}
				Draw.SpriteBatch.End();
			}
			else if (CurrPage.Transition == UltraDashPage.Transitions.Spiral)
			{
				Draw.SpriteBatch.Begin();
				Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
				Vector2 scale2 = Vector2.One * pageEase;
				float rotation = (1f - pageEase) * 12f;
				Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, Celeste.Celeste.TargetCenter, currPageBuffer.Bounds, Color.White, rotation, Celeste.Celeste.TargetCenter, scale2, SpriteEffects.None, 0f);
				Draw.SpriteBatch.End();
			}
		}
		else
		{
			Draw.SpriteBatch.Begin();
			Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, Vector2.Zero, Color.White);
			Draw.SpriteBatch.End();
		}
	}

	private void RenderQuad(Texture texture, float ease, float rotation)
	{
		float num = (float)screenBuffer.Width / (float)screenBuffer.Height;
		float num2 = num;
		float num3 = 1f;
		Vector3 position = new Vector3(0f - num2, num3, 0f);
		Vector3 position2 = new Vector3(num2, num3, 0f);
		Vector3 position3 = new Vector3(num2, 0f - num3, 0f);
		Vector3 position4 = new Vector3(0f - num2, 0f - num3, 0f);
		verts[0].Position = position;
		verts[0].TextureCoordinate = new Vector2(0f, 0f);
		verts[0].Color = Color.White;
		verts[1].Position = position2;
		verts[1].TextureCoordinate = new Vector2(1f, 0f);
		verts[1].Color = Color.White;
		verts[2].Position = position3;
		verts[2].TextureCoordinate = new Vector2(1f, 1f);
		verts[2].Color = Color.White;
		verts[3].Position = position;
		verts[3].TextureCoordinate = new Vector2(0f, 0f);
		verts[3].Color = Color.White;
		verts[4].Position = position3;
		verts[4].TextureCoordinate = new Vector2(1f, 1f);
		verts[4].Color = Color.White;
		verts[5].Position = position4;
		verts[5].TextureCoordinate = new Vector2(0f, 1f);
		verts[5].Color = Color.White;
		float num4 = 4.15f + Calc.YoYo(ease) * 1.7f;
		Matrix value = Matrix.CreateTranslation(0f, 0f, num) * Matrix.CreateRotationY(rotation) * Matrix.CreateTranslation(0f, 0f, 0f - num4) * Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4f, num, 1f, 10f);
		Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
		Engine.Instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
		Engine.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
		Engine.Instance.GraphicsDevice.Textures[0] = texture;
		GFX.FxTexture.Parameters["World"].SetValue(value);
		foreach (EffectPass pass in GFX.FxTexture.CurrentTechnique.Passes)
		{
			pass.Apply();
			Engine.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, verts.Length / 3);
		}
	}

	public override void Update()
	{
		base.Update();
		if (ShowInput)
		{
			waitingForInputTime += Engine.DeltaTime;
		}
		else
		{
			waitingForInputTime = 0f;
		}
		if (!loading && CurrPage != null && pageUpdating)
		{
			CurrPage.Update();
		}
	}

	public override void Render()
	{
		if (!loading && screenBuffer != null && !screenBuffer.IsDisposed)
		{
			float num = (float)ScreenWidth * Ease.CubeOut(Calc.ClampedMap(ease, 0f, 0.5f));
			float num2 = (float)ScreenHeight * Ease.CubeInOut(Calc.ClampedMap(ease, 0.5f, 1f, 0.2f));
			Rectangle rectangle = new Rectangle((int)((1920f - num) / 2f), (int)((1080f - num2) / 2f), (int)num, (int)num2);
			Draw.SpriteBatch.Draw((RenderTarget2D)screenBuffer, rectangle, Color.White);
			if (ShowInput && waitingForInputTime > 0.2f)
			{
				GFX.Gui["textboxbutton"].DrawCentered(new Vector2(1856f, 1016 + ((base.Scene.TimeActive % 1f < 0.25f) ? 6 : 0)), Color.Black);
			}
			if ((base.Scene as Level).Paused)
			{
				Draw.Rect(rectangle, Color.Black * 0.7f);
			}
		}
	}

	public override void Removed(Scene scene)
	{
		base.Removed(scene);
		Dispose();
	}

	public override void SceneEnd(Scene scene)
	{
		base.SceneEnd(scene);
		Dispose();
	}

	private void Dispose()
	{
		while (loading)
		{
			Thread.Sleep(1);
		}
		if (screenBuffer != null)
		{
			screenBuffer.Dispose();
		}
		screenBuffer = null;
		if (prevPageBuffer != null)
		{
			prevPageBuffer.Dispose();
		}
		prevPageBuffer = null;
		if (currPageBuffer != null)
		{
			currPageBuffer.Dispose();
		}
		currPageBuffer = null;
		Gfx.Dispose();
		Gfx = null;
	}

	private static uint PseudoRand(ref uint seed)
	{
		uint num = seed;
		num ^= num << 13;
		num ^= num >> 17;
		return seed = num ^ (num << 5);
	}

	public static float PseudoRandRange(ref uint seed, float min, float max)
	{
		return min + (float)(PseudoRand(ref seed) % 1000u) / 1000f * (max - min);
	}
}
