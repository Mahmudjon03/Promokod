const chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".split("");
const reelCount = 8;

const promoDisplay = document.getElementById("promoDisplay");

// Создаем 8 барабанов при загрузке страницы (если не созданы в HTML)
function createReels() {
    promoDisplay.innerHTML = "";
    for (let i = 0; i < reelCount; i++) {
        const reel = document.createElement("div");
        reel.className = "reel";

        const symbols = document.createElement("div");
        symbols.className = "symbols";

        // Добавляем все символы как строки
        symbols.innerHTML = chars.join("<br>");
        reel.appendChild(symbols);
        promoDisplay.appendChild(reel);
    }
}
createReels();
const selectedPrizeId = document.getElementById("prizeSelect").value;
document.getElementById("generateBtn").addEventListener("click", () => {
    fetch("/Home/GetPromoCode")
        .then((res) => res.json())
        .then((data) => {
            const code = data.promoCode.toUpperCase();
            const reels = document.querySelectorAll(".reel .symbols");

            reels.forEach((symbolsDiv, i) => {
                let position = 0;
                const targetChar = code[i];
                const targetIndex = chars.indexOf(targetChar);

                // Запускаем анимацию кручения (прокрутки вниз)
                let speed = 10; // ms между шагами прокрутки
                let steps = 80;
                const maxSteps = 380 + i * 155; // разная длительность для каждого барабана

                function spin() {
                    steps++;
                    position += 2; // смещение вниз на 1px
                    if (position > 150 * chars.length) position = 0; // зациклить

                    symbolsDiv.style.transform = `translateY(-${position}px)`;

                    if (steps < maxSteps) {
                        setTimeout(spin, speed);
                    } else {
                        // Останавливаемся на нужном символе
                        // Смещаем точно, чтобы показать нужный символ сверху
                        const finalPosition = 150 * targetIndex;
                        symbolsDiv.style.transition = "transform 1.3s ease-out";
                        symbolsDiv.style.transform = `translateY(-${finalPosition}px)`;
                    }
                }
                spin();
            });

            // Показываем приз через время, когда все барабаны остановятся
            setTimeout(() => {
                const modal = document.getElementById("resultModal");
                const select = document.getElementById("prizeSelect");
                const prizeTitle = select.options[select.selectedIndex].text;
                const winnerName = document.getElementById("winnerName");
                const promoCodeText = document.getElementById("promoCodeText");
                console.log(prizeTitle);
                // Распаковываем имя пользователя и приз
                const prizeText = data.prize; // например: "Ali 🎁 IPhone 16 Pro Max 😍!"
                const nameMatch = prizeText.match(/^(.+?)\s*🎁/);
                const prizeMatch = prizeText.match(/🎁\s*(.+)$/);

                const userName = nameMatch ? nameMatch[1] : "Пользователь";
                const prizeName = prizeMatch ? prizeMatch[1] : "Приз";

                prizeTitle.textContent = prizeName;
                winnerName.textContent = userName;
                promoCodeText.textContent = data.promoCode;

                modal.style.display = "block";
                document.getElementById("prizeTitle").textContent = prizeTitle;
                createConfetti(false);
            }, 16000);
        })
        .catch(() => {
            document.getElementById("prizeText").textContent = "Ошибка получения промокода";
        });
});

document.getElementById("nextBtn").addEventListener("click", () => {
    const selectedPrizeId = document.getElementById("prizeSelect").value;
    const promoCode = document.getElementById("promoCodeText").textContent;
    console.log(promoCode)
    if (!selectedPrizeId || !promoCode) {
        alert("Приз или промокод отсутствует");
        return;
    }

    fetch("/Home/SavePrizeResult", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({
            PrizId: selectedPrizeId,
            Promocod: promoCode
        })
    })
        .then(res => {
            if (res.ok) {
               
                location.reload(); // или перейти на другую страницу
            } else {
                alert("Ошибка при сохранении!");
            }
        });
});


document.getElementById("updateBtn").addEventListener("click", () => {
    // Скрываем модальное окно
    document.getElementById("resultModal").style.display = "none";
   
});




function createConfetti(isGold = false) {
    const colors = isGold
        ? ['gold', '#FFD700', '#FFDF00', '#FADA5E']
        : ['#f44336', '#e91e63', '#9c27b0', '#3f51b5',
            '#2196f3', '#00bcd4', '#4CAF50', '#FFEB3B', '#FF9800'];

    const confettiCount = isGold ? 250 : 200;

    for (let i = 0; i < confettiCount; i++) {
        const confetti = document.createElement('div');
        confetti.className = 'confetti';
        confetti.style.backgroundColor = colors[Math.floor(Math.random() * colors.length)];
        confetti.style.left = Math.random() * 100 + '%';
        confetti.style.top = -10 + 'px';
        confetti.style.width = Math.random() * 6 + 4 + 'px';
        confetti.style.height = Math.random() * 6 + 4 + 'px';
        confetti.style.borderRadius = Math.random() > 0.5 ? '50%' : '0';
        container.appendChild(confetti);

        const animationDuration = Math.random() * 2 + 1.5;

        // Анимация конфетти
        setTimeout(() => {
            confetti.style.opacity = '1';
            confetti.style.animation = `fall ${animationDuration}s linear forwards`;

            // Удаляем конфетти после анимации
            setTimeout(() => {
                confetti.remove();
            }, animationDuration * 1000);
        }, 0);
    }
}