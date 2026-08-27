namespace MechanicShop.Domain.Common.Results.Abstraction
{
   public interface IResult
    {

       public bool IsSuccess { get; }

       public  List<Error> error { get; }

       
    }


    public interface IResult<out Tvalu> : IResult
    {
        public Tvalu Value { get; }

    }


}
