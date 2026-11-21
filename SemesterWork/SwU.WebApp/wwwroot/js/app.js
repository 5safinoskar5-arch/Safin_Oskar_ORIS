// Клиентская логика каталога, форм и админ-панели

var PLACEHOLDER_IMAGE = "/images/cards/placeholder.svg";

// Те же регулярные выражения, что и в ValidationService.cs
var USERNAME_REGEX = /^[a-zA-Z0-9_]{3,20}$/;
var EMAIL_REGEX = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
var CARD_NUMBER_REGEX = /^[A-Z0-9]{2,5}-\d{2,3}$/;
var SET_CODE_REGEX = /^[A-Z0-9]{2,10}$/;
var DATE_REGEX = /^\d{4}-\d{2}-\d{2}$/;

var cardsLayout = "grid";
var cardsRequestId = 0;
// Последний ответ /api/cards: нужен, чтобы перерисовывать grid/table без нового запроса
var currentCards = null;

document.addEventListener("DOMContentLoaded", function () {
    initNavToggle();
    initFiltersToggle();
    initLayoutSwitch();
    initCardSearch();
    initPagination();
    initCommentForm();
    initRegisterForm();
    initLoginForm();
    initAdminSetForm();
    initAdminCardForm();
    initAbilityEditor();
    initCardDeleteForms();
    initConfirmForms();
});

/* ---------- Мобильное меню ---------- */

function initNavToggle() {
    var toggle = document.getElementById("nav-toggle");
    var nav = document.getElementById("main-nav");

    if (!toggle || !nav) {
        return;
    }

    toggle.addEventListener("click", function () {
        var isOpen = nav.classList.toggle("is-open");
        toggle.setAttribute("aria-expanded", isOpen ? "true" : "false");
    });
}

/* ---------- Каталог карт ---------- */

function initFiltersToggle() {
    var toggle = document.getElementById("filters-toggle");
    var drawer = document.getElementById("filters-drawer");
    var overlay = document.getElementById("filters-overlay");

    if (!toggle || !drawer || !overlay) {
        return;
    }

    var closeButton = document.getElementById("filters-close");
    var doneButton = document.getElementById("filters-done");
    var resetButton = document.getElementById("filters-reset");

    function openDrawer() {
        overlay.removeAttribute("hidden");
        drawer.classList.add("is-open");
        drawer.setAttribute("aria-hidden", "false");
        toggle.setAttribute("aria-expanded", "true");
    }

    function closeDrawer() {
        drawer.classList.remove("is-open");
        drawer.setAttribute("aria-hidden", "true");
        toggle.setAttribute("aria-expanded", "false");
        overlay.setAttribute("hidden", "");
    }

    toggle.addEventListener("click", openDrawer);
    overlay.addEventListener("click", closeDrawer);

    if (closeButton) {
        closeButton.addEventListener("click", closeDrawer);
    }

    if (doneButton) {
        doneButton.addEventListener("click", closeDrawer);
    }

    if (resetButton) {
        resetButton.addEventListener("click", function () {
            resetAllFilters();
        });
    }

    document.addEventListener("keydown", function (event) {
        if (event.key === "Escape") {
            closeDrawer();
        }
    });
}

function resetAllFilters() {
    var form = document.querySelector(".catalog-form");
    if (!form) {
        return;
    }

    form.querySelectorAll("select").forEach(function (select) {
        select.selectedIndex = 0;
    });

    // Trait и пары Min/Max — текстовые и числовые поля, их просто очищаем
    form.querySelectorAll("#filters-drawer input").forEach(function (input) {
        input.value = "";
    });

    setActiveAspect(form, "");
    loadCards(form);
}

function initLayoutSwitch() {
    var buttons = document.querySelectorAll(".layout-btn[data-layout]");

    if (buttons.length === 0) {
        return;
    }

    buttons.forEach(function (button) {
        button.addEventListener("click", function () {
            cardsLayout = button.getAttribute("data-layout") === "table" ? "table" : "grid";

            buttons.forEach(function (item) {
                var isActive = item === button;
                item.classList.toggle("is-active", isActive);
                item.setAttribute("aria-pressed", isActive ? "true" : "false");
            });

            if (currentCards) {
                paintCards();
            } else {
                // Статическая страница пришла в grid-виде: данные добираем из API
                var form = document.querySelector(".catalog-form");
                if (form) {
                    loadCards(form);
                }
            }
        });
    });
}

