using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace uno_client
{
    public partial class game : Form
    {
        static List<Card> hand = new List<Card>();
        static btnCard[] cardBtns = new btnCard[7];
        static Card playingCard;
        static bool pickUp = false;
        static int pickUpNum = 0;
        class btnCard : Button
        {
            public Card card;

            public btnCard()
            {
                Size = new Size(85, 120);
                Enabled = false;
            }
            public void set()
            {
                Invoke(new setText(changeText));
            }
            private delegate void setText();
            private void changeText()
            {
                Text = $"{card.colour}\n{card.type}";
            }
            private delegate void changeUi(Form sel, bool x);
            private static void UI(Form sel, bool x)
            {
                if (x) sel.Show();
                else sel.Hide();
            }
            public void btnCard_Click(object sender, EventArgs e)
            {
                //need to check current card to see if selected card can be played.
                if (pickUpNum > 0 && (card.type != "+2" || card.type != "+4"))
                {
                    MessageBox.Show($"cannot play this card - current pick up count is {pickUpNum}");
                }
                else if (playingCard.type == card.type || playingCard.colour == card.colour)
                {
                    write(card.id.ToString());
                    Thread.Sleep(3000);
                    
                    if(card.type == "+2")
                    {
                        write("2");
                    }
                    else if (card.type == "miss a go")
                    {
                        write("3");
                    }
                    else if (card.type == "change direction")
                    {
                        write("1");
                    }
                    else write("0");
                    hand.Remove(card);
                    for (int i = 0; i < cardBtns.Length; i++)
                    {
                        cardBtns[i].Enabled = false;
                    }
                }
                else if (card.type == "+4" || card.type == "wild")
                {
                    wildSelection sel = new wildSelection();
                    sel.Show();
                    Thread t = new Thread(() => readColour(sel, card));
                    t.Start();
                    hand.Remove(card);
                    for (int i = 0; i < cardBtns.Length; i++)
                    {
                        cardBtns[i].Enabled = false;
                    }
                }

                else
                {
                    MessageBox.Show("you cannot play this card");
                }
                int x = 0;
                foreach (btnCard b in cardBtns)
                {
                    try
                    {
                        b.card = hand[x];
                        b.set();
                        x++;
                    }
                    catch (Exception ex)
                    {
                        b.card = null;
                        b.Text = "";
                    }
                }
            }
            public void readColour(wildSelection sel, Card card)
            {
                Dictionary<string, int> wildId = new Dictionary<string, int>() { { "red", 108 }, { "blue", 110 }, { "green", 112 }, { "yellow", 114 } };
                while (sel.colour == "") { }
                if (card.type == "wild")
                {
                    write(wildId[sel.colour].ToString());
                    Thread.Sleep(3000);
                    write("0");
                }
                else
                {
                    write(((wildId[sel.colour])+1).ToString());
                    pickUpNum += 4;
                    Thread.Sleep(3000);
                    write(pickUpNum.ToString());
                    pickUpNum = 0;
                }

                sel.Invoke(new changeUi(UI), sel, false);
            }
        }
        public class Card
        {
            public int id;
            public string colour;
            public int number;
            public string type;

            public Card(int i, string c, int n, string t)
            {
                id = i;
                colour = c;
                number = n;
                type = t;
            }
        }

        public game()
        {
            InitializeComponent();
        }

        static public void write(string toSend)
        {
            byte[] buffer = new byte[1024];
            buffer = Encoding.UTF8.GetBytes(toSend);
            connectScreen.stream.Write(buffer, 0, buffer.Length);
        }
        public static int read()
        {
            if (pickUp == false && connectScreen.stream.DataAvailable)
            {
                string s = connectScreen.stream.ReadByte().ToString();
                connectScreen.stream.Flush();
                return int.Parse(s);
            }
            return -2;
        }
        public static int readForPick()
        {
            byte[] buffer = new byte[1024];
            connectScreen.stream.Read(buffer, 0, buffer.Length);
            int recieved = 0;
            foreach (byte b in buffer)
            {
                if (b != 0)
                {
                    recieved++;
                }
            }
            string s = Encoding.UTF8.GetString(buffer);
            connectScreen.stream.Flush();
            return int.Parse(s);
        }
        public string readString()
        {
            byte[] buffer = new byte[1024];
            connectScreen.stream.Read(buffer, 0, buffer.Length);
            int recieved = 0;
            foreach (byte b in buffer)
            {
                if (b != 0)
                {
                    recieved++;
                }
            }
            return Encoding.UTF8.GetString(buffer, 0, recieved);
        }

        public void beginGame()
        {
            Thread t = new Thread(handCreation);
            t.Start();
        }
        public void handCreation()
        {
            List<Card> master = new List<Card>();
            string[] colours = new string[] { "red", "blue", "green", "yellow" };

            for (int i = 0; i < 4; i++)
            {
                master.Add(new Card(master.Count, colours[i], 0, "0"));
                for (int k = 0; k < 2; k++)
                {
                    for (int j = 1; j < 10; j++)
                    {
                        master.Add(new Card(master.Count, colours[i], j, j.ToString()));
                    }
                }
            }
            string[] types = new string[] { "+2", "miss a go", "change direction" };
            for (int k = 0; k < 3; k++)
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        master.Add(new Card(master.Count, colours[i], 20, types[k]));
                    }
                }
            }
            for (int j = 0; j < 4; j++)
            {
                master.Add(new Card(master.Count, "", 50, "wild")); //103-100
            }
            for (int j = 0; j < 4; j++)
            {
                master.Add(new Card(master.Count, "", 50, "+4"));//107-104
            }

            for (int i = 0; i < 4; i++)
            {
                master.Add(new Card(master.Count, colours[i], 50, "wild"));
                master.Add(new Card(master.Count, colours[i], 50, "+4"));
            }
            handGen(master);
            btnPickUp.Click += delegate (object sender, EventArgs e) { btnPickUp_Click(sender, e, master); };
            waitTurn(master);
        }

        public static void handGen(List<Card> master)
        {
            for (int i = 0; i < 7; i++)
            {
                int index = read();
                while (index < 0 || index > 110)
                {
                    index = read();
                }
                hand.Add(master[index]);
            }
            for (int i = 0; i < 7; i++)
            {
                cardBtns[i].card = hand[i];
                cardBtns[i].set();
            }
        }
        private delegate void enabler(btnCard btn, bool x);
        private void enableBtn(btnCard btn, bool x)
        {
            if (x) { btn.Enabled = true; btnPickUp.Enabled = true; }
            else { btn.Enabled = false; btnPickUp.Enabled = false; }
        }
        private delegate void setCurrentCard(Label l, Card card);
        private void setCurrent(Label l, Card card)
        {
            l.Text = $"{card.colour}\n{card.type}";
            playingCard = card;
        }
        public void waitTurn(List<Card> master)
        {
            int x = 1;
            while (x != -1)
            {
                x = read();
                Console.WriteLine(x);
                if (x == 255)
                {
                    pickUpNum = readForPick();
                    lblPickUpNum.Invoke(new pickUpDel(updatePickUp), lblPickUpNum, pickUpNum.ToString());
                    foreach (btnCard b in cardBtns)
                    {
                        b.Invoke(new enabler(enableBtn), b, true);
                    }
                }
                else if (x > -1)
                {
                    currentCard.Invoke(new setCurrentCard(setCurrent), currentCard, master[x]);
                }
            }
        }
        private delegate void pickUpDel(Label l, string s);
        private void updatePickUp(Label l, string s)
        {
            l.Text = s;
        }
        private void game_Load(object sender, EventArgs e)
        {
            Point[] pointBtns = new Point[] { new Point(279 - 40, 610), new Point(380 - 45, 610), new Point(481 - 50, 610), new Point(582 - 55, 610), new Point(683 - 60, 610), new Point(784 - 65, 610), new Point(885 - 70, 610) };

            for (int i = 0; i < cardBtns.Length; i++)
            {
                cardBtns[i] = new btnCard();
                cardBtns[i].Location = pointBtns[i];
                btnCard x = cardBtns[i];
                x.Click += (Sender, EventArgs) => { x.btnCard_Click(Sender, EventArgs); };
                Controls.Add(cardBtns[i]);
            }
        }

        private void btnPickUp_Click(object sender, EventArgs e, List<Card> master)
        {
            int x;
            int y;
            pickUp = true;
            if (pickUpNum == 0) pickUpNum++;
            write("-1");
            write(pickUpNum.ToString());
            for (int i = 0; i < pickUpNum; i++)
            {
                x = readForPick();
                hand.Add(master[x]);
                y = 0;
                foreach (btnCard b in cardBtns)
                {
                    b.Invoke(new enabler(enableBtn), b, false);
                    b.card = hand[y];
                    b.set();
                    y++;
                }
            }
            pickUp= false;
            lblPickUpNum.Invoke(new pickUpDel(updatePickUp), lblPickUpNum, pickUpNum.ToString());
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void nextPage_Click(object sender, EventArgs e)
        {

        }
    }
}
//need to do miss a go properly 
//need to do +2
