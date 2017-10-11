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
            //p.clearTables();
            //Console.WriteLine(epoch.ToShortDateString());
            Player[] players=p.listAllPlayers();
            for (int ii=0; ii<players.Length; ii++)
            {
                Console.WriteLine(players[ii].ToString() + "  G:" + p.playerGameTotal(p.getPlayerNum(players[ii]))
                                  + "   W:" + p.playerWinTotal(p.getPlayerNum(players[ii]))
                                  + "   %:" + ((double)p.playerWinTotal(p.getPlayerNum(players[ii])) / p.playerGameTotal(p.getPlayerNum(players[ii])))
                                  + "   WC:" + p.playerWinCupTotal(p.getPlayerNum(players[ii]))
                                  + "   LC:" + p.playerLossCupTotal(p.getPlayerNum(players[ii]))
                                  + "   CD:" + (p.playerWinCupTotal(p.getPlayerNum(players[ii])) - p.playerLossCupTotal(p.getPlayerNum(players[ii])))
                                  +"   CD/G:" + ((double)p.playerWinCupTotal(p.getPlayerNum(players[ii])) - p.playerLossCupTotal(p.getPlayerNum(players[ii])))/p.playerGameTotal(p.getPlayerNum(players[ii])));
        }

            //p.readFromTSV("C:\\users\\jk\\documents\\Physics program test zone\\physicstest.txt");
            //p.printToTSV("C:\\users\\jk\\documents\\Physics program test zone\\physicstest2.txt");

            //p.insertGame(new Game(1, 4, 4, 4, 4, 4, 4, 4));
            //p.insertGame(new Game(1, 2, 2, 2, 2, 2, 2, 2));
            Console.ReadKey();
        }

        public void readFromTSV(string path)
        {
            String[] f = File.ReadAllLines(path);
            String[] players = f[0].Split('\t');
            for (int ii = 0; ii < players.Length; ii++)
            {
                players[ii] = players[ii].Trim();
                Player play = getPlayer(players[ii]);
                insertPlayer(play);
            }
            Game[] games = new Game[f.Length - 1];
            for (int ii = 1; ii < f.Length; ii++)
            {
                if (f[ii].Trim().Equals(""))
                {
                    ii++;
                    if (ii==f.Length)
                    {
                        break;
                    }
                }
                String[] subs = f[ii].Split('\t');
                for (int jj = 0; jj < subs.Length; jj++)
                {
                    subs[jj] = subs[jj].Trim();
                }
                DateTime date = DateTime.Parse(subs[0]);
                TimeSpan span = date - epoch;
                int cups = 0;
                int otcups = 0;
                int days = span.Days;
                string note = "";
                if (subs.Length > 5)
                {
                    note += subs[5];
                }
                if (!subs[4].Contains("OT"))
                {
                    cups = Int32.Parse(subs[4]);
                }
                else
                {
                    cups = 0;
                    otcups = Int32.Parse(subs[4].Substring(0, 1));
                }
                if (subs[2].Contains("and"))
                {
                    string[] team1Players = subs[1].Split(new string[] { " and " }, StringSplitOptions.RemoveEmptyEntries);
                    string[] team2Players = subs[2].Split(new string[] { " and " }, StringSplitOptions.RemoveEmptyEntries);
                    string[] WPlayers = subs[3].Split(new string[] { " and " }, StringSplitOptions.RemoveEmptyEntries);
                    int t1p1 = getPlayerNum(team1Players[0]);
                    int t1p2 = getPlayerNum(team1Players[1]);
                    int t2p1 = getPlayerNum(team2Players[0]);
                    int t2p2 = getPlayerNum(team2Players[1]);
                    int Wp1 = getPlayerNum(WPlayers[0]);
                    int Wp2 = getPlayerNum(WPlayers[1]);
                    if (t1p1<1||t1p2<1|| t2p1 < 1 || t2p2 < 1|| Wp1 < 1 || Wp2 < 1)
                    {
                        throw new Exception("Invalid name on line: " + ii);
                    }
                    if ((t1p1 == Wp1 && t1p2 == Wp2) || (t1p1 == Wp2 && t1p2 == Wp1))
                    {
                        games[ii - 1] = new Game(ii, days, Wp1, Wp2, t2p1, t2p2, cups, otcups, note);
                    }
                    else if ((t2p1 == Wp1 && t2p2 == Wp2) || (t2p1 == Wp2 && t2p2 == Wp1))
                    {
                        games[ii - 1] = new Game(ii, days, Wp1, Wp2, t1p1, t1p2, cups, otcups, note);
                    }
                    else
                    {
                        throw new ArgumentException("Winners on line: " + ii + "were not playing.");
                    }
                }
                else
                {
                    String wp = subs[3];
                    String lp = subs[2];
                    if (wp.ToLower().Equals(lp.ToLower()))
                    {
                        lp = subs[1];
                    }
                    games[ii - 1] = new Game(ii, days, getPlayerNum(wp), getPlayerNum(lp), cups, otcups, note);
                }
            }
            for (int ii = 0; ii < games.Length; ii++)
            {
                insertGame(games[ii]);
            }
        }
        public void printToTSV(string path)
        {
            Player[] players = listAllPlayers();
            StreamWriter sw = new StreamWriter(path);
            for (int ii=0; ii<players.Length-1; ii++)
            {
                sw.Write(players[ii]);
                sw.Write("\t");
            }
            sw.WriteLine(players[players.Length-1]);

            Game[] games = listAllGames();
            for (int ii = 0; ii < games.Length - 1; ii++)
            {
                sw.WriteLine(gameToString(games[ii]));
            }
            sw.Write(gameToString(games[games.Length-1]));
            sw.Flush();
            sw.Close();
            sw.Dispose();
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
        public Game[] listAllGames()
        {
            Object[][] data = getData("SELECT * FROM Games");
            Game[] Games = new Game[data.Length];
            for (int ii = 0; ii < Games.Length; ii++)
            {
                int gameNum = Convert.ToInt32(data[ii][0]);
                int day = Convert.ToInt32(data[ii][1]);
                int Winp1 = Convert.ToInt32(data[ii][2]);
                int Winp2 = Convert.ToInt32(data[ii][3]);
                int Lossp1 = Convert.ToInt32(data[ii][4]);
                int Lossp2 = Convert.ToInt32(data[ii][5]);
                int cups = Convert.ToInt32(data[ii][6]);
                int OTcups = Convert.ToInt32(data[ii][7]);
                string notes = (string)data[ii][8];
                Games[ii] = new Game(gameNum, day, Winp1, Winp2, Lossp1, Lossp2, cups, OTcups, notes);
            }
            return Games;
        }
        public string gameToString(Game g)
        {
            string date = PhysicsManager.epoch.AddDays(g.DaySinceEpoch).ToShortDateString();
            string Wp1 = getPlayer(g.Winp1).ToString();
            string Lp1 = getPlayer(g.Lossp1).ToString();
            string Wp2="";
            string Lp2 = "";
            if (g.Winp2!=0)
            {
                Wp2 = " and " + getPlayer(g.Winp2).ToString();
            }
            if (g.Lossp2 != 0)
            {
                Lp2 = " and " + getPlayer(g.Lossp2).ToString();
            }
            string cups = "";
            if (g.Cups > 0)
            {
                cups = "" + g.Cups;
            }
            else
            {
                cups = g.OTCups + "/OT";
            }
            if (g.Winp1 < g.Lossp1)
            {
                return date + "\t" + Wp1 + Wp2 + "\t" + Lp1 + Lp2 + "\t" + Wp1 + Wp2 + "\t" + cups + "\t" + g.Note;
            }
            else
            {
                return date + "\t" + Lp1 + Lp2 + "\t" + Wp1 + Wp2 + "\t" + Wp1 + Wp2 + "\t" + cups + "\t" + g.Note;
            }
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
            if (data.Length==0)
            {
                return null;
            }
            return new Player((string) data[0][1], (string) data[0][2]);
        }

        public int playerWinTotal(int playerNum)
        {
            Object[][] data = getData("SELECT * FROM games WHERE WinP1 = @val1 OR WinP2 = @val1", playerNum);
            return data.Length;
        }
        public int playerGameTotal(int playerNum)
        {
            Object[][] data = getData("SELECT * FROM games WHERE WinP1 = @val1 OR WinP2 = @val1 OR LossP1 = @val1 OR LossP2 = @val1", playerNum);
            return data.Length;
        }
        public int playerWinCupTotal(int playerNum)
        {
            Object[][] data = getData("SELECT cups FROM games WHERE WinP1 = @val1 OR WinP2 = @val1", playerNum);
            int total = 0;
            for (int ii = 0; ii < data.Length; ii++)
            {
                total += Convert.ToInt32(data[ii][0]);
            }
            return total;
        }
        public int playerLossCupTotal(int playerNum)
        {
            Object[][] data = getData("SELECT cups FROM games WHERE LossP1 = @val1 OR LossP2 = @val1", playerNum);
            int total = 0;
            for (int ii=0; ii<data.Length; ii++)
            {
                total += Convert.ToInt32(data[ii][0]);
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
