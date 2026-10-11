const $=s=>document.querySelector(s),$$=s=>[...document.querySelectorAll(s)];
$$(".eye").forEach(b=>b.onclick=()=>{const i=$("#"+b.dataset.target);i.type=i.type==="password"?"text":"password"});
const p=$("#password"),c=$("#confirm"),meter=[...document.querySelectorAll(".password-meter i")],mt=$("#meterText");
function update(){let n=(p.value.length>=8)+( /[0-9]/.test(p.value))+( /[^A-Za-z0-9]/.test(p.value))+(p.value.length>=12);meter.forEach((x,i)=>x.style.background=i<n?"#0875e8":"#e7edf2");mt.textContent=n<2?"Use 8+ characters with a number or special character.":n<4?"Good password.":"Strong password."}p.addEventListener("input",update);
$("#registerForm").onsubmit=e=>{e.preventDefault();if(p.value!==c.value){c.setCustomValidity("Passwords do not match.");c.reportValidity();return}c.setCustomValidity("");$("#success").hidden=false};
