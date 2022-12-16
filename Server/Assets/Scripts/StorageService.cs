using System.Collections;
using System.Collections.Generic;
using System.IO;
using Model;
using SQLite;
using UnityEngine;

public interface IStorageService
{
    ClanModel GetClan(int id);
    List<ClanModel> GetClans();
    int CreateClan(ClanModel clan);
    int UpdateClan(ClanModel clan);
    bool DeleteClan(int id);
}

/// <summary>
/// Service for local database operations using SQLite.
/// </summary>
public class StorageService : MonoBehaviour, IStorageService
{
    private const string DatabaseFilename = "altzone";

    private SQLiteConnection _connection;

    private IEnumerator Start()
    {
        var path = Path.Combine(Application.persistentDataPath, $"{DatabaseFilename}.sqlite");
        if (AppPlatform.IsWindows)
        {
            path = AppPlatform.ConvertToWindowsPath(path);
        }
        _connection = new SQLiteConnection(path);
        Debug.Log($"{name} : SQLite {_connection.LibVersionNumber} : {_connection.DatabasePath}");
        yield return null;
        _connection.CreateTable<ClanModel>();
        Debug.Log($"table {nameof(ClanModel)} count {_connection.Table<ClanModel>().Count()}");
        yield return null;
        Debug.Log("ready");
    }

    public ClanModel GetClan(int id) => _connection.Table<ClanModel>().FirstOrDefault(x => x.Id == id);

    public List<ClanModel> GetClans() => _connection.Table<ClanModel>().ToList();

    public int CreateClan(ClanModel clan) => _connection.Insert(clan);

    public int UpdateClan(ClanModel clan) => _connection.Update(clan);

    public bool DeleteClan(int id) => _connection.Delete<ClanModel>(id) == 1;

    private void OnDestroy()
    {
        _connection?.Close();
    }
}