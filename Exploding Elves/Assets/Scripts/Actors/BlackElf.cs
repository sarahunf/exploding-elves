using Actors.Enum;
namespace Actors
{
    public class BlackElf : Elf
    {
        private void Awake()
        {
            entityType = EntityType.BlackElf;
        }
    }
}