using System;
using System.Collections.Generic;
using System.Drawing;

namespace TimePostman
{
    public class GameMap
    {
        public int WindowWidth { get; } = 1240;
        public int WindowHeight { get; } = 800;
        public int UiWidth { get; } = 320;

        public Rectangle GameArea { get; }
        public Rectangle UiArea { get; }

        public List<Rectangle> Houses { get; } = new List<Rectangle>();
        public List<string> HouseNames { get; } = new List<string>();
        public List<Color> HouseColors { get; } = new List<Color>();
        public List<Rectangle> Roads { get; } = new List<Rectangle>();
        public List<Rectangle> TreeTops { get; } = new List<Rectangle>();

        public int StartPlayerX { get; }
        public int StartPlayerY { get; }

        public GameMap()
        {
            int mapMarginLeft = 40;
            int mapMarginTop = 40;
            int mapMarginBottom = 40;

            GameArea = new Rectangle(
                mapMarginLeft,
                mapMarginTop,
                WindowWidth - UiWidth - mapMarginLeft,
                WindowHeight - mapMarginTop - mapMarginBottom
            );

            UiArea = new Rectangle(GameArea.Right, 0, UiWidth, WindowHeight);

            int houseW = 92;
            int houseH = 72;
            int leftRoadX = GameArea.X + 90;

            Roads.Add(new Rectangle(420, GameArea.Y, 130, GameArea.Height));
            Roads.Add(new Rectangle(GameArea.X, 350, GameArea.Width, 100));
            Roads.Add(new Rectangle(leftRoadX, GameArea.Y, 120, GameArea.Height));

            Houses.Add(new Rectangle(285, 255, houseW, houseH));
            Houses.Add(new Rectangle(567, 98, houseW, houseH));
            Houses.Add(new Rectangle(567, 250, houseW, houseH));
            Houses.Add(new Rectangle(283, 486, houseW, houseH));
            Houses.Add(new Rectangle(567, 475, houseW, houseH));
            Houses.Add(new Rectangle(770, 475, houseW, houseH));
            Houses.Add(new Rectangle(285, 90, houseW, houseH));
            Houses.Add(new Rectangle(770, 250, houseW, houseH));
            Houses.Add(new Rectangle(283, 620, houseW, houseH));
            Houses.Add(new Rectangle(567, 620, houseW, houseH));

            FillRandomResidentNames();

            HouseColors.Add(Color.FromArgb(215, 182, 130));
            HouseColors.Add(Color.FromArgb(205, 95, 95));
            HouseColors.Add(Color.FromArgb(176, 196, 222));
            HouseColors.Add(Color.FromArgb(223, 213, 130));
            HouseColors.Add(Color.FromArgb(144, 220, 144));
            HouseColors.Add(Color.FromArgb(200, 145, 205));
            HouseColors.Add(Color.FromArgb(215, 170, 120));
            HouseColors.Add(Color.FromArgb(155, 205, 200));
            HouseColors.Add(Color.FromArgb(210, 185, 145));
            HouseColors.Add(Color.FromArgb(190, 160, 220));

            StartPlayerX = leftRoadX + 25;
            StartPlayerY = 300;

            CreateTrees();
        }

        private void FillRandomResidentNames()
        {
            HouseNames.Clear();

            List<string> possibleNames = new List<string>
            {
                "Аня", "Илья", "Дима", "Лера", "Саша",
                "Оля", "Макс", "Катя", "Рома", "Ева",
                "Миша", "Юля", "Кирилл", "Ника", "Паша",
                "Лиза", "Артём", "Соня", "Ваня", "Марина"
            };

            Random random = new Random();

            for (int i = 0; i < 10; i++)
            {
                int index = random.Next(possibleNames.Count);
                HouseNames.Add(possibleNames[index]);
                possibleNames.RemoveAt(index);
            }
        }

        private void CreateTrees()
        {
            TreeTops.Clear();

            TreeTops.Add(new Rectangle(GameArea.X + 10, GameArea.Y + 10, 34, 34));
            TreeTops.Add(new Rectangle(GameArea.X + 55, GameArea.Y + 30, 30, 30));
            TreeTops.Add(new Rectangle(GameArea.X + 20, GameArea.Bottom - 80, 38, 38));
            TreeTops.Add(new Rectangle(GameArea.X + 70, GameArea.Bottom - 55, 28, 28));

            TreeTops.Add(new Rectangle(GameArea.Right - 70, GameArea.Y + 20, 36, 36));
            TreeTops.Add(new Rectangle(GameArea.Right - 110, GameArea.Y + 55, 28, 28));
            TreeTops.Add(new Rectangle(GameArea.Right - 60, GameArea.Bottom - 85, 34, 34));
            TreeTops.Add(new Rectangle(GameArea.Right - 105, GameArea.Bottom - 50, 26, 26));

            TreeTops.Add(new Rectangle(GameArea.X + 250, GameArea.Y + 12, 26, 26));
            TreeTops.Add(new Rectangle(GameArea.X + 520, GameArea.Bottom - 42, 24, 24));
            TreeTops.Add(new Rectangle(GameArea.X + 720, GameArea.Y + 18, 24, 24));
        }
    }
}
