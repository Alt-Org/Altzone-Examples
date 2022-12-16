using System;
using SQLite;

namespace Model
{
    public class ClanModel
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    [Serializable]
    public class JsonClanModel
    {
        public int Id;

        public string Name;

        public JsonClanModel(ClanModel clan)
        {
            Id = clan.Id;
            Name = clan.Name;
        }
    }
}