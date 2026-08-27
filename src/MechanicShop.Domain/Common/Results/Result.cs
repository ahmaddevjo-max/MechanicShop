using MechanicShop.Domain.Common.Results.Abstraction;
using System.ComponentModel;
using System.Text.Json.Serialization;


namespace MechanicShop.Domain.Common.Results
{

    public static class Result           
    {
        public  static Success success => default;
        public static Created  created => default;
        public static Deleted  deleted => default;
        public static Updated  updated => default;
    }



    public sealed class Result<TValue> : IResult<TValue>
    {
        
        private readonly TValue ? _value = default;

        private readonly List<Error>? _errors = null;


      
        public bool IsSuccess { get; }

        public bool IsError => !IsSuccess;

        public List<Error> error => IsError ? _errors! : [];

        public TValue Value => IsSuccess ? _value! : default!;

        public Error TopError => (_errors?.Count > 0) ? _errors[0] : default;



        

        [JsonConstructor]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("For serializer only.", true)]
        public Result(TValue? value, List<Error>? errors, bool isSuccess)
        {

            if(isSuccess)
            {

                _value = value ?? throw  new ArgumentNullException(nameof(value));
                _errors = [];
                IsSuccess = true;
               

            }
            else
            {


                if(errors is null || errors.Count == 0)
                {

                    throw new ArgumentNullException(nameof(errors));    

                }

                _errors = errors;
                _value = default;
                isSuccess = false;

            }
        }

       
        private Result(Error error)
        {
            _errors = [ error ];
        }


        private Result(List<Error>errors)
        {
            if(errors == null || errors.Count == 0)
            {
                throw new ArgumentException
                    ("Cannot create an ErrorOr<TValue> from an empty collection of errors. Provide at least one error."
                    , nameof(errors));
            }

            _errors = errors;

            IsSuccess = false;

        }

        private Result(TValue value)
        {
            if(value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            _value = value;
            IsSuccess = true;
        }

        public static implicit operator Result<TValue>(TValue value)
              => new(value);

        public static implicit operator Result<TValue>(Error error)
            => new(error);

        public static implicit operator Result<TValue>(List<Error> errors)
            => new(errors);


        public  TNextValue Match<TNextValue>(Func<TValue ,TNextValue> onValue , Func<List<Error> ,TNextValue>onError)
            => IsSuccess ? onValue(Value) : onError(error);


    }



    public readonly record struct Success;
    public readonly record struct Created;

    public readonly record struct Deleted;
    public readonly record struct Updated;


}
