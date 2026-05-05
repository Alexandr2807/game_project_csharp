namespace TimePostman
{
    public class PlayerState
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; } = 28;
        public int Height { get; } = 28;
        public int Speed { get; } = 4;

        public PlayerState(int startX, int startY)
        {
            X = startX;
            Y = startY;
        }

        public void Reset(int startX, int startY)
        {
            X = startX;
            Y = startY;
        }
    }
}
