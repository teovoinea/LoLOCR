using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using tessnet2;
using System.Diagnostics;
using System.IO;

namespace OCRSharp
{
    public partial class Form1 : Form
    {
        //The rectangles that are only going to be read once at the beginning of the game
        Rectangle[] readOnce = new Rectangle[12];
        //The rectangles who's location doesn't change
        Rectangle[] constantLocation = new Rectangle[7];
        //The rectangles that  store KDA, and CS
        Rectangle[] score = new Rectangle[20];
        //Rectangles that store each individual's total gold
        //I decided not to keep the current gold in bank part because it gets updated rarely
        //And it doesn't seem like important information
        Rectangle[] gold = new Rectangle[10];
        Team team1 = new Team();
        Team team2 = new Team();
        Tesseract ocr;
        Bitmap mainImage;
        Bitmap bmpScreenShot;
        Graphics gfxScreenShot;
        //Where the files are stored, I'm going to change it from "Teodor" to EnvironmentVariables.Username
        string imageLocation = @"C:\Program Files\OCRSharp\currentscreen.png";
        string tessdataLocation = @"C:\Users\Teodor\Desktop\OCRSharp\tessdata";
        string gameInfoLocation = @"C:\Program Files\OCRSharp\info.txt";
        //For refreshing the information
        int seconds = 0;
        //Storing the game time, it didn't fit in the team classes
        string gameTime;
        //If the stream is showing the gold page to store the gold information instead
        bool isGoldPage = false;
        public Form1()
        {
            InitializeComponent();
            //Instantiates the rectangles, still need to give each rectangle its location
            //I was going to do that but my resolution is 1366 which makes no sense
            //So I left it blank
            Setup();
            //Takes a screenshot using code I got from stack overflow
            ScreenShot();
            Image image = new Bitmap(imageLocation);
            mainImage = (Bitmap)image;
            ocr = new Tesseract();
            //VVVVVVVVVV not required, was just for testing
            //ocr.SetVariable("tessedit_char_whitelist", "1234567890");
            ocr.Init(null, "eng", false);
            //Stores the information for the rectangles that are read only once
            AnalyzeRect(ref team1.name, readOnce[0]);
            AnalyzeRect(ref team2.name, readOnce[1]);
            AnalyzeRect(ref team1.players[0].name, readOnce[2]);
            AnalyzeRect(ref team1.players[1].name, readOnce[3]);
            AnalyzeRect(ref team1.players[2].name, readOnce[4]);
            AnalyzeRect(ref team1.players[3].name, readOnce[5]);
            AnalyzeRect(ref team1.players[4].name, readOnce[6]);
            AnalyzeRect(ref team2.players[0].name, readOnce[7]);
            AnalyzeRect(ref team2.players[1].name, readOnce[8]);
            AnalyzeRect(ref team2.players[2].name, readOnce[9]);
            AnalyzeRect(ref team2.players[3].name, readOnce[10]);
            //Every 60 seconds, update information and save to file
            while (seconds % 60 == 0)
            {
                UpdateInfo();
                SafeToFile();
            }
        }
        void SafeToFile()
        {
            //Delete the old save file if it exists
            if (File.Exists(gameInfoLocation))
            {
                File.Delete(gameInfoLocation);
            }
            else
            {
                using (StreamWriter sw = new StreamWriter(gameInfoLocation))
                {
                    //write game time to file
                    sw.WriteLine(gameTime.ToString());
                    //each team stores the same information
                    TeamInfo(sw, team1);
                    TeamInfo(sw, team2);
                }
            }
        }
        void TeamInfo(StreamWriter sw, Team team)
        {
            //team name, towers, kills and gold
            sw.WriteLine(team.name);
            sw.WriteLine(team.towers);
            sw.WriteLine(team.kills);
            sw.WriteLine(team.gold);
            //I used | to separate to be able to parse it easier locally on the mobile device
            foreach (Player p in team.players)
            {
                p.ParseKDA();
                sw.WriteLine(p.name + "|" + p.cs + "|" + p.kills + "|" + p.deaths + "|" + p.assists + "|" + p.gold);
            }
        }
        void UpdateInfo()
        {
            //Updates the info of constant location
            AnalyzeRect(ref team1.towers, constantLocation[0]);
            AnalyzeRect(ref team1.gold, constantLocation[1]);
            AnalyzeRect(ref team1.kills, constantLocation[2]);
            AnalyzeRect(ref gameTime, constantLocation[3]);
            AnalyzeRect(ref team2.kills, constantLocation[4]);
            AnalyzeRect(ref team2.gold, constantLocation[5]);
            AnalyzeRect(ref team2.towers, constantLocation[6]);
            AnalyzeRect(ref team1.towers, constantLocation[0]);
            //If the screenshot is of the gold page (which we still need to figure out HOW
            //the program can tell if the gold page is showing
            //I wanted to find a picture online but I couldn't and don't have the patience
            //to watch a whole LCS game right now just for the few seconds that they
            //have a picture of the gold page
            //going to have to figure this out later
            if (isGoldPage)
            {
                //Since it only checks the total gold the player has, it's simply team 1, team2
                for (int i = 0; i < gold.Length; i++)
                {
                    if (i < 5)
                    {
                        AnalyzeRect(ref team1.players[i].gold, gold[i]);
                    }
                    else
                    {
                        AnalyzeRect(ref team2.players[i - 5].gold, gold[i]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < score.Length; i++)
                {
                    //team 1
                    if (i < 10)
                    {
                        //KDA
                        if (i < 5)
                        {
                            AnalyzeRect(ref team1.players[i].KDA, score[i]);
                        }
                        //CS
                        else
                        {
                            AnalyzeRect(ref team1.players[i - 5].cs, score[i]);
                        }
                    }
                    //team2
                    else
                    {
                        //KDA
                        if (i < 15)
                        {
                            AnalyzeRect(ref team2.players[i - 10].KDA, score[i]);
                        }
                        //CS
                        else
                        {
                            AnalyzeRect(ref team2.players[i - 15].cs, score[i]);
                        }
                    }
                }
            }
        }
        //Analyzes the rectangle specified in the image
        void AnalyzeRect(ref string safe, Rectangle rect)
        {
            //ocr.DoOCR returns a word list, but I split it up so it really only reads 1 word
            //at a time, so because of that, I had to do a work around where it just adds all of the "words"
            //it sees to the right piece of information even though it should only see one "word" at a time
            var result = ocr.DoOCR(mainImage, rect);
            foreach (Word w in result)
            {
                safe += w.ToString();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        //Screenshot code I copied off stack overflow
        void ScreenShot()
        {
            bmpScreenShot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
            gfxScreenShot = Graphics.FromImage(bmpScreenShot);
            gfxScreenShot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
            bmpScreenShot.Save(imageLocation, ImageFormat.Png);
        }
        //Instantiate all of the rectangles so they're not null
        //Still have to input the rectangle sizes and locations
        //We'll do that once we figure out screen resolution and
        //make actualy measurements on where that information will show up
        void Setup()
        {
            for (int i = 0; i < readOnce.Length; i++)
            {
                readOnce[i] = new Rectangle();
            }
            for (int i = 0; i < constantLocation.Length; i++)
            {
                constantLocation[i] = new Rectangle();
            }
            for (int i = 0; i < score.Length; i++)
            {
                score[i] = new Rectangle();
            }
            for (int i = 0; i < gold.Length; i++)
            {
                gold[i] = new Rectangle();
            }
        }
        //Update seconds every second
        private void tmrNewShot_Tick(object sender, EventArgs e)
        {
            seconds++;
        }
    }
}
