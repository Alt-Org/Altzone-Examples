namespace Altzone.Scripts.Model
{
    public class ClanModel : AbstractModel
    {
        public readonly string Name;

        public ClanModel(int id, string name) : base(id)
        {
            Name = name;
        }
    }
}