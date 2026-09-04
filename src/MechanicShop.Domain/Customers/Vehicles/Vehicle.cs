
using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using System.Reflection;

namespace MechanicShop.Domain.Customers.Vehicles
{
    public sealed class Vehicle : AuditableEntity
    {

        public Guid CustomerId { get; }
        public string Make { get; private set; } = default!;
        public string Model { get; private set; } = default!;
        public int Year { get; private set; }
        public string LicensePlate { get; private set; } = default!;
        public Customer? Customer { get; set; } = default!;
        public string VehicleInfo => $"{Make} | {Model} | {Year}";



        private Vehicle()
        { }

        private Vehicle(Guid id, string make, string model, int year, string licensePlate)
        : base(id)
        {
            Make = make;
            Model = model;
            Year = year;
            LicensePlate = licensePlate;
        }


        public static Result<Vehicle> Create(Guid id, string make, string model, int year, string licensePlate)
        {

            if (string.IsNullOrWhiteSpace(make))
            {
                return VehicleErrors.MakeRequired;
            }

            if(string.IsNullOrWhiteSpace(model))
            {
                return VehicleErrors.ModelRequired;
            }

            if (string.IsNullOrWhiteSpace(licensePlate))
            {
                return VehicleErrors.LicensePlateRequired;
            }

            if( year < 1886  ||  year > DateTime.UtcNow.Year )
            {
                return VehicleErrors.YearInvalid;
            }

            return new Vehicle(id , make , model , year , licensePlate);
           
        }



        public Result<Updated> Update(string make, string model, int year, string licensePlate)
        {
            if(string.IsNullOrWhiteSpace(make))
            {
                return VehicleErrors.MakeRequired;
            }

            if (string.IsNullOrWhiteSpace(model))
            {
                return VehicleErrors.ModelRequired;
            }

            if (string.IsNullOrWhiteSpace(licensePlate))
            {
                return VehicleErrors.LicensePlateRequired;
            }

            if (year < 1886 || year > DateTime.UtcNow.Year)
            {
                return VehicleErrors.YearInvalid;
            }


            Make = make; 
            Model = model;
            LicensePlate =  licensePlate;
            Year = year;

            return Result.updated;
        }


    }
}
