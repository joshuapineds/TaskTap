const $=s=>document.querySelector(s),$$=s=>[...document.querySelectorAll(s)];
const toast=m=>{const e=$("#toast");e.textContent=m;e.classList.add("show");clearTimeout(window.tt);window.tt=setTimeout(()=>e.classList.remove("show"),2300)};
$$(".eye").forEach(b=>b.onclick=()=>{const i=$("#"+b.dataset.target);i.type=i.type==="password"?"text":"password"});
if(new URLSearchParams(location.search).get("mode")==="register")location.replace("register.html");
$("#loginForm").onsubmit=e=>{e.preventDefault();toast("Demo login submitted. The authenticated dashboard can be connected next.");setTimeout(()=>location.href="dashboard.html",500)};
$("#registerForm").onsubmit=e=>{e.preventDefault();toast("Account created in demo mode. Redirecting to your dashboard.");setTimeout(()=>location.href="dashboard.html",500)};
const fm=$("#forgotModal");$("#forgot").onclick=()=>location.href="forgot-password.html";$("#forgotClose").onclick=()=>fm.hidden=true;fm.onclick=e=>{if(e.target===fm)fm.hidden=true};
$("#resetForm").onsubmit=e=>{e.preventDefault();fm.hidden=true;toast("Password reset instructions submitted in demo mode.")};
$$(".social").forEach(b=>b.onclick=()=>toast("Social sign-in is a frontend placeholder for now."));
