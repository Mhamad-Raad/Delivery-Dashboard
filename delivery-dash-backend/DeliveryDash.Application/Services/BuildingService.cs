using FluentValidation;
using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Extensions;
using DeliveryDash.Application.Requests.ApartmentRequests;
using DeliveryDash.Application.Requests.BuildingRequests;
using DeliveryDash.Application.Responses.ApartmentResponses;
using DeliveryDash.Application.Responses.BuildingResponses;
using DeliveryDash.Application.Responses.Common;
using DeliveryDash.Application.Responses.FloorResponses;
using DeliveryDash.Domain.Entities;
using DeliveryDash.Domain.Entities.FloorPlan;
using DeliveryDash.Domain.Exceptions.BuildingExceptions;
using Microsoft.EntityFrameworkCore;

namespace DeliveryDash.Application.Services
{
    public class BuildingService : IBuildingService
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IValidator<CreateBuildingRequest> _createBuildingValidator;
        private readonly IValidator<UpdateBuildingRequest> _updateBuildingValidator;
        private readonly IValidator<AddApartmentRequest> _addApartmentValidator;
        private readonly IValidator<UpdateApartmentRequest> _updateApartmentValidator;

        public BuildingService(
            IBuildingRepository buildingRepository, 
            IAddressRepository addressRepository,
            IValidator<CreateBuildingRequest> createBuildingValidator,
            IValidator<UpdateBuildingRequest> updateBuildingValidator,
            IValidator<AddApartmentRequest> addApartmentValidator,
            IValidator<UpdateApartmentRequest> updateApartmentValidator)
        {
            _buildingRepository = buildingRepository;
            _addressRepository = addressRepository;
            _createBuildingValidator = createBuildingValidator;
            _updateBuildingValidator = updateBuildingValidator;
            _addApartmentValidator = addApartmentValidator;
            _updateApartmentValidator = updateApartmentValidator;
        }

        public async Task<BuildingDetailResponse> GetBuildingByIdAsync(int id)
        {
            var building = await _buildingRepository.GetByIdAsync(id);

            if (building == null)
                throw new BuildingNotFoundException("Building not found");

            return MapToBuildingDetailResponse(building);
        }

        public async Task<PagedResponse<BuildingResponse>> GetAllBuildingsAsync(int page = 1, int limit = 10, string? searchName = null)
        {
            var (buildings, total) = await _buildingRepository.GetBuildingsPagedAsync(page, limit, searchName);

            var buildingResponses = buildings.Select(MapToResponse).ToList();

            return new PagedResponse<BuildingResponse>
            {
                Data = buildingResponses,
                Page = page,
                Limit = limit,
                Total = total
            };
        }

        public async Task<BuildingResponse> CreateBuildingAsync(CreateBuildingRequest request)
        {
            await _createBuildingValidator.ValidateAndThrowCustomAsync(request);

            try
            {
                // Check if building with the same name already exists
                if (await _buildingRepository.ExistsByNameAsync(request.Name))
                    throw new DuplicateBuildingNameException(request.Name);

                // Check for duplicate floor numbers
                var floorNumbers = request.Floors.Select(f => f.FloorNumber).ToList();
                var duplicateFloors = floorNumbers.GroupBy(f => f)
                                                  .Where(g => g.Count() > 1)
                                                  .Select(g => g.Key)
                                                  .ToList();

                if (duplicateFloors.Any())
                    throw new DuplicateFloorNumberException(duplicateFloors.First());

                // Create the building entity
                var building = new Building
                {
                    Name = request.Name,
                    Floors = new List<Floor>()
                };

                // Sort floors by floor number to ensure proper apartment naming
                var sortedFloors = request.Floors.OrderBy(f => f.FloorNumber).ToList();
                
                int currentApartmentNumber = 1;

                // Create floors and apartments with continuous naming
                foreach (var floorRequest in sortedFloors)
                {
                    var floor = new Floor
                    {
                        FloorNumber = floorRequest.FloorNumber,
                        Apartments = new List<Apartment>()
                    };

                    // Create apartments for this floor with continuous naming
                    for (int i = 0; i < floorRequest.NumberOfApartments; i++)
                    {
                        floor.Apartments.Add(new Apartment
                        {
                            ApartmentName = "Apt " + currentApartmentNumber++.ToString()
                        });
                    }

                    building.Floors.Add(floor);
                }

                // Save everything in a single transaction
                var createdBuilding = await _buildingRepository.CreateAsync(building);

                return MapToResponse(createdBuilding);
            }
            catch (DbUpdateException ex)
            {
                throw new BuildingCreationFailedException("Failed to create building due to database constraints.", ex);
            }
        }

