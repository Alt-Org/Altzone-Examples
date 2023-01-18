using System;
using System.Collections.Generic;
using SQLite;

namespace Model
{
    public class ClanModel
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        [Indexed(Unique = true)]
        public string Name { get; set; }
        
        public DateTime LastUpdate { get; set; }
    }

    [Serializable]
    public class JsonClanModel
    {
        public int Id;
        public string Name;
        public DateTime LastUpdate;

        public JsonClanModel(ClanModel clan)
        {
            Id = clan.Id;
            Name = clan.Name;
            LastUpdate = clan.LastUpdate;
        }

        public static List<JsonClanModel> ConvertAll(List<ClanModel> clans)
        {
            return clans.ConvertAll(x => new JsonClanModel(x));
        }
    }
}