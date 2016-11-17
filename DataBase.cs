using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analizatior
{
    static class DataBase
    {
        static string filename = "base.db";
        static SQLiteConnection m_dbConnection;
        static SQLiteCommand command;

        public static void Init()
        {
            if (!File.Exists(filename))
                SQLiteConnection.CreateFile(filename);

            m_dbConnection = new SQLiteConnection("Data Source=" + filename + ";Version=3;");
            m_dbConnection.Open();
            string sqlCreateTable = "CREATE TABLE IF NOT EXISTS files (path VARCHAR(500) primary key, hash VARCHAR(150), size INT, date VARCHAR(100))";
            command = new SQLiteCommand(sqlCreateTable, m_dbConnection);
            command.ExecuteNonQuery();
        }

        static void DoQuery(string query)
        {
            command = new SQLiteCommand(query, m_dbConnection);
            command.ExecuteNonQuery();
        }

        public static void InsertRecord(EntityFile file)
        {
            string query = "INSERT INTO files VALUES ('" + file.FilePath + "', '" + file.Hash + "', " + file.Size.ToString() +
                ", '" + file.ModificationDate.ToString() + "' )";
            DoQuery(query);
        }

        public static void ClearTable()
        {
            string query = "DELETE from files";
            DoQuery(query);
        }
        
        public static SQLiteDataReader GetDataReader(string path)
        {
            string query =
            "SELECT * from files where SUBSTR(path, 1, " + path.Length + ")='" + path.ToString() + "'";
            DoQuery(query);
            return command.ExecuteReader();
        } 

        public static void Close()
        {
            m_dbConnection.Close();
        }
    }
}