function paintCards() {
    var container = document.getElementById("cards-container");
    if (!container || !currentCards) {
        return;
    }

    var isTable = cardsLayout === "table";
    container.classList.toggle("cards-table", isTable);
    container.classList.toggle("cards-grid", !isTable);

    container.innerHTML = isTable
        ? buildTableViewHtml(currentCards)
        : buildGridViewHtml(currentCards);
}

function initCardSearch() {
    var form = document.querySelector(".catalog-form");
    if (!form) {
        return;
    }

    var searchInput = form.querySelector('input[name="search"]');
    var runSearch = debounce(function () {
        loadCards(form);
    }, 300);

    if (searchInput) {
        searchInput.addEventListener("input", runSearch);
    }

    form.addEventListener("submit", function (event) {
        event.preventDefault();
        loadCards(form);
    });

    form.querySelectorAll("select").forEach(function (select) {
        select.addEventListener("change", function () {
            loadCards(form);
        });
    });

    // Trait и пары Min/Max внутри Filters Drawer
    form.querySelectorAll("#filters-drawer input[name]").forEach(function (input) {
        input.addEventListener("input", runSearch);
    });

    form.querySelectorAll(".aspect-btn[data-aspect]").forEach(function (button) {
        button.addEventListener("click", function () {
            var value = button.getAttribute("data-aspect");
            // Повторный клик по активной иконке сбрасывает аспект
            var nextValue = button.classList.contains("is-active") ? "" : value;
            setActiveAspect(form, nextValue);
            loadCards(form);
        });
    });
}

function setActiveAspect(form, value) {
    var input = form.querySelector('input[name="aspect"]');
    if (input) {
        input.value = value;
    }

    form.querySelectorAll(".aspect-btn[data-aspect]").forEach(function (button) {
        var isActive = button.getAttribute("data-aspect") === value;
        button.classList.toggle("is-active", isActive);
        button.setAttribute("aria-pressed", isActive ? "true" : "false");
    });
}

function collectFilterParams(form) {
    var formData = new FormData(form);
    var params = new URLSearchParams();

    formData.forEach(function (value, name) {
        var text = String(value).trim();
        if (text.length > 0) {
            params.append(name, text);
        }
    });

    return params;
}

function loadCards(form, page) {
    var requestId = cardsRequestId + 1;
    cardsRequestId = requestId;

    // Любое изменение фильтров сбрасывает страницу на первую
    var currentPage = page && page > 0 ? page : 1;
    var params = collectFilterParams(form);
    params.set("page", String(currentPage));

    fetch("/api/cards?" + params.toString(), {
        method: "GET",
        credentials: "same-origin",
        headers: { "Accept": "application/json" }
    })
        .then(function (response) {
            if (!response.ok) {
                throw new Error("HTTP " + response.status);
            }
            return response.json();
        })
        .then(function (data) {
            // Пока запрос летел, фильтры могли измениться: старый ответ игнорируем
            if (requestId !== cardsRequestId) {
                return;
            }

            renderCards(data);
        })
        .catch(function () {
            if (requestId !== cardsRequestId) {
                return;
            }

            var counter = document.getElementById("cards-count");
            if (counter) {
                counter.textContent = "Could not load cards";
            }
        });
}

/* Ответ /api/cards: { items, totalCount, page, pageSize, totalPages } */
function renderCards(data) {
    var cards = data && Array.isArray(data.items) ? data.items : [];
    var totalCount = data && typeof data.totalCount === "number" ? data.totalCount : cards.length;

    currentCards = cards;
    paintCards();
    updateCardsCount(totalCount);
    updateEmptyState(totalCount);
    renderPagination(data);
}

function updateEmptyState(count) {
    var emptyState = document.getElementById("cards-empty-state");
    if (!emptyState) {
        return;
    }

    if (count === 0) {
        emptyState.removeAttribute("hidden");
    } else {
        emptyState.setAttribute("hidden", "");
    }
}

/* ---------- Пагинация каталога ---------- */

function initPagination() {
    var nav = document.getElementById("pagination");
    if (!nav) {
        return;
    }

    // Делегирование: панель перерисовывается после каждого AJAX-запроса
    nav.addEventListener("click", function (event) {
        var target = event.target;
        if (!target || !target.closest) {
            return;
        }

        var link = target.closest("a[data-page]");
        if (!link) {
            return;
        }

        var form = document.querySelector(".catalog-form");
        var page = parseInt(link.getAttribute("data-page"), 10);
        if (!form || isNaN(page) || page < 1) {
            return;
        }

        // Без JS та же ссылка работает как обычный переход на ?page=...
        event.preventDefault();
        loadCards(form, page);
    });
}

