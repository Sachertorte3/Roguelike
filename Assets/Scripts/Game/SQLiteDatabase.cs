#nullable enable
using System.Collections.Generic;
using Tetr4lab.UnityEngine.SQLite;
using UnityEngine;

namespace Game
{
    public class SQLiteDatabase
    {
        private SQLite<SQLiteTable<SQLiteRow>, SQLiteRow> sqlDB;

        public SQLiteDatabase()
        {
            Debug.Log("Database init");
            var initDatabaseQuery = "create database if not exists data";
            sqlDB = new SQLite<SQLiteTable<SQLiteRow>, SQLiteRow>("save.db", initDatabaseQuery, path: "");
            var initQuery = "create table if not exists saves (id integer primary key, text string)";
            sqlDB.ExecuteNonQuery(initQuery);
            var initMapsQuery = "create table if not exists maps (id string primary key, text string)";
            sqlDB.ExecuteNonQuery(initMapsQuery);
            var initSettingsQuery = "create table if not exists settings (key string primary key, value string)";
            sqlDB.ExecuteNonQuery(initSettingsQuery);
            Debug.Log("Database init done");
        }
        public void Save(int id, string saveData)
        {
            var insertQuery = "insert or replace into saves values (:id, :text)";
            var row = new SQLiteRow
            {
                { "id", id },
                { "text", saveData }
            };
            sqlDB.ExecuteNonQuery(insertQuery, row);
        }
        public void SaveMap(string id, string mapData)
        {
            var insertQuery = $"insert or replace into maps values(:id, :text)";
            var row = new SQLiteRow
            {
                { "id", id },
                { "text", mapData }
            };
            sqlDB.ExecuteNonQuery(insertQuery, row);
        }
        public void SaveSettings(Dictionary<string, int> settings)
        {
            foreach (var setting in settings)
            {
                var insertQuery = $"insert or replace into settings values(:key, :value)";
                var row = new SQLiteRow
                {
                    { "key", setting.Key },
                    { "value", setting.Value }
                };
                sqlDB.ExecuteNonQuery(insertQuery, row);
            }
        }
        public string? Load(int id)
        {
            var selectQuery = $"select * from saves where id = :id";
            var row = new SQLiteRow
            {
                { "id", id }
            };
            var dataTable = sqlDB.ExecuteQuery(selectQuery, row);
            return dataTable.Rows.Count > 0 ? dataTable.Rows[0]["text"] as string : null;
        }
        public string? LoadMap(string id)
        {
            var selectQuery = $"select * from maps where id = :id";
            var row = new SQLiteRow
            {
                { "id", id }
            };
            var dataTable = sqlDB.ExecuteQuery(selectQuery, row);
            return dataTable.Rows[0]["text"] as string;
        }
        public Dictionary<string, int> LoadSettings()
        {
            var selectQuery = "select * from settings";
            var dataTable = sqlDB.ExecuteQuery(selectQuery);
            var settings = new Dictionary<string, int>();
            foreach (var row in dataTable.Rows)
            {
                if (row["key"] is string key && row["value"] is int value)
                {
                    settings[key] = value;
                }
            }
            return settings;
        }
        public void ClearSave()
        {
            var deleteQuery = "delete from saves";
            sqlDB.ExecuteNonQuery(deleteQuery);
            var deleteMapsQuery = "delete from maps";
            sqlDB.ExecuteNonQuery(deleteMapsQuery);
        }
    }
}