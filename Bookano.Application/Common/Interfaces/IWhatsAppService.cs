using Bookano.Domain.Entities;

namespace Bookano.Application.Common.Interfaces
{
    public interface IWhatsAppService<T>
    {
        Task SendWhatsApp(T entity, string template, List<object>? parameters = null);
    }
}
