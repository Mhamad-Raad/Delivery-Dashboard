using MalDash.Domain.Exceptions;
using MalDash.Domain.Exceptions.AddressExceptions;
using MalDash.Domain.Exceptions.BuildingExceptions;
using MalDash.Domain.Exceptions.UserExceptions;
using MalDash.Domain.Exceptions.VendorExceptions;
using MalDash.Domain.Exceptions.ProductExceptions;
using MalDash.Domain.Exceptions.CategoryExceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace MalDash.API.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, message, errors) = GetExceptionDetails(exception);

            _logger.LogError(exception, message);

            httpContext.Response.StatusCode = (int)statusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(new
            {
                error = message,
                errors,
                statusCode = (int)statusCode
            }, cancellationToken);

            return true;
        }

        private (HttpStatusCode statusCode, string message, string[]? errors) GetExceptionDetails(Exception exception)
        {
            return exception switch
            {
                // Authentication & Authorization
                LoginFailedException => (HttpStatusCode.Unauthorized, exception.Message, null),
                RefreshTokenException => (HttpStatusCode.Unauthorized, exception.Message, null),
                UserIsNotLoggedInException => (HttpStatusCode.Unauthorized, exception.Message, null),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message, null),

                // User-related
                UserAlreadyExistsException => (HttpStatusCode.Conflict, exception.Message, null),
                UserNotFoundException => (HttpStatusCode.NotFound, exception.Message, null),
                RegistrationFailedException => (HttpStatusCode.UnprocessableEntity, exception.Message, null),
                UserHasNoAddressException => (HttpStatusCode.NotFound, exception.Message, null),
                UserDeletionFailedException => (HttpStatusCode.Conflict, exception.Message, null),
                UserUpdateFailedException => (HttpStatusCode.UnprocessableEntity, exception.Message, null),

                // Building-related
                BuildingNotFoundException => (HttpStatusCode.NotFound, exception.Message, null),
                FloorNotFoundException => (HttpStatusCode.NotFound, exception.Message, null),
                ApartmentNotFoundException => (HttpStatusCode.NotFound, exception.Message, null),
                DuplicateBuildingNameException => (HttpStatusCode.Conflict, exception.Message, null),
                BuildingCreationFailedException => (HttpStatusCode.UnprocessableEntity, exception.Message, null),
                BuildingUpdateFailedException => (HttpStatusCode.UnprocessableEntity, exception.Message, null),
                BuildingDeletionFailedException => (HttpStatusCode.UnprocessableEntity, exception.Message, null),

                // Address-related
                AddressNotFoundException => (HttpStatusCode.NotFound, exception.Message, null),
                ApartmentAlreadyOccupiedException => (HttpStatusCode.Conflict, exception.Message, null),
                AddressAssignmentFailedException => (HttpStatusCode.UnprocessableEntity, exception.Message, null),
                AddressUnassignmentFailedException => (HttpStatusCode.UnprocessableEntity, exception.Message, null),
                AddressCreationFailedException => (HttpStatusCode.UnprocessableEntity, exception.Message, null),
                AddressUpdateFailedException => (HttpStatusCode.UnprocessableEntity, exception.Message, null),
                AddressDeletionFailedException => (HttpStatusCode.UnprocessableEntity, exception.Message, null),

                // Floor-related
                DuplicateFloorNumberException => (HttpStatusCode.Conflict, exception.Message, null),

                // Vendor-related
                VendorNotFoundException => (HttpStatusCode.NotFound, exception.Message, null),
                DuplicateVendorNameException => (HttpStatusCode.Conflict, exception.Message, null),
                UserAlreadyHasVendorException => (HttpStatusCode.Conflict, exception.Message, null),
                UserNotVendorRoleException => (HttpStatusCode.BadRequest, exception.Message, null),

                // Vendor staff-related
                VendorStaffNotFoundException => (HttpStatusCode.NotFound, exception.Message, null),
                UserAlreadyVendorStaffException => (HttpStatusCode.Conflict, exception.Message, null),
                VendorStaffInactiveException => (HttpStatusCode.Forbidden, exception.Message, null),

                // Product-related
                ProductNotFoundException => (HttpStatusCode.NotFound, exception.Message, null),

                // Category-related
                CategoryNotFoundException => (HttpStatusCode.NotFound, exception.Message, null),
                DuplicateCategoryNameException => (HttpStatusCode.Conflict, exception.Message, null),
                CategoryHasSubCategoriesException => (HttpStatusCode.Conflict, exception.Message, null),
                CategoryHasProductsException => (HttpStatusCode.Conflict, exception.Message, null),

                // Validation - with null safety
                ValidationException validationEx => (
                    HttpStatusCode.BadRequest,
                    "One or more validation errors occurred.",
                    validationEx.Errors?.ToArray() ?? []
                ),

                // File Storage & Common .NET Exceptions
                InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message, null),
                ArgumentNullException => (HttpStatusCode.BadRequest, exception.Message, null),
                ArgumentException => (HttpStatusCode.BadRequest, exception.Message, null),

                // Default
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.", null)
            };
        }
    }
}