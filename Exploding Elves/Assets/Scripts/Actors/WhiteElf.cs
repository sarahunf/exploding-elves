using Actors.Enum;
namespace Actors
{
    public class WhiteElf : Elf
    {
        private void Awake()
        {
            entityType = EntityType.WhiteElf;
        }
    }
}