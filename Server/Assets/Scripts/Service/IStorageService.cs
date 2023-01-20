using System.Collections.Generic;
using Model;

namespace Service
{
    /// <summary>
    /// Service for local database operations using SQLite.
    /// </summary>
    public interface IStorageService
    {
        ClanModel GetClan(int id);
        List<ClanModel> GetClans();
        int CreateClan(ClanModel clan);
        int UpdateClan(ClanModel clan);
        bool DeleteClan(int id);
    }
}