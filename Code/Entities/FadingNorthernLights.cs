// Celeste.NorthernLights
using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
public class FadingNorthernLights : Backdrop
{
	private class Strand
	{
		public List<Node> Nodes = new List<Node>();

		public float Duration;

		public float Percent;

		public float Alpha;

		public Strand()
		{
			Reset(Calc.Random.NextFloat());
		}

		public void Reset(float startPercent)
		{
			Percent = startPercent;
			Duration = Calc.Random.Range(6f, 16f);
			Alpha = 0f;
			Nodes.Clear();
			Vector2 position = new Vector2(Calc.Random.Range(-40, 40), Calc.Random.Range(0, 180));
			float num = Calc.Random.NextFloat();
			Color value = Calc.Random.Choose(colors);
			for (int i = 0; i < 40; i++)
			{
				Node item = new Node
				{
					Position = position,
					TextureOffset = num,
					Height = Calc.Random.Range(10, 180),
					TopAlpha = Calc.Random.Range(0.1f, 0.8f),
					BottomAlpha = Calc.Random.Range(0.3f, 1f),
					SineOffset = Calc.Random.NextFloat() * ((float)Math.PI * 2f),
					Color = Color.Lerp(value, Calc.Random.Choose(colors), Calc.Random.Range(0f, 0.4f))
				};
				num += Calc.Random.Range(0.02f, 0.2f);
				position += new Vector2(Calc.Random.Range(4, 20), Calc.Random.Range(-15, 15));
				Nodes.Add(item);
			}
		}
	}

	private class Node
	{
		public Vector2 Position;

		public float TextureOffset;

		public float Height;

		public float TopAlpha;

		public float BottomAlpha;

		public float SineOffset;

		public Color Color;
	}

	private struct Particle
	{
		public Vector2 Position;

		public float Speed;

		public Color Color;
	}

	private static readonly Color[] colors = new Color[4]
	{
		Calc.HexToColor("4abd89"),
		Calc.HexToColor("5abdab"),
		Calc.HexToColor("43b3c8"),
		Calc.HexToColor("9259c8")
	};

	private List<Strand> strands = new List<Strand>();

	private Particle[] particles = new Particle[50];

	private VertexPositionColorTexture[] verts = new VertexPositionColorTexture[1024];

	private VertexPositionColor[] gradient = new VertexPositionColor[6];

	private VirtualRenderTarget buffer;

	private float timer;

	public float OffsetY;

	public float NorthernLightsAlpha = 1f;

	public FadingNorthernLights()
	{
		for (int i = 0; i < 3; i++)
		{
			strands.Add(new Strand());
		}
		for (int j = 0; j < particles.Length; j++)
		{
			particles[j].Position = new Vector2(Calc.Random.Range(0, 320), Calc.Random.Range(0, 180));
			particles[j].Speed = Calc.Random.Range(1, 22);
			particles[j].Color = Calc.Random.Choose(colors);
		}
		Color color = Calc.HexToColor("0c2f2a");
		Color color2 = Calc.HexToColor("070e12");
		gradient[0] = new VertexPositionColor(new Vector3(0f, 0f, 0f), color);
		gradient[1] = new VertexPositionColor(new Vector3(320f, 0f, 0f), color);
		gradient[2] = new VertexPositionColor(new Vector3(320f, 180f, 0f), color2);
		gradient[3] = new VertexPositionColor(new Vector3(0f, 0f, 0f), color);
		gradient[4] = new VertexPositionColor(new Vector3(320f, 180f, 0f), color2);
		gradient[5] = new VertexPositionColor(new Vector3(0f, 180f, 0f), color2);
	}

