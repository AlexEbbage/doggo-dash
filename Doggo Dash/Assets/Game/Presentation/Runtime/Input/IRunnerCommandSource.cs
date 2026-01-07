using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.Input
{
    public interface IRunnerCommandSource
    {
        event System.Action<RunnerCommandType> OnCommand;
    }
}
