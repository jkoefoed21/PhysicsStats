using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsStats
{
    class Player
    {
        public string First { get; }
        public string Last { get; }

        public Player(string LastName, string FirstName)
        {
            if (FirstName==null||LastName==null)
            {
                throw new ArgumentNullException("Players must have non-null names");
            }
            if (LastName.Equals(""))
            {
                throw new ArgumentException("Players must have non-empty Last Name");
            }
            First = FirstName;
            Last = LastName;
        }

        public override string ToString()
        {
            if (First.Equals(""))
            {
                return Last;
            }
            return First + " " + Last;
        }

        public override bool Equals(object obj)
        {
            if (obj is Player)
            {
                Player p2 = (Player)obj;
                if (p2.First.ToLower().Equals(this.First.ToLower())&&p2.Last.ToLower().Equals(this.Last.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
