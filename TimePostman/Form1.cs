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

        int currentNearbyHouseIndex = -1;

        string statusMessage = "Курьер готов. Подходи к дому и нажимай E.";
        int statusMessageTimerMs = 0;

        bool testMode = false;
        bool gameStarted = false;

        GameSession session;

        public Form1()
        {
            InitializeComponent();

            session = new GameSession(testMode);

            this.Text = "Симулятор курьера";
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

            playerX = startPlayerX;
            playerY = startPlayerY;

            CreateTrees();

            timer.Interval = 16;
            timer.Tick += Timer_Tick;
            timer.Start();

            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
        }

        private void ResetGame()
        {
            up = false;
            down = false;
            left = false;
            right = false;

            playerX = startPlayerX;
            playerY = startPlayerY;

            currentNearbyHouseIndex = -1;

            session.Reset();
            gameStarted = true;

            statusMessage = "Новая смена началась. Доставляй заказы.";
            statusMessageTimerMs = 2200;
        }

        private void StartGame()
        {
            gameStarted = true;
            playerX = startPlayerX;
            playerY = startPlayerY;
            statusMessage = "Смена началась. Подходи к дому и нажимай E.";
            statusMessageTimerMs = 2200;
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
            if (!gameStarted)
            {
                Invalidate();
                return;
            }

            TimePhase oldPhase = session.CurrentPhase;
            bool oldWon = session.GameWon;
            bool oldLost = session.GameLost;

            if (!session.GameWon && !session.GameLost)
            {
                int dx = 0;
                int dy = 0;

                if (up) dy -= speed;
                if (down) dy += speed;
                if (left) dx -= speed;
                if (right) dx += speed;

                MovePlayer(dx, dy);
                currentNearbyHouseIndex = GetNearbyHouseIndex();
            }

            session.AdvanceTime(timer.Interval);

            if (!oldWon && session.GameWon)
            {
                statusMessage = "Все заказы доставлены. Смена закрыта успешно.";
                statusMessageTimerMs = 0;
            }

            if (!oldLost && session.GameLost)
            {
                statusMessage = "Смена закончилась. Не все заказы были закрыты.";
                statusMessageTimerMs = 0;
            }

            if (oldPhase != session.CurrentPhase && !session.GameWon && !session.GameLost)
            {
                statusMessage = "Время сменилось: " + session.GetPhaseText(session.CurrentPhase);
                statusMessageTimerMs = 2200;
            }

            if (statusMessageTimerMs > 0)
            {
                statusMessageTimerMs -= timer.Interval;

                if (statusMessageTimerMs <= 0)
                {
                    statusMessageTimerMs = 0;

                    if (!session.GameWon && !session.GameLost)
                    {
                        statusMessage = "Подойди к дому и нажми E, чтобы передать заказ.";
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

        private DeliveryOrder? GetNearbyOrder()
        {
            if (currentNearbyHouseIndex < 0)
            {
                return null;
            }

            for (int i = 0; i < session.Orders.Count; i++)
            {
                if (session.Orders[i].HouseIndex == currentNearbyHouseIndex && !session.Orders[i].Delivered)
                {
                    return session.Orders[i];
                }
            }

            return null;
        }

        private void TryInteract()
        {
            if (!gameStarted)
            {
                return;
            }

            DeliveryAttemptResult result = session.TryDeliver(currentNearbyHouseIndex);

            if (result == DeliveryAttemptResult.TooFar)
            {
                statusMessage = "Слишком далеко. Подойди ближе к дому.";
                statusMessageTimerMs = 1800;
            }
            else if (result == DeliveryAttemptResult.NoOrder)
            {
                statusMessage = "Для этого адреса активных заказов уже нет.";
                statusMessageTimerMs = 2200;
            }
            else if (result == DeliveryAttemptResult.WrongTime)
            {
                DeliveryOrder? order = null;

                for (int i = 0; i < session.Orders.Count; i++)
                {
                    if (session.Orders[i].HouseIndex == currentNearbyHouseIndex && !session.Orders[i].Delivered)
                    {
                        order = session.Orders[i];
                        break;
                    }
                }

                if (order != null)
                {
                    statusMessage = "Клиента нет дома. Доступно: " + session.GetAllowedPhasesText(order);
                }
                else
                {
                    statusMessage = "Сейчас заказ не принимается.";
                }

                statusMessageTimerMs = 3000;
            }
            else if (result == DeliveryAttemptResult.Delivered)
            {
                statusMessage = "Заказ доставлен: " + houseNames[currentNearbyHouseIndex];
                statusMessageTimerMs = 2200;

                if (session.GameWon)
                {
                    statusMessage = "Все заказы доставлены. Смена закрыта успешно.";
                    statusMessageTimerMs = 0;
                }
            }
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!gameStarted && e.KeyCode == Keys.Enter)
            {
                StartGame();
                return;
            }

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
            DrawInteractionHint(g);
            DrawUI(g);

            if (!gameStarted)
            {
                DrawStartOverlay(g);
            }
            else if (session.GameWon || session.GameLost)
            {
                DrawEndOverlay(g);
            }
        }

        private void DrawMap(Graphics g)
        {
            Color phaseOverlayColor = Color.FromArgb(0, 0, 0);

            if (session.CurrentPhase == TimePhase.Morning)
            {
                phaseOverlayColor = Color.FromArgb(30, 255, 220, 150);
            }
            else if (session.CurrentPhase == TimePhase.Day)
            {
                phaseOverlayColor = Color.FromArgb(10, 255, 255, 180);
            }
            else if (session.CurrentPhase == TimePhase.Evening)
            {
                phaseOverlayColor = Color.FromArgb(45, 255, 170, 80);
            }
            else if (session.CurrentPhase == TimePhase.Night)
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

                    if (i == currentNearbyHouseIndex && gameStarted && !session.GameWon && !session.GameLost)
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
            if (!gameStarted || session.GameWon || session.GameLost)
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

        private void DrawInteractionHint(Graphics g)
        {
            if (!gameStarted || session.GameWon || session.GameLost)
            {
                return;
            }

            if (currentNearbyHouseIndex < 0)
            {
                return;
            }

            string hintText = "";
            DeliveryOrder? order = GetNearbyOrder();

            if (order == null)
            {
                hintText = "Здесь уже нет активных заказов.";
            }
            else if (order.CanDeliverNow(session.CurrentPhase))
            {
                hintText = "Нажми E, чтобы передать заказ.";
            }
            else
            {
                hintText = "Клиент принимает: " + session.GetAllowedPhasesText(order);
            }

            Rectangle hintRect = new Rectangle(
                gameArea.X + 18,
                gameArea.Bottom - 58,
                gameArea.Width - 36,
                38
            );

            using (Brush bg = new SolidBrush(Color.FromArgb(185, 25, 25, 35)))
            using (Brush textBrush = new SolidBrush(Color.White))
            using (Font font = new Font("Arial", 10, FontStyle.Bold))
            {
                g.FillRectangle(bg, hintRect);
                g.DrawRectangle(Pens.White, hintRect);
                g.DrawString(hintText, font, textBrush, new Rectangle(hintRect.X + 10, hintRect.Y + 9, hintRect.Width - 20, 20));
            }
        }

        private void DrawUI(Graphics g)
        {
            using (SolidBrush uiBrush = new SolidBrush(Color.FromArgb(36, 38, 54)))
            using (SolidBrush titleBrush = new SolidBrush(Color.White))
            using (SolidBrush accentBrush = new SolidBrush(Color.Gold))
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

                int x = uiArea.X + 20;
                int y = 24;

                g.DrawString("Симулятор курьера", titleFont, titleBrush, x, y);
                y += 40;

                g.DrawString("Финальная версия MVP", blockFont, accentBrush, x, y);
                y += 34;

                g.DrawString("Цель смены:", blockFont, titleBrush, x, y);
                y += 24;
                g.DrawString("Закрыть все заказы до конца времени.", smallFont, textBrush, new Rectangle(x, y, uiArea.Width - 40, 35));
                y += 42;

                g.DrawString("Время суток:", blockFont, titleBrush, x, y);
                y += 24;
                g.DrawString(gameStarted ? session.GetPhaseText(session.CurrentPhase) : "-", textFont, textBrush, x, y);
                y += 20;

                g.DrawString("До смены фазы: " + (gameStarted ? session.GetPhaseSecondsLeft().ToString() : "-") + " сек", textFont, textBrush, x, y);
                y += 20;
                g.DrawString("До конца смены: " + (gameStarted ? session.GetMatchSecondsLeft().ToString() : "-") + " сек", textFont, textBrush, x, y);
                y += 32;

                g.DrawString("Управление:", blockFont, titleBrush, x, y);
                y += 24;
                g.DrawString("WASD / стрелки", textFont, textBrush, x, y);
                y += 18;
                g.DrawString("E - передать заказ", textFont, textBrush, x, y);
                y += 18;
                g.DrawString("R - новая смена", textFont, textBrush, x, y);
                y += 28;

                g.DrawString("Заказы:", blockFont, titleBrush, x, y);
                y += 24;

                for (int i = 0; i < session.Orders.Count; i++)
                {
                    DeliveryOrder order = session.Orders[i];
                    string mark = order.Delivered ? "[OK]" : "[ ]";
                    Brush lineBrush = order.Delivered ? goodBrush : badBrush;

                    g.DrawString(mark + " " + order.Title, smallFont, lineBrush, x, y);
                    y += 16;
                    g.DrawString("   " + houseNames[order.HouseIndex] + " | " + session.GetAllowedPhasesText(order), smallFont, textBrush, x, y);
                    y += 20;
                }

                y += 4;
                g.DrawString("Выполнено: " + session.GetDeliveredCount() + " / " + session.Orders.Count, blockFont, accentBrush, x, y);
                y += 32;

                g.DrawString("Статус:", blockFont, titleBrush, x, y);
                y += 22;

                Rectangle statusRect = new Rectangle(x, y, uiArea.Width - 40, 92);
                g.DrawString(statusMessage, smallFont, textBrush, statusRect);
            }
        }

        private void DrawStartOverlay(Graphics g)
        {
            using (Brush darkBrush = new SolidBrush(Color.FromArgb(170, 0, 0, 0)))
            using (Brush panelBrush = new SolidBrush(Color.FromArgb(235, 28, 30, 44)))
            using (Brush titleBrush = new SolidBrush(Color.White))
            using (Brush accentBrush = new SolidBrush(Color.Gold))
            using (Brush textBrush = new SolidBrush(Color.Gainsboro))
            using (Font titleFont = new Font("Arial", 24, FontStyle.Bold))
            using (Font subFont = new Font("Arial", 13, FontStyle.Bold))
            using (Font textFont = new Font("Arial", 11))
            {
                g.FillRectangle(darkBrush, gameArea);

                Rectangle panel = new Rectangle(
                    gameArea.X + gameArea.Width / 2 - 240,
                    gameArea.Y + gameArea.Height / 2 - 150,
                    480,
                    300
                );

                g.FillRectangle(panelBrush, panel);
                g.DrawRectangle(Pens.White, panel);

                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                g.DrawString("СИМУЛЯТОР КУРЬЕРА", titleFont, titleBrush, new Rectangle(panel.X, panel.Y + 25, panel.Width, 40), sf);
                g.DrawString("Мини-игра про доставку заказов по адресам", subFont, accentBrush, new Rectangle(panel.X, panel.Y + 80, panel.Width, 28), sf);

                Rectangle textRect = new Rectangle(panel.X + 35, panel.Y + 130, panel.Width - 70, 90);
                string info = "Твоя задача — доставить все заказы до конца смены.\nУчитывай время суток: не каждый клиент доступен всегда.\nПодходи к дому и нажимай E.";
                g.DrawString(info, textFont, textBrush, textRect, sf);

                g.DrawString("Enter — начать смену", subFont, accentBrush, new Rectangle(panel.X, panel.Y + 245, panel.Width, 28), sf);
            }
        }

        private void DrawEndOverlay(Graphics g)
        {
            using (Brush darkBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
            using (Brush panelBrush = new SolidBrush(Color.FromArgb(235, 30, 30, 40)))
            using (Brush titleBrush = new SolidBrush(Color.White))
            using (Brush textBrush = new SolidBrush(Color.Gainsboro))
            using (Brush accentBrush = new SolidBrush(session.GameWon ? Color.LightGreen : Color.Salmon))
            using (Font titleFont = new Font("Arial", 24, FontStyle.Bold))
            using (Font textFont = new Font("Arial", 12))
            using (Font bigFont = new Font("Arial", 16, FontStyle.Bold))
            {
                g.FillRectangle(darkBrush, gameArea);

                Rectangle panel = new Rectangle(
                    gameArea.X + gameArea.Width / 2 - 240,
                    gameArea.Y + gameArea.Height / 2 - 130,
                    480,
                    250
                );

                g.FillRectangle(panelBrush, panel);
                g.DrawRectangle(Pens.White, panel);

                string title = session.GameWon ? "СМЕНА УСПЕШНО ЗАВЕРШЕНА" : "СМЕНА ЗАКОНЧИЛАСЬ";
                string line1 = session.GameWon
                    ? "Ты доставил все заказы вовремя."
                    : "Не все заказы были доставлены до конца смены.";
                string line2 = "Нажми R, чтобы начать заново.";

                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                g.DrawString(title, titleFont, accentBrush, new Rectangle(panel.X + 10, panel.Y + 25, panel.Width - 20, 40), sf);
                g.DrawString(line1, bigFont, titleBrush, new Rectangle(panel.X + 20, panel.Y + 90, panel.Width - 40, 30), sf);
                g.DrawString(line2, textFont, textBrush, new Rectangle(panel.X + 20, panel.Y + 135, panel.Width - 40, 25), sf);
                g.DrawString("Выполнено: " + session.GetDeliveredCount() + " / " + session.Orders.Count, textFont, textBrush, new Rectangle(panel.X + 20, panel.Y + 175, panel.Width - 40, 25), sf);
                g.DrawString("Осталось времени: " + session.GetMatchSecondsLeft() + " сек", textFont, textBrush, new Rectangle(panel.X + 20, panel.Y + 200, panel.Width - 40, 25), sf);
            }
        }
    }
}