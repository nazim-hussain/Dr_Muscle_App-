using DrMuscle.Dependencies;
using DrMuscle.iOS;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

[assembly: Xamarin.Forms.Dependency(typeof(SQLite_iOS))]
namespace DrMuscle.iOS
{
    public class SQLite_iOS : ISQLite
    {
        public SQLite_iOS() { }
        public SQLiteConnection GetConnection()
        {
            var sqliteFilename = "dmmdb.db3";
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
            string libraryPath = Path.Combine(documentsPath, "..", "Library"); // Library folder
            var path = Path.Combine(libraryPath, sqliteFilename);
            // Create the connection
            var conn = new SQLite.SQLiteConnection(path);
            // Return the database connection
            return conn;
        }
    }
}
