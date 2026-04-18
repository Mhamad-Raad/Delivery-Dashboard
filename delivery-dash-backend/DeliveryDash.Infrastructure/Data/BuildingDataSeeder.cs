using DeliveryDash.Domain.Entities;

namespace DeliveryDash.Infrastructure.Data
{
    public static class BuildingDataSeeder
    {
        // Configuration
        private const int NumberOfBuildings = 5;
        private const int FloorsPerBuilding = 10;
        private const int ApartmentsPerFloor = 8;

        private static readonly string[] BuildingNames = 
        [
            "Sunrise Tower",
            "Ocean View Plaza",
            "Garden Heights",
            "Metropolitan Square",
            "Skyline Residence",
            "Harbor Point",
            "Parkside Manor",
            "Riverside Complex",
            "Crystal Gardens",
            "Summit Place"
        ];

        public static (List<Building> Buildings, List<Floor> Floors, List<Apartment> Apartments) GenerateBuildings()
        {
            var buildings = new List<Building>();
            var floors = new List<Floor>();
            var apartments = new List<Apartment>();

            for (int b = 0; b < NumberOfBuildings; b++)
            {
                var building = new Building
                {
                    Name = BuildingNames[b % BuildingNames.Length]
                };
                buildings.Add(building);

                // Create floors for this building
                for (int f = 1; f <= FloorsPerBuilding; f++)
                {
                    var floor = new Floor
                    {
                        FloorNumber = f,
                        Building = building  // Set navigation property instead of BuildingId
                    };
                    floors.Add(floor);

                    // Create apartments for this floor
                    for (int a = 1; a <= ApartmentsPerFloor; a++)
                    {
                        var apartment = new Apartment
                        {
                            ApartmentName = "Apt " + a.ToString(),
                            Floor = floor  // Set navigation property instead of FloorId
                        };
                        apartments.Add(apartment);
                    }
                }
            }

            return (buildings, floors, apartments);
        }

        public static List<Address> AssignUsersToApartments(
            List<User> users,
            List<Building> buildings,
            List<Floor> floors,
            List<Apartment> apartments)
        {
            var addresses = new List<Address>();
            var random = new Random(200); // Fixed seed for reproducibility

            // Get all apartments and shuffle them
            var availableApartments = apartments.ToList();
            
            // Assign each user to a random apartment
            foreach (var user in users)
            {
                if (availableApartments.Count == 0)
                {
                    // If we run out of apartments, reset the pool
                    availableApartments = apartments.ToList();
                }

                // Pick a random apartment
                var apartmentIndex = random.Next(availableApartments.Count);
                var selectedApartment = availableApartments[apartmentIndex];
                availableApartments.RemoveAt(apartmentIndex);

                // Find the floor and building for this apartment
                var floor = floors.First(f => f.Id == selectedApartment.FloorId);
                var building = buildings.First(b => b.Id == floor.BuildingId);

                var address = new Address
                {
                    UserId = user.Id,
                    BuildingId = building.Id,
                    FloorId = floor.Id,
                    ApartmentId = selectedApartment.Id
                };

                addresses.Add(address);
            }

            return addresses;
        }
    }
}