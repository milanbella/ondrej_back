using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ondrej.Controllers.Model.Api.Gui.AccountViewController;
using Ondrej.Dbo;
using Ondrej.Sessionn;

namespace Ondrej.Controllers.Api.Gui
{
    [Route("api/gui/account-view")]
    public class AccountViewController : Controller
    {
        private SessionService sessionService;
        private Db db;

        public AccountViewController(SessionService sessionService, Db db)
        {
            this.sessionService = sessionService;
            this.db = db;
        }

    }
}
