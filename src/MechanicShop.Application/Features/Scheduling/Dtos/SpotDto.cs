using MechanicShop.Domain.WorkOerders.Enums;
namespace MechanicShop.Application.Features.Scheduling.Dtos;

public class SpotDto
{
    public Spot Spot { get; set; }
    public List<AvailabilitySlotDto> Slots { get; set; } = [];


}