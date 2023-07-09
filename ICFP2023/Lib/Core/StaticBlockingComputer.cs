using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class StaticBlockingComputer
    {
        private List<List<Attendee>> PositionIndexToVisibleAttendees;

        public StaticBlockingComputer(List<Point> locations, List<Attendee> attendees, List<Pillar> pillars)
        {
            PositionIndexToVisibleAttendees = new List<List<Attendee>>();
            for (int positionIndex = 0; positionIndex < locations.Count; positionIndex++)
            {
                PositionIndexToVisibleAttendees.Add(new List<Attendee>());
                foreach (var attendee in attendees)
                {
                    if (!IsBlocked(locations[positionIndex], attendee, locations, pillars))
                    {
                        PositionIndexToVisibleAttendees[positionIndex].Add(attendee);
                    }
                }
            }
        }

        private bool IsBlocked(Point musician, Attendee attendee, List<Point> allMusicians, List<Pillar> pillars)
        {
            foreach (Point otherMusician in allMusicians)
            {
                if (musician == otherMusician)
                {
                    continue;
                }

                if (Utils.IsLineOfSightBlocked(attendee.Location, musician, otherMusician, 5))
                {
                    return true;
                }
            }

            foreach (Pillar pillar in pillars)
            {
                if (Utils.IsLineOfSightBlocked(attendee.Location, musician, pillar.Center, pillar.Radius))
                {
                    return true;
                }
            }

            return false;
        }

        public List<Attendee> GetVisibleAttendees(int positionIndex)
        {
            return PositionIndexToVisibleAttendees[positionIndex];
        }
    }
}
