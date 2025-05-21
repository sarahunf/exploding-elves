using Actors.Enum;
using Actors.Interface;

namespace Actors.Factory.Interface
{
    public interface IEntityFactory
    {
        IEntity CreateEntity(EntityType entityType);
    }
}