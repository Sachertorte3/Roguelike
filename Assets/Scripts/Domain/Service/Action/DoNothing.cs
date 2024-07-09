using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Service;

public class DoNothing : IAction
{
    public bool Doable(IActor actor, IMap world)
    {
        return true;
    }

    public UniTask Do(IActor actor, IMap world, IInput input)
    {
        actor.DoNothing();
        return UniTask.CompletedTask;
    }

    public float Evaluate(IActor actor, IMap world)
    {
        return 0;
    }

    public string Info()
    {
        return "DoNothing";
    }
}