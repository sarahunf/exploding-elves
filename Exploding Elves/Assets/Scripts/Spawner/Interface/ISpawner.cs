using Actors.Enum;

namespace Spawner.Interface
{
    public interface ISpawner
    {
        void SetSpawnInterval(float interval);
        void SpawnEntity();
        void OnEntityDestroyed();
    }
}