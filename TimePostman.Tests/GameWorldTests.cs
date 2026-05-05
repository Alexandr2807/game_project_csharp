using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimePostman;
using System.Drawing;

namespace TimePostman.Tests
{
    [TestClass]
    public class GameWorldTests
    {
        [TestMethod]
        public void StartGame_Changes_State()
        {
            GameWorld world = new GameWorld(true);
            world.StartGame();
            Assert.IsTrue(world.GameStarted);
            Assert.IsFalse(world.GamePaused);
        }

        [TestMethod]
        public void ResetGame_Returns_Player_To_Start()
        {
            GameWorld world = new GameWorld(true);
            world.StartGame();
            world.Update(16, 10, 0);
            world.ResetGame();

            Assert.AreEqual(world.Map.StartPlayerX, world.Player.X);
            Assert.AreEqual(world.Map.StartPlayerY, world.Player.Y);
        }

        [TestMethod]
        public void TogglePause_Stops_Game()
        {
            GameWorld world = new GameWorld(true);
            world.StartGame();
            world.TogglePause();
            Assert.IsTrue(world.GamePaused);
        }

        [TestMethod]
        public void Update_Does_Not_Move_When_Paused()
        {
            GameWorld world = new GameWorld(true);
            world.StartGame();
            world.TogglePause();
            int oldX = world.Player.X;
            world.Update(16, 10, 0);
            Assert.AreEqual(oldX, world.Player.X);
        }

        [TestMethod]
        public void House_State_For_First_House_Is_CanDeliver_In_Morning()
        {
            GameWorld world = new GameWorld(true);
            Assert.AreEqual(HouseInteractionState.CanDeliver, world.GetHouseInteractionState(0));
        }

        [TestMethod]
        public void Player_Rectangle_Stays_Inside_Game_Area_After_Reset()
        {
            GameWorld world = new GameWorld(true);
            Rectangle rect = world.GetPlayerRectangle();
            Assert.IsTrue(world.IsInsideGame(rect));
        }
    }
}
