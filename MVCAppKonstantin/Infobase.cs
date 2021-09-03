using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MVCAppKonstantin
{
    public class Infobase
    {
        public static XElement xdb;
        public static void BuildTables()
        {
            Database db = new Database();
            Database.connection.Open();
            Database.comm = Database.connection.CreateCommand();
            Database.comm.CommandText = @"DROP TABLE persons;";
            try { Database.comm.ExecuteNonQuery(); }
            catch (Exception ex) { Console.WriteLine($"Warning in DROP section {ex.Message}"); }
            Database.connection.Close();

            Database.connection.Open();
            Database.comm = Database.connection.CreateCommand();
            Database.comm.CommandText =
            @"CREATE TABLE persons (id INTEGER, name TEXT, father TEXT,todate INTEGER, mother TEXT, sex TEXT, comment TEXT);";
            try { Database.comm.ExecuteNonQuery(); }
            catch (Exception ex) { Console.WriteLine($"Warning in CREATE TABLE section {ex.Message}"); }
            foreach(var xel in xdb.Elements())
            {
                if (xel.Name == "person")
                {
                    Database.comm.CommandText= "INSERT INTO persons VALUES(" + xel.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value + ");";
                    Database.comm.ExecuteNonQuery();
                }
            }
            Database.connection.Close();
        }
    }
}
