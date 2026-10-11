(() => {
  const $ = s => document.querySelector(s);
  const $$ = s => [...document.querySelectorAll(s)];
  const list = $('#historyList'), search = $('#paymentSearch'), status = $('#statusFilter'), method = $('#methodFilter'), sort = $('#sortFilter'), count = $('#resultCount'), empty = $('#emptyState');
  const toast = $('#toast');
  const showToast = msg => { if (!toast) return; toast.textContent = msg; toast.classList.add('show'); clearTimeout(window.__ttToast); window.__ttToast = setTimeout(() => toast.classList.remove('show'), 2600); };
  const rows = () => $$('.payment-row');
  function render(){
    const q=(search.value||'').trim().toLowerCase(), st=status.value, mt=method.value;
    let visible=rows().filter(r => (st==='all'||r.dataset.status===st) && (mt==='all'||r.dataset.method===mt) && (!q||r.dataset.search.includes(q)));
    const mode=sort.value;
    visible.sort((a,b)=> mode==='high' ? +b.dataset.amount-+a.dataset.amount : mode==='low' ? +a.dataset.amount-+b.dataset.amount : mode==='old' ? a.dataset.date.localeCompare(b.dataset.date) : b.dataset.date.localeCompare(a.dataset.date));
    visible.forEach(r=>list.appendChild(r)); rows().forEach(r=>r.hidden=!visible.includes(r));
    count.textContent=`${visible.length} payment${visible.length===1?'':'s'}`; empty.classList.toggle('show',visible.length===0);
  }
  [search,status,method,sort].forEach(el=>el.addEventListener('input',render));
  $('#clearFilters')?.addEventListener('click',()=>{search.value='';status.value='all';method.value='all';sort.value='recent';render();showToast('Payment filters cleared.');});
  $('#emptyClear')?.addEventListener('click',()=>$('#clearFilters').click());
  $$('.payment-row a').forEach(a=>a.addEventListener('click',()=>showToast('Opening payment record…')));
  render();
})();