function renderPagination(data) {
    var nav = document.getElementById("pagination");
    if (!nav) {
        return;
    }

    nav.innerHTML = data ? buildPaginationHtml(data) : "";
}

function buildPaginationHtml(data) {
    var totalPages = parseInt(data.totalPages, 10) || 0;
    var currentPage = parseInt(data.page, 10) || 1;

    if (totalPages <= 1) {
        return "";
    }

    var html = buildPageLinkHtml("Previous", currentPage - 1, currentPage <= 1, false);

    var start = Math.max(1, currentPage - 1);
    var end = Math.min(totalPages, start + 2);
    start = Math.max(1, end - 2);

    for (var number = start; number <= end; number++) {
        html += buildPageLinkHtml(String(number), number, false, number === currentPage);
    }

    html += buildPageLinkHtml("Next", currentPage + 1, currentPage >= totalPages, false);
    return html;
}

function buildPageLinkHtml(text, targetPage, isDisabled, isActive) {
    var safeText = escapeHtml(text);
    var safePage = parseInt(targetPage, 10) || 1;

    if (isActive) {
        return '<span class="page-link is-active">' + safeText + "</span>";
    }

    if (isDisabled) {
        return '<span class="page-link is-disabled">' + safeText + "</span>";
    }

    return '<a class="page-link" href="' + escapeHtml(buildCardsUrl(safePage)) +
        '" data-page="' + safePage + '">' + safeText + "</a>";
}

// Ссылка пагинации сохраняет текущие фильтры формы, чтобы работал и обычный переход
function buildCardsUrl(page) {
    var form = document.querySelector(".catalog-form");
    var params = form ? collectFilterParams(form) : new URLSearchParams();
    params.set("page", String(page));
    return "/cards?" + params.toString();
}

/* Grid: картинка + одна строка с номером карты */
function buildGridViewHtml(cards) {
    var html = "";

    cards.forEach(function (card) {
        var detailUrl = "/cards/detail?id=" + parseCardId(card);
        var name = escapeHtml(card.name);
        var imageUrl = escapeHtml(resolveImageUrl(card.imageUrlDisplay || card.imageUrl));

        html += '<article class="card-tile">' +
            '<a class="card-cover" href="' + detailUrl + '">' +
            '<img src="' + imageUrl + '" alt="' + name + '" title="' + name + '" loading="lazy" ' +
            'onerror="this.onerror=null;this.src=\'' + PLACEHOLDER_IMAGE + '\';">' +
            "</a>" +
            '<p class="card-grid-caption">' + escapeHtml(card.cardNumber) + "</p>" +
            "</article>";
    });

    return html;
}

/* Table: 8 колонок без мини-картинок */
function buildTableViewHtml(cards) {
    var html = '<table class="cards-data-table">' +
        "<thead><tr>" +
        "<th>Name</th><th>SWU Id</th><th>Cost</th><th>Aspects</th>" +
        "<th>Traits</th><th>Power</th><th>Health</th><th>Card Type</th>" +
        "</tr></thead><tbody>";

    cards.forEach(function (card) {
        html += "<tr>" +
            '<td class="cards-data-name"><a href="/cards/detail?id=' + parseCardId(card) + '">' +
            escapeHtml(card.name) + "</a></td>" +
            "<td>" + escapeHtml(card.cardNumber) + "</td>" +
            "<td>" + escapeHtml(card.cost) + "</td>" +
            "<td>" + buildAspectBadges(card.aspect) + "</td>" +
            "<td>" + escapeHtml(card.traits || "—") + "</td>" +
            "<td>" + formatStat(card.power) + "</td>" +
            "<td>" + formatStat(card.hp) + "</td>" +
            "<td>" + escapeHtml(card.cardType) + "</td>" +
            "</tr>";
    });

    return html + "</tbody></table>";
}

function buildAspectBadges(rawAspect) {
    var text = rawAspect === null || rawAspect === undefined ? "" : String(rawAspect).trim();
    if (text.length === 0) {
        return "—";
    }

    var html = "";
    text.split(",").forEach(function (part) {
        var label = part.trim();
        if (label.length > 0) {
            html += '<span class="table-badge">' + escapeHtml(label) + "</span>";
        }
    });

    return html;
}

