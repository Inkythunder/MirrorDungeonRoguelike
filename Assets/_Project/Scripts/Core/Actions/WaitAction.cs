namespace Roguelike.Core
{
    /// <summary>
    /// Skips a turn.
    /// </summary>
    public sealed class WaitAction : IAction
    {
        public ActionResult Perform(LevelData level) => ActionResult.Waited;
    }
}