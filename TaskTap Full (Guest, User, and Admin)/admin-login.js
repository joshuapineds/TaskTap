(() => {
  const form = document.getElementById('adminLoginForm');
  const email = document.getElementById('adminEmail');
  const password = document.getElementById('adminPassword');
  const button = document.getElementById('signInButton');
  const toast = document.getElementById('loginToast');
  let toastTimer;
  function showToast(message) {
    toast.textContent = message; toast.classList.add('show');
    window.clearTimeout(toastTimer); toastTimer = window.setTimeout(() => toast.classList.remove('show'), 2500);
  }
  function validate() {
    const emailValid = email.value.trim() !== '' && email.checkValidity();
    const passwordValid = password.value.trim() !== '';
    document.getElementById('emailError').textContent = emailValid || !email.value ? '' : 'Enter a valid email address.';
    document.getElementById('passwordError').textContent = passwordValid || !password.value ? '' : 'Enter your password to continue.';
    email.setAttribute('aria-invalid', String(!emailValid));
    password.setAttribute('aria-invalid', String(!passwordValid));
    email.parentElement.classList.toggle('valid', emailValid);
    return emailValid && passwordValid;
  }
  email.addEventListener('input', validate);
  password.addEventListener('input', validate);
  document.getElementById('togglePassword').addEventListener('click', event => {
    const show = password.type === 'password';
    password.type = show ? 'text' : 'password';
    event.currentTarget.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
  });
  document.getElementById('fillDemo').addEventListener('click', () => {
    email.value = 'admin@tasktap.demo'; password.value = 'demo-preview'; validate();
    showToast('Sample values filled. Continue opens the admin demo only.');
  });
  form.addEventListener('submit', event => {
    event.preventDefault();
    if (!validate()) {
      if (!email.value) { document.getElementById('emailError').textContent = 'Enter your email address.'; email.focus(); }
      else if (!password.value) { document.getElementById('passwordError').textContent = 'Enter your password.'; password.focus(); }
      return;
    }
    button.disabled = true;
    button.querySelector('span').textContent = 'Opening admin demo…';
    window.setTimeout(() => { window.location.href = '[Admin] admin-dashboard.html'; }, 280);
  });
})();
