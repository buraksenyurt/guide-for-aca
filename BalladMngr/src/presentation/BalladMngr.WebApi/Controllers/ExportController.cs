using BalladMngr.Application.Songs.Queries.ExportSongs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BalladMngr.WebApi.Controllers
{
    /*
     * Şarkı listesini CSV formatında export etmemizi sağlayan Controller.
     * 
     * SongsController'da olduğu gibi MediatR kullanıyor ve CSV çıktısı için ilgili Query komutunu işletiyor.
     */
    [Route("api/[controller]")]
    [ApiController]
    public class ExportController 
        : ControllerBase
    {
        private readonly IMediator _mediator;
        public ExportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<FileResult> Get()
        {
            var model = await _mediator.Send(new ExportSongsQuery());
            return File(model.Content, model.ContentType, model.FileName);
        }
    }
}