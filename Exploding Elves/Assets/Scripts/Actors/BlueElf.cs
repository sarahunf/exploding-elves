using Actors.Enum;
namespace Actors
{
    public class BlueElf : Elf
    {
        private void Awake()
        {
            entityType = EntityType.BlueElf;
        }
    }
}