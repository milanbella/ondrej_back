using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailAppS.Controllers.Model.Api.Gui.AccountViewController;
using RetailAppS.Dbo;
using RetailAppS.Sessionn;

namespace RetailAppS.Controllers.Api.Gui
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
