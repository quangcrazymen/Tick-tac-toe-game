using System;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using System.Media;
using System.Collections.Generic;

namespace Game_Caro
{
    partial class GameCaro : Form
    {
        #region Properties
        GameBoard board;
        SocketManager socket;
        string PlayerName;
        int Time;

        public GameCaro()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;

            board = new GameBoard(pn_GameBoard, txt_PlayerName, pb_Avatar);            
            board.PlayerClicked += Board_PlayerClicked;
            board.GameOver += Board_GameOver;

            pgb_CountDown.Step = Constance.CountDownStep;
            pgb_CountDown.Maximum = Constance.CountDownTime;

            tm_CountDown.Interval = Constance.CountDownInterval;
            socket = new SocketManager();

            txt_Chat.Text = "";

            NewGame();
        }
        #endregion

        #region Methods

        void NewGame()
        {
            pgb_CountDown.Value = 0;
            tm_CountDown.Stop();
            gameTime.Start();
            undoToolStripMenuItem.Enabled = true;
            redoToolStripMenuItem.Enabled = true;

            btn_Undo.Enabled = true;
            btn_Redo.Enabled = true;

            board.DrawGameBoard();
        }

        void EndGame()
        {
            undoToolStripMenuItem.Enabled = false;
            redoToolStripMenuItem.Enabled = false;

            btn_Undo.Enabled = false;
            btn_Redo.Enabled = false;

            tm_CountDown.Stop();
            pn_GameBoard.Enabled = false;
            gameTime.Stop();
            MessageBox.Show("Player 1 is the winner"+"\n"+"Game end in " + Time + "s");
        }


