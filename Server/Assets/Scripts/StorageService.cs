using System;
using System.Collections;
using System.IO;
using SQLite;
using UnityEngine;

/// <summary>
/// Service for local database operations using SQLite.
/// </summary>
public class StorageService : MonoBehaviour
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
        Debug.Log($"{name} : {_connection.LibVersionNumber} : {_connection.DatabasePath}");
        yield return null;
    }

    private void OnDestroy()
    {
        _connection?.Close();
    }
}