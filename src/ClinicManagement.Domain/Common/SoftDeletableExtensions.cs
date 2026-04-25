namespace ClinicManagement.Domain.Common;

public static class SoftDeletableExtensions
{
    public static void SoftDelete(this ISoftDeletable entity)
        => entity.IsDeleted = true;

    public static void Restore(this ISoftDeletable entity)
        => entity.IsDeleted = false;
}
