using SQLite;

namespace Model
{
    public class ClanModel
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}