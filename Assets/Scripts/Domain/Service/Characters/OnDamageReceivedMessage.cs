namespace Domain.Service.Characters
{
    public record OnHealReceivedMessage(int Heal);

    public record OnDamageReceivedMessage(int Damage);

    public record OnPickUpItemMessage();
}