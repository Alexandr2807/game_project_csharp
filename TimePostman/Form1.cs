using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace TimePostman
{
    public partial class Form1 : Form
    {
        private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        private readonly GameMap map;
        private readonly GameSession session;
        private readonly GameController controller;

        private readonly Stopwatch stopwatch = new Stopwatch();
        private long lastElapsedMs = 0;

        private bool showOrdersSheet = false;

        public Form1()
        {
            InitializeComponent();

            map = new GameMap();
            session = new GameSession(false);
            controller = new GameController(session, map);

            Text = "Симулятор курьера";
            ClientSize = new Size(map.WindowWidth, map.WindowHeight);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            KeyPreview = true;

            timer.Interval = 16;
            timer.Tick += Timer_Tick;
            timer.Start();

            stopwatch.Start();
            lastElapsedMs = stopwatch.ElapsedMilliseconds;

            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            long now = stopwatch.ElapsedMilliseconds;
            int deltaMs = (int)(now - lastElapsedMs);
            lastElapsedMs = now;

            if (deltaMs < 1)
                deltaMs = 1;

            if (deltaMs > 100)
                deltaMs = 100;

            controller.Update(deltaMs);
            Invalidate();
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Q)
            {
                showOrdersSheet = !showOrdersSheet;
                Invalidate();
                return;
            }

            controller.HandleKeyDown(e.KeyCode);
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            controller.HandleKeyUp(e.KeyCode);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            DrawMap(g);
            DrawPlayer(g);
            DrawHouseHint(g);
            DrawUI(g);

            if (showOrdersSheet && controller.GameStarted)
                DrawOrdersSheet(g);

            if (!controller.GameStarted)
                DrawStartOverlay(g);
            else if (session.GameWon || session.GameLost)
                DrawEndOverlay(g);
        }

        private void DrawMap(Graphics g)
        {
            Color phaseOverlayColor = Color.FromArgb(0, 0, 0);

            if (session.CurrentPhase == TimePhase.Morning)
                phaseOverlayColor = Color.FromArgb(30, 255, 220, 150);
            else if (session.CurrentPhase == TimePhase.Day)
                phaseOverlayColor = Color.FromArgb(10, 255, 255, 180);
            else if (session.CurrentPhase == TimePhase.Evening)
                phaseOverlayColor = Color.FromArgb(45, 255, 170, 80);
            else if (session.CurrentPhase == TimePhase.Night)
                phaseOverlayColor = Color.FromArgb(80, 40, 60, 120);

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
                    map.GameArea.X - 6,
                    map.GameArea.Y - 6,
                    map.GameArea.Width + 12,
                    map.GameArea.Height + 12
                );

                g.FillRectangle(outerGrass, outerMap);
                g.FillRectangle(innerGrass, map.GameArea);

                DrawRoads(g, roadBrush, roadEdgeBrush);
                DrawTrees(g, treeBrush, treeShadowBrush);
                DrawHouses(g);

                g.FillRectangle(overlayBrush, map.GameArea);
                g.DrawRectangle(border, map.GameArea);
            }
        }

        private void DrawRoads(Graphics g, Brush roadBrush, Brush roadEdgeBrush)
        {
            for (int i = 0; i < map.Roads.Count; i++)
            {
                Rectangle road = map.Roads[i];
                Rectangle shadow = new Rectangle(road.X + 2, road.Y + 2, road.Width, road.Height);
                g.FillRectangle(roadEdgeBrush, shadow);
                g.FillRectangle(roadBrush, road);
            }
        }

        private void DrawTrees(Graphics g, Brush treeBrush, Brush treeShadowBrush)
        {
            for (int i = 0; i < map.TreeTops.Count; i++)
            {
                Rectangle t = map.TreeTops[i];
                Rectangle shadow = new Rectangle(t.X + 4, t.Y + 4, t.Width, t.Height);

                g.FillEllipse(treeShadowBrush, shadow);
                g.FillEllipse(treeBrush, t);
                g.DrawEllipse(Pens.DarkGreen, t);
            }
        }

        private void DrawHouses(Graphics g)
        {
            for (int i = 0; i < map.Houses.Count; i++)
            {
                Rectangle house = map.Houses[i];

                using (Brush shadowBrush = new SolidBrush(Color.FromArgb(90, 90, 90)))
                using (Brush houseBrush = new SolidBrush(map.HouseColors[i]))
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

                    if (controller.GameStarted && !session.GameWon && !session.GameLost && i == controller.CurrentNearbyHouseIndex)
                    {
                        Color frameColor = Color.Yellow;

                        DeliveryOrder? activeOrder = null;
                        for (int j = 0; j < session.Orders.Count; j++)
                        {
                            if (session.Orders[j].HouseIndex == i && !session.Orders[j].Delivered)
                            {
                                activeOrder = session.Orders[j];
                                break;
                            }
                        }

                        if (activeOrder == null)
                            frameColor = Color.Gray;
                        else if (activeOrder.CanDeliverNow(session.CurrentPhase))
                            frameColor = Color.LimeGreen;
                        else
                            frameColor = Color.Orange;

                        using (Pen statePen = new Pen(frameColor, 4))
                        {
                            Rectangle stateRect = new Rectangle(house.X - 3, house.Y - 3, house.Width + 6, house.Height + 6);
                            g.DrawRectangle(statePen, stateRect);
                        }
                    }

                    Rectangle textRect = new Rectangle(house.X, house.Y + 24, house.Width, 22);

                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };

                    g.DrawString(map.HouseNames[i], houseFont, Brushes.Black, textRect, sf);
                }
            }
        }

        private void DrawPlayer(Graphics g)
        {
            if (!controller.GameStarted || session.GameWon || session.GameLost)
                return;

            Rectangle player = new Rectangle(controller.PlayerX, controller.PlayerY, controller.PlayerWidth, controller.PlayerHeight);

            g.FillRectangle(Brushes.RoyalBlue, player);
            g.DrawRectangle(Pens.Black, player);

            Rectangle bag = new Rectangle(player.Right - 8, player.Y + 6, 10, 12);
            g.FillRectangle(Brushes.SaddleBrown, bag);
            g.DrawRectangle(Pens.Black, bag);
        }

        private void DrawHouseHint(Graphics g)
        {
            if (!controller.GameStarted || session.GameWon || session.GameLost)
                return;

            if (controller.CurrentNearbyHouseIndex < 0)
                return;

            int houseIndex = controller.CurrentNearbyHouseIndex;
            Rectangle house = map.Houses[houseIndex];

            DeliveryOrder? activeOrder = null;
            for (int i = 0; i < session.Orders.Count; i++)
            {
                if (session.Orders[i].HouseIndex == houseIndex && !session.Orders[i].Delivered)
                {
                    activeOrder = session.Orders[i];
                    break;
                }
            }

            string hintText;
            Color bgColor;

            if (activeOrder == null)
            {
                hintText = "Готово";
                bgColor = Color.FromArgb(220, 100, 100, 100);
            }
            else if (activeOrder.CanDeliverNow(session.CurrentPhase))
            {
                hintText = "E — передать заказ";
                bgColor = Color.FromArgb(220, 40, 120, 60);
            }
            else
            {
                hintText = "Не сейчас";
                bgColor = Color.FromArgb(220, 130, 70, 45);
            }

            using (Font font = new Font("Arial", 9, FontStyle.Bold))
            {
                Size textSize = TextRenderer.MeasureText(hintText, font);

                Rectangle hintRect = new Rectangle(
                    house.X + house.Width / 2 - textSize.Width / 2 - 10,
                    house.Y - 36,
                    textSize.Width + 20,
                    24
                );

                if (hintRect.X < map.GameArea.X + 5)
                    hintRect.X = map.GameArea.X + 5;

                if (hintRect.Right > map.GameArea.Right - 5)
                    hintRect.X = map.GameArea.Right - 5 - hintRect.Width;

                using (Brush bgBrush = new SolidBrush(bgColor))
                using (Brush textBrush = new SolidBrush(Color.White))
                {
                    g.FillRectangle(bgBrush, hintRect);
                    g.DrawRectangle(Pens.White, hintRect);

                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };

                    g.DrawString(hintText, font, textBrush, hintRect, sf);
                }
            }
        }

        private void DrawUI(Graphics g)
        {
            using (SolidBrush uiBrush = new SolidBrush(Color.FromArgb(36, 38, 54)))
            using (SolidBrush titleBrush = new SolidBrush(Color.White))
            using (SolidBrush accentBrush = new SolidBrush(Color.Gold))
            using (SolidBrush textBrush = new SolidBrush(Color.Gainsboro))
            using (Font titleFont = new Font("Arial", 16, FontStyle.Bold))
            using (Font blockFont = new Font("Arial", 11, FontStyle.Bold))
            using (Font textFont = new Font("Arial", 10))
            using (Font smallFont = new Font("Arial", 9))
            {
                g.FillRectangle(uiBrush, map.UiArea);
                g.DrawRectangle(Pens.Gray, map.UiArea);

                int x = map.UiArea.X + 20;
                int y = 24;

                g.DrawString("Симулятор курьера", titleFont, titleBrush, x, y);
                y += 42;

                g.DrawString("Время суток:", blockFont, titleBrush, x, y);
                y += 24;
                g.DrawString(controller.GameStarted ? session.GetPhaseText(session.CurrentPhase) : "-", textFont, textBrush, x, y);
                y += 20;
                g.DrawString("До смены фазы: " + (controller.GameStarted ? session.GetPhaseSecondsLeft().ToString() : "-") + " сек", textFont, textBrush, x, y);
                y += 34;

                g.DrawString("Смена:", blockFont, titleBrush, x, y);
                y += 24;
                g.DrawString("Цель: закрыть все заказы", textFont, textBrush, x, y);
                y += 20;
                g.DrawString("До конца смены: " + (controller.GameStarted ? session.GetMatchSecondsLeft().ToString() : "-") + " сек", textFont, textBrush, x, y);
                y += 20;
                g.DrawString("Доступно сейчас: " + (controller.GameStarted ? session.GetAvailableNowCount().ToString() : "-"), textFont, accentBrush, x, y);
                y += 32;

                g.DrawString("Управление:", blockFont, titleBrush, x, y);
                y += 24;
                g.DrawString("WASD / стрелки", textFont, textBrush, x, y);
                y += 18;
                g.DrawString("E - передать заказ", textFont, textBrush, x, y);
                y += 18;
                g.DrawString("Q - маршрутный лист", textFont, textBrush, x, y);
                y += 18;
                g.DrawString("R - новая смена", textFont, textBrush, x, y);
                y += 28;

                g.DrawString("Прогресс:", blockFont, titleBrush, x, y);
                y += 24;
                g.DrawString("Выполнено: " + session.GetDeliveredCount() + " / " + session.Orders.Count, textFont, accentBrush, x, y);
                y += 20;
                g.DrawString("Эффективность: " + session.GetCompletionPercent() + "%", textFont, textBrush, x, y);
                y += 32;

                g.DrawString("Статус:", blockFont, titleBrush, x, y);
                y += 22;

                Rectangle statusRect = new Rectangle(x, y, map.UiArea.Width - 40, 110);
                g.DrawString(controller.StatusMessage, smallFont, textBrush, statusRect);
            }
        }

        private void DrawOrdersSheet(Graphics g)
        {
            using (Brush darkBrush = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
            using (Brush paperBrush = new SolidBrush(Color.FromArgb(245, 235, 210)))
            using (Brush shadowBrush = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
            using (Brush textBrush = new SolidBrush(Color.FromArgb(30, 30, 30)))
            using (Brush doneBrush = new SolidBrush(Color.FromArgb(40, 120, 50)))
            using (Brush waitBrush = new SolidBrush(Color.FromArgb(140, 70, 40)))
            using (Brush greenLegend = new SolidBrush(Color.LimeGreen))
            using (Brush orangeLegend = new SolidBrush(Color.Orange))
            using (Brush grayLegend = new SolidBrush(Color.Gray))
            using (Font titleFont = new Font("Arial", 16, FontStyle.Bold))
            using (Font textFont = new Font("Arial", 10))
            using (Font smallFont = new Font("Arial", 9))
            using (Font tinyFont = new Font("Arial", 8))
            {
                g.FillRectangle(darkBrush, map.GameArea);

                Rectangle shadowRect = new Rectangle(
                    map.GameArea.X + 108,
                    map.GameArea.Y + 68,
                    map.GameArea.Width - 160,
                    map.GameArea.Height - 135
                );

                Rectangle paperRect = new Rectangle(
                    map.GameArea.X + 100,
                    map.GameArea.Y + 60,
                    map.GameArea.Width - 160,
                    map.GameArea.Height - 135
                );

                g.FillRectangle(shadowBrush, shadowRect);
                g.FillRectangle(paperBrush, paperRect);
                g.DrawRectangle(Pens.SaddleBrown, paperRect);

                int leftX = paperRect.X + 24;
                int rightX = paperRect.X + paperRect.Width / 2 + 18;
                int startY = paperRect.Y + 22;
                int columnWidth = paperRect.Width / 2 - 42;
                int cardHeight = 86;

                g.DrawString("Маршрутный лист", titleFont, textBrush, leftX, startY);
                g.DrawString("Q — закрыть список", smallFont, textBrush, paperRect.Right - 175, startY + 6);

                int summaryY = startY + 30;
                g.DrawString("Всего заказов: " + session.Orders.Count, smallFont, textBrush, leftX, summaryY);
                g.DrawString("Выполнено: " + session.GetDeliveredCount(), smallFont, textBrush, leftX + 140, summaryY);
                g.DrawString("Время суток: " + session.GetPhaseText(session.CurrentPhase), smallFont, textBrush, leftX + 255, summaryY);
                g.DrawString("Доступно сейчас: " + session.GetAvailableNowCount(), smallFont, textBrush, leftX + 395, summaryY);

                int legendY = summaryY + 24;
                g.FillRectangle(greenLegend, leftX, legendY + 2, 12, 12);
                g.DrawString("можно доставить", tinyFont, textBrush, leftX + 18, legendY);

                g.FillRectangle(orangeLegend, leftX + 120, legendY + 2, 12, 12);
                g.DrawString("не сейчас", tinyFont, textBrush, leftX + 138, legendY);

                g.FillRectangle(grayLegend, leftX + 220, legendY + 2, 12, 12);
                g.DrawString("выполнено", tinyFont, textBrush, leftX + 238, legendY);

                int yLeft = startY + 82;
                int yRight = startY + 82;

                using (Pen centerLine = new Pen(Color.FromArgb(180, 160, 120)))
                {
                    g.DrawLine(centerLine, paperRect.X + paperRect.Width / 2, paperRect.Y + 70, paperRect.X + paperRect.Width / 2, paperRect.Bottom - 18);
                }

                for (int i = 0; i < session.Orders.Count; i++)
                {
                    DeliveryOrder order = session.Orders[i];
                    Brush lineBrush = order.Delivered ? doneBrush : waitBrush;
                    string mark = order.Delivered ? "[Выполнен]" : "[Активен]";

                    int x;
                    int y;

                    if (i < 5)
                    {
                        x = leftX;
                        y = yLeft;
                    }
                    else
                    {
                        x = rightX;
                        y = yRight;
                    }

                    Rectangle titleRect = new Rectangle(x, y, columnWidth, 20);
                    Rectangle personRect = new Rectangle(x + 6, y + 24, columnWidth - 6, 18);
                    Rectangle timeRect = new Rectangle(x + 6, y + 44, columnWidth - 6, 18);

                    g.DrawString(mark + " " + order.Title, textFont, lineBrush, titleRect);
                    g.DrawString("Получатель: " + map.HouseNames[order.HouseIndex], smallFont, textBrush, personRect);
                    g.DrawString("Время: " + session.GetAllowedPhasesText(order), smallFont, textBrush, timeRect);

                    using (Pen linePen = new Pen(Color.FromArgb(180, 160, 120)))
                    {
                        g.DrawLine(linePen, x, y + cardHeight - 8, x + columnWidth - 8, y + cardHeight - 8);
                    }

                    if (i < 5)
                        yLeft += cardHeight;
                    else
                        yRight += cardHeight;
                }
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
                g.FillRectangle(darkBrush, map.GameArea);

                Rectangle panel = new Rectangle(
                    map.GameArea.X + map.GameArea.Width / 2 - 240,
                    map.GameArea.Y + map.GameArea.Height / 2 - 150,
                    480,
                    300
                );

                g.FillRectangle(panelBrush, panel);
                g.DrawRectangle(Pens.White, panel);

                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

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
                g.FillRectangle(darkBrush, map.GameArea);

                Rectangle panel = new Rectangle(
                    map.GameArea.X + map.GameArea.Width / 2 - 240,
                    map.GameArea.Y + map.GameArea.Height / 2 - 145,
                    480,
                    280
                );

                g.FillRectangle(panelBrush, panel);
                g.DrawRectangle(Pens.White, panel);

                string title = session.GameWon ? "СМЕНА УСПЕШНО ЗАВЕРШЕНА" : "СМЕНА ЗАКОНЧИЛАСЬ";
                string line1 = session.GameWon
                    ? "Ты доставил все заказы вовремя."
                    : "Не все заказы были доставлены до конца смены.";
                string line2 = "Нажми R, чтобы начать заново.";

                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                g.DrawString(title, titleFont, accentBrush, new Rectangle(panel.X + 10, panel.Y + 22, panel.Width - 20, 40), sf);
                g.DrawString(line1, bigFont, titleBrush, new Rectangle(panel.X + 20, panel.Y + 78, panel.Width - 40, 30), sf);
                g.DrawString(line2, textFont, textBrush, new Rectangle(panel.X + 20, panel.Y + 118, panel.Width - 40, 25), sf);

                g.DrawString("Выполнено: " + session.GetDeliveredCount() + " / " + session.Orders.Count, textFont, textBrush, new Rectangle(panel.X + 20, panel.Y + 158, panel.Width - 40, 22), sf);
                g.DrawString("Осталось времени: " + session.GetMatchSecondsLeft() + " сек", textFont, textBrush, new Rectangle(panel.X + 20, panel.Y + 184, panel.Width - 40, 22), sf);
                g.DrawString("Длительность смены: " + session.GetElapsedMatchSeconds() + " сек", textFont, textBrush, new Rectangle(panel.X + 20, panel.Y + 210, panel.Width - 40, 22), sf);
                g.DrawString("Эффективность: " + session.GetCompletionPercent() + "%", textFont, textBrush, new Rectangle(panel.X + 20, panel.Y + 236, panel.Width - 40, 22), sf);
            }
        }
    }
}