using Domain.Model.Item;

public class Repair : IItemEffect
{
    public void Apply(IItem item)
    {
        item.Repair();
    }
    public string Info() => "修理";
}