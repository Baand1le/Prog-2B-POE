using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RaceDayApp_POE.Controllers
{
    [Route("api/events")]
    [ApiController]
    [Authorize]
    public class EventsController : Controller
    {
        
    }//end of class: EventsController

}//end of namespace