        public async Task UpdateBuildingAsync(int id, UpdateBuildingRequest request)
        {
            await _updateBuildingValidator.ValidateAndThrowCustomAsync(request);
            
            try
            {
                var building = await _buildingRepository.GetByIdAsync(id);

                if (building == null)
                    throw new BuildingNotFoundException("Building not found");

                // Check if another building has the same name
                var existingBuilding = await _buildingRepository.GetByNameAsync(request.Name);
                if (existingBuilding != null && existingBuilding.Id != id)
                    throw new DuplicateBuildingNameException(request.Name);

                building.Name = request.Name;

                var updatedBuilding = await _buildingRepository.UpdateAsync(building);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new BuildingUpdateFailedException("Building was modified by another user.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new BuildingUpdateFailedException("Failed to update building.", ex);
            }
        }

        public async Task DeleteBuildingAsync(int id)
        {
            try
            {
                var building = await _buildingRepository.GetByIdAsync(id);

                if (building == null)
                    throw new BuildingNotFoundException("Building not found");

                // Check if any apartments have occupants
                var hasOccupants = building.Floors
                    .SelectMany(f => f.Apartments)
                    .Any(a => a.Addresses.Any(addr => addr.UserId.HasValue));

                if (hasOccupants)
                    throw new BuildingDeletionFailedException($"Cannot delete building. There are occupants living in the building.");

                await _buildingRepository.DeleteAsync(id);
            }
            catch (DbUpdateException ex)
            {
                throw new BuildingDeletionFailedException($"Failed to delete building.");
            }
        }

        public async Task<FloorDetailResponse> AddFloorToBuildingAsync(int buildingId)
        {
            try
            {
                var building = await _buildingRepository.GetByIdAsync(buildingId);

                if (building == null)
                    throw new BuildingNotFoundException("Building not found");

                // Get the highest floor number and increment by 1
                var lastFloorNumber = building.Floors.Any() 
                    ? building.Floors.Max(f => f.FloorNumber) 
                    : 0;
                var newFloorNumber = lastFloorNumber + 1;

                // Create the new floor without any apartments
                var newFloor = new Floor
                {
                    FloorNumber = newFloorNumber,
                    BuildingId = buildingId,
                    Apartments = new List<Apartment>()
                };

                building.Floors.Add(newFloor);
                var updatedBuilding = await _buildingRepository.UpdateAsync(building);

                // Find the newly created floor (it now has an ID from the database)
                var createdFloor = updatedBuilding.Floors.First(f => f.FloorNumber == newFloorNumber);

                // Return the newly created floor details with ID
                return new FloorDetailResponse
                {
                    Id = createdFloor.Id,
                    FloorNumber = createdFloor.FloorNumber,
                    Apartments = new List<ApartmentDetailResponse>()
                };
            }
            catch (DbUpdateException ex)
            {
                throw new BuildingUpdateFailedException("Failed to add floor to building.", ex);
            }
        }

        public async Task DeleteFloorAsync(int floorId)
        {
            try
            {
                var (building, hasOccupants) = await _buildingRepository.GetByFloorIdAsync(floorId);

                if (building == null)
                    throw new BuildingUpdateFailedException("Floor not found");

                if (hasOccupants)
                {
                    throw new BuildingUpdateFailedException($"Cannot delete floor. There are occupants living on this floor.");
                }

                var floorToDelete = building.Floors.First(f => f.Id == floorId);
                var deletedFloorNumber = floorToDelete.FloorNumber;

                // Remove the floor
                building.Floors.Remove(floorToDelete);

                // Adjust floor numbers for floors above the deleted one
                foreach (var floor in building.Floors.Where(f => f.FloorNumber > deletedFloorNumber))
                {
                    floor.FloorNumber--;
                }

                await _buildingRepository.UpdateAsync(building);
            }
            catch (DbUpdateException ex)
            {
                throw new BuildingUpdateFailedException("Failed to delete floor.", ex);
            }
        }

        public async Task<ApartmentDetailResponse> AddApartmentToFloorAsync(int floorId, AddApartmentRequest request)
        {
            await _addApartmentValidator.ValidateAndThrowCustomAsync(request);
            
            try
            {
                var building = await _buildingRepository.GetByFloorIdForApartmentAsync(floorId);

                if (building == null)
                    throw new BuildingUpdateFailedException("Floor not found");

                var floor = building.Floors.FirstOrDefault(f => f.Id == floorId);
                
                if (floor == null)
                    throw new BuildingUpdateFailedException("Floor not found");

                // Check if apartment name already exists on this floor
                if (floor.Apartments.Any(a => a.ApartmentName == request.ApartmentName))
                    throw new BuildingUpdateFailedException($"Apartment '{request.ApartmentName}' already exists on this floor.");

                // Create new apartment with the provided name
                var newApartment = new Apartment
                {
                    ApartmentName = request.ApartmentName,
                    FloorId = floorId
                };

                floor.Apartments.Add(newApartment);
                var updatedBuilding = await _buildingRepository.UpdateAsync(building);

                // Find the newly created apartment (it now has an ID from the database)
                var updatedFloor = updatedBuilding.Floors.First(f => f.Id == floorId);
                var createdApartment = updatedFloor.Apartments.First(a => a.ApartmentName == request.ApartmentName);

                // Return the newly created apartment details with ID
                return new ApartmentDetailResponse
                {
                    Id = createdApartment.Id,
                    ApartmentName = createdApartment.ApartmentName,
                    Occupant = null
                };
            }
            catch (DbUpdateException ex)
            {
                throw new BuildingUpdateFailedException("Failed to add apartment to floor.", ex);
            }
        }

        public async Task UpdateApartmentAsync(int apartmentId, UpdateApartmentRequest request)
        {
            await _updateApartmentValidator.ValidateAndThrowCustomAsync(request);
            
            try
            {
                var building = await _buildingRepository.GetByApartmentIdAsync(apartmentId);

                if (building == null)
                    throw new BuildingUpdateFailedException("Apartment not found");

                var floor = building.Floors.FirstOrDefault();
                
                if (floor == null)
                    throw new BuildingUpdateFailedException("Floor not found");

                var apartment = floor.Apartments.FirstOrDefault(a => a.Id == apartmentId);
                
                if (apartment == null)
                    throw new BuildingUpdateFailedException("Apartment not found");

                // Check if new apartment name already exists on this floor (excluding current apartment)
                if (floor.Apartments.Any(a => a.Id != apartmentId && a.ApartmentName == request.ApartmentName))
                    throw new BuildingUpdateFailedException($"Apartment '{request.ApartmentName}' already exists on this floor.");

                // Update apartment name
                apartment.ApartmentName = request.ApartmentName;

                // Update layout (optional - can be null to clear, or omitted to leave unchanged)
                if (request.Layout != null)
                {
                    apartment.Layout = new ApartmentLayout
                    {
                        GridSize = request.Layout.GridSize,
                        Rooms = request.Layout.Rooms.Select(r => new RoomLayout
                        {
                            Id = r.Id,
                            Type = r.Type,
                            Name = r.Name,
                            X = r.X,
                            Y = r.Y,
                            Width = r.Width,
                            Height = r.Height
                        }).ToList(),
                        Doors = request.Layout.Doors.Select(d => new Door
                        {
                            Id = d.Id,
                            RoomId = d.RoomId,
                            ConnectedRoomId = d.ConnectedRoomId,
                            Edge = d.Edge,
                            Position = d.Position,
                            Width = d.Width
                        }).ToList()
                    };
                }

                // Save apartment name and layout changes FIRST
                await _buildingRepository.UpdateAsync(building);

                // THEN handle user assignment using AddressRepository
                if (request.UserId.HasValue)
                {
                    // Check if apartment is already occupied by a different user
                    var existingAddress = await _addressRepository.GetByApartmentIdAsync(apartmentId);
                    if (existingAddress != null && existingAddress.UserId.HasValue && existingAddress.UserId != request.UserId)
                    {
                        throw new BuildingUpdateFailedException("Apartment is already occupied by another user.");
                    }

                    // Check if user is already assigned to ANY apartment
                    var userAddress = await _addressRepository.GetByUserIdAsync(request.UserId.Value);
                    if (userAddress != null && userAddress.ApartmentId != apartmentId)
                    {
                        throw new BuildingUpdateFailedException("User is already assigned to another apartment. Please remove them from their current apartment first.");
                    }

                    if (userAddress != null && userAddress.ApartmentId == apartmentId)
                    {
                        // User is already assigned to this apartment, no action needed
                        return;
                    }
                    else if (existingAddress != null && !existingAddress.UserId.HasValue)
                    {
                        // Address exists for apartment but has no user, assign the user
                        existingAddress.UserId = request.UserId;
                        await _addressRepository.UpdateAsync(existingAddress);
                    }
                    else
                    {
                        // Create new address
                        await _addressRepository.CreateAsync(new Address
                        {
                            UserId = request.UserId,
                            BuildingId = building.Id,
                            FloorId = floor.Id,
                            ApartmentId = apartmentId
                        });
                    }
                }
                else
                {
                    // Remove user from apartment (userId is null)
                    var existingAddress = await _addressRepository.GetByApartmentIdAsync(apartmentId);
                    if (existingAddress != null && existingAddress.UserId.HasValue)
                    {
                        await _addressRepository.DeleteAsync(existingAddress.Id);
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                throw new BuildingUpdateFailedException("Failed to update apartment.", ex);
            }
        }

        public async Task DeleteApartmentAsync(int apartmentId)
        {
            try
            {
                var (building, hasOccupant) = await _buildingRepository.GetByApartmentIdForDeleteAsync(apartmentId);

                if (building == null)
                    throw new BuildingUpdateFailedException("Apartment not found");

                if (hasOccupant)
                    throw new BuildingUpdateFailedException("Cannot delete apartment. There is an occupant living in this apartment.");

                var floor = building.Floors.FirstOrDefault();
                
                if (floor == null)
                    throw new BuildingUpdateFailedException("Floor not found");

                var apartment = floor.Apartments.FirstOrDefault(a => a.Id == apartmentId);
                
                if (apartment == null)
                    throw new BuildingUpdateFailedException("Apartment not found");

                // Remove the apartment
                floor.Apartments.Remove(apartment);

                await _buildingRepository.UpdateAsync(building);
            }
            catch (DbUpdateException ex)
            {
                throw new BuildingUpdateFailedException("Failed to delete apartment.", ex);
            }
        }

        private static BuildingResponse MapToResponse(Building building)
        {
            var totalApartments = building.Floors.Sum(f => f.Apartments.Count);
            var occupants = building.Addresses?.Count(a => a.UserId.HasValue) ?? 0;

            return new BuildingResponse
            {
                Id = building.Id,
                Name = building.Name,
                NumberOfFloors = building.Floors.Count,
                TotalApartments = totalApartments,
                Occupants = occupants,
            };
        }

        private static BuildingDetailResponse MapToBuildingDetailResponse(Building building)
        {
            return new BuildingDetailResponse
            {
                Id = building.Id,
                Name = building.Name,
                Floors = building.Floors
                    .OrderBy(f => f.FloorNumber)
                    .Select(floor => new FloorDetailResponse
                    {
                        Id = floor.Id,
                        FloorNumber = floor.FloorNumber,
                        Apartments = floor.Apartments
                            .OrderBy(a => a.ApartmentName)
                            .Select(apartment =>
                            {
                                var address = apartment.Addresses.FirstOrDefault(addr => addr.UserId.HasValue);
                                var user = address?.User;

                                return new ApartmentDetailResponse
                                {
                                    Id = apartment.Id,
                                    ApartmentName = apartment.ApartmentName,
                                    Occupant = user != null
                                        ? new OccupantResponse
                                        {
                                            Id = user.Id,
                                            Name = $"{user.FirstName} {user.LastName}",
                                            Email = user.Email ?? string.Empty
                                        }
                                        : null,
                                    Layout = apartment.Layout != null
                                        ? new ApartmentLayoutResponse
                                        {
                                            GridSize = apartment.Layout.GridSize,
                                            Rooms = apartment.Layout.Rooms.Select(r => new RoomLayoutResponse
                                            {
                                                Id = r.Id,
                                                Type = r.Type,
                                                Name = r.Name,
                                                X = r.X,
                                                Y = r.Y,
                                                Width = r.Width,
                                                Height = r.Height
                                            }).ToList(),
                                            Doors = apartment.Layout.Doors.Select(d => new DoorResponse
                                            {
                                                Id = d.Id,
                                                RoomId = d.RoomId,
                                                ConnectedRoomId = d.ConnectedRoomId,
                                                Edge = d.Edge,
                                                Position = d.Position,
                                                Width = d.Width
                                            }).ToList()
                                        }
                                        : null
                                };
                            })
                            .ToList()
                    })
                    .ToList()
            };
        }
    }
}