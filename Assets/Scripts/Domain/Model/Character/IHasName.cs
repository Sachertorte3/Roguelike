namespace Domain.Model.Character
{
    public interface IHasName
    {
        public string GetName(IPlayer player);
        public string GetNameIgnoreVisibility(IPlayer player);
    }
}