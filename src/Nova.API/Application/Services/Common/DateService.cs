using Nova.API.Application.Services.Data;

namespace Nova.API.Application.Services.Common
{
    public interface IDateService
    {
        DateTime Now { get; }
    }

    public class DateService : IDateService
    {
        private readonly int _timezone;

        public DateService(int timeZone = 1)
        {
            _timezone = timeZone;
        }

        public DateTime Now => DateTime.UtcNow.AddHours(_timezone);
    }
}
