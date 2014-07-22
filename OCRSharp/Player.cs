using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCRSharp
{
    class Player
    {
        //All of the player information that it's going to store
        public string name;
        public string kills;
        public string deaths;
        public string assists;
        public string gold;
        public string cs;
        public string KDA;
        //I read the KDA as one big chunk (0/0/0) instead of K:0, D:0, A:0
        //to save on rectangles and hopefully some processing
        //So I made this subprogram that parses 0/0/0 to K:0, D:0, A:0
        public void ParseKDA()
        {
            //turn the string into a char array
            char[] kdaArray = KDA.ToCharArray();
            //I know there are 3 pieces of info
            //every time I hit a "/", there's a new piece of info
            int kdaIndex = 0;
            for (int i = 0; i < kdaArray.Length; i++)
            {
                if (kdaArray[i] != '/')
                {
                    if (kdaIndex == 0)
                    {
                        kills += kdaArray[i];
                    }
                    else if (kdaIndex == 1)
                    {
                        deaths += kdaArray[i];
                    }
                    else
                    {
                        assists += kdaArray[i];
                    }
                }
                else
                {
                    kdaIndex++;
                }
            }
        }
    }
}