        private void GameCaro_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Are you sure you wanna quit!", "Oh..!", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
                e.Cancel = true;
            else
            {
                try
                {
                    socket.Send(new SocketData((int)SocketCommand.QUIT, "", new Point()));
                } catch { }
            }
        }

        private void Board_PlayerClicked(object sender, BtnClickEvent e)
        {
            tm_CountDown.Start(); 
            pgb_CountDown.Value = 0;
            soundPlayer1 = new SoundPlayer("Mario Coin Sound - Sound Effect (HD).wav");
            soundPlayer1.Play();
            if (board.PlayMode == 1)
            {
                try
                {
                    pn_GameBoard.Enabled = false;
                    socket.Send(new SocketData((int)SocketCommand.SEND_POINT, "", e.ClickedPoint));

                    undoToolStripMenuItem.Enabled = false;
                    redoToolStripMenuItem.Enabled = false;

                    btn_Undo.Enabled = false;
                    btn_Redo.Enabled = false;

                    Listen();
                }
                catch
                {
                    EndGame();
                    MessageBox.Show("Không có kết nối nào tới máy đối thủ", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Board_GameOver(object sender, EventArgs e)
        {
            EndGame();

            if (board.PlayMode == 1)
                socket.Send(new SocketData((int)SocketCommand.END_GAME, "", new Point()));
        }

        private void Tm_CountDown_Tick(object sender, EventArgs e)
        {
            pgb_CountDown.PerformStep();

            if (pgb_CountDown.Value >= pgb_CountDown.Maximum)
            {
                EndGame();

                if (board.PlayMode == 1)
                    socket.Send(new SocketData((int)SocketCommand.TIME_OUT, "", new Point()));
            }                                    
        }


        #region MenuStrip
        private void NewGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewGame();

            if (board.PlayMode == 1)
            {
                try
                {
                    socket.Send(new SocketData((int)SocketCommand.NEW_GAME, "", new Point()));
                }
                catch { }
            }
                
            pn_GameBoard.Enabled = true;
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pgb_CountDown.Value = 0;
            board.Undo();

            if (board.PlayMode == 1)
                socket.Send(new SocketData((int)SocketCommand.UNDO, "", new Point()));
        }

        private void RedoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // pgb_CountDown.Value = 0;
            board.Redo();

            if (board.PlayMode == 1)
                socket.Send(new SocketData((int)SocketCommand.REDO, "", new Point()));
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ViaLANToolStripMenuItem_Click(object sender, EventArgs e)
        {
            board.PlayMode = 1;
            NewGame();

            socket.IP = txt_IP.Text;

            if (!socket.ConnectServer())
            {
                socket.IsServer = true;
                pn_GameBoard.Enabled = true;
                socket.CreateServer();
                MessageBox.Show("You are the Server", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                socket.IsServer = false;
                pn_GameBoard.Enabled = false;
                Listen();
                MessageBox.Show("successfully connected !!!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SameComToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (board.PlayMode == 1)
            {
                try
                {
                    socket.Send(new SocketData((int)SocketCommand.QUIT, "", new Point()));
                } catch { }

                socket.CloseConnect();
                MessageBox.Show("LAN disconnected", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            board.PlayMode = 2;
            NewGame();
        }

        private void PlayerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (board.PlayMode == 1)
            {
                if (board.PlayMode == 1)
                {
                    try
                    {
                        socket.Send(new SocketData((int)SocketCommand.QUIT, "", new Point()));
                    } catch { }

                    socket.CloseConnect();
                    MessageBox.Show("Đã ngắt kết nối mạng LAN", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            board.PlayMode = 3;
            NewGame();
            board.StartAI();
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void HowToPlayToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ContactMeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void AboutThisGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Tic-tac-toe game made with C#");
        }
        #endregion     

        #region Button Settings
        private void Btn_LAN_Click(object sender, EventArgs e)
        {
            ViaLANToolStripMenuItem_Click(sender, e);
        }

        private void Btn_SameCom_Click(object sender, EventArgs e)
        {
            SameComToolStripMenuItem_Click(sender, e);
        }

        private void Btn_AI_Click(object sender, EventArgs e)
        {
            PlayerToolStripMenuItem1_Click(sender, e);
        }

        private void Btn_Undo_Click(object sender, EventArgs e)
        {
            UndoToolStripMenuItem_Click(sender, e);
        }

        private void Btn_Redo_Click(object sender, EventArgs e)
        {
            RedoToolStripMenuItem_Click(sender, e);
        }

        private void Btn_Send_Click(object sender, EventArgs e)
        {
            if (board.PlayMode != 1)
                return;

            PlayerName = board.ListPlayers[socket.IsServer ? 0 : 1].Name;
            txt_Chat.Text += "- " + PlayerName + ": " + txt_Message.Text + "\r\n";

            socket.Send(new SocketData((int)SocketCommand.SEND_MESSAGE, txt_Chat.Text, new Point()));
            Listen();
        }
        #endregion

        #region LAN settings
        private void GameCaro_Shown(object sender, EventArgs e)
        {
            txt_IP.Text = socket.GetLocalIPv4(NetworkInterfaceType.Wireless80211);

            if (string.IsNullOrEmpty(txt_IP.Text))
                txt_IP.Text = socket.GetLocalIPv4(NetworkInterfaceType.Ethernet);
        }

        private void Listen()
        {
            Thread ListenThread = new Thread(() =>
            {
                try
                {
                    SocketData data = (SocketData)socket.Receive();
                    ProcessData(data);
                }
                catch { }
            });

            ListenThread.IsBackground = true;
            ListenThread.Start();
        }

        private void ProcessData(SocketData data)
        {
            PlayerName = board.ListPlayers[board.CurrentPlayer == 1 ? 0 : 1].Name;

            switch (data.Command)
            {
                case (int)SocketCommand.SEND_POINT:
                    // UI changing
                    this.Invoke((MethodInvoker)(() =>
                    {
                        board.OtherPlayerClicked(data.Point);
                        pn_GameBoard.Enabled = true;

                        pgb_CountDown.Value = 0;
                        tm_CountDown.Start();

                        undoToolStripMenuItem.Enabled = true;
                        redoToolStripMenuItem.Enabled = true;

                        btn_Undo.Enabled = true;
                        btn_Redo.Enabled = true;
                    }));
                    break;

                case (int)SocketCommand.SEND_MESSAGE:
                    txt_Chat.Text = data.Message;
                    break;

                case (int)SocketCommand.NEW_GAME:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        NewGame();
                        pn_GameBoard.Enabled = false;
                    }));
                    break;

                case (int)SocketCommand.UNDO:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        pgb_CountDown.Value = 0;
                        board.Undo();
                    }));
                    break;

                case (int)SocketCommand.REDO:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        // pgb_CountDown.Value = 0;
                        board.Redo();
                    }));
                    break;

                case (int)SocketCommand.END_GAME:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        EndGame();
                        MessageBox.Show(PlayerName + " is the winner ♥ !!!", "Congratulation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }));
                    break;

                case (int)SocketCommand.TIME_OUT:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        EndGame();
                        MessageBox.Show("Hết giờ rồi !!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }));
                    break;

                case (int)SocketCommand.QUIT:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        tm_CountDown.Stop();
                        EndGame();
                    
                        board.PlayMode = 2;
                        socket.CloseConnect();

                        MessageBox.Show("Opponent has left the game", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }));
                    break;

                default:
                    break;
            }

            Listen();
        }
        #endregion

        #endregion

        private void historyMatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            History frm = new History();
            frm.Show();
        }

        private void gameTime_Tick(object sender, EventArgs e)
        {
            Time += 1;
        }

        private void btn_saveGame_Click(object sender, EventArgs e)
        {
            //txt_IP.Text = Time.ToString();
            DateTime now = DateTime.Now;
            WriteFile("History.txt", "Date: " + now + "\n" + "Winner: Player 1"+"\n"+ "Game end in: " + Time + "s" + "\n"+"======================================"+"\n"+"\n");
            Time = 0;
        }
        void WriteFile(string file, string str)
        {
            string[] Informations = str.Split('\n');
            Informations = Informations.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            try
            {

                File.AppendAllText(file, str);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }
        private SoundPlayer soundPlayer1;
        private SoundPlayer _soundPlayer;
        private void music_Click(object sender, EventArgs e)
        {
            
        }

        private void Music_CheckedChanged(object sender, EventArgs e)
        {
            _soundPlayer = new SoundPlayer("Canon in D.wav");
            if (Music.Checked)
            {
                Music.Text = "Stop";
                _soundPlayer.Play();
                Music.BackColor = Color.Tomato;
            }
            else
            {
                Music.Text = "Play";
                _soundPlayer.Stop();
                Music.BackColor = Color.MediumBlue;
            }
        }

        private void option1ToolStripMenuItem_Click(object sender, EventArgs e)
        { 
            board.ListPlayers = new List<Player>()
            {
                new Player("Player one", Image.FromFile(Application.StartupPath + "\\images\\p1.png"),
                                        Image.FromFile(Application.StartupPath + "\\images\\X.png")),

                new Player("Player two", Image.FromFile(Application.StartupPath + "\\images\\p2.png"),
                                   Image.FromFile(Application.StartupPath + "\\images\\O.png"))

            };

        }

        private void option2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            board.ListPlayers = new List<Player>()
            {
                new Player("Player one", Image.FromFile(Application.StartupPath + "\\images\\p1.png"),
                                        Image.FromFile(Application.StartupPath + "\\images\\star.png")),

                new Player("Player two", Image.FromFile(Application.StartupPath + "\\images\\p2.png"),
                                   Image.FromFile(Application.StartupPath + "\\images\\heart.png"))

            };
        }

        private void option3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            board.ListPlayers = new List<Player>()
            {
                new Player("Player one", Image.FromFile(Application.StartupPath + "\\images\\p1.png"),
                                        Image.FromFile(Application.StartupPath + "\\images\\blackSun.png")),

                new Player("Player two", Image.FromFile(Application.StartupPath + "\\images\\p2.png"),
                                   Image.FromFile(Application.StartupPath + "\\images\\moon.png"))

            };
        }

        private void option4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            board.ListPlayers = new List<Player>()
            {
                new Player("Player one", Image.FromFile(Application.StartupPath + "\\images\\p1.png"),
                                        Image.FromFile(Application.StartupPath + "\\images\\apple.png")),

                new Player("Player two", Image.FromFile(Application.StartupPath + "\\images\\p2.png"),
                                   Image.FromFile(Application.StartupPath + "\\images\\banana.png"))

            };
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void cyanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pn_GameBoard.BackColor = Color.Cyan;
            NewGame();
        }

        private void mediumTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pn_GameBoard.BackColor = Color.MediumTurquoise;
            NewGame();
        }

        private void dodgerBlueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pn_GameBoard.BackColor = Color.DodgerBlue;
            NewGame();
        }

        private void pinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pn_GameBoard.BackColor = Color.Pink;
            NewGame();
        }
    }
}
