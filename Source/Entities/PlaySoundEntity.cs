
using Celeste;

[CustomEntity("FemtoHelper/PlaySoundEntity")]

public class PlaySoundEntity(Vector2 position, EntityID id) : Entity(position)
{
	public PlaySoundEntity(EntityData data, Vector2 offset, EntityID id)
		: this(data.Position + offset, id)
	{
	}

	public override void Update()
    {
        Audio.Play("event:/WinterCollabLobby/frontdoor_faraway");
        RemoveAndFlagAsGone();
    }
    public void RemoveAndFlagAsGone()
    {
        RemoveSelf();
        SceneAs<Level>().Session.DoNotLoad.Add(id);
    }
}
