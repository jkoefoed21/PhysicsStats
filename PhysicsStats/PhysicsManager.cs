using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace PhysicsStats
{
    class PhysicsManager
    {
        public static readonly string DBPath = "C:\\Users\\JK\\OneDrive\\13\\Physics.sqlite";

        private SQLiteConnection sql;

        public static readonly DateTime epoch = new DateTime(2017, 9, 13); //first game was on day one.

        public PhysicsManager()
        {
            sql = new SQLiteConnection("Data Source = " + DBPath + " ; Version=3");
            sql.Open();
        }

        public static void Main(string[] args)
        {
            PhysicsManager p = new PhysicsManager();
            //p.clearPlayers();
            //p.insertGame(new Game(1,1,2,3,4,1,0,2));
            //Console.WriteLine(p.getMaxGameID());
            /*foreach (Player play in p.listAllPlayers())
            {
                Console.WriteLine(play);
            }*/
            p.clearTables();
            p.insertPlayer(new Player("Koefoed", "Jack"));
            p.insertPlayer(new Player("Koefoed", "Andy"));
            p.insertPlayer(new Player("Amado", ""));
            p.insertPlayer(new Player("Sean", ""));
            Console.WriteLine(p.getPlayerNum(new Player("Sean", null)));

            //p.insertGame(new Game(1, 4, 4, 4, 4, 4, 4, 4));
            //p.insertGame(new Game(1, 2, 2, 2, 2, 2, 2, 2));
            Console.ReadKey();
        }

        public static void readFromTSV(string path)
        {
            String[] f = File.ReadAllLines(path);
            //read Players
            Game[] games = new Game[f.Length - 1];
            for (int ii = 1; ii < f.Length; ii++)
            {
                String[] subs = f[ii].Split('\t');
                DateTime date = DateTime.Parse(subs[0]);
                TimeSpan span = date - epoch;
                int cups = 0;
                int otcups = 0;
                int days = span.Days;
                if (!subs[4].Contains("OT"))
                {
                    cups = Int32.Parse(subs[4]);
                }
                else
                {
                    cups = 0;
                    otcups = Int32.Parse(subs[4].Substring(0, 1));
                }
                for (int jj = 0; jj < subs.Length; jj++)
                {
                    subs[jj] = subs[jj].Trim();
                }
                if (subs[2].Contains("and"))
                {


                }
                else
                {
                    String wp = subs[2];
                    String lp = subs[0];
                    if (wp.ToLower().Equals(lp.ToLower()))
                    {
                        lp = subs[1];
                    }
                    //games[ii] = new Game(ii, days, wp, lp, cups, otcups, subs[5]);
                }
            }
        }
        public static void printToTSV(string path)
        {

        }


        private void clearTables()
        {
            executeInputQuery("DROP TABLE IF EXISTS Players");
            executeInputQuery("DROP TABLE IF EXISTS Games");
            createTables();
        }
        private void createTables()
        {
            executeInputQuery("CREATE TABLE Games ( ID INTEGER PRIMARY KEY, Day int NOT NULL, WinP1 int NOT NULL, WinP2 int, LossP1 int NOT NULL, LossP2 int, Cups int NOT NULL, OvertimeCups int, Notes varchar(1000))");
            executeInputQuery("CREATE TABLE Players (ID INTEGER PRIMARY KEY AUTOINCREMENT, LastName varchar(255) NOT NULL, FirstName varchar(255))");
        }

        public void insertPlayer(Player p)
        {
            executeInputQuery("INSERT INTO players (LastName, FirstName) values (@val1, @val2)", p.Last, p.First);
        }
        /*public void editPlayerName(Player oldPlayer, Player newPlayer)
        {
            //executeOutputQuery("SELECT * FROM players WHERE ");
        }*/
        public Player[] listAllPlayers()
        {
            Object[][] data = getData("SELECT * FROM Players");
            Player[] Players = new Player[data.Length];
            for (int ii = 0; ii < Players.Length; ii++)
            {
                Players[ii] = new Player((String)data[ii][1], (String)data[ii][2]);
            }
            return Players;
        }



        public void insertGame(Game g)
        {
            executeInputQuery("INSERT INTO games (ID, Day, WinP1, WinP2, LossP1, LossP2, Cups, OvertimeCups, Notes) values (@val1, @val2, @val3, @val4, @val5, @val6, @val7, @val8, @val9)",
                              g.GameNumber, g.DaySinceEpoch, g.Winp1, g.Winp2, g.Lossp1, g.Lossp2, g.Cups, g.OTCups, g.Note);
        }

        public int getMaxGameID()
        {
            Object[][] data = getData("SELECT id FROM games ORDER BY id DESC");
            return Convert.ToInt32(data[0][0]);
        }

        /// <summary>
        /// Gets the id of a player. Returns -1 if player is not in DB, throws exception if search is ambigious.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public int getPlayerNum(Player p)
        {
            Object[][] data = getData("SELECT id FROM players WHERE FirstName = @val2 AND LastName = @val1", p.Last, p.First);
            if (data.Length == 0)
            {
                return -1;
            }
            if (data.Length > 1)
            {
                throw new ArgumentException("Two players have the same name.");
            }
            return Convert.ToInt32(data[0][0]);
        }
        public int getPlayerNum(string s)
        {
            return getPlayerNum(getPlayer(s));
        }
        public Player getPlayer(string s)
        {
            if (s.Contains(" "))
            {
                string[] subs = s.Split(' ');
                return new Player(subs[1], subs[0]);
            }
            else
            {
                return new Player(s, "");
            }
        }
        public Player getPlayer(int id)
        {
            Object[][] data = getData("SELECT * FROM players WHERE id=@val1", id);
            return new Player((string) data[0][1], (string) data[0][2]);
        }

        public int playerWinTotal(int playerNum)
        {
            Object[][] data = getData("SELECT * FROM players WHERE WinP1 = @val1 OR WinP2 = @val1", playerNum);
            return data.Length;
        }
        public int playerGameTotal(int playerNum)
        {
            Object[][] data = getData("SELECT * FROM players WHERE WinP1 = @val1 OR WinP2 = @val1 OR LossP1 = @val1 OR LossP2 = @val1", playerNum);
            return data.Length;
        }
        public int playerWinCupTotal(int playerNum)
        {
            Object[][] data = getData("SELECT cups FROM players WHERE WinP1 = @val1 OR WinP2 = @val1", playerNum);
            int total = 0;
            for (int ii = 0; ii < data.Length; ii++)
            {
                total += Convert.ToInt32(data[ii]);
            }
            return total;
        }
        public int playerLossCupTotal(int playerNum)
        {
            Object[][] data = getData("SELECT cups FROM players WHERE LossP1 = @val1 OR LossP2 = @val1", playerNum);
            int total = 0;
            for (int ii=0; ii<data.Length; ii++)
            {
                total += Convert.ToInt32(data[ii]);
            }
            return total;
        }

        public Object[][] getData(string query)
        {
            SQLiteDataReader reader = executeOutputQuery(query);
            Queue<Object[]> data = new Queue<Object[]>();
            //Console.WriteLine(reader.)
            while (reader.Read())
            {
                Object[] row = new Object[reader.FieldCount];
                for (int ii = 0; ii < row.Length; ii++)
                {
                    row[ii] = reader[ii];
                }
                data.Enqueue(row);
            }
            return data.ToArray();
        }
        public Object[][] getData(string query, object val1)
        {
            SQLiteDataReader reader = executeOutputQuery(query, val1);
            Queue<Object[]> data = new Queue<Object[]>();
            //Console.WriteLine(reader.)
            while (reader.Read())
            {
                Object[] row = new Object[reader.FieldCount];
                for (int ii = 0; ii < row.Length; ii++)
                {
                    row[ii] = reader[ii];
                }
                data.Enqueue(row);
            }
            return data.ToArray();
        }
        public Object[][] getData(string query, object val1, object val2)
        {
            SQLiteDataReader reader = executeOutputQuery(query, val1, val2);
            Queue<Object[]> data = new Queue<Object[]>();
            //Console.WriteLine(reader.)
            while (reader.Read())
            {
                Object[] row = new Object[reader.FieldCount];
                for (int ii=0; ii<row.Length; ii++)
                {
                    row[ii] = reader[ii];
                }
                data.Enqueue(row);
            }
            return data.ToArray();
        }

        //SQL manipulation functions
        public void executeInputQuery(string query)
        {
            SQLiteCommand command = new SQLiteCommand(query, sql);
            command.ExecuteNonQuery();
        }
        public void executeInputQuery(string query, object val1)
        {
            SQLiteCommand command = new SQLiteCommand(query, sql);
            command.Parameters.AddWithValue("@val1", val1);
            command.ExecuteNonQuery();
        }
        public void executeInputQuery(string query, object val1, object val2)
        {
            SQLiteCommand command = new SQLiteCommand(query, sql);
            command.Parameters.AddWithValue("@val1", val1);
            command.Parameters.AddWithValue("@val2", val2);
            command.ExecuteNonQuery();
        }
        public void executeInputQuery(string query, object val1, object val2, object val3, object val4, object val5, object val6, object val7, object val8, object val9)
        {
            SQLiteCommand command = new SQLiteCommand(query, sql);
            command.Parameters.AddWithValue("@val1", val1);
            command.Parameters.AddWithValue("@val2", val2);
            command.Parameters.AddWithValue("@val3", val3);
            command.Parameters.AddWithValue("@val4", val4);
            command.Parameters.AddWithValue("@val5", val5);
            command.Parameters.AddWithValue("@val6", val6);
            command.Parameters.AddWithValue("@val7", val7);
            command.Parameters.AddWithValue("@val8", val8);
            command.Parameters.AddWithValue("@val9", val9);
            command.ExecuteNonQuery();
        }
        public SQLiteDataReader executeOutputQuery(string query)
        {
            SQLiteCommand command = new SQLiteCommand(query, sql);
            return command.ExecuteReader();
        }
        public SQLiteDataReader executeOutputQuery(string query, object val1)
        {
            SQLiteCommand command = new SQLiteCommand(query, sql);
            command.Parameters.AddWithValue("@val1", val1);
            return command.ExecuteReader();
        }
        public SQLiteDataReader executeOutputQuery(string query, object val1, object val2)
        {
            SQLiteCommand command = new SQLiteCommand(query, sql);
            command.Parameters.AddWithValue("@val1", val1);
            command.Parameters.AddWithValue("@val2", val2);
            return command.ExecuteReader();
        }
    }
}
