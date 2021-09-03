using System;
using System.Data.SQLite;
using System.Data.Common;


namespace SQLConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start SQLConsole!");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            string path = @"C:/Home/Data";
            string filename = path + "/test.db3";
            if (!System.IO.File.Exists(filename))
            {
                SQLiteConnection.CreateFile(filename);
            }

            DbProviderFactory factory = new SQLiteFactory();
            DbConnection connection = factory.CreateConnection();
            connection.ConnectionString = "Data Source=" + filename;

            connection.Open();
            DbCommand comm = connection.CreateCommand();
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
                Console.WriteLine($"{key} => {res[0]} {res[1]} {res[2]}");

                reader.Close();
            }
            transaction.Commit();
            connection.Close();
            sw.Stop();
            Console.WriteLine($"duration {sw.ElapsedMilliseconds}");



        }
    }
}
