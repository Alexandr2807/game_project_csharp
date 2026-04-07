using System.Collections.Generic;

namespace TimePostman
{
    public class LetterItem
    {
        public string Title { get; set; }
        public int HouseIndex { get; set; }
        public bool Delivered { get; set; }
        public List<TimePhase> AllowedPhases { get; set; }

        public LetterItem(string title, int houseIndex, List<TimePhase> allowedPhases)
        {
            Title = title;
            HouseIndex = houseIndex;
            AllowedPhases = allowedPhases;
            Delivered = false;
        }

        public bool CanDeliverNow(TimePhase currentPhase)
        {
            for (int i = 0; i < AllowedPhases.Count; i++)
            {
                if (AllowedPhases[i] == currentPhase)
                {
                    return true;
                }
            }

            return false;
        }
    }
}