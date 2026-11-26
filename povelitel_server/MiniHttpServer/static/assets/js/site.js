(function () {
  "use strict";

  var NAME_RE = /^[A-Za-zА-Яа-яЁё0-9\s\-']{2,60}$/;
  var NUM_RE = /^\d{1,3}$/;
  var SLUG_RE = /^[a-z0-9\-]{2,80}$/;
  var USER_RE = /^[A-Za-z0-9_]{3,30}$/;

  function $(sel, root) { return (root || document).querySelector(sel); }
  function $all(sel, root) { return Array.prototype.slice.call((root || document).querySelectorAll(sel)); }
  function esc(s) {
    return String(s == null ? "" : s).replace(/[&<>"]/g, function (c) {
      return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" }[c];
    });
  }

  function initSelects() {
    $all("select[data-selected]").forEach(function (sel) {
      var v = sel.getAttribute("data-selected");
      if (v != null && v !== "") {
        for (var i = 0; i < sel.options.length; i++) {
          if (sel.options[i].value === v) { sel.selectedIndex = i; break; }
        }
      }
    });
  }

  function initCharCounters() {
    $all(".char-count").forEach(function (badge) {
      var id = badge.getAttribute("data-for");
      var field = document.getElementById(id);
      if (!field) return;
      function upd() {
        var max = field.getAttribute("maxlength") || 0;
        badge.textContent = (field.value.length) + (max ? "/" + max : "");
      }
      field.addEventListener("input", upd);
      upd();
    });
  }

  function shrink(dataUrl, cb) {
    var img = new Image();
    img.onload = function () {
      var max = 600, w = img.width, h = img.height;
      if (w > max || h > max) { var s = Math.min(max / w, max / h); w = Math.round(w * s); h = Math.round(h * s); }
      var c = document.createElement("canvas");
      c.width = w; c.height = h;
      c.getContext("2d").drawImage(img, 0, 0, w, h);
      try { cb(c.toDataURL("image/jpeg", 0.85)); }
      catch (e) { cb(dataUrl); }
    };
    img.onerror = function () { cb(dataUrl); };
    img.src = dataUrl;
  }

  function bindUpload(input) {
    var dz = input.closest(".upload-dropzone");
    if (!dz) return;
    function handle(file) {
      if (!file) return;
      if (!file.type.startsWith("image/")) { alert("Выберите файл изображения (JPG, PNG, SVG)."); return; }
      if (file.size > 5 * 1024 * 1024) { alert("Файл слишком большой — до 5 МБ."); return; }
      var reader = new FileReader();
      reader.onload = function (e) {
        var raw = e.target.result;
        var prev = dz.querySelector(".upload-preview");
        if (prev) { prev.src = raw; prev.hidden = false; }
        dz.classList.add("has-image");
        shrink(raw, function (small) {
          var hidden = dz.querySelector("input[type=hidden]");
          if (hidden) hidden.value = small;
        });
      };
      reader.readAsDataURL(file);
    }
    input.addEventListener("change", function (e) { handle(e.target.files[0]); });
    ["dragover", "dragenter"].forEach(function (ev) {
      dz.addEventListener(ev, function (e) { e.preventDefault(); dz.classList.add("dragover"); });
    });
    ["dragleave", "dragend"].forEach(function (ev) {
      dz.addEventListener(ev, function (e) { e.preventDefault(); dz.classList.remove("dragover"); });
    });
    dz.addEventListener("drop", function (e) {
      e.preventDefault(); dz.classList.remove("dragover");
      if (e.dataTransfer && e.dataTransfer.files[0]) handle(e.dataTransfer.files[0]);
    });
  }

  function initUploads() {
    $all(".upload-input").forEach(bindUpload);
  }

  function initQuill() {
    var ed = document.getElementById("editor");
    if (!ed) return;
    var initial = document.getElementById("initial-content");
    var initialHtml = initial ? initial.textContent.trim() : "";
    var form = ed.closest("form");
    var hidden = document.getElementById("content");
    if (window.Quill) {
      var quill = new Quill("#editor", {
        theme: "snow",
        modules: { toolbar: [["header", [2, 3]], ["bold", "italic"], ["link"], ["blockquote"], ["list", "bullet"]] }
      });
      quill.root.innerHTML = initialHtml;
      if (form) form.addEventListener("submit", function () { if (hidden) hidden.value = quill.root.innerHTML; });
    } else {
      var ta = document.createElement("textarea");
      ta.name = "content";
      ta.rows = 12;
      ta.value = initialHtml;
      ta.className = "ql-fallback";
      ed.parentNode.replaceChild(ta, ed);
      if (hidden) hidden.parentNode.removeChild(hidden);
    }
  }

  function showFormErrors(form, messages) {
    var box = form.querySelector(".form-errors");
    if (!box) { box = document.createElement("div"); box.className = "form-errors"; form.insertBefore(box, form.firstChild); }
    box.innerHTML = "<strong>Исправьте ошибки:</strong><ul>" + messages.map(function (m) { return "<li>" + esc(m) + "</li>"; }).join("") + "</ul>";
  }

  function validateForm(form) {
    var errors = [];
    if (form.id === "card-form") {
      if (!NAME_RE.test($("#card-name", form).value.trim())) errors.push("Название карты: 2–60 символов.");
      var d = $("#card-desc", form).value; if (d.length < 10 || d.length > 250) errors.push("Описание: 10–250 символов.");
    } else if (form.id === "item-form") {
      if (!NAME_RE.test($("#item-name", form).value.trim())) errors.push("Название предмета: 2–60 символов.");
      if (!NUM_RE.test($("#quantity", form).value)) errors.push("Количество: число 0–999.");
      var id = $("#item-desc", form).value; if (id.length < 10 || id.length > 250) errors.push("Описание: 10–250 символов.");
    } else if (form.id === "rule-form") {
      if (!NAME_RE.test($("#rule-title", form).value.trim())) errors.push("Заголовок: 2–60 символов.");
      if (!SLUG_RE.test($("#rule-slug", form).value)) errors.push("Slug: 2–80 символов (латиница, цифры, дефис).");
    } else if (form.id === "strategy-form") {
      if (!NAME_RE.test($("#strategy-title", form).value.trim())) errors.push("Заголовок: 2–60 символов.");
      if (!SLUG_RE.test($("#strategy-slug", form).value)) errors.push("Slug: 2–80 символов.");
    } else if (form.id === "login-form") {
      if (!USER_RE.test($('input[name=username]', form).value.trim())) errors.push("Логин: 3–30 символов.");
      var pw = $('input[name=password]', form).value; if (pw.length < 6 || pw.length > 100) errors.push("Пароль: 6–100 символов.");
    }
    if (errors.length) { showFormErrors(form, errors); return false; }
    var box = form.querySelector(".form-errors"); if (box) box.remove();
    return true;
  }

  function initValidation() {
    $all("form").forEach(function (form) {
      form.addEventListener("submit", function (e) {
        if (!validateForm(form)) { e.preventDefault(); }
      });
    });
  }

  function renderCards(container, cards) {
    if (!cards.length) { container.innerHTML = '<p class="small">Ничего не найдено.</p>'; return; }
    container.innerHTML = cards.map(function (c) {
      return '<article class="card" data-type="' + esc(c.type) + '">' +
          '<img src="' + esc(c.imagePath || "/assets/images/cards_icon.svg") + '" alt="' + esc(c.name) + '">' +
          '<div class="card-body"><h3>' + esc(c.name) + '</h3>' +
          '<span class="badge">' + esc(c.type) + '</span></div></article>';
    }).join("");
  }

  function initCardSearch() {
    var grid = document.getElementById("card-grid");
    var input = document.getElementById("card-search");
    var btn = document.getElementById("card-search-btn");
    var status = document.getElementById("search-status");
    if (!grid || !input) return;
    var timer = null;
    function run() {
      var q = input.value.trim();
      var url = "/card/search?q=" + encodeURIComponent(q);
      if (status) status.textContent = "Поиск…";
      fetch(url).then(function (r) { return r.json(); }).then(function (data) {
        if (data && data.ok) {
          renderCards(grid, data.cards);
          if (status) status.textContent = data.cards.length ? ("Найдено: " + data.cards.length) : "Ничего не найдено.";
        }
      }).catch(function () { if (status) status.textContent = "Ошибка поиска."; });
    }
    input.addEventListener("input", function () { clearTimeout(timer); timer = setTimeout(run, 300); });
    if (btn) btn.addEventListener("click", run);
  }

  function initFilterTags() {
    function bind(groupId, containerId, attr) {
      var group = document.getElementById(groupId);
      var container = document.getElementById(containerId);
      if (!group || !container) return;
      $all(".tag", group).forEach(function (tag) {
        tag.addEventListener("click", function () {
          $all(".tag", group).forEach(function (t) { t.classList.remove("active"); });
          tag.classList.add("active");
          var v = tag.getAttribute("data-" + attr);
          $all(".card", container).forEach(function (card) {
            var cv = card.getAttribute("data-" + attr);
            card.style.display = (!v || v === cv) ? "" : "none";
          });
        });
      });
    }
    bind("card-filters", "card-grid", "type");
    bind("item-filters", "item-grid", "cat");
  }

  function initDeleteButtons() {
    $all(".photo-del").forEach(function (btn) {
      btn.addEventListener("click", function () {
        var id = btn.getAttribute("data-id");
        var kind = btn.getAttribute("data-kind");
        if (!confirm("Удалить запись #" + id + "?")) return;
        var url = "/admin/delete" + kind + "?id=" + encodeURIComponent(id);
        fetch(url, { method: "POST" }).then(function (r) { return r.json(); }).then(function (data) {
          if (data && data.ok) {
            var row = btn.closest("tr");
            if (row) row.parentNode.removeChild(row);
          } else {
            alert("Не удалось удалить запись.");
          }
        }).catch(function () { alert("Ошибка сети при удалении."); });
      });
    });
  }

  document.addEventListener("DOMContentLoaded", function () {
    initSelects();
    initCharCounters();
    initUploads();
    initQuill();
    initValidation();
    initCardSearch();
    initFilterTags();
    initDeleteButtons();
  });
})();
