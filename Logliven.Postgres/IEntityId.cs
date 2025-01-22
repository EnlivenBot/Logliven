namespace Logliven.Postgres;

public interface IEntityId<TType> {
    TType Id { get; }
}