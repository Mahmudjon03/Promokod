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
                let speed = 5; // ms между шагами прокрутки
                let steps = 20;
                const maxSteps = 180 + i * 95; // разная длительность для каждого барабана

                function spin() {
                    steps++;
                    position += 1; // смещение вниз на 1px
                    if (position > 50 * chars.length) position = 0; // зациклить

                    symbolsDiv.style.transform = `translateY(-${position}px)`;

                    if (steps < maxSteps) {
                        setTimeout(spin, speed);
                    } else {
                        // Останавливаемся на нужном символе
                        // Смещаем точно, чтобы показать нужный символ сверху
                        const finalPosition = 50 * targetIndex;
                        symbolsDiv.style.transition = "transform 0.8s ease-out";
                        symbolsDiv.style.transform = `translateY(-${finalPosition}px)`;
                    }
                }
                spin();
            });

            // Показываем приз через время, когда все барабаны остановятся
            setTimeout(() => {
                const modal = document.getElementById("resultModal");
                const prizeTitle = document.getElementById("prizeTitle");
                const winnerName = document.getElementById("winnerName");
                const promoCodeText = document.getElementById("promoCodeText");

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

                createConfetti(false);
            }, 8000);
        })
        .catch(() => {
            document.getElementById("prizeText").textContent = "Ошибка получения промокода";
        });
});
document.querySelector(".close").addEventListener("click", () => {
    document.getElementById("resultModal").style.display = "none";
});
window.addEventListener("click", (event) => {
    const modal = document.getElementById("resultModal");
    if (event.target === modal) modal.style.display = "none";
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