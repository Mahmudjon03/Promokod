using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PromoRandom.Models;
using PromoRandom.Services;
using PromoRandom.ViewModels;

namespace PromoRandom.Controllers
{
    [Authorize]
    public class AdminController(IWebHostEnvironment environment, DatabaseService databaseService) : Controller
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
                return RedirectToAction("Gives");
            }
            // Если есть ошибка валидации, можно вернуть частичное представление с ошибками
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

            ViewBag.Gives = await databaseService.GetActiveGivesAsync();
            return View(prizes);
        }

        // Создание приза (POST)
        [HttpPost]
        public async Task<IActionResult> CreatePrize(Prize prize, IFormFile prizeImage)
        {
            if (ModelState.IsValid && prizeImage != null && prizeImage.Length > 0)
            {
                // Временно сохраняем приз, чтобы получить ID
                prize.ImagePath = "temp"; // Временное значение
                prize.Id = await databaseService.AddPrizeAsync(prize);

                // Генерируем имя файла
                var fileName = $"prize{prize.Id}_{prize.GiveId}{Path.GetExtension(prizeImage.FileName)}";
                var filePath = Path.Combine(environment.WebRootPath, "images", "prizes", fileName);

                // Сохраняем изображение
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await prizeImage.CopyToAsync(stream);
                }

                // Обновляем путь к изображению
                prize.ImagePath = $"/images/prizes/{fileName}";
                await databaseService.UpdatePrizeAsync(prize);

                return RedirectToAction("Prizes");
            }

            // Если ошибка, возвращаемся
            ViewBag.Gives = await databaseService.GetActiveGivesAsync();
            return View("Prizes", await databaseService.GetAllPrizesAsync());
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

        // Редактирование приза (POST)
        [HttpPost]
        public async Task<IActionResult> EditPrize(Prize prize, IFormFile prizeImage)
        {
            // Получаем текущий приз из базы
            var existingPrize = await databaseService.GetPrizeByIdAsync(prize.Id);
            if (existingPrize == null)
            {
                return NotFound();
            }

            // Сохраняем текущий путь к изображению
            var currentImagePath = existingPrize.ImagePath;

            // Если загружено новое изображение
            if (prizeImage != null && prizeImage.Length > 0)
            {
                // Удаляем старое изображение, если оно существует
                if (!string.IsNullOrEmpty(currentImagePath))
                {
                    var oldImagePath = Path.Combine(environment.WebRootPath, currentImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Создаем новое имя файла
                var fileName = $"prize{prize.Id}_{prize.GiveId}{Path.GetExtension(prizeImage.FileName)}";
                var filePath = Path.Combine(environment.WebRootPath, "images", "prizes", fileName);

                // Сохраняем новое изображение
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await prizeImage.CopyToAsync(stream);
                }

                prize.ImagePath = $"/images/prizes/{fileName}";
            }
            else
            {
                // Если новое изображение не загружено, сохраняем текущее
                prize.ImagePath = currentImagePath;
            }

            await databaseService.UpdatePrizeAsync(prize);
            return RedirectToAction("Prizes");
        }


        // Удаление приза (POST)
        [HttpPost]
        public async Task<IActionResult> DeletePrize(int id)
        {
            await databaseService.DeletePrizeAsync(id);
            return RedirectToAction("Prizes");
        }

        // Промокоды
        public async Task<IActionResult> PromoCodes(
            string? search = null,
            int? orderId = null,  // Добавляем новый параметр
            int? giveId = null,
            string? status = null,
            int page = 1)
        {
            const int pageSize = 25;

            var filter = new PromoCodeFilter
            {
                Search = search,
                OrderId = orderId,  // Добавляем в фильтр
                GiveId = giveId,
                Status = status,
                Page = page,
                PageSize = pageSize
            };

            PromoCodeResult result = await databaseService.GetFilteredPromoCodesAsync(filter);
            List<Give> gives = await databaseService.GetAllGivesAsync();
            List<string> statuses = ["active", "won", "expired", "disabled"];

            ViewBag.Search = search;
            ViewBag.OrderId = orderId;  // Добавляем в ViewBag
            ViewBag.GiveId = giveId;
            ViewBag.Status = status;
            ViewBag.Gives = gives;
            ViewBag.Statuses = statuses;
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
    }
}
