const $ = selector => document.querySelector(selector);
const $$ = selector => [...document.querySelectorAll(selector)];

const requestStep = $("#requestStep");
const resetStep = $("#resetStep");
const successStep = $("#successStep");
const stepOne = $("#stepOne");
const stepTwo = $("#stepTwo");
const stepLine = $("#stepLine");
const flowIcon = $("#flowIcon");
const stageLabel = $("#stageLabel");

$$(".eye").forEach(button => {
  button.addEventListener("click", () => {
    const input = $("#" + button.dataset.target);
    const showing = input.type === "password";
    input.type = showing ? "text" : "password";
    button.setAttribute("aria-label", showing ? "Hide password" : "Show password");
  });
});

function showStep(step) {
  requestStep.hidden = step !== 1;
  resetStep.hidden = step !== 2;
  successStep.hidden = step !== 3;

  stepOne.className = step === 1 ? "active" : "done";
  stepOne.textContent = step === 1 ? "1" : "✓";
  stepLine.className = step >= 2 ? "done" : "";
  stepTwo.className = step === 2 ? "active" : step === 3 ? "done" : "";
  stepTwo.textContent = step === 3 ? "✓" : "2";
  flowIcon.className = step === 3 ? "flow-icon green" : step === 1 ? "flow-icon blue" : "flow-icon green";
  flowIcon.textContent = step === 1 ? "✉" : step === 2 ? "✓" : "✓";
  stageLabel.textContent = step === 1 ? "Step 1 of 2" : step === 2 ? "Step 2 of 2" : "Complete";
  stageLabel.hidden = step === 3;
}

const forgotForm = $("#forgotForm");
forgotForm.addEventListener("submit", event => {
  event.preventDefault();
  const contact = $("#accountContact").value.trim();
  $("#resetTarget").textContent = contact || "your account";
  showStep(2);
  $("#newPassword").focus();
});

const password = $("#newPassword");
const confirmPassword = $("#confirmPassword");
const match = $("#match");
const strengthText = $("#strengthText");
const strengthBars = [...document.querySelectorAll(".strength > span")];

function updateStrength() {
  const value = password.value;
  const score =
    (value.length >= 8) +
    (/[0-9]/.test(value)) +
    (/[^A-Za-z0-9]/.test(value)) +
    (value.length >= 12);

  strengthBars.forEach((bar, index) => {
    bar.style.background = index < score ? "#0875e8" : "#e7edf2";
  });
  strengthText.textContent = score < 2 ? "Weak password" : score < 4 ? "Good password" : "Strong password";
}

function updateMatch() {
  if (!confirmPassword.value) {
    confirmPassword.setCustomValidity("");
    match.textContent = "";
    match.className = "match";
    return;
  }

  const matches = password.value === confirmPassword.value;
  confirmPassword.setCustomValidity(matches ? "" : "Passwords do not match.");
  match.textContent = matches ? "Passwords match." : "Passwords do not match.";
  match.className = matches ? "match ok" : "match bad";
}

password.addEventListener("input", () => {
  updateStrength();
  updateMatch();
});
confirmPassword.addEventListener("input", updateMatch);

$("#resetForm").addEventListener("submit", event => {
  event.preventDefault();
  updateMatch();
  if (password.value !== confirmPassword.value) {
    confirmPassword.reportValidity();
    return;
  }
  showStep(3);
});

$("#changeContact").addEventListener("click", event => {
  event.preventDefault();
  showStep(1);
  $("#accountContact").focus();
});

if (new URLSearchParams(location.search).get("stage") === "reset") {
  showStep(2);
} else {
  showStep(1);
}
