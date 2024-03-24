using AirPlaneTicketManagement.Services;
using AirPlaneTicketManagement.Utils;

namespace AirPlaineTicketsManagementTest
{
    public class PassengerServiceTest
    {
        [Fact]
        public void GeneratePassengersAndFamilies_ShouldNotExceedTotalSeats()
        {
            // Arrange
            var service = new PassengerService(); 

            // Act
            var result = service.GeneratePassengersAndFamilies();
            var totalSeatsAllocated = result.passengers.Sum(p => p.Type == PassengerType.AdultRequiringTwoSeats ? 2 : 1) +
                                      result.families.Sum(f => f.GetTotalSeatsRequired());

            // Assert
            Assert.True(totalSeatsAllocated <= 300, $"Total seats allocated should not exceed 300. Actual: {totalSeatsAllocated}");
        }

        [Fact]
        public void Families_ShouldHaveAtLeastOneAdult()
        {
            // Arrange
            var service = new PassengerService();

            // Act
            var result = service.GeneratePassengersAndFamilies();
            var allFamiliesHaveAdult = result.families.All(f => f.GetAdultCount() >= 1);

            // Assert
            Assert.True(allFamiliesHaveAdult, "All families should have at least one adult.");
        }

        [Fact]
        public void FamiliesWithThreeChildren_ShouldHaveTwoAdults()
        {
            // Arrange, Act
            var (passengers, families) = new PassengerService().GeneratePassengersAndFamilies();

            // Assert
            var familiesWithThreeChildren = families.Where(f => f.GetChildrenCount() == 3);
            Assert.All(familiesWithThreeChildren, family => Assert.True(family.GetAdultCount() >= 2, "Families with three children should have at least two adults."));
        }
    }
}

