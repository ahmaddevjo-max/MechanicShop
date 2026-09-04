using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using System.Net.Mail;
using System.Text.RegularExpressions;
namespace MechanicShop.Domain.Customers
{
    public sealed class Customer : AuditableEntity
    {

        public string? Name { get; private set; }
        public string? PhoneNumber { get; private set; }
        public string? Email { get; private set; }

        private List<Vehicle> _vehicles = [];

        public IEnumerable<Vehicle> vehicles => _vehicles.AsReadOnly();

        private Customer()
        { }


        private Customer(Guid Id,string name , string phoneNumber , string email , List<Vehicle>vehicles):
            base(Id)
        {
            Name = name;
            PhoneNumber = phoneNumber;
            Email = email;
            _vehicles = vehicles;
        }



        public static Result<Customer> Create(Guid id, string name, string phoneNumber, string email, List<Vehicle> vehicles)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return CustomerErrors.NameRequired;
            }

            if (string.IsNullOrWhiteSpace(phoneNumber) || !Regex.IsMatch(phoneNumber, @"^\+?\d{7,15}$"))
            {
                return CustomerErrors.InvalidPhoneNumber;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return CustomerErrors.EmailRequired;
            }

            try
            {
                _ = new MailAddress(email);
            }
            catch
            {
                return CustomerErrors.EmailInvalid;
            }

            return new Customer(id, name, phoneNumber, email, vehicles);
        }


        public Result<Updated> Update(string name, string email, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return CustomerErrors.NameRequired;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return CustomerErrors.EmailRequired;
            }

            if (string.IsNullOrWhiteSpace(phoneNumber) || !Regex.IsMatch(phoneNumber, @"^\+?\d{7,15}$"))
            {
                return CustomerErrors.InvalidPhoneNumber;
            }

            Name = name;
            Email = email;
            PhoneNumber = phoneNumber;

            return Result.updated;
        }


        public Result<Updated> UpsertVehicles(List<Vehicle> incomingVehicles)
        {
            _vehicles.RemoveAll(existing => incomingVehicles.All(v => v.Id != existing.Id));

            foreach (var incoming in incomingVehicles)
            {

                var existing = _vehicles.FirstOrDefault(v => v.Id == incoming.Id); 

                if(existing is null)
                {
                    _vehicles.Add(incoming);
                }
                else
                {
                    var updateVehicleResult = existing.Update(incoming.Make , incoming.Model , incoming.Year , incoming.LicensePlate);

                    if(updateVehicleResult.IsError)
                    {
                        return updateVehicleResult.error;
                    }

                   
                }
            }

            return Result.updated;
        }







    }
}
