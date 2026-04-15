using MalDash.Application.Abstracts.IService;
using MalDash.Application.Requests.ApartmentRequests;
using MalDash.Application.Requests.BuildingRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MalDash.API.Controllers
{
    [Route("MalDashApi/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class BuildingController : ControllerBase
    {
        private readonly IBuildingService _buildingService;

        public BuildingController(IBuildingService buildingService)
        {
            _buildingService = buildingService;
        }

        [HttpGet("{id}")]
        [EndpointDescription("Retrieves detailed information for a specific building by ID. Returns building details including name, address, and all associated floors with their apartments. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> GetBuildingById(int id)
        {
            var building = await _buildingService.GetBuildingByIdAsync(id);
            return Ok(building);
        }

        [HttpGet]
        [EndpointDescription("Retrieves a paginated list of all buildings with optional name search. Returns building summaries including floor and apartment counts. Supports pagination with customizable page size. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> GetAllBuildings([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? searchName = null)
        {
            var buildings = await _buildingService.GetAllBuildingsAsync(page, limit, searchName);
            return Ok(buildings);
        }

        [HttpPost]
        [EndpointDescription("Creates a new building with the provided details. Accepts building name and address information. Returns the created building details. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> CreateBuilding([FromBody] CreateBuildingRequest request)
        {
            var result = await _buildingService.CreateBuildingAsync(request);
            return Ok(new { message = "Building was created successfully.", result });
        }

        [HttpPut("BuildingName/{id}")]
        [EndpointDescription("Updates an existing building's name by ID. Validates that the new name is unique across all buildings. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> UpdateBuilding(int id, [FromBody] UpdateBuildingRequest request)
        {
            await _buildingService.UpdateBuildingAsync(id, request);
            return Ok(new { message = "Building was updated successfully." });
        }

        [HttpDelete("{id}")]
        [EndpointDescription("Permanently deletes a building by ID. This action also removes all associated floors and apartments. Cannot be undone. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> DeleteBuilding(int id)
        {
            await _buildingService.DeleteBuildingAsync(id);
            return Ok(new { message = "Building deleted successfully." });
        }

        [HttpPost("{id}/floor")]
        [EndpointDescription("Adds a new floor to an existing building. The floor number is automatically assigned as the next sequential number. Returns the newly created floor details. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> AddFloorToBuilding(int id)
        {
            var floor = await _buildingService.AddFloorToBuildingAsync(id);
            return Ok(new 
            { 
                message = "Floor was added successfully.",
                floor = floor
            });
        }

        [HttpDelete("floor/{floorId}")]
        [EndpointDescription("Permanently deletes a floor by ID. This action also removes all apartments on the floor. Cannot be undone. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> DeleteFloor(int floorId)
        {
            await _buildingService.DeleteFloorAsync(floorId);
            return Ok(new { message = "Floor was deleted successfully." });
        }

        [HttpPost("floor/{floorId}/apartment")]
        [EndpointDescription("Adds a new apartment to an existing floor. Accepts apartment name/number. Returns the newly created apartment details. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> AddApartmentToFloor(int floorId, [FromBody] AddApartmentRequest request)
        {
            var apartment = await _buildingService.AddApartmentToFloorAsync(floorId, request);
            return Ok(new 
            { 
                message = "Apartment was added successfully.",
                apartment = apartment
            });
        }

        [HttpPut("apartment/{apartmentId}")]
        [EndpointDescription("Updates an existing apartment's details by ID. Accepts updated apartment name/number. Validates uniqueness within the floor. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> UpdateApartment(int apartmentId, [FromBody] UpdateApartmentRequest request)
        {
            await _buildingService.UpdateApartmentAsync(apartmentId, request);
            return Ok(new { message = "Apartment was updated successfully." });
        }

        [HttpDelete("apartment/{apartmentId}")]
        [EndpointDescription("Permanently deletes an apartment by ID. Removes the apartment and any associated tenant address references. Cannot be undone. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> DeleteApartment(int apartmentId)
        {
            await _buildingService.DeleteApartmentAsync(apartmentId);
            return Ok(new { message = "Apartment was deleted successfully." });
        }
    }
}