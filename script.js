const $ = (s, root=document) => root.querySelector(s);
const $$ = (s, root=document) => [...root.querySelectorAll(s)];

const toast = (message) => {
  const el = $("#toast");
  el.textContent = message;
  el.classList.add("show");
  clearTimeout(window.__toastTimer);
  window.__toastTimer = setTimeout(() => el.classList.remove("show"), 2600);
};

$("#mobileMenuBtn")?.addEventListener("click", () => {
  const btn = $("#mobileMenuBtn");
  const nav = $("#mobileNav");
  const open = nav.classList.toggle("open");
  btn.setAttribute("aria-expanded", String(open));
});

$$('#mobileNav a').forEach(a => a.addEventListener('click', () => $("#mobileNav").classList.remove("open")));

const authModal = $("#authModal");
const taskModal = $("#taskModal");

function openAuth(mode="login") {
  authModal.hidden = false;
  setAuthMode(mode);
  document.body.style.overflow = "hidden";
}
function closeModals() {
  authModal.hidden = true;
  taskModal.hidden = true;
  document.body.style.overflow = "";
}
function setAuthMode(mode) {
  $$(".auth-tab").forEach(t => t.classList.toggle("active", t.dataset.authTab === mode));
  $("#loginPanel").hidden = mode !== "login";
  $("#registerPanel").hidden = mode !== "register";
}
$$("[data-auth-tab]").forEach(btn => btn.addEventListener("click", () => setAuthMode(btn.dataset.authTab)));
$("#closeAuth").addEventListener("click", closeModals);
$("#closeTask").addEventListener("click", closeModals);
[authModal, taskModal].forEach(m => m.addEventListener("click", e => { if(e.target === m) closeModals(); }));
document.addEventListener("keydown", e => { if(e.key === "Escape") closeModals(); });

$$("[data-toggle-password]").forEach(btn => btn.addEventListener("click", () => {
  const input = $("#" + btn.dataset.togglePassword);
  input.type = input.type === "password" ? "text" : "password";
}));

$("#loginForm").addEventListener("submit", e => {
  e.preventDefault();
  closeModals();
  toast("Demo login submitted — connect this form to ASP.NET Core authentication.");
});
$("#registerForm").addEventListener("submit", e => {
  e.preventDefault();
  closeModals();
  toast("Demo registration submitted — connect this form to your registration endpoint.");
});

const taskData = {
  "House Cleaning": { price:"₱500", location:"Brgy. 176", deadline:"Tomorrow", description:"House cleaning for a local resident. Guests can preview basic task information; authenticated users can continue to request the task." },
  "Math Tutor": { price:"₱300", location:"Brgy. 178", deadline:"Oct 10", description:"Math tutoring task for a Grade 10 learner. The full task flow can later connect to task requests and messaging." },
  "Pet Care": { price:"₱400", location:"Brgy. 176", deadline:"Oct 12", description:"Pet care assistance posted within the selected barangay. Exact location details should remain protected until appropriate." },
  "Fix leaking faucet": { price:"₱600", location:"Brgy. 176", deadline:"Oct 8", description:"Basic plumbing assistance for a leaking faucet. The task details page can later connect to request, report, and message actions." }
};
$$("[data-task]").forEach(btn => btn.addEventListener("click", () => {
  const name = btn.dataset.task;
  const data = taskData[name] || taskData["House Cleaning"];
  $("#taskModalTitle").textContent = name;
  $("#taskPrice").textContent = data.price;
  $("#taskLocation").textContent = data.location;
  $("#taskDeadline").textContent = data.deadline;
  $("#taskDescription").textContent = data.description;
  taskModal.hidden = false;
  document.body.style.overflow = "hidden";
}));

$$(".heart").forEach(btn => btn.addEventListener("click", () => {
  btn.classList.toggle("saved");
  btn.textContent = btn.classList.contains("saved") ? "♥" : "♡";
  toast(btn.classList.contains("saved") ? "Task saved to your list." : "Task removed from saved tasks.");
}));

const searchInput = $("#searchInput");
const taskGrid = $("#taskGrid");
const noResults = $("#noResults");

function filterTasks() {
  const q = searchInput.value.trim().toLowerCase();
  const active = $(".category-card.active")?.dataset.category || "";
  let visible = 0;

  $$(".task-card", taskGrid).forEach(card => {
    const matchesText = !q || card.dataset.title.toLowerCase().includes(q) || card.dataset.category.toLowerCase().includes(q);
    const matchesCategory = !active || active === "All" || card.dataset.category === active;
    const show = matchesText && matchesCategory;
    card.style.display = show ? "" : "none";
    if (show) visible++;
  });

  noResults.hidden = visible !== 0;
}

$("#heroSearch").addEventListener("submit", e => {
  e.preventDefault();
  $("#available").scrollIntoView({behavior:"smooth", block:"start"});
  filterTasks();
  if(searchInput.value.trim()) toast(`Showing results for “${searchInput.value.trim()}”.`);
});

searchInput.addEventListener("input", filterTasks);

$$(".category-card").forEach(card => card.addEventListener("click", () => {
  $$(".category-card").forEach(c => c.classList.remove("active"));
  card.classList.add("active");
  searchInput.value = "";
  filterTasks();
  $("#available").scrollIntoView({behavior:"smooth", block:"start"});
}));

$("#clearCategory").addEventListener("click", () => {
  $$(".category-card").forEach(c => c.classList.remove("active"));
  $(".category-card").classList.add("active");
  taskGrid.classList.add("show-all");
  $$(".task-card", taskGrid).forEach(c => c.style.display = "");
  noResults.hidden = true;
  toast("Showing more local task categories.");
});

$("#viewAllTasks").addEventListener("click", () => {
  taskGrid.classList.add("show-all");
  filterTasks();
});
$("#resetTasks").addEventListener("click", () => {
  searchInput.value = "";
  $$(".category-card").forEach(c => c.classList.remove("active"));
  $(".category-card").classList.add("active");
  taskGrid.classList.add("show-all");
  filterTasks();
});

$$("[data-toast]").forEach(el => el.addEventListener("click", e => {
  e.preventDefault();
  toast(el.dataset.toast);
}));

// Start with all public task cards visible.
taskGrid.classList.add("show-all");
filterTasks();
