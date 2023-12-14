using System;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

[CustomEntity("FemtoHelper/BackdropSineWaveController")]

public class BackdropSineWaveController : Entity
{
	public float timer = 0f;
	public string tag;
	public float Xfrequency;
	public float Xamplitude;
	public float Xoffset;
	public float Yfrequency;
	public float Yamplitude;
	public float Yoffset;

	public BackdropSineWaveController(EntityData data, Vector2 offset) : base(data.Position + offset)
	{
		tag = data.Attr("tag", "sinewave");
		Xfrequency = data.Float("XFrequency", 50f);
		Xamplitude = data.Float("XAmplitude", 0.1f);
		Xoffset = data.Float("XOffset", 0f) / 360 * ((float)Math.PI * 2);
		Yfrequency = data.Float("YFrequency", 50f);
		Yamplitude = data.Float("YAmplitude", 0.1f);
		Yoffset = data.Float("YOffset", 0f) / 360 * ((float)Math.PI * 2);
		Xfrequency = Xfrequency < 0 ? Math.Min(Xfrequency, 0.01f) : Math.Max(Xfrequency, 0.01f);
		Yfrequency = Xfrequency < 0 ? Math.Min(Yfrequency, 0.01f) : Math.Max(Yfrequency, 0.01f);
	}
	public override void Update()
	{
		timer++;
		foreach (Backdrop item in SceneAs<Level>().Background.GetEach<Backdrop>(tag))
		{
			item.Position += new Vector2((float)Math.Sin((timer * Engine.DeltaTime / Xfrequency) + Xoffset) * Xamplitude / Xfrequency, (float)Math.Sin((timer * Engine.DeltaTime / Yfrequency) - Yoffset) * Yamplitude / Yfrequency);
		}
	}
}
