using Domain.Model.Character;

namespace Domain.Model.Effect
{
    public interface IHasName
    {
        public string GetName(IPlayer player, bool ignoreVisibility = false);
    }
}