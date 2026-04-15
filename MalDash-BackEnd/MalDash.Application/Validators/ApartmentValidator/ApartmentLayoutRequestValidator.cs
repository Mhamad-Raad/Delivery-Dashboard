using FluentValidation;
using MalDash.Application.Requests.ApartmentRequests.FloorPlan;

namespace MalDash.Application.Validators.ApartmentValidator
{
    public class ApartmentLayoutRequestValidator : AbstractValidator<ApartmentLayoutRequest>
    {
        public ApartmentLayoutRequestValidator()
        {
            RuleFor(x => x.GridSize)
                .InclusiveBetween(1, 1000)
                .WithMessage("Grid size must be between 1 and 1000");

            RuleFor(x => x.Rooms)
                .Must(rooms => rooms == null || rooms.Count <= 100)
                .WithMessage("Cannot exceed 100 rooms per apartment");

            RuleFor(x => x.Doors)
                .Must(doors => doors == null || doors.Count <= 200)
                .WithMessage("Cannot exceed 200 doors per apartment");

            RuleForEach(x => x.Rooms).SetValidator(new RoomLayoutRequestValidator());
            RuleForEach(x => x.Doors).SetValidator(new DoorRequestValidator());
        }
    }

    public class RoomLayoutRequestValidator : AbstractValidator<RoomLayoutRequest>
    {
        public RoomLayoutRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmptyWithException("Room ID");

            RuleFor(x => x.Type)
                .NotEmptyWithException("Room type");

            RuleFor(x => x.Name)
                .NotEmptyWithException("Room name");

            RuleFor(x => x.X)
                .InclusiveBetween(-1000, 1000)
                .WithMessage("X position must be between -1000 and 1000");

            RuleFor(x => x.Y)
                .InclusiveBetween(-1000, 1000)
                .WithMessage("Y position must be between -1000 and 1000");

            RuleFor(x => x.Width)
                .InclusiveBetween(0.1, 100)
                .WithMessage("Width must be between 0.1 and 100");

            RuleFor(x => x.Height)
                .InclusiveBetween(0.1, 100)
                .WithMessage("Height must be between 0.1 and 100");
        }
    }

    public class DoorRequestValidator : AbstractValidator<DoorRequest>
    {
        private static readonly string[] ValidEdges = ["top", "bottom", "left", "right"];

        public DoorRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmptyWithException("Door ID");

            RuleFor(x => x.RoomId)
                .NotEmptyWithException("Room ID");

            RuleFor(x => x.Edge)
                .NotEmptyWithException("Edge")
                .Must(edge => ValidEdges.Contains(edge.ToLowerInvariant()))
                .WithMessage("Edge must be 'top', 'bottom', 'left', or 'right'");

            RuleFor(x => x.Position)
                .InclusiveBetween(0, 1)
                .WithMessage("Position must be between 0 and 1");

            RuleFor(x => x.Width)
                .InclusiveBetween(0.1, 10)
                .WithMessage("Door width must be between 0.1 and 10");
        }
    }
}