function formatStat(value) {
    if (value === null || value === undefined) {
        return "—";
    }

    return escapeHtml(value);
}

function parseCardId(card) {
    var id = parseInt(card.id, 10);
    return isNaN(id) ? 0 : id;
}

function updateCardsCount(count) {
    var counter = document.getElementById("cards-count");
    if (counter) {
        counter.textContent = count + " cards found";
    }
}

// Разрешаем только http(s) и пути от корня сайта, чтобы не подставлять опасные схемы
function resolveImageUrl(rawUrl) {
    if (typeof rawUrl !== "string") {
        return PLACEHOLDER_IMAGE;
    }

    var url = rawUrl.trim();
    if (url.length === 0) {
        return PLACEHOLDER_IMAGE;
    }

    var lower = url.toLowerCase();
    if (lower.indexOf("http://") === 0 ||
        lower.indexOf("https://") === 0 ||
        (url.charAt(0) === "/" && url.indexOf("//") !== 0)) {
        return url;
    }

    return PLACEHOLDER_IMAGE;
}

/* ---------- Экранирование и мелкие помощники ---------- */

function escapeHtml(value) {
    if (value === null || value === undefined) {
        return "";
    }

    return String(value)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#39;");
}

function debounce(callback, delay) {
    var timerId = null;

    return function () {
        if (timerId !== null) {
            window.clearTimeout(timerId);
        }
        timerId = window.setTimeout(callback, delay);
    };
}

/* ---------- Комментарии на странице карты ---------- */

function initCommentForm() {
    var form = document.querySelector('form[action="/cards/comment"]');
    if (!form) {
        return;
    }

    var body = form.querySelector('textarea[name="body"]');
    var title = form.querySelector('input[name="title"]');
    var counter = document.getElementById("comment-char-counter");

    if (!body || !counter) {
        return;
    }

    form.noValidate = true; // Ошибки показываем сами, в .field-error

    function updateCounter() {
        var length = body.value.length;
        counter.textContent = length + " / 500";
        counter.classList.toggle("is-invalid", length < 3 || length > 500);
    }

    body.addEventListener("input", function () {
        clearFieldError(body);
        hideFormAlert(form);
        updateCounter();
    });

    updateCounter();

    form.addEventListener("submit", function (event) {
        var textLength = body.value.trim().length;
        var titleValue = title ? title.value.trim() : "";

        var valid = true;
        valid = validateField(body, textLength >= 3 && textLength <= 500, "Please enter from 3 to 500 characters.") && valid;
        valid = validateField(
            title,
            titleValue.length === 0 || (titleValue.length >= 3 && titleValue.length <= 120),
            "Title must be from 3 to 120 characters."
        ) && valid;

        handleFormValidity(event, form, valid);
    });

    bindFieldReset(form);
}

/* ---------- Формы входа и регистрации ---------- */

function initRegisterForm() {
    var form = document.querySelector('form[action="/register"]');
    if (!form) {
        return;
    }

    var username = form.querySelector('input[name="username"]');
    var email = form.querySelector('input[name="email"]');
    var password = form.querySelector('input[name="password"]');
    var confirmPassword = form.querySelector('input[name="confirmPassword"]');

    form.noValidate = true;

    form.addEventListener("submit", function (event) {
        var passwordValue = password ? password.value : "";
        var confirmValue = confirmPassword ? confirmPassword.value : "";

        var valid = true;
        valid = validateField(
            username,
            username && USERNAME_REGEX.test(username.value.trim()),
            "3–20 characters: Latin letters, digits and underscore."
        ) && valid;
        valid = validateField(
            email,
            email && EMAIL_REGEX.test(email.value.trim()),
            "Please enter a valid email address."
        ) && valid;
        valid = validateField(
            password,
            passwordValue.length >= 6 && passwordValue.length <= 100,
            "Password must be 6 to 100 characters long."
        ) && valid;
        valid = validateField(
            confirmPassword,
            confirmValue.length > 0 && confirmValue === passwordValue,
            "Passwords must match."
        ) && valid;

        handleFormValidity(event, form, valid);
    });

    bindFieldReset(form);
}