	public override void Update(Scene scene)
	{
		if (Visible)
		{
			timer += Engine.DeltaTime;
			foreach (Strand strand in strands)
			{
				strand.Percent += Engine.DeltaTime / strand.Duration;
				strand.Alpha = Calc.Approach(strand.Alpha, (strand.Percent < 1f) ? 1 : 0, Engine.DeltaTime);
				if (strand.Alpha <= 0f && strand.Percent >= 1f)
				{
					strand.Reset(0f);
				}
				foreach (Node node in strand.Nodes)
				{
					node.SineOffset += Engine.DeltaTime;
				}
			}
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Position.Y += particles[i].Speed * Engine.DeltaTime;
			}
		}
		base.Update(scene);
	}

	public override void BeforeRender(Scene scene)
	{
		if (buffer == null)
		{
			buffer = VirtualContent.CreateRenderTarget("northern-lights", 320, 180);
		}
		int vert = 0;
		foreach (Strand strand in strands)
		{
			Node node = strand.Nodes[0];
			for (int i = 1; i < strand.Nodes.Count; i++)
			{
				Node node2 = strand.Nodes[i];
				float num = Math.Min(1f, (float)i / 4f) * NorthernLightsAlpha;
				float num2 = Math.Min(1f, (float)(strand.Nodes.Count - i) / 4f) * NorthernLightsAlpha;
				float num3 = OffsetY + (float)Math.Sin(node.SineOffset) * 3f;
				float num4 = OffsetY + (float)Math.Sin(node2.SineOffset) * 3f;
				Set(ref vert, node.Position.X, node.Position.Y + num3, node.TextureOffset, 1f, node.Color * (node.BottomAlpha * strand.Alpha * num));
				Set(ref vert, node.Position.X, node.Position.Y - node.Height + num3, node.TextureOffset, 0.05f, node.Color * (node.TopAlpha * strand.Alpha * num));
				Set(ref vert, node2.Position.X, node2.Position.Y - node2.Height + num4, node2.TextureOffset, 0.05f, node2.Color * (node2.TopAlpha * strand.Alpha * num2));
				Set(ref vert, node.Position.X, node.Position.Y + num3, node.TextureOffset, 1f, node.Color * (node.BottomAlpha * strand.Alpha * num));
				Set(ref vert, node2.Position.X, node2.Position.Y - node2.Height + num4, node2.TextureOffset, 0.05f, node2.Color * (node2.TopAlpha * strand.Alpha * num2));
				Set(ref vert, node2.Position.X, node2.Position.Y + num4, node2.TextureOffset, 1f, node2.Color * (node2.BottomAlpha * strand.Alpha * num2));
				node = node2;
			}
		}
		Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
		GFX.DrawVertices(Matrix.Identity, gradient, gradient.Length);
		Engine.Graphics.GraphicsDevice.Textures[0] = GFX.Misc["northernlights"].Texture.Texture_Safe;
		Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
		GFX.DrawVertices(Matrix.Identity, verts, vert, GFX.FxTexture);
		bool clear = false;
		GaussianBlur.Blur((RenderTarget2D)buffer, GameplayBuffers.TempA, buffer, 0f, clear, GaussianBlur.Samples.Five, 0.25f, GaussianBlur.Direction.Vertical);
		Draw.SpriteBatch.Begin();
		Camera camera = (scene as Level).Camera;
		for (int j = 0; j < particles.Length; j++)
		{
			Vector2 position = default(Vector2);
			position.X = mod(particles[j].Position.X - camera.X * 0.2f, 320f);
			position.Y = mod(particles[j].Position.Y - camera.Y * 0.2f, 180f);
			Draw.Rect(position, 1f, 1f, particles[j].Color);
		}
		Draw.SpriteBatch.End();
	}

	public override void Ended(Scene scene)
	{
		if (buffer != null)
		{
			buffer.Dispose();
		}
		buffer = null;
		base.Ended(scene);
	}

	private void Set(ref int vert, float px, float py, float tx, float ty, Color color)
	{
		verts[vert].Color = color;
		verts[vert].Position.X = px;
		verts[vert].Position.Y = py;
		verts[vert].TextureCoordinate.X = tx;
		verts[vert].TextureCoordinate.Y = ty;
		vert++;
	}

	public override void Render(Scene scene)
	{
		Draw.SpriteBatch.Draw(buffer, Vector2.Zero, Color.White * FadeAlphaMultiplier);
	}

	private float mod(float x, float m)
	{
		return (x % m + m) % m;
	}
}
