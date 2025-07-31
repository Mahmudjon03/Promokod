using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PromoRandom.Models;
using PromoRandom.Services;
using PromoRandom.ViewModels;

namespace PromoRandom.Controllers
{
    [Authorize]
    public class AdminController(ImageService imageService, DatabaseService databaseService) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // -------------------- Розыгрыши (Gives) --------------------

        // Страница розыгрышей
        public async Task<IActionResult> Gives()
        {
            var gives = await databaseService.GetAllGivesAsync();
            return View(gives);
        }

        // Создание розыгрыша (POST)
        [HttpPost]
        public async Task<IActionResult> CreateGive(Give give)
        {
            if (ModelState.IsValid)
            {
                await databaseService.AddGiveAsync(give);
                return RedirectToAction("Gives");
            }
            return View("Gives", await databaseService.GetAllGivesAsync());
        }

        // GET: Возвращает частичное представление для редактирования розыгрыша
        public async Task<IActionResult> GetEditGivePartial(int id)
        {
            var give = await databaseService.GetGiveByIdAsync(id);
            if (give == null)
            {
                return NotFound();
            }
            return PartialView("_EditGivePartial", give);
        }


        // Редактирование розыгрыша (POST)
        [HttpPost]
        public async Task<IActionResult> EditGive(Give give)
        {
            if (ModelState.IsValid)
            {
                await databaseService.UpdateGiveAsync(give);

                // Если розыгрыш завершён — истекаем все его активные промокоды
                if (give.IsCompleted)
                {
                    await databaseService.ExpirePromoCodesByGiveIdAsync(give.Id);
                }

                return RedirectToAction("Gives");
            }

            return PartialView("_EditGivePartial", give);
        }


        // Удаление розыгрыша (POST) — вызывается после подтверждения удаления
        [HttpPost]
        public async Task<IActionResult> DeleteGive(int id)
        {
            await databaseService.DeleteGiveAsync(id);
            return RedirectToAction("Gives");
        }

        // -------------------- Призы (Prizes) --------------------

