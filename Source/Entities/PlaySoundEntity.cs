
using Celeste;

[CustomEntity("FemtoHelper/PlaySoundEntity")]

public class PlaySoundEntity : Entity
{
    private EntityID id;

	public PlaySoundEntity(Vector2 position, EntityID id)
		: base(position)
	{
		this.id = id;
	}

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
