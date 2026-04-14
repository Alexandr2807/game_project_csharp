using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TimePostman
{
    public partial class Form1 : Form
    {
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        int windowW = 1240;
        int windowH = 800;
        int uiWidth = 320;

        Rectangle gameArea;
        Rectangle uiArea;

        int playerX = 150;
        int playerY = 150;
        int playerW = 28;
        int playerH = 28;
        int speed = 4;

        int startPlayerX = 150;
        int startPlayerY = 300;

        bool up = false;
        bool down = false;
        bool left = false;
        bool right = false;

        List<Rectangle> houses = new List<Rectangle>();
        List<string> houseNames = new List<string>();
        List<Color> houseColors = new List<Color>();

        List<Rectangle> roads = new List<Rectangle>();
        List<Rectangle> treeTops = new List<Rectangle>();

        List<LetterItem> letters = new List<LetterItem>();

        TimePhase currentPhase = TimePhase.Morning;
        int phaseTimerMs = 0;
        int phaseDurationMs = 20000;

        int matchTimerMs = 0;
        int matchDurationMs = 300000; // 5 минут

        string statusMessage = "Добро пожаловать. Подходи к дому и нажимай E.";
        int statusMessageTimerMs = 0;

        int currentNearbyHouseIndex = -1;

        bool gameWon = false;
        bool gameLost = false;

        public Form1()
        {
            InitializeComponent();

            this.Text = "Почтальон времени";
            this.ClientSize = new Size(windowW, windowH);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            this.KeyPreview = true;

            int mapMarginLeft = 40;
            int mapMarginTop = 40;
            int mapMarginBottom = 40;

            gameArea = new Rectangle(
                mapMarginLeft,
                mapMarginTop,
                windowW - uiWidth - mapMarginLeft,
                windowH - mapMarginTop - mapMarginBottom
            );

            uiArea = new Rectangle(
                gameArea.Right,
                0,
                uiWidth,
                windowH
            );

            int houseW = 92;
            int houseH = 72;

            int leftRoadX = gameArea.X + 90;

            roads.Add(new Rectangle(420, gameArea.Y, 130, gameArea.Height));
            roads.Add(new Rectangle(gameArea.X, 350, gameArea.Width, 100));
            roads.Add(new Rectangle(leftRoadX, gameArea.Y, 120, gameArea.Height));

            houses.Add(new Rectangle(285, 255, houseW, houseH));
            houseNames.Add("Дом 1");
            houseColors.Add(Color.FromArgb(215, 182, 130));

            houses.Add(new Rectangle(567, 98, houseW, houseH));
            houseNames.Add("Дом 2");
            houseColors.Add(Color.FromArgb(205, 95, 95));

            houses.Add(new Rectangle(567, 250, houseW, houseH));
            houseNames.Add("Дом 3");
            houseColors.Add(Color.FromArgb(176, 196, 222));

            houses.Add(new Rectangle(283, 486, houseW, houseH));
            houseNames.Add("Дом 4");
            houseColors.Add(Color.FromArgb(223, 213, 130));

            houses.Add(new Rectangle(567, 475, houseW, houseH));
            houseNames.Add("Дом 5");
            houseColors.Add(Color.FromArgb(144, 220, 144));

            houses.Add(new Rectangle(770, 475, houseW, houseH));
            houseNames.Add("Дом 6");
            houseColors.Add(Color.FromArgb(200, 145, 205));

            startPlayerX = leftRoadX + 25;
            startPlayerY = 300;

            FillLetters();

            playerX = startPlayerX;
            playerY = startPlayerY;

            CreateTrees();

            timer.Interval = 16;
            timer.Tick += Timer_Tick;
            timer.Start();

            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
        }

        private void FillLetters()
        {
            letters.Clear();

            letters.Add(new LetterItem("Письмо для Дома 1", 0, new List<TimePhase> { TimePhase.Morning }));
            letters.Add(new LetterItem("Письмо для Дома 2", 1, new List<TimePhase> { TimePhase.Day }));
            letters.Add(new LetterItem("Письмо для Дома 3", 2, new List<TimePhase> { TimePhase.Evening }));
            letters.Add(new LetterItem("Письмо для Дома 4", 3, new List<TimePhase> { TimePhase.Night }));
            letters.Add(new LetterItem("Письмо для Дома 5", 4, new List<TimePhase> { TimePhase.Day, TimePhase.Evening }));
            letters.Add(new LetterItem("Письмо для Дома 6", 5, new List<TimePhase> { TimePhase.Morning, TimePhase.Night }));
        }

        private void ResetGame()
        {
            up = false;
            down = false;
            left = false;
            right = false;

            playerX = startPlayerX;
            playerY = startPlayerY;

            currentPhase = TimePhase.Morning;
            phaseTimerMs = 0;

            matchTimerMs = 0;

            gameWon = false;
            gameLost = false;

            currentNearbyHouseIndex = -1;

            statusMessage = "Игра перезапущена. Подходи к дому и нажимай E.";
            statusMessageTimerMs = 2500;

            FillLetters();
        }

        private void CreateTrees()
        {
            treeTops.Clear();

            treeTops.Add(new Rectangle(gameArea.X + 10, gameArea.Y + 10, 34, 34));
            treeTops.Add(new Rectangle(gameArea.X + 55, gameArea.Y + 30, 30, 30));
            treeTops.Add(new Rectangle(gameArea.X + 20, gameArea.Bottom - 80, 38, 38));
            treeTops.Add(new Rectangle(gameArea.X + 70, gameArea.Bottom - 55, 28, 28));

            treeTops.Add(new Rectangle(gameArea.Right - 70, gameArea.Y + 20, 36, 36));
            treeTops.Add(new Rectangle(gameArea.Right - 110, gameArea.Y + 55, 28, 28));
            treeTops.Add(new Rectangle(gameArea.Right - 60, gameArea.Bottom - 85, 34, 34));
            treeTops.Add(new Rectangle(gameArea.Right - 105, gameArea.Bottom - 50, 26, 26));

            treeTops.Add(new Rectangle(gameArea.X + 250, gameArea.Y + 12, 26, 26));
            treeTops.Add(new Rectangle(gameArea.X + 520, gameArea.Bottom - 42, 24, 24));
            treeTops.Add(new Rectangle(gameArea.X + 720, gameArea.Y + 18, 24, 24));
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!gameWon && !gameLost)
            {
                int dx = 0;
                int dy = 0;

                if (up) dy -= speed;
                if (down) dy += speed;
                if (left) dx -= speed;
                if (right) dx += speed;

                MovePlayer(dx, dy);

                phaseTimerMs += timer.Interval;
                if (phaseTimerMs >= phaseDurationMs)
                {
                    phaseTimerMs = 0;
                    NextPhase();
                }

                matchTimerMs += timer.Interval;
                if (matchTimerMs >= matchDurationMs)
                {
                    matchTimerMs = matchDurationMs;
                    gameLost = true;
                    statusMessage = "Время вышло. Нажми R, чтобы начать заново.";
                    statusMessageTimerMs = 0;
                }

                currentNearbyHouseIndex = GetNearbyHouseIndex();

                if (GetDeliveredCount() == letters.Count)
                {
                    gameWon = true;
                    statusMessage = "Все письма доставлены! Нажми R, чтобы сыграть еще раз.";
                    statusMessageTimerMs = 0;
                }
            }

            if (statusMessageTimerMs > 0)
            {
                statusMessageTimerMs -= timer.Interval;
                if (statusMessageTimerMs <= 0)
                {
                    statusMessageTimerMs = 0;

                    if (!gameWon && !gameLost)
                    {
                        statusMessage = "Подойди к дому и нажми E.";
                    }
                }
            }

            Invalidate();
        }

        private void MovePlayer(int dx, int dy)
        {
            if (dx != 0)
            {
                Rectangle newPlayer = new Rectangle(playerX + dx, playerY, playerW, playerH);

                if (InsideGame(newPlayer) && !TouchHouse(newPlayer))
                {
                    playerX += dx;
                }
            }

            if (dy != 0)
            {
                Rectangle newPlayer = new Rectangle(playerX, playerY + dy, playerW, playerH);

                if (InsideGame(newPlayer) && !TouchHouse(newPlayer))
                {
                    playerY += dy;
                }
            }
        }

        private bool InsideGame(Rectangle r)
        {
            if (r.Left < gameArea.Left) return false;
            if (r.Right > gameArea.Right) return false;
            if (r.Top < gameArea.Top) return false;
            if (r.Bottom > gameArea.Bottom) return false;

            return true;
        }

        private bool TouchHouse(Rectangle r)
        {
            for (int i = 0; i < houses.Count; i++)
            {
                if (r.IntersectsWith(houses[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private void NextPhase()
        {
            if (currentPhase == TimePhase.Morning)
            {
                currentPhase = TimePhase.Day;
            }
            else if (currentPhase == TimePhase.Day)
            {
                currentPhase = TimePhase.Evening;
            }
            else if (currentPhase == TimePhase.Evening)
            {
                currentPhase = TimePhase.Night;
            }
            else
            {
                currentPhase = TimePhase.Morning;
            }

            statusMessage = "Время сменилось: " + GetPhaseText(currentPhase);
            statusMessageTimerMs = 2500;
        }

        private string GetPhaseText(TimePhase phase)
        {
            if (phase == TimePhase.Morning) return "Утро";
            if (phase == TimePhase.Day) return "День";
            if (phase == TimePhase.Evening) return "Вечер";
            return "Ночь";
        }

        private int GetNearbyHouseIndex()
        {
            Rectangle player = new Rectangle(playerX, playerY, playerW, playerH);

            for (int i = 0; i < houses.Count; i++)
            {
                Rectangle zone = houses[i];
                zone.Inflate(35, 35);

                if (zone.IntersectsWith(player))
                {
                    return i;
                }
            }

            return -1;
        }

        private void TryInteract()
        {
            if (gameWon || gameLost)
            {
                return;
            }

            int houseIndex = GetNearbyHouseIndex();

            if (houseIndex == -1)
            {
                statusMessage = "Подойди ближе к дому.";
                statusMessageTimerMs = 2000;
                return;
            }

            LetterItem? currentLetter = null;

            for (int i = 0; i < letters.Count; i++)
            {
                if (letters[i].HouseIndex == houseIndex && !letters[i].Delivered)
                {
                    currentLetter = letters[i];
                    break;
                }
            }

            if (currentLetter == null)
            {
                statusMessage = "Для этого дома письма уже нет.";
                statusMessageTimerMs = 2000;
                return;
            }

            if (currentLetter.CanDeliverNow(currentPhase))
            {
                currentLetter.Delivered = true;
                statusMessage = "Письмо доставлено: " + houseNames[houseIndex];
                statusMessageTimerMs = 2500;

                if (GetDeliveredCount() == letters.Count)
                {
                    gameWon = true;
                    statusMessage = "Все письма доставлены! Нажми R, чтобы сыграть еще раз.";
                    statusMessageTimerMs = 0;
                }
            }
            else
            {
                statusMessage = "Сейчас в доме никого нет. Нужное время: " + GetAllowedPhasesText(currentLetter);
                statusMessageTimerMs = 3000;
            }
        }

        private string GetAllowedPhasesText(LetterItem letter)
        {
            string text = "";

            for (int i = 0; i < letter.AllowedPhases.Count; i++)
            {
                if (i > 0)
                {
                    text += ", ";
                }

                text += GetPhaseText(letter.AllowedPhases[i]);
            }

            return text;
        }

        private int GetDeliveredCount()
        {
            int count = 0;

            for (int i = 0; i < letters.Count; i++)
            {
                if (letters[i].Delivered)
                {
                    count++;
                }
            }

            return count;
        }

        private int GetMatchSecondsLeft()
        {
            int left = (matchDurationMs - matchTimerMs) / 1000;
            if (left < 0) left = 0;
            return left;
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up) up = true;
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down) down = true;
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) left = true;
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) right = true;

            if (e.KeyCode == Keys.E)
            {
                TryInteract();
            }

            if (e.KeyCode == Keys.R)
            {
                ResetGame();
            }
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up) up = false;
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down) down = false;
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) left = false;
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) right = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            DrawMap(g);
            DrawPlayer(g);
            DrawUI(g);

            if (gameWon || gameLost)
            {
                DrawEndOverlay(g);
            }
        }

        private void DrawMap(Graphics g)
        {
            Color phaseOverlayColor = Color.FromArgb(0, 0, 0);

            if (currentPhase == TimePhase.Morning)
            {
                phaseOverlayColor = Color.FromArgb(30, 255, 220, 150);
            }
            else if (currentPhase == TimePhase.Day)
            {
                phaseOverlayColor = Color.FromArgb(10, 255, 255, 180);
            }
            else if (currentPhase == TimePhase.Evening)
            {
                phaseOverlayColor = Color.FromArgb(45, 255, 170, 80);
            }
            else if (currentPhase == TimePhase.Night)
            {
                phaseOverlayColor = Color.FromArgb(80, 40, 60, 120);
            }

            using (Brush outerGrass = new SolidBrush(Color.FromArgb(120, 160, 110)))
            using (Brush innerGrass = new SolidBrush(Color.FromArgb(142, 184, 142)))
            using (Brush roadBrush = new SolidBrush(Color.FromArgb(186, 186, 186)))
            using (Brush roadEdgeBrush = new SolidBrush(Color.FromArgb(160, 160, 160)))
            using (Pen border = new Pen(Color.FromArgb(30, 60, 30), 2))
            using (Brush treeBrush = new SolidBrush(Color.FromArgb(65, 125, 65)))
            using (Brush treeShadowBrush = new SolidBrush(Color.FromArgb(45, 95, 45)))
            using (Brush overlayBrush = new SolidBrush(phaseOverlayColor))
            {
                Rectangle outerMap = new Rectangle(
                    gameArea.X - 6,
                    gameArea.Y - 6,
                    gameArea.Width + 12,
                    gameArea.Height + 12
                );

                g.FillRectangle(outerGrass, outerMap);
                g.FillRectangle(innerGrass, gameArea);

                DrawRoads(g, roadBrush, roadEdgeBrush);
                DrawTrees(g, treeBrush, treeShadowBrush);
                DrawHouses(g);

                g.FillRectangle(overlayBrush, gameArea);
                g.DrawRectangle(border, gameArea);
            }
        }

        private void DrawRoads(Graphics g, Brush roadBrush, Brush roadEdgeBrush)
        {
            for (int i = 0; i < roads.Count; i++)
            {
                Rectangle road = roads[i];

                Rectangle shadow = new Rectangle(road.X + 2, road.Y + 2, road.Width, road.Height);
                g.FillRectangle(roadEdgeBrush, shadow);
                g.FillRectangle(roadBrush, road);
            }
        }

        private void DrawTrees(Graphics g, Brush treeBrush, Brush treeShadowBrush)
        {
            for (int i = 0; i < treeTops.Count; i++)
            {
                Rectangle t = treeTops[i];
                Rectangle shadow = new Rectangle(t.X + 4, t.Y + 4, t.Width, t.Height);

                g.FillEllipse(treeShadowBrush, shadow);
                g.FillEllipse(treeBrush, t);
                g.DrawEllipse(Pens.DarkGreen, t);
            }
        }

        private void DrawHouses(Graphics g)
        {
            for (int i = 0; i < houses.Count; i++)
            {
                Rectangle house = houses[i];

                using (Brush shadowBrush = new SolidBrush(Color.FromArgb(90, 90, 90)))
                using (Brush houseBrush = new SolidBrush(houseColors[i]))
                using (Brush roofBrush = new SolidBrush(Color.FromArgb(120, 70, 50)))
                using (Font houseFont = new Font("Arial", 9, FontStyle.Bold))
                {
                    Rectangle shadow = new Rectangle(house.X + 4, house.Y + 4, house.Width, house.Height);
                    g.FillRectangle(shadowBrush, shadow);

                    g.FillRectangle(houseBrush, house);
                    g.DrawRectangle(Pens.Black, house);

                    Rectangle roof = new Rectangle(house.X + 6, house.Y - 8, house.Width - 12, 12);
                    g.FillRectangle(roofBrush, roof);
                    g.DrawRectangle(Pens.Black, roof);

                    if (i == currentNearbyHouseIndex && !gameWon && !gameLost)
                    {
                        using (Pen nearPen = new Pen(Color.Yellow, 3))
                        {
                            Rectangle nearRect = new Rectangle(house.X - 4, house.Y - 4, house.Width + 8, house.Height + 8);
                            g.DrawRectangle(nearPen, nearRect);
                        }
                    }

                    Rectangle textRect = new Rectangle(house.X, house.Y + 24, house.Width, 22);

                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    g.DrawString(houseNames[i], houseFont, Brushes.Black, textRect, sf);
                }
            }
        }

        private void DrawPlayer(Graphics g)
        {
            if (gameWon || gameLost)
            {
                return;
            }

            Rectangle player = new Rectangle(playerX, playerY, playerW, playerH);

            g.FillRectangle(Brushes.RoyalBlue, player);
            g.DrawRectangle(Pens.Black, player);

            Rectangle bag = new Rectangle(player.Right - 8, player.Y + 6, 10, 12);
            g.FillRectangle(Brushes.SaddleBrown, bag);
            g.DrawRectangle(Pens.Black, bag);
        }

        private void DrawUI(Graphics g)
        {
            using (SolidBrush uiBrush = new SolidBrush(Color.FromArgb(36, 38, 54)))
            using (SolidBrush titleBrush = new SolidBrush(Color.White))
            using (SolidBrush goldBrush = new SolidBrush(Color.Gold))
            using (SolidBrush textBrush = new SolidBrush(Color.Gainsboro))
            using (SolidBrush goodBrush = new SolidBrush(Color.LightGreen))
            using (SolidBrush badBrush = new SolidBrush(Color.Salmon))
            using (Font titleFont = new Font("Arial", 16, FontStyle.Bold))
            using (Font blockFont = new Font("Arial", 11, FontStyle.Bold))
            using (Font textFont = new Font("Arial", 10))
            using (Font smallFont = new Font("Arial", 9))
            {
                g.FillRectangle(uiBrush, uiArea);
                g.DrawRectangle(Pens.Gray, uiArea);

                int x = uiArea.X + 22;
                int y = 28;

                g.DrawString("Почтальон времени", titleFont, titleBrush, x, y);
                y += 46;

                g.DrawString("Неделя 3", blockFont, goldBrush, x, y);
                y += 38;

                g.DrawString("Время суток:", blockFont, titleBrush, x, y);
                y += 28;
                g.DrawString(GetPhaseText(currentPhase), textFont, textBrush, x, y);
                y += 24;

                int secLeft = (phaseDurationMs - phaseTimerMs) / 1000;
                if (secLeft < 0) secLeft = 0;

                g.DrawString("До смены фазы: " + secLeft + " сек", textFont, textBrush, x, y);
                y += 24;

                g.DrawString("До конца игры: " + GetMatchSecondsLeft() + " сек", textFont, textBrush, x, y);
                y += 36;

                g.DrawString("Управление:", blockFont, titleBrush, x, y);
                y += 28;
                g.DrawString("WASD / стрелки", textFont, textBrush, x, y);
                y += 22;
                g.DrawString("E - доставить письмо", textFont, textBrush, x, y);
                y += 22;
                g.DrawString("R - рестарт", textFont, textBrush, x, y);
                y += 36;

                g.DrawString("Письма:", blockFont, titleBrush, x, y);
                y += 28;

                for (int i = 0; i < letters.Count; i++)
                {
                    string mark = letters[i].Delivered ? "[OK]" : "[ ]";
                    Brush statusBrush = letters[i].Delivered ? goodBrush : badBrush;

                    g.DrawString(mark + " " + houseNames[letters[i].HouseIndex], smallFont, statusBrush, x, y);
                    y += 18;

                    string times = "   " + GetAllowedPhasesText(letters[i]);
                    g.DrawString(times, smallFont, textBrush, x, y);
                    y += 22;
                }

                y += 8;
                g.DrawString("Доставлено: " + GetDeliveredCount() + " / " + letters.Count, blockFont, goldBrush, x, y);
                y += 38;

                g.DrawString("Статус:", blockFont, titleBrush, x, y);
                y += 26;

                Rectangle statusRect = new Rectangle(x, y, uiArea.Width - 45, 80);
                g.DrawString(statusMessage, smallFont, textBrush, statusRect);

                y += 92;

                g.DrawString("Координаты:", blockFont, titleBrush, x, y);
                y += 26;
                g.DrawString("X = " + playerX, textFont, textBrush, x, y);
                y += 22;
                g.DrawString("Y = " + playerY, textFont, textBrush, x, y);
            }
        }

        private void DrawEndOverlay(Graphics g)
        {
            using (Brush darkBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
            using (Brush panelBrush = new SolidBrush(Color.FromArgb(235, 30, 30, 40)))
            using (Brush titleBrush = new SolidBrush(Color.White))
            using (Brush textBrush = new SolidBrush(Color.Gainsboro))
            using (Brush accentBrush = new SolidBrush(gameWon ? Color.LightGreen : Color.Salmon))
            using (Font titleFont = new Font("Arial", 24, FontStyle.Bold))
            using (Font textFont = new Font("Arial", 12, FontStyle.Regular))
            using (Font bigFont = new Font("Arial", 16, FontStyle.Bold))
            {
                g.FillRectangle(darkBrush, gameArea);

                Rectangle panel = new Rectangle(
                    gameArea.X + gameArea.Width / 2 - 220,
                    gameArea.Y + gameArea.Height / 2 - 110,
                    440,
                    220
                );

                g.FillRectangle(panelBrush, panel);
                g.DrawRectangle(Pens.White, panel);

                string title = gameWon ? "ПОБЕДА" : "ПОРАЖЕНИЕ";
                string line1 = gameWon
                    ? "Ты доставил все письма."
                    : "Ты не успел доставить все письма.";
                string line2 = "Нажми R, чтобы начать заново.";

                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                Rectangle titleRect = new Rectangle(panel.X, panel.Y + 25, panel.Width, 40);
                Rectangle line1Rect = new Rectangle(panel.X + 20, panel.Y + 85, panel.Width - 40, 30);
                Rectangle line2Rect = new Rectangle(panel.X + 20, panel.Y + 135, panel.Width - 40, 30);
                Rectangle statRect = new Rectangle(panel.X + 20, panel.Y + 175, panel.Width - 40, 25);

                g.DrawString(title, titleFont, accentBrush, titleRect, sf);
                g.DrawString(line1, bigFont, titleBrush, line1Rect, sf);
                g.DrawString(line2, textFont, textBrush, line2Rect, sf);
                g.DrawString("Доставлено: " + GetDeliveredCount() + " / " + letters.Count, textFont, textBrush, statRect, sf);
            }
        }
    }
}