namespace SharedKernel.Domain;

public abstract class Entity<TKey> where TKey : struct
{
    public TKey Id { get; set; }
}
