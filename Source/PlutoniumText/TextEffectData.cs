namespace Celeste.Mod.FemtoHelper;

public class TextEffectData
{
    public readonly bool Wavey;
    public Vector2 WaveAmp;
    public readonly float PhaseOffset;
    public readonly float PhaseIncrement;
    public readonly float WaveSpeed;

    public readonly bool Shakey;
    public readonly float ShakeAmount;

    public readonly bool Obfuscated;

    public readonly bool Twitchy;
    public readonly float TwitchChance;

    public readonly bool Rainbow;
    public Vector2 RainbowAnchor;

    public bool Empty => !(Twitchy || Shakey || Obfuscated || Wavey || Rainbow);
    public TextEffectData(bool wavey = false, Vector2 amp = default, float offset = 0, bool shakey = false, float amount = 0, bool obfs = false, bool twitchy = false, float twitchChance = 0, float phaseIncrement = 0, float waveSpeed = 0, bool rainbow = false, Vector2 rainbowAnchor = default)
    {
        Wavey = wavey;
        if (wavey)
        {
            WaveAmp = amp;
            PhaseOffset = offset;
            PhaseIncrement = phaseIncrement;
            WaveSpeed = waveSpeed;
        }

        Shakey = shakey;
        ShakeAmount = amount;

        Obfuscated = obfs;
        Twitchy = twitchy;
        TwitchChance = twitchChance;
        Rainbow = rainbow;
        RainbowAnchor = rainbowAnchor;
    }

    public TextEffectData()
    {
    }
}
