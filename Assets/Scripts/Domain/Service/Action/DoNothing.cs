using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Service;

public class DoNothing : IAction
{
    private float score;

    public bool Doable(IActor actor, IMap world)
    {
        return true;
    }

    public async UniTask Do(IActor actor, IMap world, IInput input)
    {
        actor.DoNothing();
    }

    public float Evaluate(IActor actor, IMap world)
    {
        score = 0;
        return score;
    }

    public string Info()
    {
        return "DoNothing";
    }
}