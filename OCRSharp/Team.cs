using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCRSharp
{
    class Team
    {
        //Team information
        public string name;
        public string towers;
        public string gold;
        public string kills;
        public Player[] players = new Player[5];
        public Team()
        {
            //instantiate the players when the team is created
            for (int i = 0; i < players.Length; i++)
            {
                players[i] = new Player();
            }
        }
    }
}
