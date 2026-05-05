using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimePostman;

namespace TimePostman.Tests
{
    [TestClass]
    public class GameSessionTests
    {
        [TestMethod]
        public void Game_Starts_With_Morning_Phase()
        {
            GameSession session = new GameSession(true);
            Assert.AreEqual(TimePhase.Morning, session.CurrentPhase);
        }

        [TestMethod]
        public void Phase_Changes_After_Enough_Time()
        {
            GameSession session = new GameSession(true);
            session.AdvanceTime(session.PhaseDurationMs);
            Assert.AreEqual(TimePhase.Day, session.CurrentPhase);
        }

        [TestMethod]
        public void Phase_Cycles_Back_To_Morning()
        {
            GameSession session = new GameSession(true);
            session.AdvanceTime(session.PhaseDurationMs * 4);
            Assert.AreEqual(TimePhase.Morning, session.CurrentPhase);
        }

        [TestMethod]
        public void Can_Deliver_Order_In_Correct_Time()
        {
            GameSession session = new GameSession(true);
            DeliveryAttemptResult result = session.TryDeliver(0);
            Assert.AreEqual(DeliveryAttemptResult.Delivered, result);
            Assert.AreEqual(1, session.GetDeliveredCount());
        }

        [TestMethod]
        public void Cannot_Deliver_Order_In_Wrong_Time()
        {
            GameSession session = new GameSession(true);
            DeliveryAttemptResult result = session.TryDeliver(1);
            Assert.AreEqual(DeliveryAttemptResult.WrongTime, result);
        }

        [TestMethod]
        public void Cannot_Deliver_When_TooFar()
        {
            GameSession session = new GameSession(true);
            DeliveryAttemptResult result = session.TryDeliver(-1);
            Assert.AreEqual(DeliveryAttemptResult.TooFar, result);
        }

        [TestMethod]
        public void Returns_NoOrder_When_Order_Already_Delivered()
        {
            GameSession session = new GameSession(true);
            session.TryDeliver(0);
            DeliveryAttemptResult result = session.TryDeliver(0);
            Assert.AreEqual(DeliveryAttemptResult.NoOrder, result);
        }

        [TestMethod]
        public void Counts_Available_Orders_For_Current_Phase()
        {
            GameSession session = new GameSession(true);
            Assert.AreEqual(4, session.GetAvailableNowCount());
        }

        [TestMethod]
        public void Completion_Percent_Changes_After_Delivery()
        {
            GameSession session = new GameSession(true);
            session.TryDeliver(0);
            Assert.AreEqual(10, session.GetCompletionPercent());
        }

        [TestMethod]
        public void Game_Wins_When_All_Orders_Are_Delivered()
        {
            GameSession session = new GameSession(true);

            session.TryDeliver(0);
            session.TryDeliver(5);
            session.TryDeliver(6);
            session.TryDeliver(9);

            session.AdvanceTime(session.PhaseDurationMs);
            session.TryDeliver(1);
            session.TryDeliver(4);
            session.TryDeliver(7);

            session.AdvanceTime(session.PhaseDurationMs);
            session.TryDeliver(2);

            session.AdvanceTime(session.PhaseDurationMs);
            session.TryDeliver(3);
            session.TryDeliver(8);

            Assert.IsTrue(session.GameWon);
        }

        [TestMethod]
        public void Game_Loses_When_Match_Time_Runs_Out()
        {
            GameSession session = new GameSession(true);
            session.AdvanceTime(session.MatchDurationMs);
            Assert.IsTrue(session.GameLost);
        }

        [TestMethod]
        public void Cannot_Deliver_When_Game_Already_Ended()
        {
            GameSession session = new GameSession(true);
            session.AdvanceTime(session.MatchDurationMs);
            DeliveryAttemptResult result = session.TryDeliver(0);
            Assert.AreEqual(DeliveryAttemptResult.GameEnded, result);
        }

        [TestMethod]
        public void Reset_After_Win_Clears_Progress()
        {
            GameSession session = new GameSession(true);

            session.TryDeliver(0);
            session.TryDeliver(5);
            session.TryDeliver(6);
            session.TryDeliver(9);
            session.AdvanceTime(session.PhaseDurationMs);
            session.TryDeliver(1);
            session.TryDeliver(4);
            session.TryDeliver(7);
            session.AdvanceTime(session.PhaseDurationMs);
            session.TryDeliver(2);
            session.AdvanceTime(session.PhaseDurationMs);
            session.TryDeliver(3);
            session.TryDeliver(8);

            Assert.IsTrue(session.GameWon);

            session.Reset();

            Assert.AreEqual(0, session.GetDeliveredCount());
            Assert.IsFalse(session.GameWon);
            Assert.IsFalse(session.GameLost);
            Assert.AreEqual(TimePhase.Morning, session.CurrentPhase);
        }
    }
}
