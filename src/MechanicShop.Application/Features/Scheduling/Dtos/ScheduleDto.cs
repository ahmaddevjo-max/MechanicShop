
namespace MechanicShop.Application.Features.Scheduling.Dtos
{
    public class ScheduleDto
    {
        public DateOnly OndDate { get; set; }
        public bool EndOfDay {  get; set; }
        public List<SpotDto> spots { get; set; } = [];
    }
}
