#nullable enable
using System.Collections.Generic;
using Tetr4lab.UnityEngine.SQLite;
using Unity.Logging;

namespace Game
{
    public class SQLiteDatabase
    {
        private SQLite<SQLiteTable<SQLiteRow>, SQLiteRow> sqlDB;

        public SQLiteDatabase()
        {
            Log.Debug("Database init");
            var initDatabaseQuery = "create database if not exists data";
            sqlDB = new SQLite<SQLiteTable<SQLiteRow>, SQLiteRow>("save.db", initDatabaseQuery, path: "");
            sqlDB.ExecuteNonQuery(
                "create table if not exists saves (id integer primary key, text string, turnWaitTime real)");
            sqlDB.ExecuteNonQuery(
                "create table if not exists maps (id string primary key, text string)");
            sqlDB.ExecuteNonQuery(
                "create table if not exists statistics (id integer primary key, text string)");
            sqlDB.ExecuteNonQuery(
                "create table if not exists settings (key string primary key, value string)");
            Log.Debug("Database init done");
        }
        public void Save(int id, string saveData, float turnWaitTime)
        {
            sqlDB.ExecuteNonQuery(
                "insert or replace into saves values (:id, :text, :turnWaitTime)",
                new SQLiteRow
                {
                    { "id", id },
                    { "text", saveData },
                    { "turnWaitTime", turnWaitTime }
                });
        }
        public void SaveMap(string id, string mapData)
        {
            sqlDB.ExecuteNonQuery(
                "insert or replace into maps values(:id, :text)",
                new SQLiteRow
                {
                    { "id", id },
                    { "text", mapData }
                });
        }
        public void SaveStatistics(int id, string statisticsData)
        {
            sqlDB.ExecuteNonQuery(
                "insert or replace into statistics values(:id, :text)",
                new SQLiteRow
                {
                    { "id", id },
                    { "text", statisticsData }
                });
        }
        public void SaveSettings(Dictionary<string, int> settings)
        {
            foreach (var setting in settings)
            {
                sqlDB.ExecuteNonQuery(
                    "insert or replace into settings values(:key, :value)",
                    new SQLiteRow
                    {
                        { "key", setting.Key },
                        { "value", setting.Value }
                    });
            }
        }
        public bool ExistSave(int id)
        {
            var dataTable = sqlDB.ExecuteQuery(
                "select * from saves where id = :id",
                new SQLiteRow { { "id", id } });
            return dataTable.Rows.Count > 0;
        }
        public (string world, float turnWaitTime) Load(int id)
        {
            var dataTable = sqlDB.ExecuteQuery(
                "select * from saves where id = :id",
                new SQLiteRow
                {
                    { "id", id }
                });
            string world = (string)dataTable.Rows[0]["text"];
            float turnWaitTime = (float)(double)dataTable.Rows[0]["turnWaitTime"];
            return (world, turnWaitTime);
        }
        public string LoadMap(string id)
        {
            var dataTable = sqlDB.ExecuteQuery(
                "select * from maps where id = :id",
                new SQLiteRow
                {
                    { "id", id }
                });
            return dataTable.Rows[0]["text"] as string;
        }
        public string? LoadStatistics(int id)
        {
            var dataTable = sqlDB.ExecuteQuery(
                "select * from statistics where id = :id",
                new SQLiteRow
                {
                    { "id", id }
                });
            return dataTable.Rows[0]["text"] as string;
        }
        public Dictionary<string, int> LoadSettings()
        {
            var dataTable = sqlDB.ExecuteQuery("select * from settings");
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
            sqlDB.ExecuteNonQuery("delete from saves");
            sqlDB.ExecuteNonQuery("delete from maps");
        }
    }
}