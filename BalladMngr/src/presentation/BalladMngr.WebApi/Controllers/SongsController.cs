using BalladMngr.Application.Songs.Commands.CreateSong;
using BalladMngr.Application.Songs.Commands.DeleteSong;
using BalladMngr.Application.Songs.Commands.UpdateSong;
using BalladMngr.Application.Songs.Queries.GetSongs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BalladMngr.WebApi.Controllers
{
    /*
     * SongsController, şarkı ekleme, silme, güncelleme ve şarkı listesini ViewModel ölçütünde çekme işlemlerini üstleniyor.
     * 
     * Constructor üstünden MediatR modülünün enjekte edildiğini görebiliyoruz.
     * 
     * Önceki versiyonuna göre en büyük fark elbetteki metotlarda Query/Command nesnelerinin kullanılması.
     * 
     * Ayrıca fonksiyon içeriklerine bakıldığında yapılan tek şey Send ile ilgili komutu MeditaR'a yönlendirmek. O gerisini halleder
     * 
     */
    [Route("api/[controller]")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SongsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<SongsViewModel>> Get()
        {
            return await _mediator.Send(new GetSongsQuery());
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(CreateSongCommand command)
        {
            return await _mediator.Send(command);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _mediator.Send(new DeleteSongCommand { SongId = id });
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdateSongCommand command)
        {
            if (id != command.SongId)
                return BadRequest();
            await _mediator.Send(command);

            return NoContent();
        }
    }
}