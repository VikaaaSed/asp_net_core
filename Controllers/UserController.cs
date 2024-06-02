using C_Sharp_lab_4.DbContexts;
using C_Sharp_lab_4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System.Diagnostics;
using static C_Sharp_lab_4.Models.FilterVM;
using static System.Net.Mime.MediaTypeNames;

namespace C_Sharp_lab_4.Controllers
{
    public class UserController : Controller
    {
        private const string KeyLogin = "login";
        private const string KeyId = "id";
        private const string KeyFIO = "fio";
        private const string KeyPassword = "Password";

        private readonly ILogger<UserController> _logger;
        private readonly MyDbContext _context;
        public UserController(ILogger<UserController> logger, MyDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Index() => View();
        public async Task<IActionResult> Registration(RegistModel RM)
        {
            User _user = await AutoLogin();

            if (_user != null) return RedirectToAction("Аccount", new { controller = "User", action = "Аccount", id = _user.Id });
            ModelState.Remove("id");
            if(ModelState.IsValid && _context != null)
            {
                if (_context.Users.Any(u => u.Login == RM.Login))
                {
                    ViewData["Registration_Error"] = "Пользователь с такил логином уже существует";
                    return View(RM);
                }
                if(RM.Password != RM.AcceptPassword)
                {
                    ViewData["Registration_Error"] = "Пороли не воспадают";
                    return View(RM);
                }
                await _context.Users.AddAsync(RM);
                await _context.SaveChangesAsync();

                _user = await _context.Users.Where(u => u.Password == RM.Password && u.Login == RM.Login).SingleOrDefaultAsync();
                if(_user != null)
                {
                    SaveCooKie(_user);
                    return RedirectToAction("Аccount", new { controller = "User", action = "Аccount"});
                }
            }
            return View(RM);
        }
        public async Task<IActionResult> Аccount(FilterVM filterVM)
        {
            var _id = Request.Cookies[KeyId];
            if (_id == null) return RedirectToAction("Login");
            
            User _user = await _context.Users.FindAsync(int.Parse(_id));
            ViewData["name"] = _user.FIO;
            ViewData["SenderSort"] = filterVM.SenderSort == "Sender" ? "Sender_desc" : "Sender";
            ViewData["HeddrSort"] = filterVM.HedderSort == "hedder" ? "hedder_desc" : "hedder";
            ViewData["DataSort"] = filterVM.DateSort == "date" ? "date_desc" : "date";

            var message = (from msg in _context.Message.ToList()
                           where msg.Id_Recipient == _user.Id
                           select msg).ToList().Where(msg => msg.Status || filterVM.Status != "on").Select(msg => new MessageModel
                           {
                                Id = msg.Id,
                                Sender = _context.Users.ToList().First(u => u.Id == msg.Id_Sender).Login,
                                Hedder = msg.Hedder,
                                TextMessage = msg.TextMessage,
                                Date = msg.DateDispatch,
                                Status = msg.Status
                           }).ToList().OrderByDescending(m => m.Date);

            if (filterVM.DateSort == "date")
                message = message.OrderBy(m => m.Date);
            else if(filterVM.HedderSort == "hedder")
                message = message.OrderBy(m => m.Hedder);
            else if (filterVM.HedderSort == "hedder_desc")
                message = message.OrderByDescending(m => m.Hedder);
            else if (filterVM.HedderSort == "Sender")
                message = message.OrderBy(m => m.Sender);
            else if (filterVM.HedderSort == "Sender_desc")
                message = message.OrderByDescending(m => m.Sender);

            return View(message);
        }
        [HttpPost]
        public IActionResult Аccount (SendMessageModel model)
        {
            var _id = Request.Cookies[KeyId];
            if (_id == null) return RedirectToAction("Login");
            
            var receiverUser = (from usr in _context.Users.ToList()
                                where usr.Login == model.Recipient
                                select usr).Take(1).ToList().FirstOrDefault(u => true, null);
            if(receiverUser != null)
            {
                _context.Message.Add(new Message
                {
                    Id_Sender = int.Parse(_id),
                    Id_Recipient = receiverUser.Id,
                    Hedder = model.Hedder,
                    TextMessage = model.TextMessage,
                    DateDispatch = DateTime.UtcNow,
                    Status = true
                });
                _context.SaveChanges();
            }
            return RedirectToAction("Аccount", new { controller = "User", action = "Аccount" });
        }
        public async Task<IActionResult> AllUsers()
            => View(await _context.Users.ToListAsync());
        public async Task<IActionResult> Login(User userModel)
        {
            var login = Request.Cookies[KeyLogin];
            var password = Request.Cookies[KeyPassword];
            User _user = await AutoLogin();
            if (_user != null) return RedirectToAction("Аccount", new { controller = "User", action = "Аccount"});

            ModelState.Remove("id");
            ModelState.Remove("fio");

            if (ModelState.IsValid && _context != null)
            {
                _user = await _context.Users.Where(u => u.Password == userModel.Password && u.Login == userModel.Login).SingleOrDefaultAsync();
                if (_user != null)
                {
                    SaveCooKie(_user);
                    return RedirectToAction("Аccount", new { controller = "User", action = "Аccount"});
                }
                ViewData["Аuthorization_Error"] = "Неверные логин или пароль";
            }
            return View(userModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult LogOut()
        {
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Append(KeyLogin, "", options);
            Response.Cookies.Append(KeyPassword, "", options);
            Response.Cookies.Append(KeyId, "", options);
            Response.Cookies.Append(KeyFIO, "", options);
            return RedirectToAction("Login");
        }
        public void SaveCooKie(User _user)
        {
            CookieOptions cookieOptions = new CookieOptions();
            cookieOptions.Expires = DateTime.Now.AddDays(1);
            Response.Cookies.Append(KeyLogin, _user.Login, cookieOptions);
            Response.Cookies.Append(KeyPassword, _user.Password, cookieOptions);
            Response.Cookies.Append(KeyId, _user.Id.ToString(), cookieOptions);
            Response.Cookies.Append(KeyFIO, _user.FIO, cookieOptions);
        }
        private async Task<User> AutoLogin()
        {
            var login = Request.Cookies[KeyLogin];
            var password = Request.Cookies[KeyPassword];
            if(login == null || password == null) return null;
            User _user = await _context.Users.Where(u => u.Password == password && u.Login == login).SingleOrDefaultAsync();
            if (_user == null) return null;
            return _user;
        }
        public IActionResult Error() 
            => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        [HttpPost]
        public IActionResult MarkAsRead(int id)
        {
            var message = _context.Message.Find(id);
            if (message == null)
            {
                return NotFound();
            }

            message.Status = false;
            _context.Message.Update(message);
            _context.SaveChanges();

            return NoContent();
        }

    }
}
