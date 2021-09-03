using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.Common;
using System.Xml.Linq;


namespace MVCAppKonstantin
{
    public class Database
    {
        public static DbCommand comm;
        public static DbConnection connection;
        public static string Count()
        {
            string count;
            connection.Open();           
            DbCommand command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM persons";
            var reader = command.ExecuteScalar();
            count = reader.ToString();
            connection.Close();
            return count;
        }
        
           public Database(IEnumerable<XElement> records)
        {
              string path = @"C:\Home\Data\";
              string filename = path + "family1234.db3";
            if (!System.IO.File.Exists(filename))
            {
                SQLiteConnection.CreateFile(filename);
            }

            DbProviderFactory factory = new SQLiteFactory();
            connection = factory.CreateConnection();
            connection.ConnectionString = "Data Source=" + filename;

            connection.Open();
            foreach(var el in records)
            {
                comm = connection.CreateCommand();
                comm.CommandText = @$"DROP TABLE {el.Name};";
            }
            connection.Close();
        }


        public static string Person(int id)
        {
            Random rnd = new Random();
            int npersons = int.Parse(Count());
            string person = " ";
            connection.Open();
            DbCommand runcommand = connection.CreateCommand();
            DbTransaction transaction = connection.BeginTransaction();
            runcommand.Transaction = transaction;
            
            runcommand.CommandText = "SELECT * FROM persons WHERE id=" + id + ";";
            object[] res = null;
            var reader = runcommand.ExecuteReader();
            int cnt = 0;
            while (reader.Read())
            {
                int ncols = reader.FieldCount;
                res = new object[ncols];
                for (int j = 0; j < ncols; j++) res[j] = reader.GetValue(j);
                cnt += 1;
            }
            if (cnt == 0) { Console.WriteLine($"no solutions. key = {id}"); }
            else if (cnt > 1) { Console.WriteLine("multiple solutions. key = {key} cnt = {cnt}"); }
            person = $"{res[0]} {res[1]} {res[2]}";
            reader.Close();
            transaction.Commit();
            connection.Close();
            return person;
        }

        public static IEnumerable<string> Show()
        {
            return Enumerable.Range(0, int.Parse(Count())).Select(el => Person(el));
        }
        public Database()
        {
            string path = @"C:\Home\Data\";
            string filename = path + "persons.db3";
            if (!System.IO.File.Exists(filename))
            {
                SQLiteConnection.CreateFile(filename);
            }

            DbProviderFactory factory = new SQLiteFactory();
            connection = factory.CreateConnection();
            connection.ConnectionString = "Data Source=" + filename;

            bool toload = false;
            if (toload)
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                connection.Open();
                comm = connection.CreateCommand();
                comm.CommandText = @"DROP TABLE persons;";
                try { comm.ExecuteNonQuery(); }
                catch (Exception ex) { Console.WriteLine($"Warning in DROP section {ex.Message}"); }
                connection.Close();

                connection.Open();
                comm = connection.CreateCommand();
                comm.CommandText =
                @"CREATE TABLE persons (id INTEGER PRIMARY KEY ASC, name TEXT, age INTEGER);";
                try { comm.ExecuteNonQuery(); }
                catch (Exception ex) { Console.WriteLine($"Warning in CREATE TABLE section {ex.Message}"); }
                connection.Close();

                int npersons = 100000;
                connection.Open();
                DbCommand runcommand = connection.CreateCommand();
                DbTransaction transaction = connection.BeginTransaction();
                runcommand.Transaction = transaction;
                for (int i = 0; i < npersons; i += 1)
                {
                    runcommand.CommandText = "INSERT INTO persons VALUES (" + i + ",'" + i + "', 21);";
                    runcommand.ExecuteNonQuery();
                }
                Console.WriteLine();
                transaction.Commit();
                connection.Close();

                // Получение записи по ключу
                Random rnd = new Random();
                sw.Restart();

                connection.Open();
                transaction = connection.BeginTransaction();
                runcommand.Transaction = transaction;
                for (long i = 0; i < 1000; i += 1)
                {
                    //int key = (int)(npersons * 2 / 3);
                    int key = rnd.Next((int)npersons);
                    runcommand.CommandText = "SELECT * FROM persons WHERE id=" + key + ";";
                    object[] res = null;
                    var reader = runcommand.ExecuteReader();
                    int cnt = 0;
                    while (reader.Read())
                    {
                        int ncols = reader.FieldCount;
                        res = new object[ncols];
                        for (int j = 0; j < ncols; j++) res[j] = reader.GetValue(j);
                        cnt += 1;
                    }
                    if (cnt == 0) { Console.WriteLine("no solutions. key = {key}"); }
                    else if (cnt > 1) { Console.WriteLine("multiple solutions. key = {key} cnt = {cnt}"); }
                    //Console.WriteLine($"{key} => {res[0]} {res[1]} {res[2]}");

                    reader.Close();
                }
                transaction.Commit();
                connection.Close();
                sw.Stop();
                Console.WriteLine($"duration {sw.ElapsedMilliseconds}");
                toload = true;
            }
            
        }
    }
}
