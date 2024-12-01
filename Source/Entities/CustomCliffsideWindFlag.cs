using System;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

[CustomEntity("FemtoHelper/CustomCliffsideWindFlag")]
public class CustomCliffsideWindFlag : Entity
{
	private class Segment
	{
		public MTexture Texture;

		public Vector2 Offset;
	}

	private readonly Segment[] segments;

	private float sine;

	private readonly float random;

	private int sign;

	private readonly float sineFrequency;

	private readonly float sineAmplitude;

	private readonly float naturalDraft;

	private float LevelWind => Calc.ClampedMap(Math.Abs((Scene as Level).Wind.X), 0f, 800f);

	private float NaturalWind => Calc.ClampedMap(Math.Abs(naturalDraft), 0f, 800f);

	public CustomCliffsideWindFlag(EntityData data, Vector2 offset) : base(data.Position + offset)
	{
		naturalDraft = data.Float("naturalDraft", 0f);
		sineFrequency = (float)Math.Max(data.Float("sineFrequency", 1f), 0.0001);
		sineAmplitude = data.Float("sineAmplitude", 1f);
		MTexture atlasSubtexturesAt = GFX.Game.GetAtlasSubtexturesAt(data.Attr("spritesPath", "scenery/cliffside/flag"), data.Int("index"));
		segments = new Segment[atlasSubtexturesAt.Width];
		for (int i = 0; i < segments.Length; i++)
		{
			Segment segment = new Segment
			{
				Texture = atlasSubtexturesAt.GetSubtexture(i, 0, 1, atlasSubtexturesAt.Height),
				Offset = new Vector2(i, 0f)
			};
			segments[i] = segment;
		}
		sine = Calc.Random.NextFloat((float)Math.PI * 2f);
		random = Calc.Random.NextFloat();
		Depth = 8999;
		Tag = Tags.TransitionUpdate;
	}

	public override void Added(Scene scene)
	{
		base.Added(scene);
		sign = 1;
		float windValue = 0f;
		if (LevelWind != 0f)
		{
			windValue = LevelWind;
			sign = Math.Sign(SceneAs<Level>().Wind.X);
		}
		else if (naturalDraft != 0f)
		{
			windValue = NaturalWind;
			sign = Math.Sign(naturalDraft);
		}
		for (int i = 0; i < segments.Length; i++)
		{
			SetFlagSegmentPosition(i, windValue, snap: true);
		}
	}

	public override void Update()
	{
		base.Update();
		float windValue = 0f;
		if (LevelWind != 0f)
		{
			windValue = LevelWind;
			sign = Math.Sign(SceneAs<Level>().Wind.X);
		}
		else if (naturalDraft != 0f)
		{
			windValue = NaturalWind;
			sign = Math.Sign(naturalDraft);
		}
		sine += Engine.DeltaTime * (4f + windValue * 4f) * (0.8f + random * 0.2f);
		for (int i = 0; i < segments.Length; i++)
		{
			SetFlagSegmentPosition(i, windValue, snap: false);
		}
	}

	private float Sin(float timer)
	{
		return (float)Math.Sin(0f - (timer / sineFrequency)) * sineAmplitude;
	}

	private void SetFlagSegmentPosition(int i, float windValue, bool snap)
	{
		Segment segment = segments[i];
		float value = i * sign * (0.2f + windValue * 0.8f * (0.8f + random * 0.2f)) * (0.9f + Sin(sine) * 0.1f);
		float num = Calc.LerpClamp(Sin(sine * 0.5f - i * 0.1f) * (i / (float)segments.Length) * i * 0.2f, value, (float)Math.Ceiling(windValue));
		float num2 = i / (float)segments.Length * Math.Max(0.1f, 1f - windValue) * 16f;
		if (!snap)
		{
			segment.Offset.X = Calc.Approach(segment.Offset.X, num, Engine.DeltaTime * 40f);
			segment.Offset.Y = Calc.Approach(segment.Offset.Y, num2, Engine.DeltaTime * 40f);
		}
		else
		{
			segment.Offset.X = num;
			segment.Offset.Y = num2;
		}
	}

	public override void Render()
	{
		base.Render();
		for (int i = 0; i < segments.Length; i++)
		{
			Segment segment = segments[i];
			float scaleFactor = i / (float)segments.Length * Sin(-i * 0.1f + sine) * 2f;
			segment.Texture.Draw(Position + segment.Offset + Vector2.UnitY * scaleFactor);
		}
	}
}