function initLoginForm() {
    var form = document.querySelector('form[action="/login"]');
    if (!form) {
        return;
    }

    var login = form.querySelector('input[name="login"]');
    var password = form.querySelector('input[name="password"]');

    form.noValidate = true;

    form.addEventListener("submit", function (event) {
        var valid = true;
        valid = validateField(
            login,
            login && login.value.trim().length > 0,
            "Please enter your login or email."
        ) && valid;
        valid = validateField(
            password,
            password && password.value.length > 0,
            "Please enter your password."
        ) && valid;

        handleFormValidity(event, form, valid);
    });

    bindFieldReset(form);
}

/* ---------- Админ-панель: сет и карта ---------- */

function initAdminSetForm() {
    var form = document.querySelector('form[action="/admin/sets/create"]');
    if (!form) {
        return;
    }

    var code = form.querySelector('input[name="code"]');
    var name = form.querySelector('input[name="name"]');
    var releaseDate = form.querySelector('input[name="releaseDate"]');

    form.noValidate = true;

    form.addEventListener("submit", function (event) {
        var valid = true;
        valid = validateField(
            code,
            code && SET_CODE_REGEX.test(code.value.trim()),
            "2–10 uppercase letters or digits."
        ) && valid;
        valid = validateField(
            name,
            name && name.value.trim().length > 0,
            "Please enter the set name."
        ) && valid;
        valid = validateField(
            releaseDate,
            releaseDate && DATE_REGEX.test(releaseDate.value),
            "Please choose a release date."
        ) && valid;

        handleFormValidity(event, form, valid);
    });

    bindFieldReset(form);
}

function initAdminCardForm() {
    var form = document.querySelector('form[action="/admin/cards/create"], form[action="/admin/cards/edit"]');
    if (!form) {
        return;
    }

    var cardNumber = form.querySelector('input[name="cardNumber"]');
    var name = form.querySelector('input[name="name"]');
    var setId = form.querySelector('select[name="setId"]');
    var cardType = form.querySelector('input[name="cardType"]');
    var aspect = form.querySelector('input[name="aspect"]');
    var rarity = form.querySelector('input[name="rarity"]');
    var cost = form.querySelector('input[name="cost"]');
    var power = form.querySelector('input[name="power"]');
    var hp = form.querySelector('input[name="hp"]');

    form.noValidate = true;

    form.addEventListener("submit", function (event) {
        var valid = true;

        valid = validateField(
            cardNumber,
            cardNumber && CARD_NUMBER_REGEX.test(cardNumber.value.trim()),
            "Use a code like SOR-001 or IC27-061."
        ) && valid;
        valid = validateField(
            name,
            name && name.value.trim().length > 0,
            "Please enter the card name."
        ) && valid;
        valid = validateField(
            setId,
            setId && parseInt(setId.value, 10) > 0,
            "Please choose a set."
        ) && valid;
        valid = validateField(
            cardType,
            cardType && cardType.value.trim().length > 0,
            "Please enter the card type."
        ) && valid;
        valid = validateField(
            aspect,
            aspect && aspect.value.trim().length > 0,
            "Please enter an aspect."
        ) && valid;
        valid = validateField(
            rarity,
            rarity && rarity.value.trim().length > 0,
            "Please enter a rarity."
        ) && valid;
        valid = validateField(
            cost,
            cost && isIntegerInRange(cost.value, 0, 20),
            "Cost must be from 0 to 20."
        ) && valid;
        valid = validateField(
            power,
            power && isOptionalIntegerInRange(power.value, 0, 30),
            "Power must be from 0 to 30."
        ) && valid;
        valid = validateField(
            hp,
            hp && isOptionalIntegerInRange(hp.value, 0, 50),
            "HP must be from 0 to 50."
        ) && valid;

        handleFormValidity(event, form, valid);
    });

    bindFieldReset(form);
}

/* ---------- AJAX-удаление карты ---------- */

function initCardDeleteForms() {
    var forms = document.querySelectorAll("[data-delete-card-form]");

    forms.forEach(function (form) {
        form.addEventListener("submit", function (event) {
            event.preventDefault();
            if (!window.confirm("Delete this card permanently?")) {
                return;
            }

            deleteCard(form);
        });
    });
}