        // Страница призов (передаём также список розыгрышей для комбобокса)
        public async Task<IActionResult> Prizes(
            string? nameFilter = null,
            int? giveFilter = null,
            string? sortField = null,
            string sortDirection = "asc")
        {
            var prizes = await databaseService.GetAllPrizesAsync();

            // Фильтрация
            if (!string.IsNullOrEmpty(nameFilter))
            {
                prizes = prizes.Where(p => p.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (giveFilter.HasValue)
            {
                prizes = prizes.Where(p => p.GiveId == giveFilter.Value).ToList();
            }

            // Сортировка
            if (!string.IsNullOrEmpty(sortField))
            {
                if (sortField == "Priority")
                {
                    prizes = sortDirection == "asc" ?
                        prizes.OrderBy(p => p.Priority).ToList() :
                        prizes.OrderByDescending(p => p.Priority).ToList();
                }
                else if (sortField == "SpinDuration")
                {
                    prizes = sortDirection == "asc" ?
                        prizes.OrderBy(p => p.SpinDuration).ToList() :
                        prizes.OrderByDescending(p => p.SpinDuration).ToList();
                }
            }

            ViewBag.Gives = await databaseService.GetAllGivesAsync();
            return View(prizes);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePrize(Prize prize, IFormFile prizeImage)
        {
            if (ModelState.IsValid && prizeImage != null)
            {
                try
                {
                    // Временное сохранение приза
                    prize.ImagePath = "temp";
                    prize.Id = await databaseService.AddPrizeAsync(prize);

                    // Сохранение изображения через сервис
                    prize.ImagePath = await imageService.SavePrizeImageAsync(prizeImage, prize);

                    // Финальное обновление
                    await databaseService.UpdatePrizeAsync(prize);
                    return RedirectToAction("Prizes");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("PrizeImage", ex.Message);
                }
            }

            ViewBag.Gives = await databaseService.GetActiveGivesAsync();
            return View("Prizes", await databaseService.GetAllPrizesAsync());
        }

        [HttpPost]
        public async Task<IActionResult> EditPrize(Prize prize, IFormFile prizeImage)
        {
            var existingPrize = await databaseService.GetPrizeByIdAsync(prize.Id);
            if (existingPrize == null) return NotFound();

            if (prizeImage != null)
            {
                try
                {
                    // Удаление старого изображения
                    if (!string.IsNullOrEmpty(existingPrize.ImagePath))
                    {
                        imageService.DeleteImage(existingPrize.ImagePath);
                    }

                    // Сохранение нового через сервис
                    prize.ImagePath = await imageService.SavePrizeImageAsync(prizeImage, prize);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("PrizeImage", ex.Message);
                }
            }
            else
            {
                prize.ImagePath = existingPrize.ImagePath;
            }

            await databaseService.UpdatePrizeAsync(prize);
            return RedirectToAction("Prizes");
        }

        // Редактирование приза (GET)
        public async Task<IActionResult> GetEditPrizePartial(int id)
        {
            var prize = await databaseService.GetPrizeByIdAsync(id);
            if (prize == null)
            {
                return NotFound();
            }
            // Если для заполнения комбобокса нужен список розыгрышей, передаём его через ViewBag.
            ViewBag.Gives = await databaseService.GetAllGivesAsync();
            return PartialView("_EditPrizePartial", prize);
        }

        // Удаление приза (POST)
        [HttpPost]
        public async Task<IActionResult> DeletePrize(int id)
        {
            var prize = await databaseService.GetPrizeByIdAsync(id);
            if (prize == null) return NotFound();

            if (!string.IsNullOrEmpty(prize.ImagePath))
            {
                imageService.DeleteImage(prize.ImagePath);
            }

            await databaseService.DeletePrizeAsync(id);
            return RedirectToAction("Prizes");
        }

        // Промокоды
        public async Task<IActionResult> PromoCodes(
            string? search = null,
            int? orderId = null,
            long? userId = null,
            int? giveId = null,
            string? status = null,
            string? userSearch = null,
            string? prizeName = null,  // Добавляем новый параметр фильтрации
            int page = 1)
        {
            const int pageSize = 25;

            var filter = new PromoCodeFilter
            {
                Search = search,
                OrderId = orderId,
                UserChatId = userId,
                GiveId = giveId,
                Status = status,
                PrizeName = prizeName,  // Добавляем фильтр по призу
                Page = page,
                PageSize = pageSize
            };

            PromoCodeResult result = await databaseService.GetFilteredPromoCodesAsync(filter);
            List<Give> gives = await databaseService.GetAllGivesAsync();
            List<string> statuses = ["active", "won", "expired", "disabled"];
            List<string> prizeNames = await databaseService.GetAllPrizeNamesAsync();  // Получаем уникальные имена призов

            ViewBag.UserSearch = userSearch;
            ViewBag.UserChatId = userId;
            ViewBag.Search = search;
            ViewBag.OrderId = orderId;
            ViewBag.GiveId = giveId;
            ViewBag.Status = status;
            ViewBag.PrizeName = prizeName;  // Сохраняем выбранный приз
            ViewBag.Gives = gives;
            ViewBag.Statuses = statuses;
            ViewBag.PrizeNames = prizeNames;  // Передаем список призов в представление
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = result.TotalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize);

            return View(result.PromoCodes);
        }

        public async Task<IActionResult> GetPromoCodeDetails(int id)
        {
            var promoCode = await databaseService.GetPromoCodeByIdAsync(id);
            if (promoCode == null) return NotFound();

            var user = await databaseService.GetUserByChatIdAsync(promoCode.UserChatId);
            var give = await databaseService.GetGiveByIdAsync(promoCode.GiveId);
            var order = await databaseService.GetOrderByIdAsync(promoCode.OrderId);

            Prize? prize = null;
            if (promoCode.Status == "won")
            {
                // If the promo code is won, retrieve prize information  
                prize = await databaseService.GetPrizeByPromoCodeIdAsync(promoCode.Id);
            }

            var viewModel = new PromoCodeViewModel
            {
                Id = promoCode.Id,
                Code = promoCode.Code,
                Status = promoCode.Status,
                SubmittedAt = promoCode.SubmittedAt,
                OrderId = promoCode.OrderId,
                User = user ?? new User(), // Handle possible null reference  
                Give = give ?? new Give(), // Handle possible null reference  
                Order = order ?? new Order(), // Handle possible null reference  
                Prize = prize ?? new Prize(), // Assign the prize variable  
            };

            return PartialView("_PromoCodeDetailsPartial", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string term)
        {
            var users = await databaseService.SearchUsersAsync(term);
            return Json(users.Select(u => new {
                id = u.ChatId,
                name = u.Name ?? "Без имени",
                phone = u.Phone ?? "Нет телефона"
            }));
        }

        // POST: Сохранение изменений
        [HttpPost]
        public async Task<IActionResult> EditPromoCode(int id, string status)
        {
            var promo = await databaseService.GetPromoCodeByIdAsync(id);
            if (promo == null) return NotFound();

            // Проверяем, что статус допустимый
            var allowedStatuses = new[] { "active", "won", "expired", "disabled" };
            if (!allowedStatuses.Contains(status))
            {
                return BadRequest("Недопустимый статус промо-кода");
            }

            promo.Status = status;
            await databaseService.UpdatePromoCodeAsync(promo);

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var minimumAmount = await databaseService.GetMinimumAmountAsync();
            ViewBag.MinimumAmount = minimumAmount;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSettings(int minimumAmount)
        {
            if (minimumAmount < 0)
            {
                return BadRequest("Минимальная сумма не может быть отрицательной");
            }

            await databaseService.UpdateMinimumAmountAsync(minimumAmount);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> UserManagement(
            string? search = null,
            int page = 1,
            string sortBy = "chat_id", // Добавляем параметры сортировки
            string sortOrder = "desc")
        {
            const int pageSize = 25;

            var filter = new UserFilter
            {
                Search = search,
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,       // Передаем в фильтр
                SortOrder = sortOrder  // Передаем в фильтр
            };

            var (users, totalCount) =
                await databaseService.GetFilteredUserViewModelsAsync(filter);

            // Сохраняем параметры в ViewBag
            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.SortBy = sortBy;          // Для сохранения состояния
            ViewBag.SortOrder = sortOrder;    // Для сохранения состояния

            return View(users);
        }


        [HttpGet]
        public async Task<IActionResult> UserDetails(long chatId)
        {
            var user = await databaseService.GetUserByChatIdAsync(chatId);
            if (user == null) return NotFound();

            // Получаем промокоды пользователя
            var filter = new PromoCodeFilter
            {
                UserChatId = chatId,
            };

            PromoCodeResult result = await databaseService.GetFilteredPromoCodesAsync(filter);

            // Получаем заказы пользователя
            var orders = await databaseService.GetOrdersByUserAsync(chatId);

            var viewModel = new UserDetailsViewModel
            {
                User = user,
                PromoCodes = result.PromoCodes,
                Orders = orders
            };

            return View(viewModel);
        }
    }
}
