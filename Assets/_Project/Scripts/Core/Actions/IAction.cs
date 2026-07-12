namespace Roguelike.Core
{
    public interface IAction
    {
        ActionResult Perform(LevelData level);
    }
}