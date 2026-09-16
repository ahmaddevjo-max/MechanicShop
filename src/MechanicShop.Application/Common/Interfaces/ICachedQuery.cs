using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace MechanicShop.Application.Common.Interfaces
{
    public interface ICachedQuery 
    {
        public string CacheKey { get; }
        public string [] Tags { get; }
        public TimeSpan Expiration { get; }
    }



    public interface ICachedQuery<TRequst> : IRequest<TRequst>, ICachedQuery;


}
