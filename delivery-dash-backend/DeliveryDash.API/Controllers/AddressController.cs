using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Requests.AddressRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryDash.API.Controllers
{
    [Route("DeliveryDashApi/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer,Vendor,Driver,VendorStaff")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet("me")]
        [EndpointDescription("Returns all addresses owned by the currently authenticated user.")]
        public async Task<IActionResult> GetMyAddresses()
        {
            var addresses = await _addressService.GetMyAddressesAsync();
            return Ok(addresses);
        }

        [HttpGet("{id}")]
        [EndpointDescription("Returns a specific address by ID. Owner only.")]
        public async Task<IActionResult> GetById(int id)
        {
            var address = await _addressService.GetByIdAsync(id);
            return Ok(address);
        }

        [HttpPost]
        [EndpointDescription("Creates a new address for the current user. Type-specific fields are validated. Admins cannot own addresses.")]
        public async Task<IActionResult> Create([FromBody] CreateAddressRequest request)
        {
            var address = await _addressService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = address.Id }, address);
        }

        [HttpPut("{id}")]
        [EndpointDescription("Updates an existing address. Owner only.")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAddressRequest request)
        {
            var address = await _addressService.UpdateAsync(id, request);
            return Ok(address);
        }

        [HttpDelete("{id}")]
        [EndpointDescription("Deletes an address. Owner only.")]
        public async Task<IActionResult> Delete(int id)
        {
            await _addressService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPatch("{id}/default")]
        [EndpointDescription("Marks an address as the default. Clears any previous default for the user.")]
        public async Task<IActionResult> SetDefault(int id)
        {
            var address = await _addressService.SetDefaultAsync(id);
            return Ok(address);
        }
    }
}
