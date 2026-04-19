using FluentValidation;
using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Extensions;
using DeliveryDash.Application.Requests.AddressRequests;
using DeliveryDash.Application.Responses.AddressResponses;
using DeliveryDash.Domain.Entities;
using DeliveryDash.Domain.Exceptions.AddressExceptions;

namespace DeliveryDash.Application.Services
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IValidator<CreateAddressRequest> _createValidator;
        private readonly IValidator<UpdateAddressRequest> _updateValidator;

        public AddressService(
            IAddressRepository addressRepository,
            ICurrentUserService currentUser,
            IValidator<CreateAddressRequest> createValidator,
            IValidator<UpdateAddressRequest> updateValidator)
        {
            _addressRepository = addressRepository;
            _currentUser = currentUser;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<List<AddressResponse>> GetMyAddressesAsync()
        {
            var userId = _currentUser.GetCurrentUserId();
            var addresses = await _addressRepository.GetByUserIdAsync(userId);
            return addresses.Select(MapToResponse).ToList();
        }

        public async Task<AddressResponse> GetByIdAsync(int id)
        {
            var address = await _addressRepository.GetByIdAsync(id)
                ?? throw new AddressNotFoundException(id);

            var userId = _currentUser.GetCurrentUserIdOrNull();
            if (address.UserId != userId)
                throw new UnauthorizedAccessException();

            return MapToResponse(address);
        }

        public async Task<AddressResponse> CreateAsync(CreateAddressRequest request)
        {
            await _createValidator.ValidateAndThrowCustomAsync(request);

            var userId = _currentUser.GetCurrentUserId();

            if (request.IsDefault)
                await _addressRepository.ClearDefaultForUserAsync(userId);

            var address = new Address
            {
                UserId = userId,
                Type = request.Type,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                PhoneNumber = request.PhoneNumber,
                Street = request.Street,
                BuildingName = request.BuildingName,
                Floor = request.Floor,
                ApartmentNumber = request.ApartmentNumber,
                HouseName = request.HouseName,
                HouseNumber = request.HouseNumber,
                CompanyName = request.CompanyName,
                AdditionalDirections = request.AdditionalDirections,
                Label = request.Label,
                IsDefault = request.IsDefault,
            };

            var created = await _addressRepository.CreateAsync(address);
            return MapToResponse(created);
        }

        public async Task<AddressResponse> UpdateAsync(int id, UpdateAddressRequest request)
        {
            await _updateValidator.ValidateAndThrowCustomAsync(request);

            var address = await _addressRepository.GetByIdAsync(id)
                ?? throw new AddressNotFoundException(id);

            var userId = _currentUser.GetCurrentUserIdOrNull();
            if (address.UserId != userId)
                throw new UnauthorizedAccessException();

            if (request.IsDefault && !address.IsDefault)
                await _addressRepository.ClearDefaultForUserAsync(address.UserId);

            address.Type = request.Type;
            address.Latitude = request.Latitude;
            address.Longitude = request.Longitude;
            address.PhoneNumber = request.PhoneNumber;
            address.Street = request.Street;
            address.BuildingName = request.BuildingName;
            address.Floor = request.Floor;
            address.ApartmentNumber = request.ApartmentNumber;
            address.HouseName = request.HouseName;
            address.HouseNumber = request.HouseNumber;
            address.CompanyName = request.CompanyName;
            address.AdditionalDirections = request.AdditionalDirections;
            address.Label = request.Label;
            address.IsDefault = request.IsDefault;

            var updated = await _addressRepository.UpdateAsync(address);
            return MapToResponse(updated);
        }

        public async Task DeleteAsync(int id)
        {
            var address = await _addressRepository.GetByIdAsync(id)
                ?? throw new AddressNotFoundException(id);

            var userId = _currentUser.GetCurrentUserIdOrNull();
            if (address.UserId != userId)
                throw new UnauthorizedAccessException();

            await _addressRepository.DeleteAsync(id);
        }

        public async Task<AddressResponse> SetDefaultAsync(int id)
        {
            var address = await _addressRepository.GetByIdAsync(id)
                ?? throw new AddressNotFoundException(id);

            var userId = _currentUser.GetCurrentUserIdOrNull();
            if (address.UserId != userId)
                throw new UnauthorizedAccessException();

            await _addressRepository.ClearDefaultForUserAsync(address.UserId);
            address.IsDefault = true;
            var updated = await _addressRepository.UpdateAsync(address);
            return MapToResponse(updated);
        }

        private static AddressResponse MapToResponse(Address a) => new()
        {
            Id = a.Id,
            UserId = a.UserId,
            Type = a.Type,
            Latitude = a.Latitude,
            Longitude = a.Longitude,
            PhoneNumber = a.PhoneNumber,
            Street = a.Street,
            BuildingName = a.BuildingName,
            Floor = a.Floor,
            ApartmentNumber = a.ApartmentNumber,
            HouseName = a.HouseName,
            HouseNumber = a.HouseNumber,
            CompanyName = a.CompanyName,
            AdditionalDirections = a.AdditionalDirections,
            Label = a.Label,
            IsDefault = a.IsDefault,
            CreatedAt = a.CreatedAt,
            LastModifiedAt = a.LastModifiedAt,
        };
    }
}
