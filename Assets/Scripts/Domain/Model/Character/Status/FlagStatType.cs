using StringSerializableEnum;

namespace Domain.Model.Character.Status
{
    [StringSerializable]
    public enum FlagStatType
    {
        CannotAct,
        CannotMove,
        Confused,
        Clairvoyant,
        Blind,
        OverDrive,
        Hard,
        ExplosionProof,
        Heavy,
        SecureHold,
        CurseProof,
        Haggle,
        IsAffectedByTrap,
        AutoIdentify,
        RandomTeleport,
        RandomExplosion,
    }
}