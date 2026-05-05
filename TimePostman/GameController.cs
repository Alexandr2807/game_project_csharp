using System.Windows.Forms;

namespace TimePostman
{
    public class GameController
    {
        private bool up;
        private bool down;
        private bool left;
        private bool right;

        public GameWorld World { get; }
        public bool ShowOrdersSheet { get; private set; }

        public GameController(GameWorld world)
        {
            World = world;
        }

        public void HandleKeyDown(Keys key)
        {
            if (key == Keys.Q && World.GameStarted)
            {
                ShowOrdersSheet = !ShowOrdersSheet;
                return;
            }

            if (!World.GameStarted && key == Keys.Enter)
            {
                World.StartGame();
                return;
            }

            if (key == Keys.Escape)
            {
                World.TogglePause();
                return;
            }

            if (key == Keys.R)
            {
                ShowOrdersSheet = false;
                World.ResetGame();
                return;
            }

            if (key == Keys.E)
            {
                World.TryInteract();
                return;
            }

            if (key == Keys.W || key == Keys.Up) up = true;
            if (key == Keys.S || key == Keys.Down) down = true;
            if (key == Keys.A || key == Keys.Left) left = true;
            if (key == Keys.D || key == Keys.Right) right = true;
        }

        public void HandleKeyUp(Keys key)
        {
            if (key == Keys.W || key == Keys.Up) up = false;
            if (key == Keys.S || key == Keys.Down) down = false;
            if (key == Keys.A || key == Keys.Left) left = false;
            if (key == Keys.D || key == Keys.Right) right = false;
        }

        public void Update(int deltaMs)
        {
            int dx = 0;
            int dy = 0;

            if (up) dy -= World.Player.Speed;
            if (down) dy += World.Player.Speed;
            if (left) dx -= World.Player.Speed;
            if (right) dx += World.Player.Speed;

            World.Update(deltaMs, dx, dy);
        }
    }
}
