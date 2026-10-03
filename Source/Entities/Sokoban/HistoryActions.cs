namespace Celeste.Mod.FemtoHelper.Entities.Sokoban;

public interface IHistoryAction
{
    public void Do();
    public void Undo();
}

public record struct SokobanMove(SokobanObject Obj, Vector2 From, Vector2 To) : IHistoryAction
{
    public readonly void Do() {
        Obj.Pos = To;
    }
    public readonly void Undo()
    {
        Obj.Pos = From;
    }
}
