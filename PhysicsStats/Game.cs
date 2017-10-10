using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsStats
{
    class Game
    {
        public int Winp1 { get; }
        public int Winp2 { get; }
        public int Lossp1 { get; }
        public int Lossp2 { get; }
        public int Cups { get; set; }
        public int OTCups { get; set; }
        public int DaySinceEpoch { get; set; }
        public int GameNumber { get; }
        public string Note { get; }

        public Game(int gameNum, int day, int Wp1, int Wp2, int Lp1, int Lp2, int c, int OTc, string remarks)
        {
            GameNumber = gameNum;
            Winp1 = Wp1;
            Winp2 = Wp2;
            Lossp1 = Lp1;
            Lossp2 = Lp2;
            DaySinceEpoch = day;
            OTCups = OTc;
            Cups = c;
            Note = remarks;
        }

        public Game(int gameNum, int day, int Wp, int Lp, int c, int OTc, string remarks)
        {
            GameNumber = gameNum;
            Winp1 = Wp;
            Lossp1 = Lp;
            DaySinceEpoch = day;
            OTCups = OTc;
            Cups = c;
            Note = remarks;
        }

        public bool didPlayerPlay(Player p)
        {
            return (p.Equals(Winp1) || p.Equals(Winp2) || p.Equals(Lossp1) || p.Equals(Lossp2));
        }

        public bool didPlayerWin(Player p)
        {
            return (p.Equals(Winp1) || p.Equals(Winp2));
        }

        public int getCupDifferential(Player p)
        {
            if (!didPlayerPlay(p))
            {
                return 0;
            }
            else if (didPlayerWin(p))
            {
                return Cups;
            }
            else
            {
                return 0-Cups;
            }
        }

        public int getOTCupDifferential(Player p)
        {
            if (!didPlayerPlay(p) || Cups!=0)
            {
                return 0;
            }
            else if (didPlayerWin(p))
            {
                return Cups;
            }
            else
            {
                return 0 - Cups;
            }
        }
    }
}
