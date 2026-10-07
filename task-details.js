const $=s=>document.querySelector(s),$$=s=>[...document.querySelectorAll(s)];
const auth=$("#auth"),toast=m=>{const e=$("#toast");e.textContent=m;e.classList.add("show");clearTimeout(window.t);window.t=setTimeout(()=>e.classList.remove("show"),2300)};
function openAuth(mode="login"){auth.hidden=false;setAuth(mode);document.body.style.overflow="hidden"}
function setAuth(mode){$$(".tab").forEach(x=>x.classList.toggle("active",x.dataset.tab===mode));$("#login").hidden=mode!=="login";$("#register").hidden=mode!=="register"}
function closeAuth(){auth.hidden=true;document.body.style.overflow=""}
$$("[data-auth]").forEach(x=>x.onclick=()=>openAuth(x.dataset.auth));
$("#authClose").onclick=closeAuth;auth.addEventListener("click",e=>{if(e.target===auth)closeAuth()});
$$(".tab").forEach(x=>x.onclick=()=>setAuth(x.dataset.tab));
document.addEventListener("keydown",e=>{if(e.key==="Escape")closeAuth()});
$("#save").onclick=()=>{const b=$("#save");b.classList.toggle("saved");b.textContent=b.classList.contains("saved")?"♥ Saved":"♡ Save task";toast(b.classList.contains("saved")?"Task saved.":"Task removed from saved tasks.")};
$("#request").onclick=()=>location.href="auth.html?mode=login";
$("#report").onclick=()=>toast("Please sign in before reporting a task.");
$("#loginForm").onsubmit=e=>{e.preventDefault();closeAuth();toast("Demo login submitted. Request flow can connect to the backend next.")};
$("#registerForm").onsubmit=e=>{e.preventDefault();closeAuth();toast("Demo registration submitted.")};
$("#menuBtn").onclick=()=>toast("Mobile navigation can be connected to the shared menu component.");
