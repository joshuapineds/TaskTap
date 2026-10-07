(() => {
  const $ = s => document.querySelector(s), $$ = s => [...document.querySelectorAll(s)];
  const toast = (msg) => { const el=$('#toast'); if(!el) return; el.textContent=msg; el.classList.add('show'); clearTimeout(window.__tt); window.__tt=setTimeout(()=>el.classList.remove('show'),2400); };
  const sidebar=$('#sidebar'), backdrop=$('#backdrop');
  $('#mobileMenu')?.addEventListener('click',()=>{sidebar?.classList.add('open');backdrop?.classList.add('show')});
  backdrop?.addEventListener('click',()=>{sidebar?.classList.remove('open');backdrop?.classList.remove('show')});
  const pairs=[['#notificationTrigger','#notificationDropdown'],['#profileTrigger','#profileDropdown']];
  pairs.forEach(([btn,menu])=>$(btn)?.addEventListener('click',e=>{e.stopPropagation();pairs.forEach(([,m])=>$(m)?.classList.remove('open'));$(menu)?.classList.toggle('open')}));
  document.addEventListener('click',()=>pairs.forEach(([,m])=>$(m)?.classList.remove('open')));
  $$('.demo-action').forEach(a=>a.addEventListener('click',e=>{e.preventDefault();toast(a.dataset.message||'This demo action is not connected yet.')}));
  $('#markRead')?.addEventListener('click',e=>{e.preventDefault();toast('Notifications marked as read.');$('#notificationDropdown')?.classList.remove('open')});
  const searchPopover=$('#searchPopover'); $('#searchBtn')?.addEventListener('click',()=>searchPopover?.classList.toggle('open'));
  $('#logout')?.addEventListener('click',e=>{e.preventDefault();toast('Logging out…');setTimeout(()=>location.href='auth.html?mode=login',450)});
})();
