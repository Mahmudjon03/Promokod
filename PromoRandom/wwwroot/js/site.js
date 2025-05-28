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
    const select = document.getElementById("prizeSelect");
    const selectedValue = select.value;
    const prizeName = select.options[select.selectedIndex].text;
    if (selectedValue === "0") {
        showToast("Лутфан, аввал призро интихоб намоед.");
        return;
    }
    fetch("/Home/GetPromoCode?prizeName=" + encodeURIComponent(prizeName))
        .then((res) => res.json())
        .then((data) => {
            const code = data.promoCode.toUpperCase();
            const reels = document.querySelectorAll(".reel .symbols");
            console.log(code);
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
             
                prizeTitle.textContent = prizeTitle;
                winnerName.textContent = data.userName
                promoCodeText.textContent = data.promoCode;

                modal.style.display = "block";
                document.getElementById("prizeTitle").textContent = prizeTitle;
                createConfetti(false);
            }, 24000);
        })
        .catch(() => {
            showToast("Вахтро нодуруст интихоб кардед!");
        });
});

document.getElementById("nextBtn").addEventListener("click", () => {
    const selectedPrizeId = document.getElementById("prizeSelect").value;
    const promoCode = document.getElementById("promoCodeText").textContent;
    console.log(promoCode)
    console.log(selectedPrizeId)
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
            PrizeId: selectedPrizeId,
            Promocode: promoCode
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

function showToast(message, type = "info") {
    let toast = document.getElementById("customToast");
    if (!toast) {
        toast = document.createElement('div');
        toast.id = 'customToast';
        toast.style.position = 'fixed';
        toast.style.bottom = '20px';
        toast.style.left = '50%';
        toast.style.transform = 'translateX(-50%)';
        toast.style.padding = '10px 20px';
        toast.style.borderRadius = '5px';
        toast.style.zIndex = '9999';
        toast.style.fontSize = '16px';
        toast.style.boxShadow = '0 2px 6px rgba(0,0,0,0.3)';
        toast.style.opacity = '0';
        toast.style.transition = 'opacity 0.5s ease';
        document.body.appendChild(toast);
    }

    // Цвет по типу
    const colors = {
        info: 'rgba(51, 51, 51, 0.9)',
        success: '#28a745',
        error: '#dc3545',
        warning: '#ffc107'
    };

    toast.style.backgroundColor = colors[type] || colors.info;
    toast.innerText = message;
    toast.style.opacity = '1';

    setTimeout(() => {
        toast.style.opacity = '0';
    }, 3000);
}
