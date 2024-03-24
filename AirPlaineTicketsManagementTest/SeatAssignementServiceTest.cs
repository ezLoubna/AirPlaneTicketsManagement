using AirPlaneTicketManagement.Contracts;
using AirPlaneTicketManagement.Data;
using AirPlaneTicketManagement.Services;
using AirPlaneTicketManagement.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirPlaineTicketsManagementTest
{
    public class SeatAssignementServiceTest
    {

        private readonly SeatAssignementService _service;

        public SeatAssignementServiceTest()
        {
            _service = new SeatAssignementService();
        }
        [Fact]
        public void FindAvailableSeatsForFamily_ShouldAllocateInSingleRow_WhenPossible()
        {
            var passengers = new List<Passenger>
        {
            // Créer des passagers individuels
            new Passenger(1, "Passenger A", PassengerType.Adult, 30, null),
            new Passenger(2, "Passenger B", PassengerType.AdultRequiringTwoSeats, 35, null)
        };

            var family1 = new Family(1);
            family1.AddMember(new Passenger(3, "Family1 Member1", PassengerType.Adult, 40, 1));
            family1.AddMember(new Passenger(4, "Family1 Member2", PassengerType.Child, 12, 1));

            var family2 = new Family(2);
            family2.AddMember(new Passenger(5, "Family2 Member1", PassengerType.Adult, 45, 2));
            family2.AddMember(new Passenger(6, "Family2 Member2", PassengerType.Child, 10, 2));
            family2.AddMember(new Passenger(7, "Family2 Member3", PassengerType.AdultRequiringTwoSeats, 50, 2));

            var families = new List<Family> { family1, family2 };



            // Act
            _service.AssignSeats(passengers, families);

            // Assert
            Assert.True(families.All(f => f.AllocatedSeats != null), "the family should have seats assigned.");
            Assert.True(passengers.All(p => p.Seats.Count > 0), "All passengers should have seats assigned.");
            Assert.True(families.All(f => f.AllocatedSeats.Count == f.GetTotalSeatsRequired()), "All family members should have seats assigned.");

        }

        [Fact]
        public void CanFamilyFitInSingleRow_WithEnoughContinuousSeats_ShouldReturnTrue()
        {
            // Arrange
            var family = new Family(1);
            family.AddMember(new Passenger(1, "Adult 1", PassengerType.Adult, 30, 1));
            family.AddMember(new Passenger(2, "Child 1", PassengerType.Child, 8, 1));
            var rowSeats = GenerateSeats(6, occupiedIndexes: Array.Empty<int>()); // 6 seats, all available

            // Act
            bool canFit = _service.CanFamilyFitInSingleRow(family, rowSeats);

            // Assert
            Assert.True(canFit);
        }

        [Fact]
        public void CanFamilyFitInSingleRow_WithNotEnoughContinuousSeats_ShouldReturnFalse()
        {
            // Arrange
            var family = new Family(1);
            family.AddMember(new Passenger(1, "Adult 1", PassengerType.Adult, 30, 1));
            family.AddMember(new Passenger(2, "Child 1", PassengerType.Child, 8, 1));
            family.AddMember(new Passenger(3, "Adult 2", PassengerType.Child, 8, 1));
            family.AddMember(new Passenger(4, "Child 2", PassengerType.Child, 8, 1));
            var rowSeats = GenerateSeats(6, occupiedIndexes: [2, 3]); // Breaks continuous availability

            // Act
            bool canFit = _service.CanFamilyFitInSingleRow(family, rowSeats);

            // Assert
            Assert.False(canFit);
        }

        [Fact]
        public void CanFamilyFitInSingleRow_WithAdultsRequiringTwoSeats_ShouldEvaluateCorrectly()
        {
            // Arrange
            var family = new Family(1);
            family.AddMember(new Passenger(1, "Adult 1", PassengerType.AdultRequiringTwoSeats, 30, 1));
            family.AddMember(new Passenger(1, "Adult 1", PassengerType.AdultRequiringTwoSeats, 30, 1));
            family.AddMember(new Passenger(1, "Child 1", PassengerType.Child, 10, 1));
            var rowSeats = GenerateSeats(6, occupiedIndexes: Array.Empty<int>()); // 6 seats, all available

            // Act
            bool canFit = _service.CanFamilyFitInSingleRow(family, rowSeats);

            // Assert
            Assert.True(canFit); // Assuming the criteria is met for a single adult requiring two seats.
        }

        private List<Seat> GenerateSeats(int totalSeats, int[] occupiedIndexes)
        {
            var seats = new List<Seat>();
            for (int i = 0; i < totalSeats; i++)
            {
                seats.Add(new Seat(i / 6, i % 6) { IsOccupied = occupiedIndexes.Contains(i) });
            }
            return seats;
        }


        [Fact]
        public void CanFamilyBeSplitAcrossRows_WithSufficientSeats_ShouldReturnTrue()
        {
            // Arrange
            var family = new Family(1);
            family.AddMember(new Passenger(1, "Adult 1", PassengerType.Adult, 30, 1));
            family.AddMember(new Passenger(2, "Adult 2", PassengerType.Adult, 30, 1));
            family.AddMember(new Passenger(3, "Child 1", PassengerType.Child, 8, 1));
            var currentRowSeats = GenerateSeatsTwoRows(2, occupiedIndexes: Array.Empty<int>());
            var nextRowSeats = GenerateSeatsTwoRows(2, occupiedIndexes: Array.Empty<int>());

            // Act
            bool result = _service.CanFamilyBeSplitAcrossRows(family, currentRowSeats, nextRowSeats);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanFamilyBeSplitAcrossRows_WithInsufficientSeatsInOneRow_ShouldReturnFalse()
        {
            // Arrange
            var family = new Family(1);
            family.AddMember(new Passenger(1, "Adult 1", PassengerType.Adult, 30, 1));
            family.AddMember(new Passenger(2, "Adult 2", PassengerType.AdultRequiringTwoSeats, 30, 1));
            family.AddMember(new Passenger(3, "Child 1", PassengerType.Child, 8, 1));
            family.AddMember(new Passenger(4, "Child 2", PassengerType.Child, 8, 1));
            var currentRowSeats = GenerateSeatsTwoRows(3, occupiedIndexes: [1]); // Not enough seats in current row
            var nextRowSeats = GenerateSeatsTwoRows(3, occupiedIndexes: [1]); ;

            // Act
            bool result = _service.CanFamilyBeSplitAcrossRows(family, currentRowSeats, nextRowSeats);

            // Assert
            Assert.False(result);
        }


        private static List<Seat> GenerateSeatsTwoRows(int totalSeats, int[] occupiedIndexes)
        {
            List<Seat> seats = new List<Seat>();
            for (int i = 0; i < totalSeats; i++)
            {
                seats.Add(new Seat(i / totalSeats, i % totalSeats) { IsOccupied = occupiedIndexes.Contains(i) });
            }
            return seats;
        }

        //Testing the allocation of seats for a family in a single Row

        [Fact]
        public void AllocateFamilySeatsInRow_AllocatesCorrectly_WhenEnoughSeats()
        {
            // Arrange
            var family = new Family(1);
            family.AddMember(new Passenger(1, "Adult 1", PassengerType.Adult, 30, 1));
            family.AddMember(new Passenger(2, "Adult 2", PassengerType.AdultRequiringTwoSeats, 35, 1));
            family.AddMember(new Passenger(3, "Child 1", PassengerType.Child, 8, 1));
            family.AddMember(new Passenger(4, "Child 2", PassengerType.Child, 10, 1));
            var rowSeats = GenerateRowSeats(6); // a helper method that creates 6 available seats in a row.

            // Act
            var allocatedSeats = _service.AllocateFamilySeatsInRow(family, rowSeats);

            // Assert
            Assert.NotNull(allocatedSeats);
            Assert.Equal(5, allocatedSeats.Count);
            Assert.True(family.Members.All(m => m.Seats.Count > 0), "All family members should have seats allocated.");
        }

        [Fact]
        public void AllocateFamilySeatsInRow_Fails_WhenNotEnoughSeats()
        {
            // Arrange
            var family = new Family(1);
            family.AddMember(new Passenger(1, "Adult 1", PassengerType.AdultRequiringTwoSeats, 30, 1));
            family.AddMember(new Passenger(2, "Child 1", PassengerType.Child, 8, 1));
            var rowSeats = GenerateRowSeats(1); // Not enough seats for both.

            // Act
            var allocatedSeats = _service.AllocateFamilySeatsInRow(family, rowSeats);

            // Assert
            Assert.Null(allocatedSeats); // Allocation should fail
        }

        // Additional test methods to cover other scenarios, like adults requiring two seats, children next to adults, etc.

        private List<Seat> GenerateRowSeats(int count)
        {
            List<Seat> seats = new List<Seat>();
            for (int i = 0; i < count; i++)
            {
                seats.Add(new Seat(1, i)); // Assume all seats are in row 1 for simplicity
            }
            return seats;
        }

        [Fact]
        public void AllocateFamilyMembersAcrossRows_WithOneChild_ShouldAllocateCorrectly()
        {
            // Arrange
            var family = new Family(1);
            family.AddMember(new Passenger(1, "Parent 1", PassengerType.Adult, 30, 1));
            family.AddMember(new Passenger(2, "Parent 2", PassengerType.Adult, 30, 1));
            family.AddMember(new Passenger(3, "Child 1", PassengerType.Child, 8, 1));
            var currentRowSeats = GenerateRowSeats(1); // 6 sièges disponibles dans la rangée courante
            var nextRowSeats = GenerateRowSeats(2); // 6 sièges disponibles dans la rangée suivante

            // Act
            var allocatedSeats = _service.AllocateFamilyMembersAcrossRows(family, currentRowSeats, nextRowSeats);

            // Assert
            Assert.NotEmpty(allocatedSeats); // S'assurer que des sièges ont été alloués
            Assert.True(allocatedSeats.Count == family.GetTotalSeatsRequired(), "Chaque famille doit avoir suffisamment de sièges alloués.");
        }
        [Fact]
        public void AllocateTwoChildren_RequiresSplittingAcrossRows_ShouldAllocateCorrectly()
        {
            // Arrange: Family of 2 adults and 2 children
            var family = new Family(1);
            var adult1 = new Passenger(1, "Adult 1", PassengerType.Adult, 30, 1);
            var adult2 = new Passenger(2, "Adult 2", PassengerType.Adult, 32, 1);
            var child1 = new Passenger(3, "Child 1", PassengerType.Child, 5, 1);
            var child2 = new Passenger(4, "Child 2", PassengerType.Child, 7, 1);
            family.AddMember(adult1);
            family.AddMember(adult2);
            family.AddMember(child1);
            family.AddMember(child2);

            // Simulate seat availability that forces family splitting across rows
            // Example: First row can only fit 1 adult and 1 child, second row can fit the rest
            var currentRowSeats = GenerateSpecificRowSeats(1, [0, 1, 2, 3], [1]); // 1st row with 2 seats available, but 1 is occupied
            var nextRowSeats = GenerateSpecificRowSeats(2, [0, 1, 2, 3], []); // 2nd row with 3 seats available

            // Act
            var allocatedSeats = _service.AllocateTwoChildren(new List<Passenger> { child1, child2 }, new List<Passenger> { adult1, adult2 }, currentRowSeats, nextRowSeats);

            // Assert
            Assert.NotEmpty(allocatedSeats);
            Assert.Equal(4, allocatedSeats.Count); // Validate the total number of allocated seats matches family size
                                                   // Additional assertions to verify correct allocation across rows
        }

        // Helper to generate specific row setup based on provided indexes
        private List<Seat> GenerateSpecificRowSeats(int rowNum, int[] seatIndexes, int[] occupiedIndexes)
        {
            List<Seat> seats = new List<Seat>();
            foreach (int index in seatIndexes)
            {
                Seat seat = new Seat(rowNum, index) { IsOccupied = occupiedIndexes.Contains(index) };
                seats.Add(seat);
            }
            return seats;
        }

        [Fact]
        public void AllocateTwoChildren_InsufficientSeatsAcrossRows_FailsGracefully()
        {
            // Arrange
            var family = new Family(1);
            var adult1 = new Passenger(1, "Adult 1", PassengerType.Adult, 30, 1);
            var adult2 = new Passenger(2, "Adult 2", PassengerType.Adult, 32, 1);
            var child1 = new Passenger(3, "Child 1", PassengerType.Child, 5, 1);
            var child2 = new Passenger(4, "Child 2", PassengerType.Child, 7, 1);
            family.AddMember(adult1);
            family.AddMember(adult2);
            family.AddMember(child1);
            family.AddMember(child2);

            // Simulate row seats where the first row has 3 seats available but not next to each other,
            // and the second row has only 2 seats available, also not next to each other.
            var currentRowSeats = GenerateSpecificRowSeats(1, new int[] { 0, 2, 4 }, new int[] { 1, 3 }); // 1st row with disjoint seats
            var nextRowSeats = GenerateSpecificRowSeats(2, new int[] { 0, 3 }, new int[] { 1, 2, 4 }); // 2nd row with insufficient seats

            // Act
            var allocatedSeats = _service.AllocateTwoChildren(new List<Passenger> { child1, child2 }, new List<Passenger> { adult1, adult2 }, currentRowSeats, nextRowSeats);

            // Assert
            Assert.True(allocatedSeats == null || allocatedSeats.Count == 0, "Allocation should fail gracefully when seats across rows are insufficient.");
        }
    }

}