function deleteCard(form) {
    var idInput = form.querySelector('input[name="id"]');
    if (!idInput) {
        return;
    }

    var button = form.querySelector('button[type="submit"]');
    if (button) {
        button.disabled = true;
    }

    var body = new URLSearchParams();
    body.set("id", idInput.value);

    fetch("/api/admin/cards/delete", {
        method: "POST",
        credentials: "same-origin",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded;charset=UTF-8"
        },
        body: body.toString()
    })
        .then(function (response) {
            return response.json().then(function (data) {
                return { ok: response.ok, data: data };
            });
        })
        .then(function (result) {
            if (!result.ok || !result.data.success) {
                window.alert(result.data.error || "Could not delete the card.");
                return;
            }

            var row = form.closest("tr");
            if (row) {
                row.remove();
            }
        })
        .catch(function () {
            window.alert("Network error while deleting the card.");
        })
        .finally(function () {
            if (button) {
                button.disabled = false;
            }
        });
}

/* ---------- Подтверждение опасных действий ---------- */

function initConfirmForms() {
    var forms = document.querySelectorAll("[data-confirm]");

    forms.forEach(function (form) {
        form.addEventListener("submit", function (event) {
            var message = form.getAttribute("data-confirm") || "Confirm this action?";
            if (!window.confirm(message)) {
                event.preventDefault();
            }
        });
    });
}

/* ---------- Валидация: помощники ---------- */

function validateField(control, isValid, message) {
    if (!control) {
        return true;
    }

    if (isValid) {
        clearFieldError(control);
        return true;
    }

    setFieldError(control, message);
    return false;
}

function findFieldError(control) {
    var field = control.closest ? control.closest(".field") : null;
    if (!field) {
        return null;
    }

    return field.querySelector(".field-error");
}

function setFieldError(control, message) {
    var error = findFieldError(control);
    if (error) {
        error.textContent = message;
        error.hidden = false;
    }

    control.classList.add("is-invalid");
    control.setAttribute("aria-invalid", "true");
}

function clearFieldError(control) {
    if (!control) {
        return;
    }

    control.classList.remove("is-invalid");
    control.removeAttribute("aria-invalid");

    var error = findFieldError(control);
    if (error) {
        error.hidden = true;
    }
}

function handleFormValidity(event, form, isValid) {
    if (isValid) {
        hideFormAlert(form);
        return;
    }

    event.preventDefault();
    showFormAlert(form, "Please fix the highlighted fields.");

    var firstInvalid = form.querySelector(".is-invalid");
    if (firstInvalid) {
        firstInvalid.focus();
    }
}

function showFormAlert(form, message) {
    var alert = form.querySelector("[data-client-alert]");

    if (!alert) {
        alert = document.createElement("div");
        alert.className = "alert alert-error";
        alert.setAttribute("data-client-alert", "");
        form.insertBefore(alert, form.firstChild);
    }

    alert.textContent = message;
    alert.hidden = false;
}

function hideFormAlert(form) {
    var alert = form.querySelector("[data-client-alert]");
    if (alert) {
        alert.hidden = true;
    }
}

function bindFieldReset(form) {
    form.addEventListener("input", function (event) {
        var control = event.target;
        if (control.closest && control.closest(".field")) {
            clearFieldError(control);
        }
        hideFormAlert(form);
    });
}

function isIntegerInRange(rawValue, min, max) {
    var text = String(rawValue === null || rawValue === undefined ? "" : rawValue).trim();
    if (!/^-?\d+$/.test(text)) {
        return false;
    }

    var value = parseInt(text, 10);
    return value >= min && value <= max;
}

function isOptionalIntegerInRange(rawValue, min, max) {
    var text = String(rawValue === null || rawValue === undefined ? "" : rawValue).trim();
    if (text.length === 0) {
        return true;
    }

    return isIntegerInRange(text, min, max);
}

/* ---------- Редактор Quill в форме карты ---------- */

function initAbilityEditor() {
    var editorElement = document.getElementById("ability-editor");
    var source = document.getElementById("ability-text");

    if (!editorElement || !source || typeof window.Quill === "undefined") {
        return;
    }

    var initialHtml = source.value || "";
    editorElement.style.display = "block";
    source.style.display = "none";

    var quill = new window.Quill(editorElement, {
        theme: "snow",
        placeholder: "Enter ability text...",
        modules: {
            toolbar: [
                ["bold", "italic", "underline"],
                [{ list: "ordered" }, { list: "bullet" }],
                ["clean"]
            ]
        }
    });

    quill.root.innerHTML = initialHtml;

    var form = source.closest("form");
    if (form) {
        form.addEventListener("submit", function () {
            var html = quill.root.innerHTML;
            // Пустой редактор Quill отдаёт <p><br></p> — сохраняем пустую строку
            source.value = html === "<p><br></p>" ? "" : html;
        });
    }
}
